using System.Collections.Generic;
using Expedition0.Health;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

namespace Expedition0.Combat.Laser
{
    public enum DamageMode
    {
        Instant,
        OverTime
    }

    public class LaserBeam : MonoBehaviour
    {
        [Header("Beam Settings")] [SerializeField]
        private float initialDamage = 10f;

        [SerializeField] private DamageMode damageMode = DamageMode.Instant;
        [SerializeField] private int maxBounces = 5;
        [SerializeField] private float maxDistance = 100f;

        [FormerlySerializedAs("jumpDistance")] [SerializeField]
        private float nudgeDistance = 0.001f;

        [Tooltip("Layers the laser can hit.")] [SerializeField]
        private LayerMask hitLayers = -1;

        [Header("Dynamics")]
        [Tooltip("If true, recalculates the path in Update. Use for moving mirrors/shooters.")]
        [SerializeField]
        private bool updateEveryFrame = true;

        [Header("Visuals")]
        [Tooltip("Prefab with pivot at bottom, 1m height, 1m diameter (e.g. 8-sided cylinder).")]
        [SerializeField]
        private GameObject beamPrefab;

        [SerializeField] private float beamWidth = 0.05f;

        [ColorUsage(true, true)] [SerializeField]
        private Color beamColor = Color.yellow;

        [Header("Hit Effects")] [SerializeField]
        private GameObject hitEffectPrefab;

        [Tooltip("If true, the effect follows the hit point. If false, it spawns once per impact.")] [SerializeField]
        private bool persistentHitEffect = true;

        [Tooltip("Force respawn if the laser moves more than this distance in one frame.")] [SerializeField]
        private float teleportThreshold = 1f;

        // Internal Data Structures
        private struct BeamSegment
        {
            public Vector3 Start;
            public Vector3 End;
            public float Intensity;
        }

        private readonly List<BeamSegment> _segmentsData = new List<BeamSegment>();
        private readonly List<GameObject> _visualInstances = new List<GameObject>();

        // Damage Tracking
        private readonly HashSet<IDamageable> _alreadyDamagedThisFrame = new HashSet<IDamageable>();
        private readonly HashSet<IDamageable> _alreadyDamagedInstant = new HashSet<IDamageable>();

        private Transform _visualsParent;
        private GameObject _spawnedHitEffect;
        private MaterialPropertyBlock _mpb;

        // Hit Tracking
        private Vector3 _lastHitPos;
        private bool _hadHitLastFrame;

        // Cached IDs
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();

            // Create a tidy container for segments in the scene
            var parentObj = new GameObject($"{name}_Visuals");
            _visualsParent = parentObj.transform;
        }

        private void Update()
        {
            if (updateEveryFrame)
            {
                CalculateBeam();
            }
        }

        private void OnDestroy()
        {
            if (_visualsParent != null)
            {
                Destroy(_visualsParent.gameObject);
            }

            if (_spawnedHitEffect != null)
            {
                Destroy(_spawnedHitEffect);
            }
        }

