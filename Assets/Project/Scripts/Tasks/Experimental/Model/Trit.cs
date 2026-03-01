using System;

namespace Expedition0.Tasks.Experimental
{
    public enum Trit: byte
    {
        False = 0, // False (unbalanced: 0, balanced: -1)
        Neutral = 1, // Neutral (unbalanced: 1, balanced: 0)
        True = 2 // True (unbalanced: 2, balanced: 1)
    }
    
    public static class TritExtensions
    {
        public static int ToUnbalancedInt(this Trit t)
        {
            return (int)t; // 0,1,2
        }

        public static int ToBalancedInt(this Trit t)
        {
            return (int)t - 1; // 0->-1,1->0,2->1
        }

        public static Trit FromUnbalancedInt(int value)
        {
            if (value < 0 || value > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return (Trit)value;
        }

        public static Trit FromBalancedInt(int value)
        {
            if (value < -1 || value > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return (Trit)(value + 1);
        }

        public static Trit CyclePrevious(this Trit t)
        {
            return (Trit)(((int)t + 3 - 1) % 3);
        }

        public static Trit CycleNext(this Trit t)
        {
            return (Trit)(((int)t + 1) % 3);
        }
    }
}