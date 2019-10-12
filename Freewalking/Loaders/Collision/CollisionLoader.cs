using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Freewalking.UI;
using ICities;
using UnityEngine;

namespace Freewalking.Loaders.Collision
{
    public abstract class CollisionLoader : ILoadingExtension
    {
        private Dictionary<ushort, GameObject> colliders = new Dictionary<ushort, GameObject>();

        public virtual void OnCreated(ILoading loading)
        {
        }

        public virtual void OnReleased()
        {
        }

        public virtual void OnLevelLoaded(LoadMode mode)
        {
            FreewalkingCamera.OnEnterFreewalking += OnEnterFreewalking;
        }

        public virtual void OnLevelUnloading()
        {
            FreewalkingCamera.OnEnterFreewalking -= OnEnterFreewalking;
        }

        private void OnEnterFreewalking(ICamera camera)
        {
            camera.StartRoutine(AddColliders(camera));
        }

        private IEnumerator AddColliders(ICamera camera)
        {
            while (FreewalkingCamera.IsFreewalking)
            {
                camera.GetCameraPosition(out float x, out float y, out float z);
                ushort id = GetNearestObjectId(camera, new Vector3(x, y, z));
                if (!colliders.ContainsKey(id))
                {
                    GameObject gameObject = new GameObject($"object {id}");
                    MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                    gameObject.AddComponent<MeshRenderer>();
                    MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();

                    Mesh mesh = GetMesh(id);
                    CalculateMeshPosition(id, out Vector3 meshPosition, out Quaternion meshRotation);

                    meshFilter.mesh = mesh;
                    meshCollider.sharedMesh = mesh;

                    gameObject.transform.position = meshPosition;
                    gameObject.transform.rotation = meshRotation;
                    colliders.Add(id, gameObject);
                }

                yield return camera.WaitForNextFrame();
            }
        }

        protected abstract ushort GetNearestObjectId(ICamera camera, Vector3 position);
        protected abstract Mesh GetMesh(ushort id);
        protected abstract void CalculateMeshPosition(ushort id, out Vector3 meshPosition, out Quaternion meshRotation);
    }
}