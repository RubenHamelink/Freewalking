using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.Plugins;
using Freewalking.UI;
using ICities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Freewalking.Player
{
    public class FirstPersonController : MonoBehaviour, ILoadingExtension
    {
        public ICamera CinematicCamera;

        private Camera camera;
        private MouseLook mouseLook;
        private bool isWalking;
        private float walkSpeed = 25;
        private float runSpeed = 50;
        private float runstepLenghten = 0.7f;
        private float jumpSpeed = 10;
        private float stickToGroundForce = 10;
        private float gravityMultiplier = 2;
        private bool useFovKick = true;
        private bool useHeadBob = true;
        private float stepInterval = 5;
        private AudioClip[] footstepSounds; // an array of footstep sounds that will be randomly selected from.
        private AudioClip jumpSound; // the sound played when character leaves the ground.
        private AudioClip landSound; // the sound played when character touches back on ground.

        private bool jump;
        private float yRotation;
        private Vector2 input;
        private Vector3 moveDir = Vector3.zero;
        private CharacterController characterController;
        private CollisionFlags collisionFlags;
        private bool previouslyGrounded;
        private Vector3 originalCameraPosition;
        private float stepCycle;
        private float nextStep;
        private bool jumping;
        private AudioSource audioSource;

        private GameObject player;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            stepCycle = 0f;
            nextStep = stepCycle / 2f;
            camera = Camera.main;
            mouseLook = new MouseLook(transform, camera.transform, CinematicCamera);
        }

        private void Update()
        {
            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!jump)
            {
                jump = Input.GetKeyDown(KeyCode.Space);
            }

            if (!previouslyGrounded && characterController.isGrounded)
            {
//                StartCoroutine(m_JumpBob.DoBobCycle());
//                PlayLandingSound();
                moveDir.y = 0f;
                jumping = false;
            }
            if (!characterController.isGrounded && !jumping && previouslyGrounded)
            {
                moveDir.y = 0f;
            }

            previouslyGrounded = characterController.isGrounded;
        }

        private void FixedUpdate()
        {
            GetInput(out float speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = camera.transform.forward * input.y + camera.transform.right * input.x;

            // get a normal for the surface that is being touched to move along it
            Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out RaycastHit hitInfo,
                characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            moveDir.x = desiredMove.x * speed;
            moveDir.z = desiredMove.z * speed;
            
            if (characterController.isGrounded)
            {
                moveDir.y = -stickToGroundForce;

                if (jump)
                {
                    moveDir.y = jumpSpeed;
//                    PlayJumpSound();
                    jump = false;
                    jumping = true;
                }
            }
            else
            {
                moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
            }
            
            collisionFlags = characterController.Move(moveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
//            UpdateCameraPosition(speed);
            mouseLook.UpdateCursorLock();
        }

        private void ProgressStepCycle(float speed)
        {
            if (characterController.velocity.sqrMagnitude > 0 && (input.x != 0 || input.y != 0))
            {
                stepCycle += (characterController.velocity.magnitude +
                                (speed * (isWalking ? 1f : runstepLenghten))) *
                               Time.fixedDeltaTime;
            }

            if (!(stepCycle > nextStep))
            {
                return;
            }

            nextStep = stepCycle + stepInterval;

//            PlayFootStepAudio();
        }

        private void RotateView()
        {
            mouseLook.LookRotation(transform, camera.transform);
        }

        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            bool waswalking = isWalking;

            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            isWalking = !Input.GetKey(KeyCode.LeftShift);

            // set the desired speed to be walking or running
            speed = isWalking ? walkSpeed : runSpeed;
            input = new Vector2(horizontal, vertical);

            if (input == Vector2.zero)
            {
                if (Input.GetKey(KeyCode.W))
                    input.y += 1;
                if (Input.GetKey(KeyCode.S))
                    input.y -= 1;
                if (Input.GetKey(KeyCode.D))
                    input.x += 1;
                if (Input.GetKey(KeyCode.A))
                    input.x -= 1;
            }

            // normalize input if it exceeds 1 in combined length:
            if (input.sqrMagnitude > 1)
            {
                input.Normalize();
            }
            
            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
//            if (isWalking != waswalking && useFovKick && characterController.velocity.sqrMagnitude > 0)
//            {
//                StopAllCoroutines();
//                StartCoroutine(!isWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
//            }
        }

        public void OnCreated(ILoading loading)
        {
        }

        public void OnReleased()
        {
        }

        public void OnLevelLoaded(LoadMode mode)
        {
            FreewalkingCamera.OnEnterFreewalking += OnEnterFreewalking;
            FreewalkingCamera.OnExitFreewalking += OnExitFreewalking;
        }

        public void OnLevelUnloading()
        {
            FreewalkingCamera.OnEnterFreewalking -= OnEnterFreewalking;
            FreewalkingCamera.OnExitFreewalking -= OnExitFreewalking;
        }

        private void OnEnterFreewalking(ICamera camera)
        {
            player = new GameObject("Player");
            Rigidbody rigidbody = player.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;

            CharacterController character = player.AddComponent<CharacterController>();

            FirstPersonController controller = player.AddComponent<FirstPersonController>();
            controller.CinematicCamera = camera;

            Camera current = UnityEngine.Camera.current;
            Vector3 position = current.transform.position;
            position.y = camera.managers.terrain.SampleTerrainHeight(position.x, position.z) + 5;
            player.transform.position = position;

            camera.StartRoutine(FollowTransform(camera, player.transform, new Vector3(0, 0.5f, 0)));
        }

        private void OnExitFreewalking()
        {
            Destroy(player);
        }

        private IEnumerator FollowTransform(ICamera camera, Transform follow, Vector3 relativeDistance)
        {
            while (FreewalkingCamera.IsFreewalking)
            {
                Camera.current.transform.position = new Vector3(
                    follow.position.x + relativeDistance.x,
                    follow.position.y + relativeDistance.y,
                    follow.position.z + relativeDistance.z);
                yield return camera.WaitForNextFrame();
            }
        }
    }
}