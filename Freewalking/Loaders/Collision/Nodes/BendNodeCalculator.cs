using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            out Bezier3 leftBazier,
            out Bezier3 rightBezier)
        {
            throw new NotImplementedException();
        }
    }
}
