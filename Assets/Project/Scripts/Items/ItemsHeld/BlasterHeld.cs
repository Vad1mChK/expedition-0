using System.Collections;
using Expedition0.Combat.Laser;
using Expedition0.Items.Core;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Items.ItemsHeld
{
    public class BlasterHeld : ItemHeld
    {
        [Header("Laser Setup")]
        [SerializeField] private LaserBeam laserPrefab;
        [SerializeField] private bool isLaserParented = true;
        [SerializeField] private Transform firePoint;

        [Header("Fire Settings")]
        [SerializeField] private float ammoConsumedPerShot = 1f;
        [SerializeField] private bool isAutomatic;
        [ShowIf(nameof(isAutomatic))] [SerializeField] private float roundsPerMinute = 600f;
        [SerializeField] private float shotDuration = 0.1f;

        [Header("Accuracy")]
        [Range(0f, 45f)] [SerializeField] private float maxScatterAngle = 0f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [FormerlySerializedAs("shotSound")] [SerializeField] private AudioClip overrideShotSound;
        
        private LaserBeam _spawnedLaser;
        private Coroutine _fireCoroutine;
        private bool _triggerHeld;
        private float _lastFireTime;

        private void Awake()
        {
            if (laserPrefab != null)
            {
                _spawnedLaser = Instantiate(laserPrefab, isLaserParented ? firePoint : null);
                _spawnedLaser.gameObject.SetActive(false);
            }
        }

        public override void ProcessTrigger(bool pressed)
        {
            base.ProcessTrigger(pressed);
            _triggerHeld = pressed;

            if (pressed)
            {
                if (isAutomatic)
                {
                    if (_fireCoroutine == null) _fireCoroutine = StartCoroutine(AutoFireRoutine());
                }
                else
                {
                    TryFire();
                }
            }
            else
            {
                if (_fireCoroutine != null)
                {
                    StopCoroutine(_fireCoroutine);
                    _fireCoroutine = null;
                }
            }
        }

        private IEnumerator AutoFireRoutine()
        {
            float delay = 60f / roundsPerMinute;
            while (_triggerHeld)
            {
                TryFire();
                yield return new WaitForSeconds(delay);
            }
            _fireCoroutine = null;
        }

        private void TryFire()
        {
            // Add Ammo check logic here if needed
            
            // Apply Scatter
            Quaternion scatterRotation = Quaternion.Euler(
                Random.Range(-maxScatterAngle, maxScatterAngle),
                Random.Range(-maxScatterAngle, maxScatterAngle),
                0
            );
            
            firePoint.localRotation = scatterRotation;
            
            StartCoroutine(ShotLifecycle());
        }

        private IEnumerator ShotLifecycle()
        {
            var laserTransform = _spawnedLaser.transform;
            // 1. Activate
            laserTransform.position = firePoint.position;
            laserTransform.rotation = firePoint.rotation;
            _spawnedLaser.gameObject.SetActive(true);
            
            // Reset the internal damage tracking so this "pulse" hits targets again
            _spawnedLaser.ResetInstantDamage();
            _spawnedLaser.CalculateBeam();

            if (audioSource)
            {
                if (overrideShotSound)
                {
                    audioSource.PlayOneShot(overrideShotSound);
                }
                else
                {
                    audioSource.Play();
                }
            }

            // 2. Wait
            yield return new WaitForSeconds(shotDuration);

            // 3. Deactivate
            _spawnedLaser.gameObject.SetActive(false);
        }
        
        public override void OnTriggerReleased()
        {
            _triggerHeld = false;
            // Coroutine will clean itself up via the _triggerHeld check
        }

        private void OnDisable()
        {
            if (_spawnedLaser != null) _spawnedLaser.gameObject.SetActive(false);
            _triggerHeld = false;
        }
    }
}