using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace Freewalking.Loaders.Collision.Nodes
{
    public class BendNodeCalculator : INodeCalculator
    {
        public void GetBeziers(ushort nodeId,
            NetNode node,
            NetInfo info,
            Vector3 position,
            out Bezier3 leftBezier,
            out Bezier3 rightBezier)
        {
            Vector3 cornerPos1 = Vector3.zero;
            Vector3 cornerPos2 = Vector3.zero;
            Vector3 cornerPos3 = Vector3.zero;
            Vector3 cornerPos4 = Vector3.zero;
            Vector3 cornerDirection1 = Vector3.zero;
            Vector3 cornerDirection2 = Vector3.zero;
            Vector3 cornerDirection3 = Vector3.zero;
            Vector3 cornerDirection4 = Vector3.zero;
            int num = 0;
            for (int index = 0; index < 8; ++index)
            {
                ushort segment = node.GetSegment(index);
                if (segment != (ushort)0)
                {
                    NetSegment netSegment = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment];
                    if (netSegment.Info != null)
                    {
                        bool start = (int)netSegment.m_startNode == (int)nodeId;
                        bool smooth;
                        if (++num == 1)
                        {
                            netSegment.CalculateCorner(segment, true, start, false, out cornerPos1, out cornerDirection1, out smooth);
                            netSegment.CalculateCorner(segment, true, start, true, out cornerPos2, out cornerDirection2, out smooth);
                        }
                        else
                        {
                            netSegment.CalculateCorner(segment, true, start, true, out cornerPos3, out cornerDirection3, out smooth);
                            netSegment.CalculateCorner(segment, true, start, false, out cornerPos4, out cornerDirection4, out smooth);
                        }
                    }
                }
            }
            Vector3 middlePos1_1;
            Vector3 middlePos2_1;
            NetSegment.CalculateMiddlePoints(cornerPos1, -cornerDirection1, cornerPos3, -cornerDirection3, true, true, out middlePos1_1, out middlePos2_1);
            Vector3 middlePos1_2;
            Vector3 middlePos2_2;
            NetSegment.CalculateMiddlePoints(cornerPos2, -cornerDirection2, cornerPos4, -cornerDirection4, true, true, out middlePos1_2, out middlePos2_2);
            
            rightBezier = new Bezier3(
                cornerPos3 - position,
                middlePos2_1 - position,
                middlePos1_1 - position,
                cornerPos1 - position);
            leftBezier = new Bezier3(
                cornerPos4 - position,
                middlePos2_2 - position,
                middlePos1_2 - position,
                cornerPos2 - position);
        }
    }
}
