using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Expedition0.CharacterAI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class IllusionAI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The player's head/camera transform.")]
        [SerializeField] private Transform playerHead;
        
        [Tooltip("The exact point on Illusion where distance is measured (e.g., her eyes/lips).")]
        [SerializeField] private Transform illusionFace;

        [Header("Distance Parameters")]
        [Tooltip("r1: Distance to trigger kiss.")]
        [SerializeField] private float kissDistance = 0.5f;     // r1
        
        [Tooltip("r2: Distance to stop walking and wait.")]
        [SerializeField] private float standByDistance = 1.5f;  // r2
        
        [Tooltip("r3: Max distance to start tracking/walking.")]
        [SerializeField] private float trackingDistance = 5.0f; // r3

        [Header("Angle Parameters")]
        [Tooltip("Alpha: Max angle for both parties to see each other.")]
        [Range(0, 90)]
        [SerializeField] private float fieldOfViewAngle = 45f;

        [Header("Events")] [SerializeField] private UnityEvent onKiss;

        [Header("Durations")]
        [SerializeField] private float kissCooldownDuration = 5f;
        [SerializeField] private float kissAnimationDuration = 2f; // Adjust to match Anim clip length

        // State Machine
        private enum AIState { IdleFar, Approaching, IdleClose, Kissing, Cooldown }
        [SerializeField] private AIState currentState; // 'ReadOnlyInspector' is pseudo-code for debug viewing

        // Components
        private NavMeshAgent _agent;
        private Animator _animator;
        private float _cooldownTimer;
        private bool _isKissing;

        // Animator Hashes
        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
        private static readonly int KissTriggerHash = Animator.StringToHash("Kiss");
        
        // Gizmo Colors
        private static readonly Color Red = Color.red;
        private static readonly Color RedDisabled = new Color(1f, 0f, 0f, 0.3f);
        private static readonly Color Yellow = new Color(1f, 0.92f, 0.016f);
        private static readonly Color YellowDisabled = new Color(1f, 0.92f, 0.016f, 0.3f);
        private static readonly Color Green = Color.green;
        private static readonly Color GreenDisabled = new Color(0f, 1f, 0f, 0.3f);

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            // Validation
            if (illusionFace == null) illusionFace = transform;
            if (playerHead == null && Camera.main != null) playerHead = Camera.main.transform;
            
            // Enforce r1 < r2 < r3
            ValidateDistances();
        }

        private void ValidateDistances()
        {
            if (kissDistance <= 0.1f) kissDistance = 0.1f;
            if (standByDistance < kissDistance) standByDistance = kissDistance + 0.1f;
            if (trackingDistance < standByDistance) trackingDistance = standByDistance + 0.1f;
        }

        private void Update()
        {
            if (!playerHead) return;

            // Calculate Distance (d)
            float d = DistanceToPlayer();

            // State Logic
            switch (currentState)
            {
                case AIState.IdleFar:
                    HandleIdleFar(d);
                    break;
                case AIState.Approaching:
                    HandleApproaching(d);
                    break;
                case AIState.IdleClose:
                    HandleIdleClose(d);
                    break;
                case AIState.Kissing:
                    // Logic handled by coroutine/timer, waiting for animation
                    break;
                case AIState.Cooldown:
                    HandleCooldown();
                    break;
            }

            // Sync Animations
            UpdateAnimator();
        }

        // --- State Handlers ---

        private void HandleIdleFar(float d)
        {
            _agent.isStopped = true;

            // Condition 4: If r2 <= d < r3 -> Start Walking
            if (d < trackingDistance && d >= standByDistance)
            {
                currentState = AIState.Approaching;
            }
        }

        private void HandleApproaching(float d)
        {
            _agent.isStopped = false;
            _agent.SetDestination(playerHead.position); // Walk to player

            // Condition 3: d >= r3 -> Stop (Player ran away)
            if (d >= trackingDistance)
            {
                currentState = AIState.IdleFar;
                return;
            }

            // Condition 5: r1 <= d < r2 -> Stop and Wait
            if (d < standByDistance && d >= kissDistance)
            {
                currentState = AIState.IdleClose;
            }
            
            // Edge Case: Player ran straight into her (d < r1)
            if (d < kissDistance)
            {
                currentState = AIState.IdleClose; // Transition to check angles
            }
        }

        private void HandleIdleClose(float d)
        {
            _agent.isStopped = true;
            
            // Look at player while waiting
            RotateTowardsPlayer();

            // Condition 4: Player moved back to range r2 -> Walk again
            if (d >= standByDistance && d < trackingDistance)
            {
                currentState = AIState.Approaching;
                return;
            }

            // Condition 6: d < r1 -> Check Angles for Kiss
            if (d < kissDistance)
            {
                if (CheckKissAngles())
                {
                    StartCoroutine(PerformKissRoutine());
                }
            }
        }

        private void HandleCooldown()
        {
            _agent.isStopped = true;
            _cooldownTimer -= Time.deltaTime;

            if (_cooldownTimer <= 0)
            {
                // Condition 7: After cooldown, check distances again
                float d = DistanceToPlayer();
                
                if (d < kissDistance) currentState = AIState.IdleClose;
                else if (d < standByDistance) currentState = AIState.IdleClose;
                else if (d < trackingDistance) currentState = AIState.Approaching;
                else currentState = AIState.IdleFar;
            }
        }

        // --- Helpers ---

        private bool CheckKissAngles()
        {
            // Vector from Illusion to Player
            Vector3 dirToPlayer = (playerHead.position - illusionFace.position).normalized;
            // Vector from Player to Illusion (Head forward direction)
            Vector3 playerLookDir = playerHead.forward;

            // 1. Is Player within Illusion's sight cone?
            // (Angle between Illusion's forward and direction to player)
            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

            // 2. Is Illusion within Player's sight cone?
            // (Angle between Player's forward and direction to Illusion [which is -dirToPlayer])
            float angleToIllusion = Vector3.Angle(playerLookDir, -dirToPlayer);

            // Debug logic
            // Debug.Log($"AngleToPlayer: {angleToPlayer}, AngleToIllusion: {angleToIllusion}");

            return angleToPlayer < fieldOfViewAngle && angleToIllusion < fieldOfViewAngle;
        }

        private System.Collections.IEnumerator PerformKissRoutine()
        {
            currentState = AIState.Kissing;
            _agent.isStopped = true;
            Debug.Log("Attempting kiss");
            onKiss?.Invoke();
            
            // Face exactly
            transform.LookAt(new Vector3(playerHead.position.x, transform.position.y, playerHead.position.z));

            // Trigger Animation
            _animator.SetTrigger(KissTriggerHash);
            
            // Wait for animation
            yield return new WaitForSeconds(kissAnimationDuration);

            // Start Cooldown
            _cooldownTimer = kissCooldownDuration;
            currentState = AIState.Cooldown;
        }

        private void RotateTowardsPlayer()
        {
            Vector3 direction = (playerHead.position - transform.position).normalized;
            direction.y = 0; // Keep rotation flat
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }

        private void UpdateAnimator()
        {
            // Only walk if moving and not stuck in a special state
            bool moving = !_agent.isStopped && _agent.velocity.sqrMagnitude > 0.1f;
            _animator.SetBool(IsWalkingHash, moving);
        }

        // --- Gizmos ---
        private void OnDrawGizmos()
        {
            if (illusionFace == null) return;

            // Centers
            Vector3 center = illusionFace.position;

            // r1 - Kiss (Red)
            Gizmos.color = (DistanceToPlayer() < kissDistance) ? 
                Red : RedDisabled;
            Gizmos.DrawWireSphere(center, kissDistance);

            // r2 - Standby (Yellow)
            Gizmos.color = (DistanceToPlayer() < standByDistance) ?
                Yellow : YellowDisabled;
            Gizmos.DrawWireSphere(center, standByDistance);

            // r3 - Detection (Green)
            Gizmos.color = (DistanceToPlayer() < trackingDistance) ?
                Green : GreenDisabled;
            Gizmos.DrawWireSphere(center, trackingDistance);

            // Sight Cone (Blue Lines)
            Gizmos.color = Color.cyan;
            Vector3 forward = illusionFace.forward,
                up = Vector3.up,
                left = -illusionFace.right;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-fieldOfViewAngle, up),
                rightRayRotation = Quaternion.AngleAxis(fieldOfViewAngle, up),
                topRayRotation = Quaternion.AngleAxis(fieldOfViewAngle, left),
                bottomRayRotation = Quaternion.AngleAxis(-fieldOfViewAngle, left);
            Gizmos.DrawRay(center, leftRayRotation * forward * kissDistance);
            Gizmos.DrawRay(center, rightRayRotation * forward * kissDistance);
            Gizmos.DrawRay(center, topRayRotation * forward * kissDistance);
            Gizmos.DrawRay(center, bottomRayRotation * forward * kissDistance);
        }

        private float DistanceToPlayer() => Vector3.Distance(illusionFace.position, playerHead.position);
    }
}