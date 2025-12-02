using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Expedition0.Tasks
{
    /// <summary>
    /// Отображение слота значения для заданий 3-го типа с поддержкой префабов объектов
    /// Вместо спрайтов использует 3D объекты для троичных значений
    /// </summary>
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
    public class Type3ValueSlotView : MonoBehaviour
    {
        [Header("Value Prefabs")]
        [SerializeField] private GameObject value0Prefab; // Префаб для False (0)
        [SerializeField] private GameObject value1Prefab; // Префаб для Neutral (1)
        [SerializeField] private GameObject value2Prefab; // Префаб для True (2)

        [Header("Spawn Settings")]
        [SerializeField] private Transform valueSpawnPoint; // Точка спавна значений
        [SerializeField] private bool destroyPreviousValue = true; // Уничтожать предыдущее значение
        [SerializeField] private Vector3 valueScale = Vector3.one; // Масштаб значений
        [SerializeField] private Vector3 valueRotation = Vector3.zero; // Поворот значений

        [Header("Interaction")]
        [SerializeField] private Collider interactableCollider;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material lockedMaterial; // Материал для заблокированных значений
        [SerializeField] private Material unlockedMaterial; // Материал для разблокированных значений
        [SerializeField] private Renderer[] feedbackRenderers; // Рендереры для обратной связи

        [Header("Animation")]
        [SerializeField] private bool animateValueChange = true; // Анимировать смену значений
        [SerializeField] private float animationDuration = 0.3f; // Длительность анимации
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Кривая анимации

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable xrInteractable;
        private ValueSlotNode boundNode;
        private GameObject currentValueObject; // Текущий объект значения

        public Trit? CurrentValue { get; private set; }
        public bool IsLocked { get; private set; }

        private void Awake()
        {
            xrInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            Debug.Log($"Type3ValueSlotView Awake: XRInteractable found = {xrInteractable != null}");
            
            if (xrInteractable != null)
            {
                xrInteractable.selectEntered.AddListener(OnInteractableSelected);
                xrInteractable.selectExited.AddListener(OnSelectExited);
                xrInteractable.activated.AddListener(OnActivated);
                xrInteractable.hoverEntered.AddListener(OnHoverEntered);
                xrInteractable.hoverExited.AddListener(OnHoverExited);
                Debug.Log("Type3ValueSlotView: XR listeners added");
            }

            // Если точка спавна не задана, используем текущий объект
            if (valueSpawnPoint == null)
            {
                valueSpawnPoint = transform;
            }
        }

        private void OnDestroy()
        {
            if (xrInteractable != null)
            {
                xrInteractable.selectEntered.RemoveListener(OnInteractableSelected);
                xrInteractable.selectExited.RemoveListener(OnSelectExited);
                xrInteractable.activated.RemoveListener(OnActivated);
                xrInteractable.hoverEntered.RemoveListener(OnHoverEntered);
                xrInteractable.hoverExited.RemoveListener(OnHoverExited);
            }

            // Уничтожаем текущий объект значения
            if (currentValueObject != null)
            {
                DestroyImmediate(currentValueObject);
            }
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            Debug.Log($"Type3ValueSlotView: XR Hover ENTERED by: {args.interactorObject.transform.name}");
            ApplyHoverEffect(true);
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            Debug.Log($"Type3ValueSlotView: XR Hover EXITED by: {args.interactorObject.transform.name}");
            ApplyHoverEffect(false);
        }

        private void OnInteractableSelected(SelectEnterEventArgs args)
        {
            Debug.Log($"Type3ValueSlotView: XR SELECT ENTERED by: {args.interactorObject.transform.name}");
            OnClick();
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            Debug.Log($"Type3ValueSlotView: XR SELECT EXITED by: {args.interactorObject.transform.name}");
        }

        private void OnActivated(ActivateEventArgs args)
        {
            Debug.Log($"Type3ValueSlotView: XR ACTIVATED by: {args.interactorObject.transform.name}");
            OnClick();
        }

        // Поддержка клика мышью для тестирования
        private void OnMouseDown()
        {
            Debug.Log("Type3ValueSlotView: Mouse click detected");
            OnClick();
        }

        public void OnClick()
        {
            Debug.Log($"Type3ValueSlotView: OnClick called - IsLocked: {IsLocked}");
            if (!IsLocked)
            {
                CycleValue();
            }
            else
            {
                Debug.Log("Type3ValueSlotView: Click ignored - slot is locked");
            }
        }

        public void BindNode(ValueSlotNode node)
        {
            if (node == null)
            {
                Debug.LogError("Type3ValueSlotView: Trying to bind null node!");
                return;
            }
            
            boundNode = node;
            Debug.Log($"Type3ValueSlotView: Successfully bound to AST node with value {node.CurrentValue}, locked: {node.IsLocked}");
            ApplyValue(node.CurrentValue, node.IsLocked);
        }

        public void ApplyValue(Trit? value, bool isLocked)
        {
            CurrentValue = value;
            IsLocked = isLocked;
            UpdateVisuals();
            UpdateInteractable();
            UpdateFeedback();
        }

        private void CycleValue()
        {
            Debug.Log("Type3ValueSlotView: Cycling value");
            if (IsLocked) 
            {
                Debug.Log("Type3ValueSlotView: Cannot cycle - value is locked");
                return;
            }

            Trit? previousValue = CurrentValue;

            if (!CurrentValue.HasValue)
            {
                CurrentValue = Trit.False;
            }
            else
            {
                int currentInt = CurrentValue.Value.ToInt();
                int nextInt = (currentInt + 1) % 3;
                CurrentValue = TritLogic.FromInt(nextInt);
            }

            Debug.Log($"Type3ValueSlotView: Changed value from {previousValue} to {CurrentValue}");

            // Обновляем AST узел
            if (boundNode != null && CurrentValue.HasValue)
            {
                try
                {
                    boundNode.SetValue(CurrentValue.Value);
                    Debug.Log($"Type3ValueSlotView: Successfully updated AST node with value {CurrentValue.Value}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Type3ValueSlotView: Failed to update AST node: {e.Message}");
                    // Возвращаем предыдущее значение при ошибке
                    CurrentValue = previousValue;
                }
            }
            else
            {
                if (boundNode == null)
                    Debug.LogWarning("Type3ValueSlotView: boundNode is null - AST not updated!");
                if (!CurrentValue.HasValue)
                    Debug.LogWarning("Type3ValueSlotView: CurrentValue is null - AST not updated!");
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (animateValueChange && currentValueObject != null)
            {
                StartCoroutine(AnimateValueChange());
            }
            else
            {
                UpdateVisualsImmediate();
            }
        }

        private void UpdateVisualsImmediate()
        {
            // Уничтожаем предыдущий объект значения
            if (currentValueObject != null && destroyPreviousValue)
            {
                DestroyImmediate(currentValueObject);
                currentValueObject = null;
            }

            if (!CurrentValue.HasValue)
            {
                return;
            }

            // Получаем префаб для текущего значения
            GameObject prefabToSpawn = GetValuePrefab(CurrentValue.Value);
            
            if (prefabToSpawn != null)
            {
                // Создаем новый объект значения
                currentValueObject = Instantiate(prefabToSpawn, valueSpawnPoint);
                
                // Настраиваем трансформ
                currentValueObject.transform.localPosition = Vector3.zero;
                currentValueObject.transform.localRotation = Quaternion.Euler(valueRotation);
                currentValueObject.transform.localScale = valueScale;
                
                Debug.Log($"Type3ValueSlotView: Spawned value object for {CurrentValue.Value} ({CurrentValue.Value.ToInt()})");
            }
            else
            {
                Debug.LogWarning($"Type3ValueSlotView: No prefab found for value {CurrentValue.Value}");
            }
        }

        private System.Collections.IEnumerator AnimateValueChange()
        {
            GameObject oldObject = currentValueObject;
            
            // Создаем новый объект
            if (CurrentValue.HasValue)
            {
                GameObject prefabToSpawn = GetValuePrefab(CurrentValue.Value);
                if (prefabToSpawn != null)
                {
                    currentValueObject = Instantiate(prefabToSpawn, valueSpawnPoint);
                    currentValueObject.transform.localPosition = Vector3.zero;
                    currentValueObject.transform.localRotation = Quaternion.Euler(valueRotation);
                    currentValueObject.transform.localScale = Vector3.zero; // Начинаем с нулевого масштаба
                }
            }

            float elapsedTime = 0f;
            
            while (elapsedTime < animationDuration)
            {
                float t = elapsedTime / animationDuration;
                float curveValue = scaleCurve.Evaluate(t);
                
                // Уменьшаем старый объект
                if (oldObject != null)
                {
                    oldObject.transform.localScale = valueScale * (1f - curveValue);
                }
                
                // Увеличиваем новый объект
                if (currentValueObject != null)
                {
                    currentValueObject.transform.localScale = valueScale * curveValue;
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Завершаем анимацию
            if (oldObject != null)
            {
                DestroyImmediate(oldObject);
            }
            
            if (currentValueObject != null)
            {
                currentValueObject.transform.localScale = valueScale;
            }
        }

        private GameObject GetValuePrefab(Trit value)
        {
            switch (value.ToInt())
            {
                case 0: return value0Prefab; // False
                case 1: return value1Prefab; // Neutral
                case 2: return value2Prefab; // True
                default: return null;
            }
        }

        private void UpdateInteractable()
        {
            if (interactableCollider != null) 
            {
                interactableCollider.enabled = !IsLocked;
            }
            
            if (xrInteractable != null)
            {
                xrInteractable.enabled = !IsLocked;
            }
        }

        private void UpdateFeedback()
        {
            Material targetMaterial = IsLocked ? lockedMaterial : unlockedMaterial;
            
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

        private void ApplyHoverEffect(bool isHovering)
        {
            if (IsLocked) return;
            
            // Эффекты при наведении
            if (currentValueObject != null)
            {
                // Изменение масштаба при наведении
                float scaleMultiplier = isHovering ? 1.1f : 1.0f;
                currentValueObject.transform.localScale = valueScale * scaleMultiplier;
            }
        }

        // Публичные методы для управления
        public void SetValuePrefabs(GameObject value0, GameObject value1, GameObject value2)
        {
            value0Prefab = value0;
            value1Prefab = value1;
            value2Prefab = value2;
        }

        public GameObject GetCurrentValueObject()
        {
            return currentValueObject;
        }

        /// <summary>
        /// Проверяет синхронизацию между UI и AST узлом
        /// </summary>
        public bool IsInSync()
        {
            if (boundNode == null) return false;
            return CurrentValue == boundNode.CurrentValue && IsLocked == boundNode.IsLocked;
        }

        /// <summary>
        /// Принудительно синхронизирует UI с AST узлом
        /// </summary>
        public void SyncWithAST()
        {
            if (boundNode != null)
            {
                ApplyValue(boundNode.CurrentValue, boundNode.IsLocked);
                Debug.Log($"Type3ValueSlotView: Synced with AST - Value: {boundNode.CurrentValue}, Locked: {boundNode.IsLocked}");
            }
        }

        // Методы для тестирования в инспекторе
        [ContextMenu("Test Cycle Value")]
        public void TestCycleValue()
        {
            CycleValue();
        }

        [ContextMenu("Test Set False (0)")]
        public void TestSetFalse()
        {
            ApplyValue(Trit.False, false);
        }

        [ContextMenu("Test Set Neutral (1)")]
        public void TestSetNeutral()
        {
            ApplyValue(Trit.Neutral, false);
        }

        [ContextMenu("Test Set True (2)")]
        public void TestSetTrue()
        {
            ApplyValue(Trit.True, false);
        }

        [ContextMenu("Check AST Sync")]
        public void TestCheckSync()
        {
            bool inSync = IsInSync();
            Debug.Log($"Type3ValueSlotView: AST Sync Status: {inSync}");
            if (boundNode != null)
            {
                Debug.Log($"  UI: Value={CurrentValue}, Locked={IsLocked}");
                Debug.Log($"  AST: Value={boundNode.CurrentValue}, Locked={boundNode.IsLocked}");
            }
            else
            {
                Debug.Log("  boundNode is null!");
            }
        }

        [ContextMenu("Force Sync with AST")]
        public void TestSyncWithAST()
        {
            SyncWithAST();
        }
    }
}