using System;

namespace Expedition0.Tasks.Experimental
{
    [Serializable]
    public abstract class LogicNode
    {
        public int id;
        public bool locked;
        
        public abstract int EvaluateInt();
        public virtual void Cycle() {}
    }
    
    [Serializable]
    public abstract class TernaryLogicNode : LogicNode
    {
        public abstract Trit EvaluateTrit();

        public sealed override int EvaluateInt()
        {
            return EvaluateTrit().ToUnbalancedInt();
        }
    }
    
    [Serializable]
    public abstract class NonaryLogicNode : LogicNode
    {
    }

    [Serializable]
    public sealed class TernaryValueNode : TernaryLogicNode
    {
        public Trit currentValue;

        public override Trit EvaluateTrit() => currentValue;

        public override void Cycle()
        {
            if (locked) return;
            currentValue = currentValue.CycleNext();
        }
    }

    [Serializable]
    public sealed class NonaryValueNode : NonaryLogicNode
    {
        // Simple single-digit 0..8 for now
        public int currentValue;

        public override int EvaluateInt() => currentValue;

        public override void Cycle()
        {
            if (locked) return;
            if (currentValue < 0) currentValue = 0;
            currentValue = (currentValue + 1) % 9;
        }
    }

    [Serializable]
    public sealed class TernaryUnaryOperatorNode : TernaryLogicNode
    {
        public TernaryLogicNode input;
        public TernaryUnaryOperatorType op;

        public override Trit EvaluateTrit()
        {
            var value = input.EvaluateTrit();
            return op switch
            {
                TernaryUnaryOperatorType.Identity => TernaryMath.Identity(value),
                TernaryUnaryOperatorType.Not      => TernaryMath.Not(value),
                _                                 => value
            };
        }

        public override void Cycle()
        {
            if (locked) return;
            op = op.Next();
        }
    }

    [Serializable]
    public sealed class TernaryBinaryOperatorNode : TernaryLogicNode
    {
        public TernaryLogicNode leftInput;
        public TernaryLogicNode rightInput;
        public TernaryBinaryOperatorType op;

        public override Trit EvaluateTrit()
        {
            var a = leftInput.EvaluateTrit();
            var b = rightInput.EvaluateTrit();

            return op switch
            {
                TernaryBinaryOperatorType.And            => TernaryMath.And(a, b),
                TernaryBinaryOperatorType.Or             => TernaryMath.Or(a, b),
                TernaryBinaryOperatorType.Xor            => TernaryMath.Xor(a, b),
                TernaryBinaryOperatorType.ImplKleene     => TernaryMath.ImplKleene(a, b),
                TernaryBinaryOperatorType.ImplLukasiewicz=> TernaryMath.ImplLukasiewicz(a, b),
                _                                        => a
            };
        }

        public override void Cycle()
        {
            if (locked) return;
            op = op.Next();
        }
    }

    [Serializable]
    public sealed class NonaryOperatorNode : NonaryLogicNode
    {
        public NonaryLogicNode leftInput;
        public NonaryLogicNode rightInput;
        public NonaryOperatorType op;

        private int LeftInt  => leftInput.EvaluateInt();
        private int RightInt => rightInput.EvaluateInt();

        public override int EvaluateInt()
        {
            return op switch
            {
                NonaryOperatorType.NonaryConcat => LeftInt * 9 + RightInt,
                NonaryOperatorType.NonaryMinus  => LeftInt - RightInt,
                NonaryOperatorType.NonaryPlus   => LeftInt + RightInt,
                _                               => LeftInt
            };
        }

        public override void Cycle()
        {
            if (locked) return;
            op = op.Next();
        }
    }
}