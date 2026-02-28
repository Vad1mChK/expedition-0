using UnityEngine;

namespace Expedition0.Util
{
    public static class RandomUtils
    {
        public static float NextUniform(float a, float b)
        {
            // No i will not perform a swap when a > b
            return a + Random.value * (b - a);
        }
        
        public static float NextStandardNormal()
        {
            float u1 = Mathf.Max(Random.value, 1e-7f), u2 = Random.value;
            float z1 = Mathf.Sqrt(-2 * Mathf.Log(u1)) * Mathf.Cos(2 * Mathf.PI * u2);
            // float z2 = Mathf.Sqrt(-2 * Mathf.Log(u1)) * Mathf.Sin(2 * Mathf.PI * u2);
            // z2 can be kept for a full fledged random generator and alter between the two
            return z1;
        }

        public static float NextNormal(float mean, float std)
        {
            return NextStandardNormal() * std + mean;
        }

        public static float NextTruncatedNormal(float mean = 0, float std = 1, float maxStds = 3)
        {
            if (maxStds <= 0) return mean;
            
            float r = NextStandardNormal();
            if (r < -maxStds || r > maxStds)
            {
                r = NextUniform(-maxStds, maxStds);
            }
            
            return r * std + mean;
        }
    }
}