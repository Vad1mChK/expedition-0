using UnityEngine;
using UnityEngine.UI;

namespace Expedition0.Tasks
{
    /// <summary>
    /// Специализированный привязчик для заданий 3-го типа
    /// Значения используют спрайты, операторы используют префабы объектов
    /// </summary>
    public class Type3TaskBoardBinder : MonoBehaviour
    {
        [Header("Slot Mapping")]
        [SerializeField] private GameObject[] valueSlots;
        [SerializeField] private GameObject[] operatorSlots;



        [Header("Operator Prefabs")]
        [SerializeField] private GameObject notPrefab;
        [SerializeField] private GameObject andPrefab;
        [SerializeField] private GameObject orPrefab;
        [SerializeField] private GameObject xorPrefab;



        [Header("Result Display")]
        [SerializeField] private Type3ResultDisplay resultDisplay; // Компонент для отображения результата левой части

        [Header("Pipe System")]
        [SerializeField] private Type3PipeManager pipeManager; // Менеджер труб для визуализации

        [Header("Fallback UI (Optional)")]
        [SerializeField] private Image[] valueImages; // Резервные UI изображения
        [SerializeField] private Image answerImage; // Резервное UI изображение для ответа

        [Header("Digit Sprites (for fallback UI)")]
        [SerializeField] private Sprite digit0Sprite;
        [SerializeField] private Sprite digit1Sprite;
        [SerializeField] private Sprite digit2Sprite;

        public void Bind(ASTTemplate template)
        {
            Debug.Log("Type3TaskBoardBinder: Starting bind process");
            
            // Привязываем значения
            BindValueSlots(template.ValueSlots);
            
            // Привязываем операторы
            BindOperatorSlots(template.OperatorSlots);
            
            // Отображаем ответ
            DisplayAnswer(template.Answer);
            
            // Обновляем систему труб
            UpdatePipeSystem(template);
            
            // Настраиваем отображение результата левой части
            SetupResultDisplay(template);
            
            Debug.Log($"Type3TaskBoardBinder: Bind completed - {template.ValueSlots.Count} values, {template.OperatorSlots.Count} operators");
        }

        private void BindValueSlots(System.Collections.Generic.IReadOnlyList<ValueSlotNode> valueSlots)
        {
            for (int i = 0; i < valueSlots.Count && i < this.valueSlots.Length; i++)
            {
                var slotNode = valueSlots[i];
                var slotGameObject = this.valueSlots[i];
                
                if (slotGameObject != null)
                {
                    // Для значений используем стандартный ValueSlotView (со спрайтами)
                    var standardView = slotGameObject.GetComponentInChildren<ValueSlotView>();
                    if (standardView != null)
                    {
                        standardView.BindNode(slotNode);
                        Debug.Log($"Type3TaskBoardBinder: Bound ValueSlotView {i} to AST node");
                    }
                    else
                    {
                        Debug.LogWarning($"Type3TaskBoardBinder: No ValueSlotView found on slot {i}");
                    }
                }

                // Обновляем UI изображения для значений (спрайты)
                if (valueImages != null && i < valueImages.Length && valueImages[i] != null)
                {
                    ApplyDigitImage(valueImages[i], slotNode.CurrentValue);
                }
            }
        }

        private void BindOperatorSlots(System.Collections.Generic.IReadOnlyList<OperatorSlotNode> operatorSlots)
        {
            for (int i = 0; i < operatorSlots.Count && i < this.operatorSlots.Length; i++)
            {
                var slotNode = operatorSlots[i];
                var slotGameObject = this.operatorSlots[i];
                
                if (slotGameObject != null)
                {
                    // Ищем Type3OperatorSlotView компонент
                    var type3View = slotGameObject.GetComponentInChildren<Type3OperatorSlotView>();
                    if (type3View != null)
                    {
                        // Настраиваем префабы для Type3OperatorSlotView
                        type3View.SetOperatorPrefabs(
                            notPrefab, andPrefab, orPrefab, xorPrefab
                        );
                        type3View.BindNode(slotNode);
                        Debug.Log($"Type3TaskBoardBinder: Bound Type3OperatorSlotView {i} to AST node with operator {slotNode.CurrentOperator}");
                    }
                    else
                    {
                        // Fallback: ищем обычный OperatorSlotView
                        var standardView = slotGameObject.GetComponentInChildren<OperatorSlotView>();
                        if (standardView != null)
                        {
                            standardView.BindNode(slotNode);
                            Debug.Log($"Type3TaskBoardBinder: Bound standard OperatorSlotView {i} to AST node");
                        }
                        else
                        {
                            Debug.LogWarning($"Type3TaskBoardBinder: No OperatorSlotView found on slot {i}");
                        }
                    }
                }
            }
        }

