using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Expedition0.Tasks
{
    /// <summary>
    /// Отображение слота оператора для заданий 3-го типа с поддержкой префабов объектов
    /// Вместо спрайтов использует 3D объекты операторов
    /// </summary>
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
    public class Type3OperatorSlotView : MonoBehaviour
    {
        [Header("Operator Prefabs")]
        [SerializeField] private GameObject notPrefab;
        [SerializeField] private GameObject andPrefab;
        [SerializeField] private GameObject orPrefab;
        [SerializeField] private GameObject xorPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Transform operatorSpawnPoint; // Точка спавна операторов
        [SerializeField] private bool destroyPreviousOperator = true; // Уничтожать предыдущий оператор
        [SerializeField] private Vector3 operatorScale = Vector3.one; // Масштаб операторов
        [SerializeField] private Vector3 operatorRotation = Vector3.zero; // Поворот операторов

        [Header("Interaction")]
        [SerializeField] private Collider interactableCollider;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material lockedMaterial; // Материал для заблокированных операторов
        [SerializeField] private Material unlockedMaterial; // Материал для разблокированных операторов
        [SerializeField] private Renderer[] feedbackRenderers; // Рендереры для обратной связи

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable xrInteractable;
        private OperatorSlotNode boundNode;
        private GameObject currentOperatorObject; // Текущий объект оператора

        public Operator? CurrentOperator { get; private set; }
        public bool IsLocked { get; private set; }

        private void Awake()
        {
            xrInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            Debug.Log($"Type3OperatorSlotView Awake: XRInteractable found = {xrInteractable != null}");
            
            if (xrInteractable != null)
            {
                xrInteractable.selectEntered.AddListener(OnInteractableSelected);
                xrInteractable.selectExited.AddListener(OnSelectExited);
                xrInteractable.activated.AddListener(OnActivated);
                xrInteractable.hoverEntered.AddListener(OnHoverEntered);
                xrInteractable.hoverExited.AddListener(OnHoverExited);
                Debug.Log("Type3OperatorSlotView: XR listeners added");
            }

            // Если точка спавна не задана, используем текущий объект
            if (operatorSpawnPoint == null)
            {
                operatorSpawnPoint = transform;
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

            // Уничтожаем текущий объект оператора
            if (currentOperatorObject != null)
            {
                DestroyImmediate(currentOperatorObject);
            }
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            Debug.Log($"Type3OperatorSlotView: XR Hover ENTERED by: {args.interactorObject.transform.name}");
            ApplyHoverEffect(true);
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            Debug.Log($"Type3OperatorSlotView: XR Hover EXITED by: {args.interactorObject.transform.name}");
            ApplyHoverEffect(false);
        }

        private void OnInteractableSelected(SelectEnterEventArgs args)
        {
            Debug.Log($"Type3OperatorSlotView: XR SELECT ENTERED by: {args.interactorObject.transform.name}");
            OnClick();
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            Debug.Log($"Type3OperatorSlotView: XR SELECT EXITED by: {args.interactorObject.transform.name}");
        }

        private void OnActivated(ActivateEventArgs args)
        {
            Debug.Log($"Type3OperatorSlotView: XR ACTIVATED by: {args.interactorObject.transform.name}");
            OnClick();
        }

        // Поддержка клика мышью для тестирования
        private void OnMouseDown()
        {
            Debug.Log("Type3OperatorSlotView: Mouse click detected");
            OnClick();
        }

        public void OnClick()
        {
            Debug.Log($"Type3OperatorSlotView: OnClick called - IsLocked: {IsLocked}");
            if (!IsLocked)
            {
                CycleOperator();
            }
            else
            {
                Debug.Log("Type3OperatorSlotView: Click ignored - slot is locked");
            }
        }

        public void BindNode(OperatorSlotNode node)
        {
            if (node == null)
            {
                Debug.LogError("Type3OperatorSlotView: Trying to bind null node!");
                return;
            }
            
            boundNode = node;
            Debug.Log($"Type3OperatorSlotView: Successfully bound to AST node with operator {node.CurrentOperator}, locked: {node.IsLocked}");
            ApplyOperator(node.CurrentOperator, node.IsLocked);
        }

        public void ApplyOperator(Operator? op, bool isLocked)
        {
            CurrentOperator = op;
            IsLocked = isLocked;
            UpdateVisuals();
            UpdateInteractable();
            UpdateFeedback();
        }

        private void CycleOperator()
        {
            Debug.Log("Type3OperatorSlotView: Cycling operator");
            if (IsLocked) 
            {
                Debug.Log("Type3OperatorSlotView: Cannot cycle - operator is locked");
                return;
            }

            // Определяем доступные операторы для переключения (включая новые операторы для 3-го типа)
            Operator[] availableOperators = { 
                Operator.NOT, 
                Operator.AND, 
                Operator.OR, 
                Operator.XOR
            };
            
            Operator previousOperator = CurrentOperator ?? Operator.NOT;
            
            if (!CurrentOperator.HasValue)
            {
                CurrentOperator = availableOperators[0];
            }
            else
            {
                // Находим текущий индекс и переходим к следующему
                int currentIndex = System.Array.IndexOf(availableOperators, CurrentOperator.Value);
                int nextIndex = (currentIndex + 1) % availableOperators.Length;
                CurrentOperator = availableOperators[nextIndex];
            }

            Debug.Log($"Type3OperatorSlotView: Changed operator from {previousOperator} to {CurrentOperator}");

            // Обновляем AST узел
            if (boundNode != null && CurrentOperator.HasValue)
            {
                try
                {
                    boundNode.SetOperator(CurrentOperator.Value);
                    Debug.Log($"Type3OperatorSlotView: Successfully updated AST node with operator {CurrentOperator.Value}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Type3OperatorSlotView: Failed to update AST node: {e.Message}");
                    // Возвращаем предыдущее значение при ошибке
                    CurrentOperator = previousOperator;
                }
            }
            else
            {
                if (boundNode == null)
                    Debug.LogWarning("Type3OperatorSlotView: boundNode is null - AST not updated!");
                if (!CurrentOperator.HasValue)
                    Debug.LogWarning("Type3OperatorSlotView: CurrentOperator is null - AST not updated!");
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // Уничтожаем предыдущий объект оператора
            if (currentOperatorObject != null && destroyPreviousOperator)
            {
                DestroyImmediate(currentOperatorObject);
                currentOperatorObject = null;
            }

            if (!CurrentOperator.HasValue)
            {
                return;
            }

            // Получаем префаб для текущего оператора
            GameObject prefabToSpawn = GetOperatorPrefab(CurrentOperator.Value);
            
            if (prefabToSpawn != null)
            {
                // Создаем новый объект оператора
                currentOperatorObject = Instantiate(prefabToSpawn, operatorSpawnPoint);
                
                // Настраиваем трансформ
                currentOperatorObject.transform.localPosition = Vector3.zero;
                currentOperatorObject.transform.localRotation = Quaternion.Euler(operatorRotation);
                currentOperatorObject.transform.localScale = operatorScale;
                
                Debug.Log($"Type3OperatorSlotView: Spawned operator object for {CurrentOperator.Value}");
            }
            else
            {
                Debug.LogWarning($"Type3OperatorSlotView: No prefab found for operator {CurrentOperator.Value}");
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
            
            // Можно добавить эффекты при наведении
            if (currentOperatorObject != null)
            {
                // Например, изменение масштаба или свечения
                float scaleMultiplier = isHovering ? 1.1f : 1.0f;
                currentOperatorObject.transform.localScale = operatorScale * scaleMultiplier;
            }
        }

        // Публичные методы для управления
        public void SetOperatorPrefabs(
            GameObject not, GameObject and, GameObject or, GameObject xor)
        {
            notPrefab = not;
            andPrefab = and;
            orPrefab = or;
            xorPrefab = xor;
        }

        public GameObject GetCurrentOperatorObject()
        {
            return currentOperatorObject;
        }

        /// <summary>
        /// Проверяет синхронизацию между UI и AST узлом
        /// </summary>
        public bool IsInSync()
        {
            if (boundNode == null) return false;
            return CurrentOperator == boundNode.CurrentOperator && IsLocked == boundNode.IsLocked;
        }

        /// <summary>
        /// Принудительно синхронизирует UI с AST узлом
        /// </summary>
        public void SyncWithAST()
        {
            if (boundNode != null)
            {
                ApplyOperator(boundNode.CurrentOperator, boundNode.IsLocked);
                Debug.Log($"Type3OperatorSlotView: Synced with AST - Operator: {boundNode.CurrentOperator}, Locked: {boundNode.IsLocked}");
            }
        }

        // Методы для тестирования в инспекторе
        [ContextMenu("Test Cycle Operator")]
        public void TestCycleOperator()
        {
            CycleOperator();
        }

        [ContextMenu("Test Set NOT")]
        public void TestSetNOT()
        {
            ApplyOperator(Operator.NOT, false);
        }

        [ContextMenu("Test Set AND")]
        public void TestSetAND()
        {
            ApplyOperator(Operator.AND, false);
        }

        [ContextMenu("Test Set XOR")]
        public void TestSetXOR()
        {
            ApplyOperator(Operator.XOR, false);
        }

        [ContextMenu("Check AST Sync")]
        public void TestCheckSync()
        {
            bool inSync = IsInSync();
            Debug.Log($"Type3OperatorSlotView: AST Sync Status: {inSync}");
            if (boundNode != null)
            {
                Debug.Log($"  UI: Operator={CurrentOperator}, Locked={IsLocked}");
                Debug.Log($"  AST: Operator={boundNode.CurrentOperator}, Locked={boundNode.IsLocked}");
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