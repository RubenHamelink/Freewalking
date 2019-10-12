using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Freewalking
{
    public class MeshDrawer : MonoBehaviour
    {
        public Mesh Mesh;
        public Vector3 Position;
        public Quaternion Rotation;
        public Material Material;
        public LayerMask Layer;
        public MaterialPropertyBlock MaterialBlock;

        public void Update()
        {
            Graphics.DrawMesh(Mesh, Position, Rotation, Material, Layer, (Camera)null, 0, MaterialBlock);
        }

    }
}
