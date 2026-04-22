using UnityEngine;

namespace Expedition0.Util
{
    public static class VectorUtils
    {
        public static float Distance(Vector3 left, Vector3 right)
        {
            return (right - left).magnitude;
        }
    }
}