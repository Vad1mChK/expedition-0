using System;

namespace Expedition0.Tasks.Experimental
{
    public enum TernaryUnaryOperatorType
    {
        Identity,
        Not,
    }
    
    public enum TernaryBinaryOperatorType
    {
        And,
        Or,
        Xor,
        ImplKleene,
        ImplLukasiewicz,
    }
    
    public enum NonaryOperatorType
    {
        NonaryPlus,
        NonaryMinus,
        NonaryConcat
    }

    public static class LogicOperatorExtensions
    {
        public static TernaryUnaryOperatorType[] TernaryUnaryOperatorCycleOrder => 
            new[] {
                TernaryUnaryOperatorType.Identity,
                TernaryUnaryOperatorType.Not
            };

        public static TernaryBinaryOperatorType[] TernaryBinaryOperatorCycleOrder =>
            new[]
            {
                TernaryBinaryOperatorType.And,
                TernaryBinaryOperatorType.Or,
                TernaryBinaryOperatorType.Xor,
                TernaryBinaryOperatorType.ImplLukasiewicz
            };

        public static NonaryOperatorType[] NonaryOperatorCycleOrder =>
            new[]
            {
                NonaryOperatorType.NonaryPlus,
                NonaryOperatorType.NonaryMinus
            };
        
        public static TernaryUnaryOperatorType Next(this TernaryUnaryOperatorType op)
        {
            var order = TernaryUnaryOperatorCycleOrder;
            var index = Array.IndexOf(order, op);
            if (index < 0) index = 0;
            return order[(index + 1) % order.Length];
        }

        public static TernaryBinaryOperatorType Next(this TernaryBinaryOperatorType op)
        {
            var order = TernaryBinaryOperatorCycleOrder;
            var index = Array.IndexOf(order, op);
            if (index < 0) index = 0;
            return order[(index + 1) % order.Length];
        }

        public static NonaryOperatorType Next(this NonaryOperatorType op)
        {
            var order = NonaryOperatorCycleOrder;
            var index = Array.IndexOf(order, op);
            if (index < 0) index = 0;
            return order[(index + 1) % order.Length];
        }
    }
}