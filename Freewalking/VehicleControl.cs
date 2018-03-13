using ColossalFramework;
using UnityEngine;

namespace Freewalking
{
    class VehicleControl : MonoBehaviour
    {
        VehicleManager vm;
        public GameObject vehicleGO;
        float currentSpeed;
        float acceleration;
        float maxSpeed;
        float rotSpeed = 2;
        Rigidbody rb;
        Vehicle vehicle;
        VehicleParked parkedVehicle;
        public GameObject car = null;

        public void Start()
        {
            currentSpeed = 0;
            acceleration = 20;
            maxSpeed = 3000;

            if (car == null)
            {
                vehicleGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                vehicleGO.name = "VehicleGO";
                Destroy(vehicleGO.GetComponent<BoxCollider>());

                vm = Singleton<VehicleManager>.instance;

                ushort vehicleID = GetComponent<PlayerControl>().vehicleID;
                ushort parkedID = GetComponent<PlayerControl>().parkedID;

                float vehicleDist = Vector3.SqrMagnitude(transform.position - vm.m_vehicles.m_buffer[(int)vehicleID].GetSmoothPosition(vehicleID));
                float parkedDist = Vector3.SqrMagnitude(transform.position - vm.m_parkedVehicles.m_buffer[(int)parkedID].m_position);

                if (parkedDist > vehicleDist)
                {
                    vehicle = vm.m_vehicles.m_buffer[(int)vehicleID];
                    vm.ReleaseVehicle(vehicleID);

                    vehicleGO.transform.position = vehicle.GetSmoothPosition(vehicleID) + new Vector3(0, 1, 0);
                    vehicleGO.transform.rotation = vehicle.m_frame0.m_rotation;
                    vehicleGO.GetComponent<MeshFilter>().mesh = vehicle.Info.m_mesh;
                    vehicleGO.GetComponent<MeshRenderer>().material = vehicle.Info.m_material;
                }
                else
                {
                    parkedVehicle = vm.m_parkedVehicles.m_buffer[(int)parkedID];
                    vm.ReleaseParkedVehicle(parkedID);

                    vehicleGO.transform.position = parkedVehicle.m_position + new Vector3(0, 1, 0);
                    vehicleGO.transform.rotation = parkedVehicle.m_rotation;
                    vehicleGO.GetComponent<MeshFilter>().mesh = parkedVehicle.Info.m_mesh;
                    vehicleGO.GetComponent<MeshRenderer>().material = parkedVehicle.Info.m_material;
                }
                Bounds bounds = vehicleGO.GetComponent<MeshFilter>().mesh.bounds;
                CapsuleCollider cc = vehicleGO.AddComponent<CapsuleCollider>();
                cc.center = new Vector3(0, bounds.extents.z, 0);

                rb = vehicleGO.AddComponent<Rigidbody>();
            }
            else
            {
                vehicleGO = car;
                rb = vehicleGO.GetComponent<Rigidbody>();
                rb.useGravity = true;
            }
            Quaternion lastRot = vehicleGO.transform.rotation;
            lastRot.z = 0;
            vehicleGO.transform.rotation = lastRot;

            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.angularDrag = 0;

            transform.position = vehicleGO.transform.position;
        }

        void FixedUpdate()
        {
            Vector3 rot = vehicleGO.transform.rotation.eulerAngles;
            Vector3 v = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetAxis("Vertical") > 0)
            {
                currentSpeed = Mathf.Min(currentSpeed + acceleration, maxSpeed);
                if (Input.GetAxis("Vertical") > 0)
                    transform.rotation = vehicleGO.transform.rotation;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetAxis("Vertical") < 0)
            {
                currentSpeed = Mathf.Max(currentSpeed - acceleration, -(maxSpeed / 2));
            }
            if(!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && Input.GetAxis("Vertical") == 0)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0, acceleration);
            }
            if ((Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < -0.2f) && (currentSpeed > 200 || currentSpeed < -200))
            {
                rot -= new Vector3(0, rotSpeed, 0);
            }
            if ((Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0.2f) && (currentSpeed > 200 || currentSpeed < -200))
            {
                rot += new Vector3(0, rotSpeed, 0);
            }

            vehicleGO.transform.rotation = Quaternion.Euler(rot);
            v += vehicleGO.transform.forward * (currentSpeed * Time.deltaTime) * SettingsPanel.speedMod;

            rb.velocity = v;
        }

        void OnCollisionEnter(Collision collision)
        {
            currentSpeed = 0;
        }

        void OnDestroy()
        {
            vehicleGO.GetComponent<Rigidbody>().useGravity = false;
            vehicleGO.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
