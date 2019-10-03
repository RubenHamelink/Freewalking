using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ICities;
using UnityEngine;

namespace Freewalking
{
    public class FreewalkingCamera : ICameraExtension
    {
        private const string name = "Freewalking Camera";

        public static bool IsFreewalking = false;
        public event Action OnEnterFreewalking;
        public event Action OnExitFreewalking;

        public void OnCreated(ICamera Camera)
        {
            OnEnterFreewalking += delegate { IsFreewalking = true; };
            OnExitFreewalking += delegate { IsFreewalking = false; };
        }

        public void OnReleased()
        {
        }

        public IEnumerator OnStart(ICamera camera)
        {
            OnEnterFreewalking?.Invoke();
            camera.FaceTowards(100, 1, 100);
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
