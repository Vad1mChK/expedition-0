using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public static class TaskColorUtil
    {
        private static Color Red = new Color(0.8392f, 0f, 0.1176f);
        private static Color Yellow = new Color(0.9752f, 0.7804f, 0.0941f);
        private static Color Blue = new Color(0.0274f, 0.5451f, 0.7882f);
        private static Color Gray = new Color(0.6392f, 0.6242f, 0.6542f);
        
        // Example mapping: 0 → red, 1 → yellow, 2 → blue
        public static Color GetColorForTrit(Trit trit) => trit switch
        {
            Trit.False => Red,
            Trit.Neutral => Yellow,
            Trit.True => Blue,
            _ => Gray
        };
        
        // Similarly for nonary digits, but here we will get color for range [0, 1]
        public static Color GetColorFor(float t)
        {
            float x = Mathf.Clamp01(t);

            // Bottom half: Red → Yellow
            if (x <= 0.5f)
            {
                float k = x * 2f;
                return Color.Lerp(Red, Yellow, k);
            }

            // Top half: Yellow → Blue
            {
                float k = (x - 0.5f) * 2f;
                return Color.Lerp(Yellow, Blue, k);
            }
        }
    }
}