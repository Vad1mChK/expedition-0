using UnityEngine;

namespace Expedition0.Tasks
{
    /// <summary>
    /// Компонент для отображения результата левой части уравнения в реальном времени
    /// Автоматически обновляется при изменении значений или операторов
    /// </summary>
    public class Type3ResultDisplay : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool animateResultChange = true;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Result Display")]
        [SerializeField] private UnityEngine.UI.Image resultImage; // UI Image для отображения результата
        
        [Header("Result Sprites")]
        [SerializeField] private Sprite result0Sprite; // Спрайт для False (0)
        [SerializeField] private Sprite result1Sprite; // Спрайт для Neutral (1)
        [SerializeField] private Sprite result2Sprite; // Спрайт для True (2)
        [SerializeField] private Sprite errorSprite; // Спрайт для ошибки вычисления
        

        
        [Header("Visual Feedback")]
        [SerializeField] private Material validResultMaterial; // Материал для валидного результата
        [SerializeField] private Material invalidResultMaterial; // Материал для невалидного результата
        [SerializeField] private Renderer[] feedbackRenderers; // Рендереры для обратной связи
        
        private Type3TaskLoader taskLoader;
        private Trit? currentResult;
        private bool isResultValid;

        public Trit? CurrentResult => currentResult;
        public bool IsResultValid => isResultValid;

        private void Awake()
        {
            // Инициализация компонента
        }

        private void Start()
        {
            // Находим TaskLoader в сцене
            if (taskLoader == null)
            {
                taskLoader = FindObjectOfType<Type3TaskLoader>();
            }
            
            // Подписываемся на изменения в слотах
            SubscribeToSlotChanges();
            
            // Обновляем результат при старте
            UpdateResult();
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            UnsubscribeFromSlotChanges();
        }

        public void SetTaskLoader(Type3TaskLoader loader)
        {
            if (taskLoader != null)
            {
                UnsubscribeFromSlotChanges();
            }
            
            taskLoader = loader;
            SubscribeToSlotChanges();
            UpdateResult();
        }

        private void SubscribeToSlotChanges()
        {
            if (taskLoader == null || taskLoader.GetTemplate() == null) return;
            
            // Подписываемся на изменения через периодическую проверку
            // Поскольку у нас нет событий в слотах, будем проверять изменения каждый кадр
            InvokeRepeating(nameof(CheckForChanges), 0.1f, 0.1f);
        }

        private void UnsubscribeFromSlotChanges()
        {
            CancelInvoke(nameof(CheckForChanges));
        }

        private void CheckForChanges()
        {
            if (taskLoader == null || taskLoader.GetTemplate() == null) return;
            
            // Вычисляем текущий результат
            Trit? newResult = CalculateCurrentResult();
            
            // Если результат изменился, обновляем отображение
            if (newResult != currentResult)
            {
                currentResult = newResult;
                UpdateResultDisplay();
            }
        }

        private Trit? CalculateCurrentResult()
        {
            if (taskLoader == null || taskLoader.GetTemplate() == null) return null;
            
            try
            {
                ASTNode rootNode = taskLoader.GetRootNode();
                if (rootNode == null) return null;
                
                Trit result = SolutionAST.Solution(rootNode);
                isResultValid = true;
                return result;
            }
            catch (System.Exception e)
            {
                // Если не удается вычислить (не все слоты заполнены), возвращаем null
                isResultValid = false;
                Debug.Log($"Type3ResultDisplay: Cannot calculate result - {e.Message}");
                return null;
            }
        }

        public void UpdateResult()
        {
            currentResult = CalculateCurrentResult();
            UpdateResultDisplay();
        }

        private void UpdateResultDisplay()
        {
            if (animateResultChange && resultImage != null)
            {
                StartCoroutine(AnimateResultChange());
            }
            else
            {
                UpdateResultImmediate();
            }
            
            UpdateFeedback();
        }