        private void DisplayAnswer(Trit answer)
        {
            // Отображаем ответ как спрайт
            if (answerImage != null)
            {
                ApplyDigitImage(answerImage, answer);
                Debug.Log($"Type3TaskBoardBinder: Displayed answer {answer} ({answer.ToInt()}) as sprite");
            }
        }

        private void UpdatePipeSystem(ASTTemplate template)
        {
            if (pipeManager != null)
            {
                pipeManager.UpdatePipeConfiguration(template);
                Debug.Log("Type3TaskBoardBinder: Updated pipe system configuration");
            }
        }

        private void SetupResultDisplay(ASTTemplate template)
        {
            if (resultDisplay != null)
            {
                // Находим TaskLoader для привязки к ResultDisplay
                var taskLoader = FindObjectOfType<Type3TaskLoader>();
                if (taskLoader != null)
                {
                    resultDisplay.SetTaskLoader(taskLoader);
                    
                    // Для результата используем спрайты (как для значений)
                    // Настраиваем спрайты для отображения результата
                    resultDisplay.SetResultSprites(digit0Sprite, digit1Sprite, digit2Sprite);
                    
                    // Принудительно обновляем результат
                    resultDisplay.ForceUpdateResult();
                    
                    Debug.Log("Type3TaskBoardBinder: Setup result display for real-time updates");
                }
                else
                {
                    Debug.LogWarning("Type3TaskBoardBinder: No Type3TaskLoader found for result display");
                }
            }
        }



        private GameObject GetOperatorPrefab(Operator op)
        {
            switch (op)
            {
                case Operator.NOT: return notPrefab;
                case Operator.AND: return andPrefab;
                case Operator.OR: return orPrefab;
                case Operator.XOR: return xorPrefab;
                default: return null;
            }
        }

        // Fallback методы для UI изображений
        private void ApplyDigitImage(Image image, Trit? value)
        {
            if (image == null) return;

            if (!value.HasValue)
            {
                image.sprite = null;
                image.enabled = false;
                return;
            }

            image.sprite = GetDigitSprite(value.Value);
            image.enabled = image.sprite != null;
        }


        private Sprite GetDigitSprite(Trit value)
        {
            switch (value.ToInt())
            {
                case 0: return digit0Sprite;
                case 1: return digit1Sprite;
                case 2: return digit2Sprite;
                default: return null;
            }
        }

        // Публичные методы для настройки

        public void SetOperatorPrefabs(
            GameObject not, GameObject and, GameObject or, GameObject xor)
        {
            notPrefab = not;
            andPrefab = and;
            orPrefab = or;
            xorPrefab = xor;
           
        }

        public void SetPipeManager(Type3PipeManager manager)
        {
            pipeManager = manager;
        }

        public void SetResultDisplay(Type3ResultDisplay display)
        {
            resultDisplay = display;
        }

        // Методы для тестирования в инспекторе
        [ContextMenu("Test Bind Current Template")]
        public void TestBindCurrentTemplate()
        {
            var taskLoader = FindObjectOfType<Type3TaskLoader>();
            if (taskLoader != null && taskLoader.GetTemplate() != null)
            {
                Bind(taskLoader.GetTemplate());
            }
            else
            {
                Debug.LogWarning("Type3TaskBoardBinder: No task loader or template found for testing");
            }
        }

        [ContextMenu("Test Display Answer True")]
        public void TestDisplayAnswerTrue()
        {
            DisplayAnswer(Trit.True);
        }

        [ContextMenu("Test Display Answer Neutral")]
        public void TestDisplayAnswerNeutral()
        {
            DisplayAnswer(Trit.Neutral);
        }

        [ContextMenu("Test Display Answer False")]
        public void TestDisplayAnswerFalse()
        {
            DisplayAnswer(Trit.False);
        }
    }
}