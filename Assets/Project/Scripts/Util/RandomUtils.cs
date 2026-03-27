using System.Collections.Generic;
using System.Linq;
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

        public static T? Choice<T>(IEnumerable<T> choices)
        {
            var choicesList = choices.ToList();

            if (choicesList.Count == 0)
            {
                return default;
            }

            int roll = Random.Range(0, choicesList.Count);

            return choicesList[roll];
        }
        
        /// <summary>
        /// Selects an item based on weighted probability.
        /// </summary>
        /// <typeparam name="T">The type of items to choose from.</typeparam>
        /// <param name="choices">The collection of possible items.</param>
        /// <param name="weights">The corresponding weights for each item.</param>
        /// <returns>A chosen item of type T, or null/default if no choice could be made.</returns>
        public static T? WeightedChoice<T>(IEnumerable<T> choices, IEnumerable<int> weights)
        {
            // Use a list to avoid multiple enumerations of the IEnumerable
            var choicesList = choices.ToList();
            var weightsList = weights.ToList();

            if (choicesList.Count == 0 || choicesList.Count != weightsList.Count)
            {
                return default; 
            }

            var cumulativeWeight = new List<int>();
            int totalWeight = 0;

            foreach (int w in weightsList)
            {
                totalWeight += w;
                cumulativeWeight.Add(totalWeight);
            }

            // UnityEngine.Random.Range(int, int) is max-exclusive for integers
            int roll = Random.Range(0, totalWeight);

            for (int i = 0; i < choicesList.Count; i++)
            {
                if (roll < cumulativeWeight[i])
                {
                    return choicesList[i];
                }
            }

            return default;
        }
    }
}