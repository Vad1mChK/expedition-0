using UnityEngine;
using System.Collections;
using UnityEngine.Events; // Required for Coroutines

namespace Expedition0.Health
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 2f;
        [SerializeField] private float currentHealth;
        private bool _isDead = false;

        [Header("Death Settings")]
        [Tooltip("Time in seconds before the GameObject is destroyed after death.")]
        [SerializeField] private float destructionDelay = 0f;
        [Tooltip("The particle effect prefab to spawn upon death.")]
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private Transform deathEffectTransform;

        [Header("Events")] [Tooltip("Called on the enemy's death")]
        [SerializeField] private UnityEvent onDie;
        [SerializeField] private UnityEvent onTakeDamage;

        public bool IsDead() => _isDead;

        void Start()
        {
            currentHealth = maxHealth;
            _isDead = false;
            // Ensure components are active on start
            SetComponentsActive(true);

            if (deathEffectTransform != null)
            {
                deathEffectTransform = transform;
            }
            
            Debug.Log($"Enemy '{gameObject.name}' spawned with {currentHealth} HP");
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return; // Prevent damage if already dead

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            Debug.Log($"Enemy '{gameObject.name}' took {damage} damage. Current HP: {currentHealth}/{maxHealth}");
            onTakeDamage?.Invoke();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float health)
        {
            if (_isDead) return;

            // Only heal up to max health
            currentHealth = Mathf.Min(maxHealth, currentHealth + health);
            
            Debug.Log($"Enemy '{gameObject.name}' healed by {health} HP. Current HP: {currentHealth}/{maxHealth}");
        }

        public float GetCurrentHealth() => currentHealth;

        public float GetMaxHealth() => maxHealth;

        private void Die()
        {
            if (_isDead) return; // Prevent double-death logic
            
            Debug.Log($"Enemy '{gameObject.name}' died!");
            _isDead = true;

            // 1. Instantly deactivate physical/visual components
            SetComponentsActive(false);

            // 2. Instantiate death effect (optional)
            if (deathEffectPrefab != null)
            {
                // Instantiate the effect at the enemy's position
                GameObject effect = Instantiate(deathEffectPrefab, deathEffectTransform.position, Quaternion.identity);
                // Ensure the effect cleans itself up after its lifetime
                // (Though you might need a separate script for effect cleanup if the prefab doesn't auto-destroy)
            }
            
            onDie?.Invoke();

            // 3. Start coroutine for delayed destruction
            StartCoroutine(DelayedDestruction(destructionDelay));
        }
        
        /// <summary>
        /// Finds and toggles all visible components (MeshRenderers) and physical components (Colliders).
        /// </summary>
        /// <param name="active">True to enable, False to disable.</param>
        private void SetComponentsActive(bool active)
        {
            // Deactivate all colliders (stops all further physics interactions/triggers)
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var coll in colliders)
            {
                coll.enabled = active;
            }

            // Deactivate all visual renderers
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                rend.enabled = active;
            }

            // If using a NavMeshAgent, disable it to stop movement
            if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
            {
                agent.enabled = active;
            }
        }

        private IEnumerator DelayedDestruction(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Final cleanup
            Destroy(gameObject);
        }
    }
}