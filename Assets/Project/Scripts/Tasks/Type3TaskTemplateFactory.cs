namespace Expedition0.Tasks
{
    /// <summary>
    /// Фабрика шаблонов для заданий типа 3: троичная логика с трубами
    /// Поддерживает AST из 3-х значений и двух бинарных операций
    /// </summary>
    public static class Type3TaskTemplateFactory
    {
        /// <summary>
        /// Создает простой бинарный шаблон: OP(V1, V2) = Answer
        /// </summary>
        public static ASTTemplate CreateBinary(
            Operator predefinedOperator, 
            Trit answer, 
            bool lockOperator = true, 
            Trit? leftLocked = null, 
            Trit? rightLocked = null)
        {
            var left = new ValueSlotNode();
            var right = new ValueSlotNode();

            if (leftLocked.HasValue) left.LockValue(leftLocked.Value);
            if (rightLocked.HasValue) right.LockValue(rightLocked.Value);

            var op = new OperatorSlotNode(left, right);
            if (lockOperator) op.LockOperator(predefinedOperator); 
            else op.SetOperator(predefinedOperator);

            return new ASTTemplate(op, answer);
        }

        /// <summary>
        /// Создает тройной шаблон с левой ассоциативностью: OP1(OP2(V1, V2), V3) = Answer
        /// Пример: AND(OR(V1, V2), V3)
        /// </summary>
        public static ASTTemplate CreateTripleLeftAssoc(
            Operator innerOperator, 
            Operator outerOperator, 
            Trit answer,
            bool lockOperators = true,
            Trit? value1 = null,
            Trit? value2 = null, 
            Trit? value3 = null)
        {
            var v1 = new ValueSlotNode(); if (value1.HasValue) v1.LockValue(value1.Value);
            var v2 = new ValueSlotNode(); if (value2.HasValue) v2.LockValue(value2.Value);
            var v3 = new ValueSlotNode(); if (value3.HasValue) v3.LockValue(value3.Value);

            // Внутренняя операция: OP2(V1, V2)
            var innerOp = new OperatorSlotNode(v1, v2);
            if (lockOperators) innerOp.LockOperator(innerOperator);
            else innerOp.SetOperator(innerOperator);

            // Внешняя операция: OP1(inner, V3)
            var outerOp = new OperatorSlotNode(innerOp, v3);
            if (lockOperators) outerOp.LockOperator(outerOperator);
            else outerOp.SetOperator(outerOperator);

            return new ASTTemplate(outerOp, answer);
        }

        /// <summary>
        /// Создает тройной шаблон с правой ассоциативностью: OP1(V1, OP2(V2, V3)) = Answer
        /// Пример: OR(V1, AND(V2, V3))
        /// </summary>
        public static ASTTemplate CreateTripleRightAssoc(
            Operator outerOperator,
            Operator innerOperator, 
            Trit answer,
            bool lockOperators = true,
            Trit? value1 = null,
            Trit? value2 = null, 
            Trit? value3 = null)
        {
            var v1 = new ValueSlotNode(); if (value1.HasValue) v1.LockValue(value1.Value);
            var v2 = new ValueSlotNode(); if (value2.HasValue) v2.LockValue(value2.Value);
            var v3 = new ValueSlotNode(); if (value3.HasValue) v3.LockValue(value3.Value);

            // Внутренняя операция: OP2(V2, V3)
            var innerOp = new OperatorSlotNode(v2, v3);
            if (lockOperators) innerOp.LockOperator(innerOperator);
            else innerOp.SetOperator(innerOperator);

            // Внешняя операция: OP1(V1, inner)
            var outerOp = new OperatorSlotNode(v1, innerOp);
            if (lockOperators) outerOp.LockOperator(outerOperator);
            else outerOp.SetOperator(outerOperator);

            return new ASTTemplate(outerOp, answer);
        }

        /// <summary>
        /// Создает унарный шаблон: NOT(V) = Answer
        /// </summary>
        public static ASTTemplate CreateUnary(
            Trit answer,
            bool lockOperator = true,
            Trit? valueLocked = null)
        {
            var value = new ValueSlotNode();
            if (valueLocked.HasValue) value.LockValue(valueLocked.Value);

            // Для NOT операции правый операнд не используется
            var op = new OperatorSlotNode(value, null);
            if (lockOperator) op.LockOperator(Operator.NOT);
            else op.SetOperator(Operator.NOT);

            return new ASTTemplate(op, answer);
        }

        /// <summary>
        /// Создает сложный шаблон с тремя операциями: OP1(OP2(V1, V2), OP3(V3, V4)) = Answer
        /// Пример: AND(OR(V1, V2), XOR(V3, V4))
        /// </summary>
        public static ASTTemplate CreateComplexBinary(
            Operator leftOperator,
            Operator rightOperator,
            Operator rootOperator,
            Trit answer,
            bool lockOperators = true,
            Trit? value1 = null,
            Trit? value2 = null,
            Trit? value3 = null,
            Trit? value4 = null)
        {
            var v1 = new ValueSlotNode(); if (value1.HasValue) v1.LockValue(value1.Value);
            var v2 = new ValueSlotNode(); if (value2.HasValue) v2.LockValue(value2.Value);
            var v3 = new ValueSlotNode(); if (value3.HasValue) v3.LockValue(value3.Value);
            var v4 = new ValueSlotNode(); if (value4.HasValue) v4.LockValue(value4.Value);

            // Левая ветка: OP2(V1, V2)
            var leftBranch = new OperatorSlotNode(v1, v2);
            if (lockOperators) leftBranch.LockOperator(leftOperator);
            else leftBranch.SetOperator(leftOperator);

            // Правая ветка: OP3(V3, V4)
            var rightBranch = new OperatorSlotNode(v3, v4);
            if (lockOperators) rightBranch.LockOperator(rightOperator);
            else rightBranch.SetOperator(rightOperator);

            // Корневая операция: OP1(leftBranch, rightBranch)
            var root = new OperatorSlotNode(leftBranch, rightBranch);
            if (lockOperators) root.LockOperator(rootOperator);
            else root.SetOperator(rootOperator);

            return new ASTTemplate(root, answer);
        }
    }
}