        public void CalculateBeam()
        {
            // Reset frame-specific data
            _segmentsData.Clear();
            _alreadyDamagedThisFrame.Clear();

            var cachedTrans = transform;
            Vector3 currentPos = cachedTrans.position;
            Vector3 currentDir = cachedTrans.forward;
            float currentIntensity = 1.0f;

            int bounces = 0;
            int safeGuard = 0;
            int maxIterations = maxBounces * 2 + 5;

            Vector3 finalHitPoint = Vector3.zero;
            bool hitSomething = false;

            while (bounces < maxBounces && safeGuard < maxIterations)
            {
                safeGuard++;

                if (Physics.Raycast(currentPos, currentDir, out RaycastHit hit, maxDistance, hitLayers))
                {
                    hitSomething = true;
                    finalHitPoint = hit.point;

                    // 1. Check for Surface interactions
                    if (hit.collider.TryGetComponent(out LaserSurface surface))
                    {
                        // Case: Ignore
                        if (surface.surfaceType == LaserSurfaceType.Ignore)
                        {
                            _segmentsData.Add(new BeamSegment
                                { Start = currentPos, End = hit.point, Intensity = currentIntensity });
                            currentPos = hit.point + currentDir * nudgeDistance;
                            continue;
                        }

                        // Case: Absorb (Stop)
                        if (surface.surfaceType == LaserSurfaceType.Absorb)
                        {
                            _segmentsData.Add(new BeamSegment
                                { Start = currentPos, End = hit.point, Intensity = currentIntensity });
                            TryDamage(hit.collider, currentIntensity);
                            break;
                        }

                        // Apply intensity loss for bounces/passthrough
                        float previousIntensity = currentIntensity;
                        currentIntensity *= surface.bounceIntensity;

                        // Damage the surface we just hit using the IMPACT intensity
                        TryDamage(hit.collider, previousIntensity);

                        // Stop beam if too dim
                        if (currentIntensity <= 0.01f)
                        {
                            _segmentsData.Add(new BeamSegment
                                { Start = currentPos, End = hit.point, Intensity = previousIntensity });
                            break;
                        }

                        // Case: Passthrough
                        if (surface.surfaceType == LaserSurfaceType.Passthrough)
                        {
                            _segmentsData.Add(new BeamSegment
                                { Start = currentPos, End = hit.point, Intensity = previousIntensity });
                            currentPos = hit.point + currentDir * nudgeDistance;
                            continue;
                        }

                        // Case: Reflect
                        if (surface.surfaceType == LaserSurfaceType.Reflect)
                        {
                            _segmentsData.Add(new BeamSegment
                                { Start = currentPos, End = hit.point, Intensity = previousIntensity });
                            currentDir = Vector3.Reflect(currentDir, hit.normal);
                            currentPos = hit.point + currentDir * nudgeDistance;
                            bounces++;
                        }
                        // Case: Refract
                        else if (surface.surfaceType == LaserSurfaceType.Refract)
                        {
                            _segmentsData.Add(new BeamSegment
                                { Start = currentPos, End = hit.point, Intensity = previousIntensity });
                            Vector3 entryDir = Refract(currentDir, hit.normal, 1.0f, surface.ior);

                            if (entryDir == Vector3.zero)
                            {
                                // Total Internal Reflection at entry (rare) -> Reflect
                                currentDir = Vector3.Reflect(currentDir, hit.normal);
                                currentPos = hit.point + currentDir * nudgeDistance;
                                bounces++;
                            }
                            else
                            {
                                // Trace through the medium
                                Vector3 insideStart = hit.point + entryDir * nudgeDistance;
                                if (Physics.Raycast(insideStart, entryDir, out RaycastHit exitHit, maxDistance,
                                        hitLayers))
                                {
                                    // Segment inside the glass
                                    _segmentsData.Add(new BeamSegment
                                        { Start = hit.point, End = exitHit.point, Intensity = currentIntensity });
                                    finalHitPoint = exitHit.point; // Update final point to exit

                                    Vector3 exitDir = Refract(entryDir, -exitHit.normal, surface.ior, 1.0f);

                                    if (exitDir == Vector3.zero)
                                    {
                                        // TIR inside -> Reflect internally (Terminate for simplicity or reflect)
                                        currentDir = Vector3.Reflect(entryDir, -exitHit.normal);
                                        currentPos = exitHit.point + currentDir * nudgeDistance;
                                        break;
                                    }

                                    // Clean exit
                                    currentDir = exitDir;
                                    currentPos = exitHit.point + currentDir * nudgeDistance;
                                    bounces++;
                                }
                                else
                                {
                                    // Trapped inside or infinite medium
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // 2. Hit non-surface object (Standard Wall/Enemy)
                        _segmentsData.Add(new BeamSegment
                            { Start = currentPos, End = hit.point, Intensity = currentIntensity });
                        TryDamage(hit.collider, currentIntensity);
                        break;
                    }
                }
                else
                {
                    // Hit nothing
                    _segmentsData.Add(new BeamSegment
                    {
                        Start = currentPos, End = currentPos + currentDir * maxDistance, Intensity = currentIntensity
                    });
                    hitSomething = false;
                    break;
                }
            }

            UpdateVisuals();
            HandleHitEffect(hitSomething, finalHitPoint);
        }

        private void TryDamage(Collider col, float intensity)
        {
            if (col.TryGetComponent(out IDamageable health))
            {
                ApplyDamageInternal(health, intensity);
            }
        }

        private void UpdateVisuals()
        {
            if (beamPrefab == null) return;

            int neededCount = _segmentsData.Count;

            // 1. Expand pool if necessary
            while (_visualInstances.Count < neededCount)
            {
                GameObject obj = Instantiate(beamPrefab, _visualsParent);
                obj.SetActive(false);
                _visualInstances.Add(obj);
            }

            // 2. Update active segments
            for (int i = 0; i < _visualInstances.Count; i++)
            {
                if (i < neededCount)
                {
                    GameObject segObj = _visualInstances[i];
                    BeamSegment data = _segmentsData[i];

                    if (!segObj.activeSelf) segObj.SetActive(true);

                    Vector3 vector = data.End - data.Start;
                    float length = vector.magnitude;

                    // Position (Pivot at bottom)
                    segObj.transform.position = data.Start;

                    // Rotation
                    if (length > 0.001f)
                    {
                        // Align Y-up cylinder to vector
                        segObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, vector);
                    }

                    // Scale
                    float scaledWidth = beamWidth * data.Intensity;
                    segObj.transform.localScale = new Vector3(scaledWidth, length, scaledWidth);

                    // Color / Emission
                    if (segObj.TryGetComponent(out MeshRenderer mr))
                    {
                        mr.GetPropertyBlock(_mpb);

                        Color finalColor = beamColor * data.Intensity;
                        _mpb.SetColor(EmissionColorId, finalColor);

                        Color baseCol = beamColor;
                        baseCol.a *= data.Intensity;
                        _mpb.SetColor(BaseColorId, baseCol);

                        mr.SetPropertyBlock(_mpb);
                    }
                }
                else
                {
                    // Disable unused pool items
                    if (_visualInstances[i].activeSelf)
                        _visualInstances[i].SetActive(false);
                }
            }
        }

        private void HandleHitEffect(bool active, Vector3 position)
        {
            if (hitEffectPrefab == null) return;

            // --- Persistent Mode (Effect stays alive and moves) ---
            if (persistentHitEffect)
            {
                if (!active)
                {
                    if (_spawnedHitEffect != null)
                        _spawnedHitEffect.SetActive(false);
                    return;
                }

                if (_spawnedHitEffect == null)
                {
                    _spawnedHitEffect = Instantiate(hitEffectPrefab);
                }

                // Teleport logic: if distance is too big, disable briefly to reset trails/particles
                if (Vector3.Distance(_spawnedHitEffect.transform.position, position) > teleportThreshold)
                {
                    _spawnedHitEffect.SetActive(false);
                    _spawnedHitEffect.transform.position = position;
                    _spawnedHitEffect.SetActive(true);
                }
                else
                {
                    if (!_spawnedHitEffect.activeSelf) _spawnedHitEffect.SetActive(true);
                    _spawnedHitEffect.transform.position = position;
                }

                return;
            }

            // --- Transient Mode (Spawn new effect on impact/move) ---
            if (!active)
            {
                _hadHitLastFrame = false;
                return;
            }

            float sqrTeleport = teleportThreshold * teleportThreshold;

            // Only spawn if this is a new hit sequence OR the hit point moved significantly
            if (!_hadHitLastFrame || (position - _lastHitPos).sqrMagnitude > sqrTeleport)
            {
                Instantiate(hitEffectPrefab, position, Quaternion.identity);
                _lastHitPos = position;
                _hadHitLastFrame = true;
            }
        }

        private void ApplyDamageInternal(IDamageable health, float intensity)
        {
            // Avoid hitting the same target multiple times in one laser path calculation
            if (_alreadyDamagedThisFrame.Contains(health)) return;

            if (damageMode == DamageMode.Instant)
            {
                // Only damage once per "Trigger pull" (until ResetInstantDamage is called)
                if (!_alreadyDamagedInstant.Contains(health))
                {
                    health.TakeDamage(initialDamage * intensity);
                    _alreadyDamagedInstant.Add(health);
                }
            }
            else // OverTime
            {
                health.TakeDamage(initialDamage * intensity * Time.deltaTime);
            }

            _alreadyDamagedThisFrame.Add(health);
        }

        /// <summary>
        /// Call this when the weapon/drone stops firing to allow Instant damage to apply again.
        /// </summary>
        public void ResetInstantDamage()
        {
            _alreadyDamagedInstant.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = beamColor;
            Vector3 center = transform.position;
            Vector3 direction = transform.forward;

            Gizmos.DrawRay(center, direction);
            Gizmos.DrawWireSphere(center, 0.05f);
        }
        
        private void OnDisable()
        {
            // Hide the beam segments immediately
            foreach (var seg in _visualInstances)
            {
                if (seg != null) seg.SetActive(false);
            }
    
            // Hide the hit effect
            if (_spawnedHitEffect != null)
            {
                _spawnedHitEffect.SetActive(false);
            }

            // Reset instant damage tracking so it can fire again next time
            ResetInstantDamage();
        }

        private static Vector3 Refract(Vector3 incident, Vector3 normal, float n1, float n2)
        {
            float eta = n1 / n2;
            float cosTheta1 = Vector3.Dot(-incident, normal);
            float k = 1.0f - eta * eta * (1.0f - cosTheta1 * cosTheta1);

            if (k < 0.0f) return Vector3.zero; // Total Internal Reflection

            return eta * incident + (eta * cosTheta1 - Mathf.Sqrt(k)) * normal;
        }
    }
}