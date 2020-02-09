using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
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

        public override void OnCreated(ILoading loading)
        {
            netManager = Singleton<NetManager>.instance;
        }

        protected override ushort GetNearestObjectId(ICamera camera, Vector3 position)
        {
            ushort road = FindRoad(position);
            return road;
        }

        protected override Mesh GetMesh(ushort segmentId)
        {
            if (segmentId == 0)
                return null;

            NetSegment segment = netManager.m_segments.m_buffer[segmentId];
            Mesh mesh = GetSegmentMesh(segmentId);

            return mesh;
        }

        private Mesh GetSegmentMesh(ushort segmentId)
        {
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

            Mesh mesh = new Mesh
            {
                vertices = seg.m_segmentMesh.vertices,
                triangles = seg.m_segmentMesh.triangles
            };
            List<Vector3> vertices = mesh.vertices.ToList();

            Bezier3 leftBezier = new Bezier3(
                leftMatrix.GetColumn(0),
                leftMatrix.GetColumn(1),
                leftMatrix.GetColumn(2),
                leftMatrix.GetColumn(3));
            Bezier3 rightBezier = new Bezier3(
                rightMatrix.GetColumn(0),
                rightMatrix.GetColumn(1),
                rightMatrix.GetColumn(2),
                rightMatrix.GetColumn(3));

            for (var i = 0; i < vertices.Count; i++)
            {
                Vector3 vertex = vertices[i];
                vertex = TransformVertex(vertex.x < 0 ? leftBezier : rightBezier, vertex, info.m_segmentLength,
                    info.m_halfWidth);

                vertices[i] = vertex;
            }

            mesh.vertices = vertices.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        private void CreateActualRoad(ushort segmentId)
        {
            // this is a method for creating the actual road
            // the road is displayed using shaders and meshcollider therefor does not work
            // this code is not called and only used for possible future investigation

            NetSegment segment = netManager.m_segments.m_buffer[segmentId];
            NetInfo info = segment.Info;
            NetInfo.Segment seg = info.m_segments[0];

            Vector3 position1 = netManager.m_nodes.m_buffer[(int)segment.m_startNode].m_position;
            Vector3 position2 = netManager.m_nodes.m_buffer[(int)segment.m_endNode].m_position;
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

        private Vector3 TransformVertex(Bezier3 bezier, Vector3 vertex, float length, float width)
        {
            float t = Map(0, 1, -length / 2, length / 2, vertex.z);
            var position = bezier.Position(t);
            var tangent = bezier.Tangent(t);
            var dir = Vector3.Cross(Vector3.up, tangent);
            var sideDir = dir.normalized * vertex.x + (vertex.x < 0 ? dir.normalized * width : -dir.normalized * width);
            position.y += vertex.y;
            position += sideDir;
            return position;
        }

        private float Map(float newMin, float newMax, float originalMin, float originalMax, float value)
        {
            return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(originalMin, originalMax, value));
        }

        protected override void CalculateMeshPosition(ushort id, out Vector3 meshPosition, out Quaternion meshRotation)
        {
            NetSegment segment = netManager.m_segments.m_buffer[id];
            Vector3 position1 = netManager.m_nodes.m_buffer[(int) segment.m_startNode].m_position;
            Vector3 position2 = netManager.m_nodes.m_buffer[(int) segment.m_endNode].m_position;
            meshPosition = (position1 + position2) * 0.5f;
            meshRotation = Quaternion.identity;
        }

        public ushort FindRoad(Vector3 position)
        {
            Bounds bounds = new Bounds(position, new Vector3(20f, 20f, 20f));
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
                                        VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.All, VehicleInfo.VehicleType.None, false, out Vector3 b,
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