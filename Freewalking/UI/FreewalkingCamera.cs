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
        public static event Action<ICamera> OnEnterFreewalking;
        public static event Action OnExitFreewalking;


        public void OnCreated(ICamera Camera)
        {
            OnEnterFreewalking += delegate(ICamera camera)
            {
                IsFreewalking = true;
                Camera current = UnityEngine.Camera.current;
                current.nearClipPlane = 0.3f;
                camera.SetAperture(0);
                camera.SetFocusDistance(0);
            };
            OnExitFreewalking += delegate
            {
                IsFreewalking = false;
                Camera current = UnityEngine.Camera.current;
                current.nearClipPlane = 5;
            };
        }


        public void OnReleased()
        {
        }

        public IEnumerator OnStart(ICamera camera)
        {
            OnEnterFreewalking?.Invoke(camera);
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