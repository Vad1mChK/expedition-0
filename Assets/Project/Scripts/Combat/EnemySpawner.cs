using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Expedition0.Combat
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform spawnTransform;
        [SerializeField] private int defaultSpawnCount = 1;
        [SerializeField] private float spawnIntervalSeconds = 2f;

        [FormerlySerializedAs("playerTransform")]
        [Header("Settings Assigned On Spawn")]
        [SerializeField] private Transform playerLegsTransform;
        [SerializeField] private Transform playerHeadTransform;
        
        [Header("Spawn Effects")]
        [SerializeField] private GameObject spawnEffectPrefab;

        [Header("Events")]
        [SerializeField] private UnityEvent onEnemySpawned;

        /// <summary>
        /// Spawns a single enemy and injects the player reference.
        /// </summary>
        public void SpawnSingle()
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("EnemyPrefab is missing on Spawner!");
                return;
            }

            Vector3 pos = spawnTransform != null ? spawnTransform.position : transform.position;
            Quaternion rot = spawnTransform != null ? spawnTransform.rotation : transform.rotation;

            // Visual/Audio Effect
            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, pos, rot);
            }

            GameObject enemyObj = Instantiate(enemyPrefab, pos, rot);
            
            // Inject dependency to the EnemyController
            if (enemyObj.TryGetComponent(out EnemyController controller))
            {
                controller.SetPlayer(playerLegsTransform, playerHeadTransform);
            }
            
            onEnemySpawned?.Invoke();
        }

        /// <summary>
        /// Spawns multiple enemies over time using a Coroutine.
        /// </summary>
        public void Spawn(int count)
        {
            StartCoroutine(SpawnRoutine(count));
        }

        // Using a Coroutine ensures we don't block the main thread and 
        // respects the spawnIntervalSeconds.
        private IEnumerator SpawnRoutine(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnSingle();
                
                if (i < count - 1) // Don't wait after the last spawn
                {
                    yield return new WaitForSeconds(spawnIntervalSeconds);
                }
            }
        }

        // Context menu allows you to test spawning directly from the Inspector
        [ContextMenu("Test Default Spawn")]
        private void TestSpawn()
        {
            Spawn(defaultSpawnCount);
        }
    }
}