        private void UpdateResultImmediate()
        {
            // Обновляем UI Image со спрайтом
            if (resultImage != null)
            {
                Sprite spriteToShow = GetResultSprite();
                resultImage.sprite = spriteToShow;
                resultImage.enabled = spriteToShow != null;
                
                Debug.Log($"Type3ResultDisplay: Updated result display to {currentResult} (valid: {isResultValid})");
            }
            else
            {
                Debug.LogWarning("Type3ResultDisplay: Result Image is not assigned!");
            }
        }

        private System.Collections.IEnumerator AnimateResultChange()
        {
            if (resultImage == null) yield break;
            
            // Анимация смены спрайта через масштаб
            Vector3 originalScale = resultImage.transform.localScale;
            Sprite newSprite = GetResultSprite();
            
            float elapsedTime = 0f;
            
            // Фаза 1: уменьшаем масштаб до 0
            while (elapsedTime < animationDuration * 0.5f)
            {
                float t = elapsedTime / (animationDuration * 0.5f);
                float curveValue = scaleCurve.Evaluate(t);
                
                resultImage.transform.localScale = originalScale * (1f - curveValue);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Меняем спрайт в середине анимации
            resultImage.sprite = newSprite;
            resultImage.enabled = newSprite != null;
            
            elapsedTime = 0f;
            
            // Фаза 2: увеличиваем масштаб обратно до оригинального
            while (elapsedTime < animationDuration * 0.5f)
            {
                float t = elapsedTime / (animationDuration * 0.5f);
                float curveValue = scaleCurve.Evaluate(t);
                
                resultImage.transform.localScale = originalScale * curveValue;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Завершаем анимацию
            resultImage.transform.localScale = originalScale;
        }

        private Sprite GetResultSprite()
        {
            if (!currentResult.HasValue)
            {
                return isResultValid ? null : errorSprite;
            }
            
            switch (currentResult.Value.ToInt())
            {
                case 0: return result0Sprite; // False
                case 1: return result1Sprite; // Neutral
                case 2: return result2Sprite; // True
                default: return errorSprite;
            }
        }

        private void UpdateFeedback()
        {
            Material targetMaterial = isResultValid ? validResultMaterial : invalidResultMaterial;
            
            if (targetMaterial != null && feedbackRenderers != null)
            {
                foreach (var renderer in feedbackRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.material = targetMaterial;
                    }
                }
            }
        }

        // Публичные методы для настройки
        public void SetResultSprites(Sprite result0, Sprite result1, Sprite result2, Sprite error = null)
        {
            result0Sprite = result0;
            result1Sprite = result1;
            result2Sprite = result2;
            errorSprite = error;
        }

        public UnityEngine.UI.Image GetResultImage()
        {
            return resultImage;
        }

        public void ForceUpdateResult()
        {
            UpdateResult();
        }

        // Методы для тестирования в инспекторе
        [ContextMenu("Force Update Result")]
        public void TestForceUpdate()
        {
            ForceUpdateResult();
        }

        [ContextMenu("Test Show False Result")]
        public void TestShowFalse()
        {
            currentResult = Trit.False;
            isResultValid = true;
            UpdateResultDisplay();
        }

        [ContextMenu("Test Show Neutral Result")]
        public void TestShowNeutral()
        {
            currentResult = Trit.Neutral;
            isResultValid = true;
            UpdateResultDisplay();
        }

        [ContextMenu("Test Show True Result")]
        public void TestShowTrue()
        {
            currentResult = Trit.True;
            isResultValid = true;
            UpdateResultDisplay();
        }

        [ContextMenu("Test Show Error")]
        public void TestShowError()
        {
            currentResult = null;
            isResultValid = false;
            UpdateResultDisplay();
        }

        [ContextMenu("Print Current Result")]
        public void TestPrintResult()
        {
            Debug.Log($"Current Result: {currentResult}, Valid: {isResultValid}");
        }
    }
}