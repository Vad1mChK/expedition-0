using System;

namespace Expedition0.Tasks
{
    // Слот для значения (трита), пока не заполнен — Evaluate бросает исключение
    public class ValueSlotNode : ASTNode
    {
        private bool _isFilled;
        private Trit _value;
         private bool _isLocked;
        public string Key { get; set; }

        public bool IsFilled => _isFilled;
        public bool IsLocked => _isLocked;
        // Если не заполнен — возвращает null
        public Trit? CurrentValue => _isFilled ? _value : (Trit?)null;

        public void SetValue(Trit value)
        {
            if (_isLocked) throw new InvalidOperationException("Value slot is locked");
            _value = value;
            _isFilled = true;
        }

        public void LockValue(Trit value)
        {
            _value = value;
            _isFilled = true;
            _isLocked = true;
        }

        public override Trit Evaluate()
        {
            if (!_isFilled) throw new InvalidOperationException("Value slot is not filled");
            return _value;
        }
    }

    // Слот для оператора между двумя поддеревьями
    public class OperatorSlotNode : ASTNode
    {
        private bool _isFilled;
        private Operator _operator;
        private bool _isLocked;

        public ASTNode Left { get; }
        public ASTNode Right { get; }

        public bool IsFilled => _isFilled;
        public bool IsLocked => _isLocked;
        public Operator? CurrentOperator => _isFilled ? _operator : (Operator?)null;

        public OperatorSlotNode(ASTNode left, ASTNode right)
        {
            Left = left;
            Right = right;
            if (Left != null)
            {
                Left.Parent = this;
                Children.Add(Left);
            }
            if (Right != null)
            {
                Right.Parent = this;
                Children.Add(Right);
            }
        }

        public void SetOperator(Operator op)
        {
            if (_isLocked) throw new InvalidOperationException("Operator slot is locked");
            _operator = op;
            _isFilled = true;
        }

        public void LockOperator(Operator op)
        {
            _operator = op;
            _isFilled = true;
            _isLocked = true;
        }

        public override Trit Evaluate()
        {
            if (!_isFilled) throw new InvalidOperationException("Operator slot is not filled");

            switch (_operator)
            {
                case Operator.NOT:
                    return Left.Evaluate().Not();
                case Operator.AND:
                    return Left.Evaluate().And(Right.Evaluate());
                case Operator.OR:
                    return Left.Evaluate().Or(Right.Evaluate());
                case Operator.XOR:
                    return Left.Evaluate().Xor(Right.Evaluate());
                case Operator.IMPLY:
                    return Left.Evaluate().ImplyKleene(Right.Evaluate());
                case Operator.NAND:
                    return Left.Evaluate().Nand(Right.Evaluate());
                case Operator.NOR:
                    return Left.Evaluate().Nor(Right.Evaluate());
                case Operator.EQUIV:
                    return Left.Evaluate().Equiv(Right.Evaluate());
                case Operator.IMPLY_LUK:
                    return Left.Evaluate().ImplyLukasiewicz(Right.Evaluate());
                case Operator.PLUS:
                case Operator.MINUS:
                    throw new InvalidOperationException($"Arithmetic operator {_operator} not supported in logic evaluation");
                default:
                    throw new Exception($"Invalid operator for template node: {_operator}");
            }
        }
    }
}


