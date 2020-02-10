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
using Freewalking.Loaders.Collision.Nodes;
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
            Mesh mesh = GetSegmentMesh(segmentId, out Vector3 position);
            Mesh startMesh = GetNodeMesh(segment.m_startNode, out Vector3 startPosition);
            Mesh endMesh = GetNodeMesh(segment.m_endNode, out Vector3 endPosition);

            CombineInstance[] combineInstances = 
            {
                new CombineInstance {mesh = mesh, transform = Matrix4x4.identity},
                new CombineInstance {mesh = startMesh, transform = Matrix4x4.Translate(startPosition - position)},
                new CombineInstance {mesh = endMesh, transform = Matrix4x4.Translate(endPosition - position)}
            };
            Mesh total = new Mesh();
            total.CombineMeshes(combineInstances);

            return total;
        }

        private Mesh GetSegmentMesh(ushort segmentId, out Vector3 position)
        {
            NetSegment segment = netManager.m_segments.m_buffer[segmentId];
            NetInfo info = segment.Info;

            NetInfo.Segment seg = info.m_segments.Length == 1 ? info.m_segments[0] : info.m_segments[2];
            
            Vector3 position1 = netManager.m_nodes.m_buffer[(int) segment.m_startNode].m_position;
            Vector3 position2 = netManager.m_nodes.m_buffer[(int) segment.m_endNode].m_position;
            position = (position1 + position2) * 0.5f;
            
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
                cornerPos4, cornerPos3, middlePos1_2, middlePos2_2, cornerPos2, position, vscale);
            Matrix4x4 rightMatrix = NetSegment.CalculateControlMatrix(cornerPos3, middlePos1_2, middlePos2_2,
                cornerPos2, cornerPos1, middlePos1_1, middlePos2_1, cornerPos4, position, vscale);

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

            Mesh mesh = BendMesh(seg.m_segmentMesh, leftBezier, rightBezier, info);
            return mesh;
        }

        private Mesh BendMesh(Mesh original, Bezier3 leftBezier, Bezier3 rightBezier, NetInfo info)
        {
            Mesh mesh = new Mesh
            {
                vertices = original.vertices,
                triangles = original.triangles
            };
            List<Vector3> vertices = mesh.vertices.ToList();


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

        private Mesh GetNodeMesh(ushort nodeId, out Vector3 position)
        {
            NetNode node = netManager.m_nodes.m_buffer[nodeId];
            NetInfo info = node.Info;

            position = node.m_position;

            INodeCalculator nodeCalculator = null;

            if ((node.m_flags & NetNode.Flags.End) != NetNode.Flags.None)
            {
                nodeCalculator = new EndNodeCalculator();
            }

            if (nodeCalculator == null)
                return null;

            nodeCalculator.GetBeziers(nodeId, node, info, position, out Bezier3 leftBezier, out Bezier3 rightBezier);

            Mesh mesh = BendMesh(info.m_nodes[0].m_nodeMesh, leftBezier, rightBezier, info);
            return mesh;
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
                                        VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.All,
                                        VehicleInfo.VehicleType.None, false, out Vector3 b,
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