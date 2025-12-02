namespace Expedition0.Tasks
{
    /// <summary>
    /// Утилиты для создания заданий типа 3 с троичной логикой
    /// Содержит предопределенные шаблоны заданий
    /// </summary>
    public static class Create3TypeTasks
    {
        /// <summary>
        /// Пример: AND(OR(1, X), Y) = 2
        /// Внутренняя операция OR заблокирована, левое значение = 1 (Neutral)
        /// Внешняя операция AND заблокирована
        /// </summary>
        public static ASTTemplate CreateAndOrNeutralXY()
        {
            return Type3TaskTemplateFactory.CreateTripleLeftAssoc(
                innerOperator: Operator.OR,
                outerOperator: Operator.AND,
                answer: Trit.True,
                lockOperators: true,
                value1: Trit.Neutral,  // Заблокировано на 1
                value2: null,          // X - свободно
                value3: null           // Y - свободно
            );
        }

        /// <summary>
        /// Пример: OR(X, AND(0, Y)) = 1
        /// Внутренняя операция AND заблокирована, левое значение = 0 (False)
        /// Внешняя операция OR заблокирована
        /// </summary>
        public static ASTTemplate CreateOrXAndFalseY()
        {
            return Type3TaskTemplateFactory.CreateTripleRightAssoc(
                outerOperator: Operator.OR,
                innerOperator: Operator.AND,
                answer: Trit.Neutral,
                lockOperators: true,
                value1: null,          // X - свободно
                value2: Trit.False,    // Заблокировано на 0
                value3: null           // Y - свободно
            );
        }

        /// <summary>
        /// Пример: XOR(AND(X, Y), OR(1, Z)) = 0
        /// Сложное дерево с тремя операциями
        /// </summary>
        public static ASTTemplate CreateComplexXorAndOr()
        {
            return Type3TaskTemplateFactory.CreateComplexBinary(
                leftOperator: Operator.AND,
                rightOperator: Operator.OR,
                rootOperator: Operator.XOR,
                answer: Trit.False,
                lockOperators: true,
                value1: null,          // X - свободно
                value2: null,          // Y - свободно
                value3: Trit.Neutral,  // Заблокировано на 1
                value4: null           // Z - свободно
            );
        }

        /// <summary>
        /// Пример: NOT(X) = 2
        /// Простая унарная операция
        /// </summary>
        public static ASTTemplate CreateNotX()
        {
            return Type3TaskTemplateFactory.CreateUnary(
                answer: Trit.True,
                lockOperator: true,
                valueLocked: null      // X - свободно
            );
        }

        /// <summary>
        /// Пример: NOT(X) AND Y = ans
        /// Структура: AND(NOT(X), Y) = ans
        /// Порядок слотов на сцене: X, NOT, Y, AND (слева направо)
        /// </summary>
        public static ASTTemplate CreateNotXAndY()
        {
            // Создаем значения
            var xValue = new ValueSlotNode(); // X - свободно
            var yValue = new ValueSlotNode(); // Y - свободно
            
            // Создаем операторы
            var notOperator = new OperatorSlotNode(xValue, null); // NOT(X)
            notOperator.LockOperator(Operator.NOT);
            
            var andOperator = new OperatorSlotNode(notOperator, yValue); // AND(NOT(X), Y)
            andOperator.LockOperator(Operator.AND);
            
            // Создаем списки слотов в правильном порядке для сцены:
            // Значения: X (слот 0), Y (слот 1)
            var valueSlots = new System.Collections.Generic.List<ValueSlotNode> { xValue, yValue };
            
            // Операторы: NOT (слот 0), AND (слот 1)
            var operatorSlots = new System.Collections.Generic.List<OperatorSlotNode> { notOperator, andOperator };
            
            // Используем конструктор с ручным порядком слотов
            return new ASTTemplate(andOperator, Trit.Neutral, valueSlots, operatorSlots);
        }

        /// <summary>
        /// Пример: IMPLY(X, Y) = 1 с разблокированным оператором
        /// Игрок должен выбрать правильный оператор
        /// </summary>
        public static ASTTemplate CreateImplyXYUnlocked()
        {
            return Type3TaskTemplateFactory.CreateBinary(
                predefinedOperator: Operator.IMPLY,
                answer: Trit.Neutral,
                lockOperator: false,   // Оператор разблокирован!
                leftLocked: null,      // X - свободно
                rightLocked: null      // Y - свободно
            );
        }

        /// <summary>
        /// Пример: OP(2, 0) = 1 - игрок должен найти правильный оператор
        /// Значения заблокированы, оператор свободен
        /// </summary>
        public static ASTTemplate CreateFindOperator()
        {
            return Type3TaskTemplateFactory.CreateBinary(
                predefinedOperator: Operator.OR, // Начальное значение (будет изменено игроком)
                answer: Trit.Neutral,
                lockOperator: false,
                leftLocked: Trit.True,   // Заблокировано на 2
                rightLocked: Trit.False  // Заблокировано на 0
            );
        }

        /// <summary>
        /// Пример сложного задания: AND(OR(X, 1), XOR(Y, 2)) = Z
        /// Все операторы заблокированы, но ответ неизвестен - игрок должен его вычислить
        /// </summary>
        public static ASTTemplate CreateCalculateResult()
        {
            return Type3TaskTemplateFactory.CreateComplexBinary(
                leftOperator: Operator.OR,
                rightOperator: Operator.XOR,
                rootOperator: Operator.AND,
                answer: Trit.Neutral, // Это нужно будет вычислить
                lockOperators: true,
                value1: null,          // X - свободно
                value2: Trit.Neutral,  // Заблокировано на 1
                value3: null,          // Y - свободно
                value4: Trit.True      // Заблокировано на 2
            );
        }

        /// <summary>
        /// Проверяет соответствие левой части ответу
        /// </summary>
        public static bool Check(ASTTemplate template)
        {
            var lhs = SolutionAST.Solution(template.Root);
            return lhs == template.Answer;
        }

        /// <summary>
        /// Вычисляет результат AST для заданных входных значений
        /// Полезно для проверки правильности шаблона
        /// </summary>
        public static Trit CalculateResult(ASTTemplate template)
        {
            return SolutionAST.Solution(template.Root);
        }
    }
}