using System;
using System.Collections.Generic;
using Expedition0.Save;
using Expedition0.Visuals;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Expedition0.Health
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Serializable]
        public struct ConditionalScene
        {
            public GameProgress progressConditions; // required flags
            public int priority; // higher wins on ties
            public string sceneName;

            public bool IsSatisfied(GameProgress p)
            {
                return (p & progressConditions) == progressConditions;
            }
        }

        [Header("Respawn Settings")] [SerializeField]
        private List<ConditionalScene> respawnRules;
        [SerializeField] private string defaultRespawnSceneName;
        
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;

        [SerializeField] private float currentHealth;

        [Header("UI Reference")]
        [SerializeField] private HealthBar healthBar; // Ensure you have this script or remove this line if not

        [SerializeField] private VisualEffectsController vfx;

        [Header("Events")]
        public UnityEvent<float> OnTakeDamage;

        public UnityEvent<float> OnHealthPercentageRemaining;
        public UnityEvent OnDeath;
        public UnityEvent OnRespawn;
        
        private bool isDead;
        private Vector3 respawnPosition;
        private Quaternion respawnRotation;

        private void Start()
        {
            currentHealth = maxHealth;
            respawnPosition = transform.position;
            respawnRotation = transform.rotation;

            InitializeUI();
        }

        // --- Interface Implementation ---
        public void TakeDamage(float damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateVisualization();
            OnTakeDamage?.Invoke(damage);

            if (currentHealth <= 0) Die();
        }

        // --- Getters ---
        public bool IsDead() => isDead;

        public float GetCurrentHealth() => currentHealth;

        public float GetMaxHealth() => maxHealth;

        public float GetHealthPercentage() => (maxHealth > 0f) ? (currentHealth / maxHealth) : 0;

        private void InitializeUI()
        {
            if (healthBar == null) healthBar = FindFirstObjectByType<HealthBar>();

            if (healthBar != null)
            {
                healthBar.SetMaxHealth(maxHealth);
                healthBar.SetHealth(currentHealth);
            }
        }

        public void Heal(float healAmount)
        {
            if (isDead) return;

            currentHealth += healAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateVisualization();
        }

        private void UpdateVisualization()
        {
            if (healthBar != null) healthBar.SetHealth(currentHealth);
            if (vfx)
            {
                float damage01 = 1f - currentHealth / maxHealth;
                vfx.SetDamage01(damage01);
            }
        }

        private void Die()
        {
            isDead = true;
            Debug.Log($"{gameObject.name} has died!");
            OnDeath?.Invoke();

            // Only auto-respawn if this is the Player (you might want to check tag)
            if (gameObject.CompareTag("Player"))
                Invoke(nameof(Respawn), 3f);
            else
                // If it's an enemy, destroy it
                Destroy(gameObject, 0.1f);
        }

        public void Respawn()
        {
            // Scene Logic from System A
            var respawnScene = GetRespawnScene();

            if (SceneManager.GetActiveScene().name != respawnScene)
                SceneManager.LoadScene(respawnScene);
            else
                RespawnAtPosition(respawnPosition, respawnRotation);
        }

        private string GetRespawnScene()
        {
            return ResolveRespawnScene(SaveManager.LoadProgress());
        }

        public void RespawnAtPosition(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;

            currentHealth = maxHealth;
            isDead = false;
            UpdateVisualization();

            OnRespawn?.Invoke();
            Debug.Log($"{gameObject.name} Respawned!");
        }

        public void SetRespawnPoint(Vector3 position, Quaternion rotation)
        {
            respawnPosition = position;
            respawnRotation = rotation;
        }
        
        private string ResolveRespawnScene(GameProgress p)
        {
            var best = defaultRespawnSceneName;
            var bestPr = int.MinValue;

            foreach (var r in respawnRules)
                if (!string.IsNullOrEmpty(r.sceneName) && r.IsSatisfied(p) && r.priority >= bestPr)
                {
                    best = r.sceneName;
                    bestPr = r.priority;
                }

            return best;
        }
    }
}