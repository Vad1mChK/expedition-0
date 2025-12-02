using Expedition0.Health;
using UnityEngine;
using UnityEngine.Events;

namespace Expedition0.Environment
{
    public class KillArea: CollisionEventCaller
    {
        [SerializeField] protected UnityEvent<IDamageable> onKill;
        [SerializeField] protected UnityEvent<PlayerHealth> onKillPlayer;
        
        protected new void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                KillDamageable(damageable);
            }
        }

        protected new void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
            {
                KillDamageable(damageable);
            }
        }

        private void KillDamageable(IDamageable damageable)
        {
            damageable.TakeDamage(damageable.GetCurrentHealth() + 1f);
            onKill?.Invoke(damageable);

            var player = damageable as PlayerHealth;
            if (player != null)
            {
                onKillPlayer?.Invoke(player);
            }
        }
    }
}