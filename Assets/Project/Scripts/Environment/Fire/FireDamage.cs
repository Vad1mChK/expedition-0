using System.Collections;
using System.Collections.Generic;
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

        }

        private void OnEnable()
        {
            // Just in case: clear state when re-enabled
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
            // NOTE: This script must be on the same GameObject as the trigger collider (or use a forwarder).
            if (!other.TryGetComponent<IDamageable>(out var damageable))
                return;

            _targets.Add(damageable);

            if (_damageRoutine == null)
                _damageRoutine = StartCoroutine(DamageLoop());
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<IDamageable>(out var damageable))
                return;

            _targets.Remove(damageable);

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
                if (_targets.Count > 0)
                {
                    // Copy to avoid issues if targets change during iteration
                    var snapshot = new List<IDamageable>(_targets);
                    foreach (var t in snapshot)
                    {
                        if (t == null) continue; // object destroyed / disabled
                        t.TakeDamage(damagePerTick);
                    }
                }

                yield return wait;
            }
        }
    }
}