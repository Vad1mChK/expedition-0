using UnityEngine;

namespace Expedition0.Util
{
    public static class MathUtils
    {
        public static float SinLerp(float a, float b, float t) =>
            Mathf.Lerp(a, b, SinSmoothParameter(t));

        public static Vector2 SinLerp(Vector2 a, Vector2 b, float t) =>
            Vector2.Lerp(a, b, SinSmoothParameter(t));

        private static float SinSmoothParameter(float t) =>
            (1 - Mathf.Cos(Mathf.PI * t)) / 2;
    }
}