using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace Freewalking.Loaders.Collision.Nodes
{
    public class EndNodeCalculator : INodeCalculator
    {
        public void GetBeziers(ushort nodeId,
            NetNode node,
            NetInfo info,
            Vector3 position,
            out Bezier3 leftBezier,
            out Bezier3 rightBezier)
        {
            Vector3 lhs1 = Vector3.zero;
            Vector3 rhs1 = Vector3.zero;
            Vector3 cornerPos1 = Vector3.zero;
            Vector3 cornerPos2 = Vector3.zero;
            Vector3 cornerDirection1 = Vector3.zero;
            Vector3 cornerDirection2 = Vector3.zero;
            Vector3 vector3_3 = Vector3.zero;
            Vector3 vector3_4 = Vector3.zero;
            for (int index = 0; index < 8; ++index)
            {
                ushort segment = node.GetSegment(index);
                if (segment != (ushort) 0)
                {
                    NetSegment netSegment = Singleton<NetManager>.instance.m_segments.m_buffer[(int) segment];
                    bool start = (int) netSegment.m_startNode == (int) nodeId;
                    bool smooth;
                    netSegment.CalculateCorner(segment, true, start, false, out cornerPos1, out cornerDirection1,
                        out smooth);
                    netSegment.CalculateCorner(segment, true, start, true, out cornerPos2, out cornerDirection2,
                        out smooth);
                    lhs1 = cornerPos2;
                    rhs1 = cornerPos1;
                    vector3_3 = cornerDirection2;
                    vector3_4 = cornerDirection1;
                }
            }

            float num = info.m_netAI.GetEndRadius() * 1.333333f;
            Vector3 lhs2 = cornerPos1 - cornerDirection1 * num;
            Vector3 lhs3 = lhs1 - vector3_3 * num;
            Vector3 rhs2 = cornerPos2 + cornerDirection2 * num;
            Vector3 rhs3 = rhs1 + vector3_4 * num;
            rightBezier = new Bezier3(
                lhs1 - position,
                lhs3 - position,
                lhs2 - position,
                rhs1 - position);

            Vector3 m = new Vector3(0, -0.25f, 0);
            leftBezier = new Bezier3(m, m, m, m);
        }
    }
}