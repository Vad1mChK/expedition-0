using System;
using System.Collections.Generic;
using Expedition0.Save;
using Expedition0.Save.Experimental;
using Expedition0.Visuals;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Expedition0.Health
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
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
            var respawnLevelId = PlaythroughLifecycleManager.Instance.CurrentData.respawnLevel;
            var respawnScene = PlaythroughLifecycleManager.Instance.levelRegistry.GetItem(respawnLevelId);
            
            if (SceneManager.GetActiveScene().name != respawnScene)
            {
                PlaythroughLifecycleManager.Instance.RespawnAndLoadRespawnLevel();
            }
            else
            {
                PlaythroughLifecycleManager.Instance.ResetHealthAndIncrementDeath();
                RespawnAtPosition(respawnPosition, respawnRotation);
            }
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
    }
}