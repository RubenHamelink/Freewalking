using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;

namespace Freewalking.Player
{
    public class FirstPersonController : MonoBehaviour
    {
        public ICamera CinematicCamera;

        private Camera camera;
        private MouseLook mouseLook;
        private bool isWalking;
        private float m_WalkSpeed = 5;
        private float m_RunSpeed = 10;
        private float m_RunstepLenghten = 0.7f;
        private float m_JumpSpeed = 10;
        private float m_StickToGroundForce = 10;
        private float m_GravityMultiplier = 2;
        private MouseLook m_MouseLook;
        private bool m_UseFovKick = true;
        private bool m_UseHeadBob = true;
        private float m_StepInterval = 5;
        private AudioClip[] m_FootstepSounds; // an array of footstep sounds that will be randomly selected from.
        private AudioClip m_JumpSound; // the sound played when character leaves the ground.
        private AudioClip m_LandSound; // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            camera = Camera.main;
            mouseLook = new MouseLook(transform, camera.transform, CinematicCamera);
        }

        private void Update()
        {
            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = Input.GetKeyDown(KeyCode.Space);
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
//                StartCoroutine(m_JumpBob.DoBobCycle());
//                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        private void FixedUpdate()
        {
            GetInput(out float speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = camera.transform.forward * m_Input.y + camera.transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out RaycastHit hitInfo,
                m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;
            
            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
//                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
//            UpdateCameraPosition(speed);
            mouseLook.UpdateCursorLock();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude +
                                (speed * (isWalking ? 1f : m_RunstepLenghten))) *
                               Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

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
            speed = isWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            if (m_Input == Vector2.zero)
            {
                if (Input.GetKey(KeyCode.W))
                    m_Input.y += 1;
                if (Input.GetKey(KeyCode.S))
                    m_Input.y -= 1;
                if (Input.GetKey(KeyCode.D))
                    m_Input.x += 1;
                if (Input.GetKey(KeyCode.A))
                    m_Input.x -= 1;
            }

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }
            
            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
//            if (isWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
//            {
//                StopAllCoroutines();
//                StartCoroutine(!isWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
//            }
        }
    }
}