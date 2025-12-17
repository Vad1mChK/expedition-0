using Expedition0.Health;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Expedition0.Combat
{
    public class EnemyController : MonoBehaviour
    {
        [FormerlySerializedAs("player")]
        [Header("Target Settings")]
        [Tooltip("Если не указан, будет искать объект с тегом 'Player'")]
        [SerializeField] private Transform playerLegs;
        [Tooltip("Обычно это камера игрока")]
        [SerializeField] private Transform playerHead;
        

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float attackDistance = 8f;
        [SerializeField] private float minDistance = 6f;
        [Tooltip("Расстояние, ближе которого враг не подлетает")]

        [Header("Shoot Settings")]
        [SerializeField] private Transform shootPoint;
        [SerializeField] private Material beamMaterial;
        [SerializeField] private float beamLength = 15f;
        [SerializeField] private float beamWidth = 0.1f;
        [SerializeField] private float beamDuration = 0.1f;

        [Header("Attack Settings")]
        [SerializeField] private float damage = 1f;
        [SerializeField] private float shootCooldown = 1.5f;
        [Tooltip("Время между выстрелами")]

        [Header("Layer Detection")]
        [SerializeField] private LayerMask hitLayers;

        [Header("Events")]
        [Tooltip("Вызывается в момент выстрела")]
        public UnityEvent onShoot = new UnityEvent();
        [Tooltip("Вызывается при уничтожении врага")]
        public UnityEvent onDestroyed = new UnityEvent();

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 3f;
        [Tooltip("Скорость поворота к игроку")]
        [SerializeField] private Vector3 forwardOffset = Vector3.zero;
        [Tooltip("Коррекция направления (например, 0,180,0 если дрон смотрит назад)")]

        private float lastShootTime = -999f;
        private bool canShoot = true;

        void Start()
        {
            // Если игрок не назначен, ищем по тегу
            if (playerLegs == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerLegs = playerObj.transform;
                }
                else
                {
                    Debug.LogError("Player not found! Please assign player or add 'Player' tag");
                }
            }

            if (playerHead == null)
            {
                playerHead = playerLegs;
            }

            // Если точка выстрела не назначена, используем позицию врага
            if (shootPoint == null)
            {
                shootPoint = transform;
            }
        }

        void Update()
        {
            if (playerLegs == null || playerHead == null) return;

            Vector3 targetPoint = Vector3.Lerp(playerLegs.position, playerHead.position, Random.value);

            float distanceToPlayer = Vector3.Distance(transform.position, targetPoint);

            // Движение к игроку или от него
            MoveTowardsPlayer(distanceToPlayer);

            // Поворот к игроку
            RotateTowardsPlayer();

            // Стрельба если в зоне атаки
            if (distanceToPlayer <= attackDistance && distanceToPlayer >= minDistance)
            {
                TryShoot(targetPoint);
            }
        }

        private void MoveTowardsPlayer(float distance)
        {
            Vector3 direction = (playerLegs.position - transform.position).normalized;

            // Если слишком далеко - двигаемся ближе
            if (distance > attackDistance)
            {
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
            // Если слишком близко - отлетаем назад
            else if (distance < minDistance)
            {
                transform.position -= direction * moveSpeed * Time.deltaTime;
            }
            // Если в зоне атаки - можем медленно корректировать позицию
            else
            {
                // Держимся на оптимальной дистанции (середина между min и attack)
                float optimalDistance = (minDistance + attackDistance) / 2f;
                if (Mathf.Abs(distance - optimalDistance) > 0.5f)
                {
                    float correction = distance > optimalDistance ? -1f : 1f;
                    transform.position += direction * correction * moveSpeed * 0.3f * Time.deltaTime;
                }
            }
        }

        private void RotateTowardsPlayer()
        {
            Vector3 directionToPlayer = (playerLegs.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

            // Применяем коррекцию угла если нужно
            if (forwardOffset != Vector3.zero)
            {
                lookRotation *= Quaternion.Euler(forwardOffset);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        private void TryShoot(Vector3 targetPoint)
        {
            if (!canShoot) return;

            if (Time.time >= lastShootTime + shootCooldown)
            {
                Shoot(targetPoint);
                lastShootTime = Time.time;
            }
        }

        private void Shoot(Vector3 targetPoint)
        {
            if (shootPoint == null || beamMaterial == null)
            {
                Debug.LogWarning("ShootPoint or BeamMaterial is not assigned!");
                return;
            }

            // Вызываем событие выстрела
            onShoot?.Invoke();

            // Вычисляем направление к игроку
            Vector3 shootDirection = (targetPoint - shootPoint.position).normalized;

            // Создаём луч
            GameObject beam = new GameObject("EnemyBeam");
            beam.transform.position = shootPoint.position;

            // Добавляем LineRenderer
            LineRenderer lineRenderer = beam.AddComponent<LineRenderer>();
            lineRenderer.material = beamMaterial;
            lineRenderer.startWidth = beamWidth;
            lineRenderer.endWidth = beamWidth;
            lineRenderer.positionCount = 2;

            // Raycast для проверки попаданий
            RaycastHit hit;
            Vector3 endPoint;

            if (Physics.Raycast(shootPoint.position, shootDirection, out hit, beamLength, hitLayers))
            {
                endPoint = hit.point;

                // Пытаемся нанести урон через интерфейс
                IDamageable damageable = hit.collider != null
                    ? hit.collider.GetComponentInParent<IDamageable>()
                    : null;
                
                if (damageable != null)
                {
                    Debug.Log($"Enemy hit target for {damage} damage");
                    damageable.TakeDamage(damage);
                }

                // Проверяем слой для логов
                if (hit.collider?.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    Debug.Log($"Enemy hit Player: {hit.collider.gameObject.name}");
                }
            }
            else
            {
                endPoint = shootPoint.position + shootDirection * beamLength;
            }

            // Устанавливаем точки луча
            lineRenderer.SetPosition(0, shootPoint.position);
            lineRenderer.SetPosition(1, endPoint);

            // Уничтожаем луч через заданное время
            Destroy(beam, beamDuration);
        }

        // Публичные методы для внешнего управления
        public void SetCanShoot(bool value)
        {
            canShoot = value;
        }

        public void SetPlayer(Transform newPlayerLegs, [CanBeNull] Transform newPlayerHead = null)
        {
            playerLegs = newPlayerLegs;
            
            if (newPlayerHead == null) return;
            playerHead = newPlayerHead;
        }

        public void DestroyEnemy()
        {
            onDestroyed?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // Вызываем событие при уничтожении объекта
            onDestroyed?.Invoke();
        }

        // Визуализация в редакторе
        private void OnDrawGizmosSelected()
        {
            // Показываем зону атаки
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackDistance);

            // Показываем минимальную дистанцию
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minDistance);
        }
    }
}