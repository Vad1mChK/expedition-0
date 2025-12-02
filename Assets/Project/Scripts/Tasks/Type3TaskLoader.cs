using UnityEngine;

namespace Expedition0.Tasks
{
    /// <summary>
    /// Типы заданий 3-го типа с троичной логикой
    /// </summary>
    public enum Type3TaskType
    {
        AndOrNeutralXY,        // AND(OR(1, X), Y) = 2
        OrXAndFalseY,          // OR(X, AND(0, Y)) = 1
        ComplexXorAndOr,       // XOR(AND(X, Y), OR(1, Z)) = 0
        NotX,                  // NOT(X) = 2
        NotXAndY,              // NOT(X) AND Y = ans
        ImplyXYUnlocked,       // IMPLY(X, Y) = 1 (оператор разблокирован)
        FindOperator,          // OP(2, 0) = 1 (найти оператор)
        CalculateResult,       // AND(OR(X, 1), XOR(Y, 2)) = ? (вычислить результат)
        CustomTriple,          // Пользовательское тройное задание
        CustomBinary,          // Пользовательское бинарное задание
        CustomUnary            // Пользовательское унарное задание
    }

    /// <summary>
    /// Загрузчик заданий 3-го типа с троичной логикой и трубами
    /// Поддерживает различные конфигурации AST с 3-мя значениями и двумя операциями
    /// </summary>
    public class Type3TaskLoader : MonoBehaviour
    {
        [Header("Task Configuration")]
        public Type3TaskBoardBinder binder;
        
        [Header("Task Selection")]
        public Type3TaskType selectedTaskType = Type3TaskType.AndOrNeutralXY;
        
        [Header("Custom Triple Task Settings")]
        public Operator customInnerOperator = Operator.OR;
        public Operator customOuterOperator = Operator.AND;
        public Trit customTripleAnswer = Trit.True;
        public bool lockTripleOperators = true;
        public bool useLeftAssociativity = true; // true = left, false = right
        
        [Header("Custom Triple Values")]
        public bool lockValue1 = false;
        public Trit customValue1 = Trit.False;
        public bool lockValue2 = false;
        public Trit customValue2 = Trit.False;
        public bool lockValue3 = false;
        public Trit customValue3 = Trit.False;
        
        [Header("Custom Binary Task Settings")]
        public Operator customBinaryOperator = Operator.AND;
        public Trit customBinaryAnswer = Trit.True;
        public bool lockBinaryOperator = true;
        public bool lockBinaryLeft = false;
        public bool lockBinaryRight = false;
        public Trit customBinaryLeftValue = Trit.False;
        public Trit customBinaryRightValue = Trit.False;
        
        [Header("Custom Unary Task Settings")]
        public Trit customUnaryAnswer = Trit.True;
        public bool lockUnaryOperator = true;
        public bool lockUnaryValue = false;
        public Trit customUnaryValue = Trit.False;

        public ASTTemplate template;

        private void Start()
        {
            if (binder == null)
            {
                binder = GetComponent<Type3TaskBoardBinder>();
            }
            if (binder == null) return;

            template = CreateTaskByType(selectedTaskType);
            
            if (template != null)
            {
                binder.Bind(template);
                Debug.Log($"Type3TaskLoader: Loaded task type: {selectedTaskType}");
            }
            else
            {
                Debug.LogError("Type3TaskLoader: Failed to create task template!");
            }
        }

        private ASTTemplate CreateTaskByType(Type3TaskType taskType)
        {
            switch (taskType)
            {
                case Type3TaskType.AndOrNeutralXY:
                    return Create3TypeTasks.CreateAndOrNeutralXY();
                
                case Type3TaskType.OrXAndFalseY:
                    return Create3TypeTasks.CreateOrXAndFalseY();
                
                case Type3TaskType.ComplexXorAndOr:
                    return Create3TypeTasks.CreateComplexXorAndOr();
                
                case Type3TaskType.NotX:
                    return Create3TypeTasks.CreateNotX();
                
                case Type3TaskType.NotXAndY:
                    return Create3TypeTasks.CreateNotXAndY();
                
                case Type3TaskType.ImplyXYUnlocked:
                    return Create3TypeTasks.CreateImplyXYUnlocked();
                
                case Type3TaskType.FindOperator:
                    return Create3TypeTasks.CreateFindOperator();
                
                case Type3TaskType.CalculateResult:
                    return Create3TypeTasks.CreateCalculateResult();
                
                case Type3TaskType.CustomTriple:
                    return CreateCustomTripleTask();
                
                case Type3TaskType.CustomBinary:
                    return CreateCustomBinaryTask();
                
                case Type3TaskType.CustomUnary:
                    return CreateCustomUnaryTask();
                
                default:
                    Debug.LogError($"Type3TaskLoader: Unknown task type: {taskType}");
                    return null;
            }
        }

        private ASTTemplate CreateCustomTripleTask()
        {
            Trit? value1 = lockValue1 ? customValue1 : null;
            Trit? value2 = lockValue2 ? customValue2 : null;
            Trit? value3 = lockValue3 ? customValue3 : null;
            
            if (useLeftAssociativity)
            {
                return Type3TaskTemplateFactory.CreateTripleLeftAssoc(
                    innerOperator: customInnerOperator,
                    outerOperator: customOuterOperator,
                    answer: customTripleAnswer,
                    lockOperators: lockTripleOperators,
                    value1: value1,
                    value2: value2,
                    value3: value3
                );
            }
            else
            {
                return Type3TaskTemplateFactory.CreateTripleRightAssoc(
                    outerOperator: customOuterOperator,
                    innerOperator: customInnerOperator,
                    answer: customTripleAnswer,
                    lockOperators: lockTripleOperators,
                    value1: value1,
                    value2: value2,
                    value3: value3
                );
            }
        }

