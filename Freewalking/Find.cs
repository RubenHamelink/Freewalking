using System;
using ColossalFramework;
using UnityEngine;

namespace Freewalking
{
    class Find
    {
        public static ushort FindRoad(Vector3 position)
        {
            Bounds bounds = new Bounds(position, new Vector3(0f, 0f, 0f));
            int num = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);
            NetManager instance = Singleton<NetManager>.instance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = instance.m_segmentGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        NetInfo info = instance.m_segments.m_buffer[(int)num5].Info;
                        if (info.m_class.m_service == ItemClass.Service.Road && !info.m_netAI.IsUnderground() && (info.m_hasForwardVehicleLanes || info.m_hasBackwardVehicleLanes))
                        {
                            ushort startNode = instance.m_segments.m_buffer[(int)num5].m_startNode;
                            ushort endNode = instance.m_segments.m_buffer[(int)num5].m_endNode;
                            Vector3 position2 = instance.m_nodes.m_buffer[(int)startNode].m_position;
                            Vector3 position3 = instance.m_nodes.m_buffer[(int)endNode].m_position;
                            float num7 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z), Mathf.Max(position2.x - bounds.max.x - 64f, position2.z - bounds.max.z - 64f));
                            float num8 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position3.x, bounds.min.z - 64f - position3.z), Mathf.Max(position3.x - bounds.max.x - 64f, position3.z - bounds.max.z - 64f));
                            Vector3 b;
                            int num9;
                            float num10;
                            Vector3 vector;
                            int num11;
                            float num12;
                            if ((num7 < 0f || num8 < 0f) && instance.m_segments.m_buffer[(int)num5].m_bounds.Intersects(bounds) && instance.m_segments.m_buffer[(int)num5].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, VehicleInfo.VehicleType.None, false, out b, out num9, out num10, out vector, out num11, out num12))
                            {
                                float num13 = Vector3.SqrMagnitude(position - b);
                                if (num13 < 400f)
                                {
                                    return num5;
                                }
                            }
                        }
                        num5 = instance.m_segments.m_buffer[(int)num5].m_nextGridSegment;
                        if (++num6 >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return 0;
        }

        public static ushort FindVehicle(Vector3 pos, float maxDistance, Vehicle.Flags flagsRequired, Vehicle.Flags flagsForbidden)
        {
            int num = Mathf.Max((int)((pos.x - maxDistance) / 32f + 270f), 0);
            int num2 = Mathf.Max((int)((pos.z - maxDistance) / 32f + 270f), 0);
            int num3 = Mathf.Min((int)((pos.x + maxDistance) / 32f + 270f), 539);
            int num4 = Mathf.Min((int)((pos.z + maxDistance) / 32f + 270f), 539);
            ushort result = 0;
            float num5 = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    VehicleManager vm = Singleton<VehicleManager>.instance;
                    ushort num6 = vm.m_vehicleGrid[i * 540 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        Vehicle.Flags flags = vm.m_vehicles.m_buffer[(int)num6].m_flags;
                        if ((flags & (flagsRequired | flagsForbidden)) == flagsRequired)
                        {
                            float num8 = Vector3.SqrMagnitude(pos - vm.m_vehicles.m_buffer[(int)num6].GetSmoothPosition(num6));
                            if (num8 < num5)
                            {
                                result = num6;
                                num5 = num8;
                            }
                        }
                        num6 = vm.m_vehicles.m_buffer[(int)num6].m_nextGridVehicle;
                        if (++num7 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public static ushort FindParkedVehicle(Vector3 pos, float maxDistance)
        {
            int num = Mathf.Max((int)((pos.x - maxDistance) / 32f + 270f), 0);
            int num2 = Mathf.Max((int)((pos.z - maxDistance) / 32f + 270f), 0);
            int num3 = Mathf.Min((int)((pos.x + maxDistance) / 32f + 270f), 539);
            int num4 = Mathf.Min((int)((pos.z + maxDistance) / 32f + 270f), 539);
            ushort result = 0;
            float num5 = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    VehicleManager vm = Singleton<VehicleManager>.instance;
                    ushort num6 = vm.m_parkedGrid[i * 540 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        float num8 = Vector3.SqrMagnitude(pos - vm.m_parkedVehicles.m_buffer[(int)num6].m_position);
                        if (num8 < num5)
                        {
                            result = num6;
                            num5 = num8;
                        }

                        num6 = vm.m_parkedVehicles.m_buffer[(int)num6].m_nextGridParked;
                        if (++num7 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
