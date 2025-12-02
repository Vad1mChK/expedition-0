using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Expedition0.Util;

namespace Expedition0.Tasks
{
    /// <summary>
    /// Специализированный проверяльщик решений для заданий 3-го типа
    /// Поддерживает троичную логику с трубами и сложные AST структуры
    /// </summary>
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
    public class Type3SolutionChecker : MonoBehaviour
    {
        [Header("Solution Settings")]
        [SerializeField] private Type3TaskLoader taskLoader; // Ссылка на загрузчик заданий 3-го типа
        private Trit answer;
        
        [Header("Visual Feedback")]
        [SerializeField] private MaterialAssigner equalsAssigner; // MaterialAssigner для символа "равно"
        [SerializeField] private MaterialAssigner notEqualsAssigner; // MaterialAssigner для символа "не равно"
        
        [Header("Pipe Visual Effects")]
        [SerializeField] private Type3PipeManager pipeManager; // Менеджер системы труб
        [SerializeField] private Renderer[] pipeRenderers; // Рендереры труб для визуализации потока данных (fallback)
        [SerializeField] private Material pipeCorrectMaterial; // Материал для правильного решения
        [SerializeField] private Material pipeIncorrectMaterial; // Материал для неправильного решения
        [SerializeField] private Material pipeNeutralMaterial; // Нейтральный материал для труб
        
        [Header("Alternative: Direct Renderer Control")]
        [SerializeField] private Renderer targetRenderer; // Renderer модели
        [SerializeField] private int equalsSlotIndex = 0; // Индекс слота для символа "="
        [SerializeField] private int slashSlotIndex = 1; // Индекс слота для символа "/"
        
        [Header("Materials")]
        [SerializeField] private Material correctMaterial;
        [SerializeField] private Material incorrectMaterial;
        [SerializeField] private Material transparentMaterial; // Прозрачный материал для скрытия символов
        
        [Header("Error Counter")]
        [SerializeField] private int errorCount = 0;
        [SerializeField] private int nthErrorTrigger = 3; // Каждый n-ый неправильный ответ
        
        [Header("Solution Events")]
        [SerializeField] private UnityEvent onCorrectSolution;
        [SerializeField] private UnityEvent onIncorrectSolution;
        [SerializeField] private UnityEvent onNthIncorrectSolution;
        
        [Header("Pipe Animation")]
        [SerializeField] private bool animatePipes = true;
        [SerializeField] private float animationDuration = 1.0f;
        
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable xrInteractable;

        public int ErrorCount => errorCount;

        private void Awake()
        {
            xrInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            
            if (xrInteractable != null)
            {
                xrInteractable.selectEntered.AddListener(OnInteractableSelected);
                Debug.Log("Type3SolutionChecker: XR Select listener added");
            }
            
            // Проверяем наличие MaterialAssigner'ов
            if (equalsAssigner == null)
            {
                Debug.LogWarning("Type3SolutionChecker: Equals MaterialAssigner is not assigned!");
            }
            
            if (notEqualsAssigner == null)
            {
                Debug.LogWarning("Type3SolutionChecker: Not Equals MaterialAssigner is not assigned!");
            }
        }

        private void OnDestroy()
        {
            if (xrInteractable != null)
            {
                xrInteractable.selectEntered.RemoveListener(OnInteractableSelected);
            }
        }

        private void OnInteractableSelected(SelectEnterEventArgs args)
        {
            CheckSolution();
        }

        // Альтернативный метод для тестирования мышью
        private void OnMouseDown()
        {
            Debug.Log("Type3SolutionChecker: Mouse click detected");
            CheckSolution();
        }

