namespace Expedition0.Environment.Fire
{
    using System.Collections;
    using UnityEngine;

    namespace Expedition0.Environment.Fire
    {
        // We inherit from or require your existing FireDamage to handle the DOT logic
        [RequireComponent(typeof(FireDamage))]
        public class FireExplosion : MonoBehaviour
        {
            [Header("Expansion Settings")]
            [SerializeField] private AnimationCurve expansionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            [SerializeField] private float duration = 15.0f;
            [SerializeField] private float maxRadius = 2.0f;

            [Header("VFX")]
            [SerializeField] private ParticleSystem fireParticles;

            private void Awake()
            {
                StartCoroutine(ExplosionRoutine());
            }

            private IEnumerator ExplosionRoutine()
            {
                float elapsed = 0;
                Vector3 initialScale = Vector3.zero;
            
                // Set initial state
                transform.localScale = initialScale;

                if (fireParticles != null)
                    fireParticles.Play();

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float normalizedTime = elapsed / duration;
                
                    // Evaluate the curve (0 to 1) and multiply by max radius
                    float currentScale = expansionCurve.Evaluate(normalizedTime) * maxRadius;
                    transform.localScale = new Vector3(currentScale, currentScale, currentScale);

                    yield return null;
                }

                // Cleanup
                if (fireParticles != null)
                {
                    fireParticles.Stop();
                    // Wait for particles to finish if you want a soft fade out
                    yield return new WaitForSeconds(1.0f); 
                }

                Destroy(gameObject);
            }
        }
    }
}