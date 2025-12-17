using Expedition0.Health;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Expedition0.Combat
{
    public class BlasterController : MonoBehaviour
    {
        [Header("Shoot Settings")]
        [SerializeField] private Transform shootPoint;
        [SerializeField] private Material beamMaterial;
        [SerializeField] private float beamLength = 10f;
        [SerializeField] private float beamWidth = 0.1f;
        [SerializeField] private float beamDuration = 0.1f;

        [Header("Damage Settings")]
        [SerializeField] private float damage = 1f;

        [Header("Layer Detection")]
        [SerializeField] private LayerMask hitLayers;

        [Header("Controller Settings")]
        [SerializeField] private Transform rightHandController; // Правый контроллер
        [SerializeField] private Vector3 equippedOffset = Vector3.zero; // Смещение когда в руке
        [SerializeField] private Vector3 equippedRotation = Vector3.zero; // Поворот когда в руке

        [Header("Input Settings")]
        [Tooltip("Действие для переключения бластера (например A Button)")]
        [SerializeField] private InputActionProperty toggleBlasterAction;
        [Tooltip("Действие для стрельбы (например Trigger или Grip)")]
        [SerializeField] private InputActionProperty shootAction;

        [Header("Events")]
        [Tooltip("Вызывается в момент выстрела")]
        public UnityEvent onShoot = new UnityEvent();
        [Tooltip("Вызывается когда бластер достали")]
        public UnityEvent onEquipped = new UnityEvent();
        [Tooltip("Вызывается когда бластер убрали")]
        public UnityEvent onHolstered = new UnityEvent();

        private XRGrabInteractable grabInteractable;
        private bool isPickedUp = false; // Был ли бластер взят хоть раз
        private bool isEquipped = false; // Достан ли сейчас
        private Rigidbody rb;
        private Collider[] colliders;

        void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();
        }

        void OnEnable()
        {
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.AddListener(OnGrabbed);
            }

            // Подписываемся на действие переключения
            if (toggleBlasterAction.action != null)
            {
                toggleBlasterAction.action.Enable();
                toggleBlasterAction.action.performed += OnToggleButtonPressed;
            }

            // Подписываемся на действие стрельбы
            if (shootAction.action != null)
            {
                shootAction.action.Enable();
                shootAction.action.performed += OnShootButtonPressed;
            }
        }

        void OnDisable()
        {
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            }

            // Отписываемся от действия переключения
            if (toggleBlasterAction.action != null)
            {
                toggleBlasterAction.action.performed -= OnToggleButtonPressed;
                toggleBlasterAction.action.Disable();
            }

            // Отписываемся от действия стрельбы
            if (shootAction.action != null)
            {
                shootAction.action.performed -= OnShootButtonPressed;
                shootAction.action.Disable();
            }
        }

        void Update()
        {
            // Если бластер привязан к контроллеру, обновляем его позицию
            if (isPickedUp && rightHandController != null && isEquipped)
            {
                UpdateBlasterPosition();
            }
        }

        private void OnToggleButtonPressed(InputAction.CallbackContext context)
        {
            // Переключаем бластер только если он уже был подобран
            if (isPickedUp)
            {
                ToggleBlaster();
            }
        }

        private void OnShootButtonPressed(InputAction.CallbackContext context)
        {
            // Стреляем только если бластер подобран и достан
            if (isPickedUp && isEquipped)
            {
                Shoot();
            }
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            // Первое взятие бластера
            if (!isPickedUp)
            {
                isPickedUp = true;
                isEquipped = true;

                // Отключаем XRGrabInteractable, теперь он управляется скриптом
                if (grabInteractable != null)
                {
                    grabInteractable.enabled = false;
                }

                // Делаем бластер ребенком контроллера
                if (rightHandController != null)
                {
                    transform.SetParent(rightHandController);
                }

                // Отключаем физику
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                SetEquippedState(true);
                onEquipped?.Invoke();

                Debug.Log("Blaster picked up for the first time");
            }
        }

        private void ToggleBlaster()
        {
            isEquipped = !isEquipped;

            if (isEquipped)
            {
                SetEquippedState(true);
                onEquipped?.Invoke();
                Debug.Log("Blaster equipped");
            }
            else
            {
                SetEquippedState(false);
                onHolstered?.Invoke();
                Debug.Log("Blaster holstered");
            }
        }

        private void SetEquippedState(bool equipped)
        {
            // Включаем/выключаем коллайдеры
            foreach (var col in colliders)
            {
                col.enabled = equipped;
            }

            // Включаем/выключаем MeshRenderer (делаем видимым/невидимым)
            var renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = equipped;
            }
        }

        private void UpdateBlasterPosition()
        {
            // Позиция когда бластер в руке (достан и видим)
            transform.localPosition = equippedOffset;
            transform.localRotation = Quaternion.Euler(equippedRotation);
        }

        public void Shoot()
        {
            if (!isEquipped)
            {
                Debug.LogWarning("Cannot shoot while blaster is holstered!");
                return;
            }

            if (shootPoint == null || beamMaterial == null)
            {
                Debug.LogWarning("ShootPoint or BeamMaterial is not assigned!");
                return;
            }

            onShoot?.Invoke();

            // Создаём луч
            GameObject beam = new GameObject("Beam");
            beam.transform.position = shootPoint.position;
            beam.transform.rotation = shootPoint.rotation;

            // Добавляем LineRenderer
            LineRenderer lineRenderer = beam.AddComponent<LineRenderer>();
            lineRenderer.material = beamMaterial;
            lineRenderer.startWidth = beamWidth;
            lineRenderer.endWidth = beamWidth;
            lineRenderer.positionCount = 2;

            // Raycast для проверки попаданий
            RaycastHit hit;
            Vector3 endPoint;

            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, beamLength, hitLayers))
            {
                endPoint = hit.point;

                // Пытаемся нанести урон через интерфейс
                IDamageable damageable = hit.collider?.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Debug.Log($"Damageable takes {damage} damage");
                    damageable.TakeDamage(damage);
                }

                // Проверяем слой для логов
                if (hit.collider?.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    Debug.Log($"Hit Player: {hit.collider.gameObject.name}");
                }
                else if (hit.collider?.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    Debug.Log($"Hit Enemy: {hit.collider.gameObject.name}");
                }
            }
            else
            {
                endPoint = shootPoint.position + shootPoint.forward * beamLength;
            }

            // Устанавливаем точки луча
            lineRenderer.SetPosition(0, shootPoint.position);
            lineRenderer.SetPosition(1, endPoint);

            // Уничтожаем луч через заданное время
            Destroy(beam, beamDuration);
        }

        // Публичные методы для внешнего управления
        public void ForceEquip()
        {
            if (isPickedUp && !isEquipped)
            {
                ToggleBlaster();
            }
        }

        public void ForceHolster()
        {
            if (isPickedUp && isEquipped)
            {
                ToggleBlaster();
            }
        }

        public bool IsEquipped()
        {
            return isEquipped;
        }

        public bool IsPickedUp()
        {
            return isPickedUp;
        }
    }
}