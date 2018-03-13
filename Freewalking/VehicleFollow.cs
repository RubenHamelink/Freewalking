using ColossalFramework;
using UnityEngine;

namespace Freewalking
{
    class VehicleFollow : MonoBehaviour
    {
        Vehicle vehicle;
        ushort vehicleID;

        void Start()
        {
            vehicleID = GetComponent<PlayerControl>().vehicleID;
        }
        void Update()
        {
            VehicleManager vm = Singleton<VehicleManager>.instance;
            vehicle = vm.m_vehicles.m_buffer[(int)vehicleID];
            transform.position = vehicle.GetSmoothPosition(vehicleID);
        }

    }
}
