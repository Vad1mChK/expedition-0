using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Expedition0.Health;
using UnityEngine;

namespace Expedition0.Environment.Fire
{
    [RequireComponent(typeof(Collider))]
    public class FireDamage : MonoBehaviour
    {
        [SerializeField] private float areaDamagePerSecond = 5f;
        [SerializeField] private float areaDamageDealInterval = 0.5f;

        private readonly HashSet<IDamageable> _targets = new HashSet<IDamageable>();
        private Coroutine _damageRoutine;

        private void Awake()
        {
            // Assuming IDamageable interface has the IsDead() method
            // If IDamageable is a simple interface, you might need to cast or use a wrapper.
        }

        private void OnEnable()
        {
            _targets.Clear();
        }

        private void OnDisable()
        {
            if (_damageRoutine != null)
            {
                StopCoroutine(_damageRoutine);
                _damageRoutine = null;
            }
            _targets.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable = other.GetComponentInParent<IDamageable>();
                
                if (damageable == null)
                    return;
            }
            
            // Check if they are already dead before adding
            if (damageable.IsDead()) // Assuming IDamageable has IsDead()
                return;

            // if (other.gameObject != null)
            // {
            //     Debug.Log($"FireDamage: {other.gameObject.name} has entered the fire");
            // }
            
            _targets.Add(damageable);

            if (_targets.Count == 0 && _damageRoutine != null)
                StopCoroutine(_damageRoutine); // Safety check in case it was running but empty
            
            if (_damageRoutine == null)
                _damageRoutine = StartCoroutine(DamageLoop());
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable = other.GetComponentInParent<IDamageable>();
                
                if (damageable == null)
                    return; // No damageable found anywhere in the hierarchy
            }
            
            // if (other.gameObject != null)
            // {
            //     Debug.Log($"FireDamage: {other.gameObject.name} has exited the fire");
            // }

            _targets.Remove(damageable);
            // This removal must happen regardless of death, as they left the area.

            if (_targets.Count == 0 && _damageRoutine != null)
            {
                StopCoroutine(_damageRoutine);
                _damageRoutine = null;
            }
        }

        private IEnumerator DamageLoop()
        {
            var wait = new WaitForSeconds(areaDamageDealInterval);
            float damagePerTick = areaDamagePerSecond * areaDamageDealInterval;

            while (true)
            {
                // Create a temporary list to hold targets we need to remove
                var targetsToRemove = new List<IDamageable>();

                if (_targets.Count > 0)
                {
                    // Use a snapshot of the current targets to iterate safely
                    var snapshot = new List<IDamageable>(_targets);
                    
                    foreach (var t in snapshot)
                    {
                        // 1. Check for null (destroyed object, even if the reference remains)
                        if (t == null)
                        {
                            targetsToRemove.Add(t);
                            continue;
                        }

                        // 2. Check if the target is already dead
                        if (t.IsDead())
                        {
                            targetsToRemove.Add(t);
                            continue;
                        }

                        // 3. Apply Damage
                        t.TakeDamage(damagePerTick);
                        
                        // 4. Re-check if the damage killed them (important!)
                        if (t.IsDead())
                        {
                            targetsToRemove.Add(t);
                        }
                    }
                }

                // --- Cleanup Phase ---
                foreach (var deadTarget in targetsToRemove)
                {
                    _targets.Remove(deadTarget);
                    Debug.Log($"FireDamage: Removed dead/null target.");
                }

                // Stop the loop if the fire is empty
                if (_targets.Count == 0)
                {
                    _damageRoutine = null;
                    yield break; // Exit the coroutine
                }

                yield return wait;
            }
        }
    }
}