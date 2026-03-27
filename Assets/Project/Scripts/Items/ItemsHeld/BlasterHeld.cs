using System.Collections;
using Expedition0.Combat.Laser;
using Expedition0.Items.Core;
using NaughtyAttributes;
using UnityEngine;

namespace Expedition0.Items.ItemsHeld
{
    public class BlasterHeld : ItemHeld
    {
        [Header("Laser Setup")]
        [SerializeField] private LaserBeam laserPrefab;
        [SerializeField] private bool isLaserParented = true;
        [SerializeField] private Transform firePoint;

        [Header("Fire Settings")]
        [SerializeField] private bool isAutomatic;
        [SerializeField] private float shotDuration = 0.1f;
        [ShowIf(nameof(isAutomatic))] 
        [SerializeField] private float durationBetweenShots = 0.1f;

        [Header("Accuracy")]
        [Range(0f, 45f)] [SerializeField] private float maxScatterAngle = 0f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip overrideShotSound;
        
        private Coroutine _fireCoroutine;
        private bool _triggerHeld;
        private bool _isFiringCycleActive;

        public override void OnTriggerPressed()
        {
            _triggerHeld = true;
            if (isAutomatic)
            {
                if (_fireCoroutine == null) _fireCoroutine = StartCoroutine(AutoFireRoutine());
            }
            else if (!_isFiringCycleActive) 
            {
                StartCoroutine(ShotLifecycle());
            }
        }

        public override void OnTriggerReleased() => _triggerHeld = false;

        private IEnumerator AutoFireRoutine()
        {
            while (_triggerHeld)
            {
                yield return StartCoroutine(ShotLifecycle());
                yield return new WaitForSeconds(durationBetweenShots);
            }
            _fireCoroutine = null;
        }

        private IEnumerator ShotLifecycle()
        {
            _isFiringCycleActive = true;

            // 1. Calculate Scatter
            Quaternion scatterRotation = Quaternion.Euler(
                Random.Range(-maxScatterAngle, maxScatterAngle),
                Random.Range(-maxScatterAngle, maxScatterAngle),
                0
            );
            firePoint.localRotation = scatterRotation;

            // 2. Instantiate
            LaserBeam beamInstance = Instantiate(laserPrefab, firePoint.position, firePoint.rotation);
            if (isLaserParented) beamInstance.transform.SetParent(firePoint);

            // 3. Initialize & Fire
            beamInstance.ResetInstantDamage();
            beamInstance.CalculateBeam();

            // 4. Audio
            if (audioSource)
            {
                if (overrideShotSound) audioSource.PlayOneShot(overrideShotSound);
                else audioSource.Play();
            }

            // 5. Lifecycle
            yield return new WaitForSeconds(shotDuration);
            
            // Clean up the instance
            if (beamInstance != null) Destroy(beamInstance.gameObject);
            
            _isFiringCycleActive = false;
        }

        private void OnDisable()
        {
            _triggerHeld = false;
            _isFiringCycleActive = false;
            _fireCoroutine = null;
        }
    }
}