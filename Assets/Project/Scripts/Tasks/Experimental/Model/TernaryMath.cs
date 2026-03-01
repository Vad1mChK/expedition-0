using System;

namespace Expedition0.Tasks.Experimental
{
    public static class TernaryMath
    {
        // Unary operations
        public static Trit Identity(Trit t) => t;
        public static Trit Not(Trit t) => (Trit)(2 - (byte)t);

        // Binary operations
        public static Trit And(Trit a, Trit b) => (Trit)Math.Min((byte)a, (byte)b);
        public static Trit Or(Trit a, Trit b) => (Trit)Math.Max((byte)a, (byte)b);
        public static Trit Xor(Trit a, Trit b) => Or(And(Not(a), b), And(a, Not(b)));
        public static Trit ImplKleene(Trit a, Trit b) => Or(Not(a), b);
        public static Trit ImplLukasiewicz(Trit a, Trit b) => (Trit)Math.Min(2, 2 - (byte)a + (byte)b);

        // Nonary arithmetic
        public static int ToNonary(Trit high, Trit low) => (int)high * 3 + (int)low;
        
        // Cycling
        public static Trit CyclePrevious(Trit t) => t.CyclePrevious();
        public static Trit CycleNext(Trit t) => t.CycleNext();
    }
}