        public void CheckSolution()
        {
            if (taskLoader == null)
            {
                Debug.LogError("Type3SolutionChecker: Task loader is not assigned!");
                return;
            }

            ASTNode leftSideRoot = taskLoader.GetRootNode();
            answer = taskLoader.GetTemplate().Answer;
            
            if (leftSideRoot == null)
            {
                Debug.LogError("Type3SolutionChecker: Template or root node is not available!");
                return;
            }

            try
            {
                Trit leftResult = SolutionAST.Solution(leftSideRoot);
                
                Debug.Log($"Type3SolutionChecker: Left side = {leftResult}, Answer = {answer}");
                bool isCorrect = leftResult == answer;
                
                if (isCorrect)
                {
                    Debug.Log("Type3SolutionChecker: Solution is CORRECT!");
                    ApplyCorrectVisuals();
                    onCorrectSolution?.Invoke();
                }
                else
                {
                    Debug.Log("Type3SolutionChecker: Solution is INCORRECT!");
                    errorCount++;
                    Debug.Log($"Type3SolutionChecker: Error count increased to {errorCount}");
                    ApplyIncorrectVisuals();
                    
                    // Вызываем событие неправильного ответа
                    onIncorrectSolution?.Invoke();
                    
                    // Проверяем, нужно ли вызвать событие n-го неправильного ответа
                    if (errorCount % nthErrorTrigger == 0)
                    {
                        Debug.Log($"Type3SolutionChecker: Triggering nth incorrect solution event (error #{errorCount})");
                        onNthIncorrectSolution?.Invoke();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Type3SolutionChecker: Error evaluating solution: {e.Message}");
                errorCount++;
                ApplyIncorrectVisuals();
                
                // Вызываем события при ошибке вычисления
                onIncorrectSolution?.Invoke();
                
                if (errorCount % nthErrorTrigger == 0)
                {
                    Debug.Log($"Type3SolutionChecker: Triggering nth incorrect solution event (error #{errorCount})");
                    onNthIncorrectSolution?.Invoke();
                }
            }
        }

        private void ApplyCorrectVisuals()
        {
            // Применяем материалы для символов равенства
            ApplyCorrectMaterial();
            
            // Применяем эффекты для труб
            ApplyPipeEffects(true);
        }

        private void ApplyIncorrectVisuals()
        {
            // Применяем материалы для символов неравенства
            ApplyIncorrectMaterial();
            
            // Применяем эффекты для труб
            ApplyPipeEffects(false);
        }

        private void ApplyCorrectMaterial()
        {
            // Способ 1: Через MaterialAssigner'ы
            if (equalsAssigner != null && notEqualsAssigner != null)
            {
                if (correctMaterial != null)
                {
                    equalsAssigner.CurrentMaterial = correctMaterial;
                    Debug.Log("Type3SolutionChecker: Applied correct material to equals symbol (=)");
                }
                
                // Делаем символ "/" прозрачным или невидимым
                if (transparentMaterial != null)
                {
                    notEqualsAssigner.CurrentMaterial = transparentMaterial;
                    Debug.Log("Type3SolutionChecker: Made slash symbol (/) transparent");
                }
            }
            // Способ 2: Прямое управление Renderer'ом
            else if (targetRenderer != null)
            {
                ApplyDirectRendererMaterials(true);
            }
        }

        private void ApplyIncorrectMaterial()
        {
            // Способ 1: Через MaterialAssigner'ы
            if (equalsAssigner != null && notEqualsAssigner != null)
            {
                // Для символа "не равно" оба символа "=" и "/" должны светиться красным
                if (incorrectMaterial != null)
                {
                    equalsAssigner.CurrentMaterial = incorrectMaterial;
                    notEqualsAssigner.CurrentMaterial = incorrectMaterial;
                    Debug.Log("Type3SolutionChecker: Applied incorrect material to both symbols (= and /) to create ≠");
                }
            }
            // Способ 2: Прямое управление Renderer'ом
            else if (targetRenderer != null)
            {
                ApplyDirectRendererMaterials(false);
            }
        }

        private void ApplyDirectRendererMaterials(bool isCorrect)
        {
            if (targetRenderer == null) return;

            Material[] materials = targetRenderer.materials;
            
            if (isCorrect)
            {
                // Правильный ответ: показать "=", скрыть "/"
                if (equalsSlotIndex < materials.Length && correctMaterial != null)
                {
                    materials[equalsSlotIndex] = correctMaterial;
                }
                if (slashSlotIndex < materials.Length)
                {
                    materials[slashSlotIndex] = transparentMaterial;
                }
                Debug.Log("Type3SolutionChecker: Applied correct materials directly to renderer");
            }
            else
            {
                // Неправильный ответ: оба символа "=" и "/" светятся красным для создания "≠"
                if (equalsSlotIndex < materials.Length && incorrectMaterial != null)
                {
                    materials[equalsSlotIndex] = incorrectMaterial;
                }
                if (slashSlotIndex < materials.Length && incorrectMaterial != null)
                {
                    materials[slashSlotIndex] = incorrectMaterial;
                }
                Debug.Log("Type3SolutionChecker: Applied incorrect materials to both symbols (= and /) directly to renderer");
            }
            
            targetRenderer.materials = materials;
        }

        private void ApplyPipeEffects(bool isCorrect)
        {
            // Приоритет: используем Type3PipeManager если доступен
            if (pipeManager != null && taskLoader != null && taskLoader.GetTemplate() != null)
            {
                if (animatePipes)
                {
                    pipeManager.AnimateDataFlow(taskLoader.GetTemplate(), isCorrect);
                }
                else
                {
                    pipeManager.ApplyResultMaterial(isCorrect);
                }
                Debug.Log($"Type3SolutionChecker: Applied pipe effects via PipeManager - Correct: {isCorrect}");
                return;
            }

            // Fallback: используем прямое управление рендерерами
            if (pipeRenderers == null || pipeRenderers.Length == 0) return;

            Material targetMaterial = isCorrect ? pipeCorrectMaterial : pipeIncorrectMaterial;
            
            if (targetMaterial == null)
            {
                Debug.LogWarning("Type3SolutionChecker: Target pipe material is not assigned!");
                return;
            }

            if (animatePipes)
            {
                StartCoroutine(AnimatePipeEffects(targetMaterial));
            }
            else
            {
                ApplyPipeMaterialsImmediate(targetMaterial);
            }
        }

        private void ApplyPipeMaterialsImmediate(Material material)
        {
            foreach (var pipeRenderer in pipeRenderers)
            {
                if (pipeRenderer != null)
                {
                    pipeRenderer.material = material;
                }
            }
            
            Debug.Log($"Type3SolutionChecker: Applied pipe material {material.name} to {pipeRenderers.Length} pipes");
        }

        private System.Collections.IEnumerator AnimatePipeEffects(Material targetMaterial)
        {
            // Анимация: последовательно применяем материал к трубам
            float delayBetweenPipes = animationDuration / pipeRenderers.Length;
            
            for (int i = 0; i < pipeRenderers.Length; i++)
            {
                if (pipeRenderers[i] != null)
                {
                    pipeRenderers[i].material = targetMaterial;
                    yield return new WaitForSeconds(delayBetweenPipes);
                }
            }
            
            Debug.Log($"Type3SolutionChecker: Animated pipe effects with {targetMaterial.name}");
        }

        public void ResetPipeEffects()
        {
            // Приоритет: используем Type3PipeManager если доступен
            if (pipeManager != null)
            {
                pipeManager.ResetAllPipes();
                Debug.Log("Type3SolutionChecker: Reset pipe effects via PipeManager");
                return;
            }

            // Fallback: прямое управление рендерерами
            if (pipeNeutralMaterial != null)
            {
                ApplyPipeMaterialsImmediate(pipeNeutralMaterial);
            }
        }

        // Публичные методы для настройки из других скриптов
        public void SetTaskLoader(Type3TaskLoader loader)
        {
            taskLoader = loader;
        }

        public void SetAnswer(Trit answerValue)
        {
            answer = answerValue;
        }

        public void ResetErrorCount()
        {
            errorCount = 0;
            Debug.Log("Type3SolutionChecker: Error count reset to 0");
        }

        public void SetPipeRenderers(Renderer[] renderers)
        {
            pipeRenderers = renderers;
        }

        public void SetPipeManager(Type3PipeManager manager)
        {
            pipeManager = manager;
        }

        // Методы для ручной проверки в инспекторе
        [ContextMenu("Test Solution")]
        public void TestSolution()
        {
            CheckSolution();
        }

        [ContextMenu("Reset Pipe Effects")]
        public void TestResetPipeEffects()
        {
            ResetPipeEffects();
        }

        [ContextMenu("Test Pipe Animation")]
        public void TestPipeAnimation()
        {
            if (pipeCorrectMaterial != null)
            {
                StartCoroutine(AnimatePipeEffects(pipeCorrectMaterial));
            }
        }
    }
}