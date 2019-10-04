using System;
using System.Collections;
using ColossalFramework;
using ColossalFramework.Plugins;
using Freewalking.Player;
using ICities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Freewalking.UI
{
    public class FreewalkingCamera : ICameraExtension
    {
        private const string name = "Freewalking Camera";

        public static bool IsFreewalking = false;
        public event Action OnEnterFreewalking;
        public event Action OnExitFreewalking;

        private GameObject player;

        public void OnCreated(ICamera Camera)
        {
            OnEnterFreewalking += delegate
            {
                IsFreewalking = true;
                player = new GameObject("Player");
                Rigidbody rigidbody = player.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;

                CharacterController character = player.AddComponent<CharacterController>();

                FirstPersonController controller = player.AddComponent<FirstPersonController>();
                controller.CinematicCamera = Camera;

                Camera current = UnityEngine.Camera.current;
                Vector3 position = current.transform.position;
                position.y = Camera.managers.terrain.SampleTerrainHeight(position.x, position.z) + 5;
                player.transform.position = position;
                
                current.nearClipPlane = 0.3f;
                Camera.SetAperture(0);
                Camera.SetFocusDistance(0);
                Camera.StartRoutine(FollowTransform(Camera, player.transform, new Vector3(0, 0.8f, 0)));
            };
            OnExitFreewalking += delegate
            {
                IsFreewalking = false;
                Camera current = UnityEngine.Camera.current;
                current.nearClipPlane = 5;
                Object.Destroy(player);
            };
        }

        private IEnumerator FollowTransform(ICamera camera, Transform follow, Vector3 relativeDistance)
        {
            while (IsFreewalking)
            {
                Camera.current.transform.position = new Vector3(
                    follow.position.x + relativeDistance.x, 
                    follow.position.y + relativeDistance.y, 
                    follow.position.z + relativeDistance.z);
                yield return camera.WaitForNextFrame();
            }
        }

        public void OnReleased()
        {
        }

        public IEnumerator OnStart(ICamera camera)
        {
            OnEnterFreewalking?.Invoke();
            yield return new WaitForEndOfFrame();
        }

        public void OnAbort()
        {
            OnExitFreewalking?.Invoke();
        }

        public string Name()
        {
            return name;
        }

        public static bool ShouldAbort(ICameraExtension currentScript)
        {
            return currentScript.Name() == name ? Input.GetKeyDown(KeyCode.Escape) : Input.anyKey;
        }
    }
}