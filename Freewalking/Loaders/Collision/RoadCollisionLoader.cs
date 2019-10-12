using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Freewalking.Loaders.Collision
{
    public class RoadCollisionLoader : CollisionLoader
    {
        private NetManager netManager;
        private ushort lastId = 0;

        public override void OnCreated(ILoading loading)
        {
            netManager = Singleton<NetManager>.instance;
        }

        protected override ushort GetNearestObjectId(ICamera camera, Vector3 position)
        {
            ushort road = FindRoad(position);


            lastId = road;
            return road;
        }

        protected override Mesh GetMesh(ushort segmentId)
        {
            if (segmentId == 0)
                return null;

            NetSegment segment = netManager.m_segments.m_buffer[segmentId];
            NetInfo info = segment.Info;
            NetInfo.Segment seg = info.m_segments[0];

            Vector3 position1 = netManager.m_nodes.m_buffer[(int) segment.m_startNode].m_position;
            Vector3 position2 = netManager.m_nodes.m_buffer[(int) segment.m_endNode].m_position;
            Vector3 m_position = (position1 + position2) * 0.5f;

            float vscale = info.m_netAI.GetVScale();
            segment.CalculateCorner(segmentId, true, true, true, out Vector3 cornerPos1, out Vector3 cornerDirection1,
                out bool smooth1);
            segment.CalculateCorner(segmentId, true, false, true, out Vector3 cornerPos2, out Vector3 cornerDirection2,
                out bool smooth2);
            segment.CalculateCorner(segmentId, true, true, false, out Vector3 cornerPos3, out Vector3 cornerDirection3,
                out smooth1);
            segment.CalculateCorner(segmentId, true, false, false, out Vector3 cornerPos4, out Vector3 cornerDirection4,
                out smooth2);

            NetSegment.CalculateMiddlePoints(cornerPos1, cornerDirection1, cornerPos4, cornerDirection4, smooth1,
                smooth2, out Vector3 middlePos1_1, out Vector3 middlePos2_1);
            NetSegment.CalculateMiddlePoints(cornerPos3, cornerDirection3, cornerPos2, cornerDirection2, smooth1,
                smooth2, out Vector3 middlePos1_2, out Vector3 middlePos2_2);

            Matrix4x4 leftMatrix = NetSegment.CalculateControlMatrix(cornerPos1, middlePos1_1, middlePos2_1,
                cornerPos4, cornerPos3, middlePos1_2, middlePos2_2, cornerPos2, m_position, vscale);
            Matrix4x4 rightMatrix = NetSegment.CalculateControlMatrix(cornerPos3, middlePos1_2, middlePos2_2,
                cornerPos2, cornerPos1, middlePos1_1, middlePos2_1, cornerPos4, m_position, vscale);

            Vector4 scale = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            GetMeshPart(cornerPos1, middlePos1_1, middlePos2_1, cornerPos4,
                cornerPos3, middlePos1_2, middlePos2_2, cornerPos2,
                ref vertices, ref triangles);

            Mesh mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
//                vertices = new[]
//                {
//                    cornerPos1,
//                    cornerPos2,
//                    cornerPos3,
//                    cornerPos4,
//                    middlePos1_1,
//                    middlePos1_2,
//                    middlePos2_1,
//                    middlePos2_2,
//                },
//                normals = new Vector3[8],
//                triangles = new []
//                {
//                    0, 4, 2,
//                    2, 4, 5,
//                    4, 7, 5,
//                    6, 7, 4,
//                    1, 7, 3,
//                    3, 7, 6
//                }
            };

//            mesh = seg.m_segmentMesh;

//            mesh.vertices[0] = mesh.vertices[0] - new Vector3(leftMatrix.m00, leftMatrix.m01, leftMatrix.m02);
            
            return mesh;
        }

        private void GetMeshPart(
            Vector3 startPos,
            Vector3 middlePos1,
            Vector3 middlePos2,
            Vector3 endPos,
            Vector3 startPosB,
            Vector3 middlePosB1,
            Vector3 middlePosB2,
            Vector3 endPosB,
            ref List<Vector3> vertices, 
            ref List<int> triangles)
        {
            int i = vertices.Count;
            vertices.AddRange(new[]
            {
                startPos, middlePos1, middlePos2, endPos,
                startPosB, middlePosB1, middlePosB2, endPosB
            });
            triangles.AddRange(new[]
            {
                0, 1, 5,
                5, 1, 6,
                1, 2, 6,
                6, 7, 2,
                2, 3, 6,
                6, 3, 7
            });
        }

        private void CreateActualRoad(ushort segmentId)
        {
            // this is a method for creating the actual road
            // the road is displayed using shaders and meshcollider therefor does not work
            // this code is not called and only used for possible future investigation

            NetSegment segment = netManager.m_segments.m_buffer[segmentId];
            NetInfo info = segment.Info;
            NetInfo.Segment seg = info.m_segments[0];

            Vector3 position1 = netManager.m_nodes.m_buffer[(int) segment.m_startNode].m_position;
            Vector3 position2 = netManager.m_nodes.m_buffer[(int) segment.m_endNode].m_position;
            Vector3 m_position = (position1 + position2) * 0.5f;

            float vscale = info.m_netAI.GetVScale();
            segment.CalculateCorner(segmentId, true, true, true, out Vector3 cornerPos1, out Vector3 cornerDirection1,
                out bool smooth1);
            segment.CalculateCorner(segmentId, true, false, true, out Vector3 cornerPos2, out Vector3 cornerDirection2,
                out bool smooth2);
            segment.CalculateCorner(segmentId, true, true, false, out Vector3 cornerPos3, out Vector3 cornerDirection3,
                out smooth1);
            segment.CalculateCorner(segmentId, true, false, false, out Vector3 cornerPos4, out Vector3 cornerDirection4,
                out smooth2);

            NetSegment.CalculateMiddlePoints(cornerPos1, cornerDirection1, cornerPos4, cornerDirection4, smooth1,
                smooth2, out Vector3 middlePos1_1, out Vector3 middlePos2_1);
            NetSegment.CalculateMiddlePoints(cornerPos3, cornerDirection3, cornerPos2, cornerDirection2, smooth1,
                smooth2, out Vector3 middlePos1_2, out Vector3 middlePos2_2);

            Matrix4x4 m_dataMatrix0 = NetSegment.CalculateControlMatrix(cornerPos1, middlePos1_1, middlePos2_1,
                cornerPos4, cornerPos3, middlePos1_2, middlePos2_2, cornerPos2, m_position, vscale);
            Matrix4x4 m_dataMatrix1 = NetSegment.CalculateControlMatrix(cornerPos3, middlePos1_2, middlePos2_2,
                cornerPos2, cornerPos1, middlePos1_1, middlePos2_1, cornerPos4, m_position, vscale);

            Vector4 m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);

            GameObject road = new GameObject("Road");

            Material material = new Material(seg.m_segmentMaterial);
            material.SetMatrix(netManager.ID_LeftMatrix, m_dataMatrix0);
            material.SetMatrix(netManager.ID_RightMatrix, m_dataMatrix1);

            material.SetVector(netManager.ID_MeshScale, m_dataVector0);

            MeshFilter filter = road.AddComponent<MeshFilter>();
            MeshRenderer renderer = road.AddComponent<MeshRenderer>();
            road.transform.position = m_position + Vector3.up;
            road.transform.rotation = Quaternion.identity;
            renderer.material = material;
            filter.mesh = seg.m_segmentMesh;
        }

        protected override void CalculateMeshPosition(ushort id, out Vector3 meshPosition, out Quaternion meshRotation)
        {
            NetSegment segment = netManager.m_segments.m_buffer[id];
            Vector3 position1 = netManager.m_nodes.m_buffer[(int) segment.m_startNode].m_position;
            Vector3 position2 = netManager.m_nodes.m_buffer[(int) segment.m_endNode].m_position;
            meshPosition = /*(position1 + position2) * 0.5f +*/Vector3.zero;
            meshRotation = Quaternion.identity;
        }

        public ushort FindRoad(Vector3 position)
        {
            Bounds bounds = new Bounds(position, new Vector3(100f, 20f, 100f));
            int num = Mathf.Max((int) ((bounds.min.x - 64f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int) ((bounds.min.z - 64f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int) ((bounds.max.x + 64f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int) ((bounds.max.z + 64f) / 64f + 135f), 269);
            NetManager instance = netManager;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = instance.m_segmentGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        NetInfo info = instance.m_segments.m_buffer[(int) num5].Info;
                        if (info.m_class.m_service == ItemClass.Service.Road && !info.m_netAI.IsUnderground() &&
                            (info.m_hasForwardVehicleLanes || info.m_hasBackwardVehicleLanes))
                        {
                            ushort startNode = instance.m_segments.m_buffer[(int) num5].m_startNode;
                            ushort endNode = instance.m_segments.m_buffer[(int) num5].m_endNode;
                            Vector3 position2 = instance.m_nodes.m_buffer[(int) startNode].m_position;
                            Vector3 position3 = instance.m_nodes.m_buffer[(int) endNode].m_position;
                            float num7 =
                                Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z),
                                    Mathf.Max(position2.x - bounds.max.x - 64f, position2.z - bounds.max.z - 64f));
                            float num8 =
                                Mathf.Max(Mathf.Max(bounds.min.x - 64f - position3.x, bounds.min.z - 64f - position3.z),
                                    Mathf.Max(position3.x - bounds.max.x - 64f, position3.z - bounds.max.z - 64f));
                            if ((num7 < 0f || num8 < 0f) &&
                                instance.m_segments.m_buffer[(int) num5].m_bounds.Intersects(bounds) && instance
                                    .m_segments.m_buffer[(int) num5].GetClosestLanePosition(position,
                                        NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle,
                                        VehicleInfo.VehicleType.Car, VehicleInfo.VehicleType.None, false, out Vector3 b,
                                        out int num9, out float num10, out Vector3 vector, out int num11,
                                        out float num12))
                            {
                                float num13 = Vector3.SqrMagnitude(position - b);
                                if (num13 < 400f)
                                {
                                    return num5;
                                }
                            }
                        }

                        num5 = instance.m_segments.m_buffer[(int) num5].m_nextGridSegment;
                        if (++num6 >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core,
                                "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            return 0;
        }
    }
}