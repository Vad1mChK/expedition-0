using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Expedition0.Tasks
{
    /// <summary>
    /// Менеджер системы труб для визуализации потока данных в заданиях 3-го типа
    /// Управляет анимацией и цветовой индикацией труб
    /// </summary>
    public class Type3PipeManager : MonoBehaviour
    {
        [Header("Pipe Configuration")]
        [SerializeField] private Renderer[] pipeRenderers; // Рендереры всех труб
        [SerializeField] private Transform[] pipeSegments; // Сегменты труб для анимации
        
        [Header("Materials")]
        [SerializeField] private Material neutralMaterial; // Нейтральный материал
        [SerializeField] private Material falseMaterial;   // Материал для False (0)
        [SerializeField] private Material neutralValueMaterial; // Материал для Neutral (1)
        [SerializeField] private Material trueMaterial;    // Материал для True (2)
        [SerializeField] private Material correctFlowMaterial; // Материал для правильного потока
        [SerializeField] private Material incorrectFlowMaterial; // Материал для неправильного потока
        
        [Header("Animation Settings")]
        [SerializeField] private bool enableFlowAnimation = true;
        [SerializeField] private float flowSpeed = 2.0f; // Скорость потока
        [SerializeField] private float animationDuration = 1.0f; // Длительность анимации
        [SerializeField] private AnimationCurve flowCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Pipe Mapping")]
        [SerializeField] private PipeConnection[] pipeConnections; // Связи между элементами и трубами
        
        private Dictionary<ASTNode, List<int>> nodeToPipeMapping; // Маппинг узлов AST к индексам труб
        private Coroutine currentFlowAnimation;

        [System.Serializable]
        public class PipeConnection
        {
            public int pipeIndex; // Индекс трубы в массиве pipeRenderers
            public PipeConnectionType connectionType; // Тип соединения
            public int sourceSlotIndex; // Индекс исходного слота
            public int targetSlotIndex; // Индекс целевого слота
            public Vector3[] flowPath; // Путь потока через трубу
        }

        public enum PipeConnectionType
        {
            ValueToOperator,    // От значения к оператору
            OperatorToOperator, // От оператора к оператору
            OperatorToResult    // От оператора к результату
        }

        private void Awake()
        {
            InitializePipeMapping();
            ResetAllPipes();
        }

        private void InitializePipeMapping()
        {
            nodeToPipeMapping = new Dictionary<ASTNode, List<int>>();
            
            // Инициализируем маппинг на основе конфигурации pipeConnections
            // Это будет настроено при привязке шаблона
        }

        public void UpdatePipeConfiguration(ASTTemplate template)
        {
            if (template == null) return;
            
            Debug.Log("Type3PipeManager: Updating pipe configuration for new template");
            
            // Сбрасываем все трубы
            ResetAllPipes();
            
            // Обновляем маппинг узлов к трубам
            UpdateNodeToPipeMapping(template);
            
            // Применяем начальные материалы на основе текущих значений
            ApplyInitialMaterials(template);
        }

        private void UpdateNodeToPipeMapping(ASTTemplate template)
        {
            nodeToPipeMapping.Clear();
            
            // Создаем маппинг для значений
            for (int i = 0; i < template.ValueSlots.Count; i++)
            {
                var valueNode = template.ValueSlots[i];
                var pipesForNode = GetPipesForValueSlot(i);
                nodeToPipeMapping[valueNode] = pipesForNode;
            }
            
            // Создаем маппинг для операторов
            for (int i = 0; i < template.OperatorSlots.Count; i++)
            {
                var operatorNode = template.OperatorSlots[i];
                var pipesForNode = GetPipesForOperatorSlot(i);
                nodeToPipeMapping[operatorNode] = pipesForNode;
            }
        }

        private List<int> GetPipesForValueSlot(int slotIndex)
        {
            var pipes = new List<int>();
            
            foreach (var connection in pipeConnections)
            {
                if (connection.connectionType == PipeConnectionType.ValueToOperator && 
                    connection.sourceSlotIndex == slotIndex)
                {
                    pipes.Add(connection.pipeIndex);
                }
            }
            
            return pipes;
        }

        private List<int> GetPipesForOperatorSlot(int slotIndex)
        {
            var pipes = new List<int>();
            
            foreach (var connection in pipeConnections)
            {
                if ((connection.connectionType == PipeConnectionType.OperatorToOperator || 
                     connection.connectionType == PipeConnectionType.OperatorToResult) && 
                    connection.sourceSlotIndex == slotIndex)
                {
                    pipes.Add(connection.pipeIndex);
                }
            }
            
            return pipes;
        }

        private void ApplyInitialMaterials(ASTTemplate template)
        {
            // Применяем материалы для значений
            for (int i = 0; i < template.ValueSlots.Count; i++)
            {
                var valueNode = template.ValueSlots[i];
                if (valueNode.CurrentValue.HasValue)
                {
                    ApplyValueMaterialToPipes(valueNode, valueNode.CurrentValue.Value);
                }
            }
        }

        public void AnimateDataFlow(ASTTemplate template, bool isCorrectSolution)
        {
            if (!enableFlowAnimation) return;
            
            if (currentFlowAnimation != null)
            {
                StopCoroutine(currentFlowAnimation);
            }
            
            currentFlowAnimation = StartCoroutine(AnimateFlowCoroutine(template, isCorrectSolution));
        }

        private IEnumerator AnimateFlowCoroutine(ASTTemplate template, bool isCorrectSolution)
        {
            Debug.Log($"Type3PipeManager: Starting flow animation - Correct: {isCorrectSolution}");
            
            // Фаза 1: Анимация потока от значений к операторам
            yield return AnimateValueFlow(template);
            
            // Фаза 2: Анимация вычислений в операторах
            yield return AnimateOperatorFlow(template);
            
            // Фаза 3: Анимация результата
            yield return AnimateResultFlow(template, isCorrectSolution);
            
            Debug.Log("Type3PipeManager: Flow animation completed");
        }

        private IEnumerator AnimateValueFlow(ASTTemplate template)
        {
            // Анимируем поток от каждого значения
            for (int i = 0; i < template.ValueSlots.Count; i++)
            {
                var valueNode = template.ValueSlots[i];
                if (valueNode.CurrentValue.HasValue && nodeToPipeMapping.ContainsKey(valueNode))
                {
                    var pipes = nodeToPipeMapping[valueNode];
                    Material valueMaterial = GetMaterialForValue(valueNode.CurrentValue.Value);
                    
                    yield return AnimatePipeFlow(pipes, valueMaterial, flowSpeed);
                }
            }
        }

        private IEnumerator AnimateOperatorFlow(ASTTemplate template)
        {
            // Анимируем поток через операторы (в порядке вычисления)
            foreach (var operatorNode in template.OperatorSlots)
            {
                if (nodeToPipeMapping.ContainsKey(operatorNode))
                {
                    Trit? result = null;
                    try
                    {
                        result = operatorNode.Evaluate();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Type3PipeManager: Could not evaluate operator: {e.Message}");
                        continue; // Пропускаем этот оператор
                    }
                    
                    if (result.HasValue)
                    {
                        Material resultMaterial = GetMaterialForValue(result.Value);
                        var pipes = nodeToPipeMapping[operatorNode];
                        yield return AnimatePipeFlow(pipes, resultMaterial, flowSpeed);
                    }
                }
            }
        }

        private IEnumerator AnimateResultFlow(ASTTemplate template, bool isCorrectSolution)
        {
            // Анимируем финальный результат
            Material finalMaterial = isCorrectSolution ? correctFlowMaterial : incorrectFlowMaterial;
            
            // Находим трубы, ведущие к результату
            var resultPipes = new List<int>();
            foreach (var connection in pipeConnections)
            {
                if (connection.connectionType == PipeConnectionType.OperatorToResult)
                {
                    resultPipes.Add(connection.pipeIndex);
                }
            }
            
            if (resultPipes.Count > 0)
            {
                yield return AnimatePipeFlow(resultPipes, finalMaterial, flowSpeed * 0.5f);
            }
        }

        private IEnumerator AnimatePipeFlow(List<int> pipeIndices, Material material, float speed)
        {
            float duration = animationDuration / speed;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float curveValue = flowCurve.Evaluate(t);
                
                // Применяем материал к трубам постепенно
                int pipesToActivate = Mathf.RoundToInt(pipeIndices.Count * curveValue);
                
                for (int i = 0; i < pipesToActivate && i < pipeIndices.Count; i++)
                {
                    int pipeIndex = pipeIndices[i];
                    if (pipeIndex < pipeRenderers.Length && pipeRenderers[pipeIndex] != null)
                    {
                        pipeRenderers[pipeIndex].material = material;
                    }
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Убеждаемся, что все трубы активированы
            foreach (int pipeIndex in pipeIndices)
            {
                if (pipeIndex < pipeRenderers.Length && pipeRenderers[pipeIndex] != null)
                {
                    pipeRenderers[pipeIndex].material = material;
                }
            }
        }

        public void ApplyValueMaterialToPipes(ASTNode node, Trit value)
        {
            if (!nodeToPipeMapping.ContainsKey(node)) return;
            
            Material material = GetMaterialForValue(value);
            var pipes = nodeToPipeMapping[node];
            
            foreach (int pipeIndex in pipes)
            {
                if (pipeIndex < pipeRenderers.Length && pipeRenderers[pipeIndex] != null)
                {
                    pipeRenderers[pipeIndex].material = material;
                }
            }
        }

        public void ApplyResultMaterial(bool isCorrect)
        {
            Material material = isCorrect ? correctFlowMaterial : incorrectFlowMaterial;
            
            // Применяем к трубам результата
            foreach (var connection in pipeConnections)
            {
                if (connection.connectionType == PipeConnectionType.OperatorToResult)
                {
                    int pipeIndex = connection.pipeIndex;
                    if (pipeIndex < pipeRenderers.Length && pipeRenderers[pipeIndex] != null)
                    {
                        pipeRenderers[pipeIndex].material = material;
                    }
                }
            }
        }

        public void ResetAllPipes()
        {
            if (pipeRenderers == null) return;
            
            foreach (var renderer in pipeRenderers)
            {
                if (renderer != null && neutralMaterial != null)
                {
                    renderer.material = neutralMaterial;
                }
            }
            
            Debug.Log("Type3PipeManager: Reset all pipes to neutral material");
        }

        private Material GetMaterialForValue(Trit value)
        {
            switch (value.ToInt())
            {
                case 0: return falseMaterial;
                case 1: return neutralValueMaterial;
                case 2: return trueMaterial;
                default: return neutralMaterial;
            }
        }

        // Публичные методы для настройки
        public void SetPipeRenderers(Renderer[] renderers)
        {
            pipeRenderers = renderers;
        }

        public void SetMaterials(Material neutral, Material falseVal, Material neutralVal, Material trueVal, Material correct, Material incorrect)
        {
            neutralMaterial = neutral;
            falseMaterial = falseVal;
            neutralValueMaterial = neutralVal;
            trueMaterial = trueVal;
            correctFlowMaterial = correct;
            incorrectFlowMaterial = incorrect;
        }

        public void SetPipeConnections(PipeConnection[] connections)
        {
            pipeConnections = connections;
            InitializePipeMapping();
        }

        // Методы для тестирования в инспекторе
        [ContextMenu("Test Reset All Pipes")]
        public void TestResetAllPipes()
        {
            ResetAllPipes();
        }

        [ContextMenu("Test Animate Correct Flow")]
        public void TestAnimateCorrectFlow()
        {
            var taskLoader = FindObjectOfType<Type3TaskLoader>();
            if (taskLoader != null && taskLoader.GetTemplate() != null)
            {
                AnimateDataFlow(taskLoader.GetTemplate(), true);
            }
        }

        [ContextMenu("Test Animate Incorrect Flow")]
        public void TestAnimateIncorrectFlow()
        {
            var taskLoader = FindObjectOfType<Type3TaskLoader>();
            if (taskLoader != null && taskLoader.GetTemplate() != null)
            {
                AnimateDataFlow(taskLoader.GetTemplate(), false);
            }
        }

        [ContextMenu("Test Apply Result Material (Correct)")]
        public void TestApplyCorrectResult()
        {
            ApplyResultMaterial(true);
        }

        [ContextMenu("Test Apply Result Material (Incorrect)")]
        public void TestApplyIncorrectResult()
        {
            ApplyResultMaterial(false);
        }
    }
}