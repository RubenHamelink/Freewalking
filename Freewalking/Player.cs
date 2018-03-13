using ColossalFramework;
using UnityEngine;

namespace Freewalking
{
    class Player
    {
        public GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);

        public Player()
        {
            player.AddComponent<PlayerControl>();
            player.AddComponent<PlayerWalk>();
            player.layer = 2;
            Rigidbody rb = player.AddComponent<Rigidbody>();
            player.GetComponent<CapsuleCollider>().radius = 1f;
            //rb.useGravity = false;
            rb.maxAngularVelocity = 90;
            rb.angularDrag = 0;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            Physics.gravity = new Vector3(0, -100, 0);

            player.GetComponent<MeshRenderer>().enabled = false;

            CitizenManager cm = Singleton<CitizenManager>.instance;
            CitizenInstance ci = cm.m_instances.m_buffer[(int)UnityEngine.Random.Range(0f, cm.m_instances.m_size)];
            GameObject playerModel = new GameObject();
            Mesh mesh = ci.Info.m_lodMesh;
            playerModel.AddComponent<MeshFilter>().mesh = mesh;
            playerModel.AddComponent<MeshRenderer>().material = ci.Info.m_lodMaterial;
            playerModel.transform.parent = player.transform;
            playerModel.transform.localPosition = new Vector3(0, -mesh.bounds.extents.y, 0);
        }
    }
}
