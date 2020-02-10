using System.Collections.Generic;
using UnityEngine;

namespace Freewalking
{
    public class Utility
    {
        public static void CreateDebugCubes(List<Vector3> points)
        {
            foreach (Vector3 point in points)
            {
                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gameObject.transform.position = point;
            }
        }


        public static float Map(float newMin, float newMax, float originalMin, float originalMax, float value)
        {
            return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(originalMin, originalMax, value));
        }
    }
}