        private ASTTemplate CreateCustomBinaryTask()
        {
            Trit? leftValue = lockBinaryLeft ? customBinaryLeftValue : null;
            Trit? rightValue = lockBinaryRight ? customBinaryRightValue : null;
            
            return Type3TaskTemplateFactory.CreateBinary(
                predefinedOperator: customBinaryOperator,
                answer: customBinaryAnswer,
                lockOperator: lockBinaryOperator,
                leftLocked: leftValue,
                rightLocked: rightValue
            );
        }

        private ASTTemplate CreateCustomUnaryTask()
        {
            Trit? value = lockUnaryValue ? customUnaryValue : null;
            
            return Type3TaskTemplateFactory.CreateUnary(
                answer: customUnaryAnswer,
                lockOperator: lockUnaryOperator,
                valueLocked: value
            );
        }

        // Публичные методы для управления заданиями
        public ASTTemplate GetTemplate()
        {
            return template;
        }

        public ASTNode GetRootNode()
        {
            return template?.Root;
        }

        public void ChangeTaskType(Type3TaskType newTaskType)
        {
            selectedTaskType = newTaskType;
            ReloadTask();
        }

        public void ReloadTask()
        {
            template = CreateTaskByType(selectedTaskType);
            
            if (template != null && binder != null)
            {
                binder.Bind(template);
                Debug.Log($"Type3TaskLoader: Reloaded task type: {selectedTaskType}");
            }
            else
            {
                Debug.LogError("Type3TaskLoader: Failed to reload task template!");
            }
        }

        public void LoadCustomTripleTask(
            Operator innerOp, 
            Operator outerOp, 
            Trit answer, 
            bool lockOps, 
            bool leftAssoc = true,
            Trit? val1 = null, 
            Trit? val2 = null, 
            Trit? val3 = null)
        {
            customInnerOperator = innerOp;
            customOuterOperator = outerOp;
            customTripleAnswer = answer;
            lockTripleOperators = lockOps;
            useLeftAssociativity = leftAssoc;
            
            lockValue1 = val1.HasValue;
            lockValue2 = val2.HasValue;
            lockValue3 = val3.HasValue;
            
            if (val1.HasValue) customValue1 = val1.Value;
            if (val2.HasValue) customValue2 = val2.Value;
            if (val3.HasValue) customValue3 = val3.Value;
            
            ChangeTaskType(Type3TaskType.CustomTriple);
        }

        public string GetTaskInfo()
        {
            if (template == null) return "No task loaded";
            
            string info = $"Task Type: {selectedTaskType}\n";
            info += $"Answer: {template.Answer}\n";
            info += $"Value Slots: {template.ValueSlots.Count}\n";
            info += $"Operator Slots: {template.OperatorSlots.Count}\n";
            
            // Добавляем специфичную информацию для каждого типа
            switch (selectedTaskType)
            {
                case Type3TaskType.CustomTriple:
                    info += $"\nTriple Settings:\n";
                    info += $"  Inner Op: {customInnerOperator}, Outer Op: {customOuterOperator}\n";
                    info += $"  Associativity: {(useLeftAssociativity ? "Left" : "Right")}\n";
                    info += $"  Operators Locked: {lockTripleOperators}\n";
                    break;
                    
                case Type3TaskType.CustomBinary:
                    info += $"\nBinary Settings:\n";
                    info += $"  Operator: {customBinaryOperator} (Locked: {lockBinaryOperator})\n";
                    break;
                    
                case Type3TaskType.CustomUnary:
                    info += $"\nUnary Settings:\n";
                    info += $"  Operator: NOT (Locked: {lockUnaryOperator})\n";
                    break;
            }
            
            return info;
        }

        public bool ValidateTaskSettings()
        {
            // Проверка корректности настроек
            if (selectedTaskType == Type3TaskType.CustomUnary)
            {
                // Для унарных операций проверяем только NOT
                return true;
            }
            
            return true;
        }

        // Методы для тестирования в инспекторе
        [ContextMenu("Reload Current Task")]
        public void TestReloadTask()
        {
            ReloadTask();
        }

        [ContextMenu("Load AND(OR(1,X),Y) Task")]
        public void TestLoadAndOrTask()
        {
            ChangeTaskType(Type3TaskType.AndOrNeutralXY);
        }

        [ContextMenu("Load Complex XOR Task")]
        public void TestLoadComplexTask()
        {
            ChangeTaskType(Type3TaskType.ComplexXorAndOr);
        }

        [ContextMenu("Load Find Operator Task")]
        public void TestLoadFindOperatorTask()
        {
            ChangeTaskType(Type3TaskType.FindOperator);
        }

        [ContextMenu("Load Custom Triple (AND-OR)")]
        public void TestLoadCustomTriple()
        {
            LoadCustomTripleTask(Operator.AND, Operator.OR, Trit.True, true, true, Trit.Neutral, null, null);
        }

        [ContextMenu("Print Task Info")]
        public void TestPrintTaskInfo()
        {
            Debug.Log(GetTaskInfo());
        }
    }
}