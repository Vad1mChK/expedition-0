using System.Collections;
using Expedition0.Save;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Expedition0.CharacterAI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class IllusionAltAI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The player's head/camera transform.")]
        [SerializeField] private Transform playerHead;

        [Tooltip("The exact point on Illusion where distance is measured (e.g., her eyes/lips).")]
        [SerializeField] private Transform illusionFace;

        [Tooltip("The animator that controls Illusion's animations.")]
        [SerializeField] private Animator animator;

        [Tooltip("XRSimpleInteractable used to detect ray select (grab held).")]
        [SerializeField] private XRSimpleInteractable interactable;

        [Header("Distance Parameters")]
        [Tooltip("r1: Kiss range. If d < r1 -> attempt kiss (angle-gated).")]
        [SerializeField] private float kissRange = 0.5f; // r1

        [Tooltip("r2: Min follow range. If d > r2 and ray select is held -> follow. If r1 < d <= r2 -> stand still.")]
        [SerializeField] private float minFollowRange = 1.5f; // r2

        [Header("Angle Parameters")]
        [Tooltip("Alpha: Max angle for both parties to see each other (kiss gating).")]
        [Range(0, 90)]
        [SerializeField] private float fieldOfViewAngle = 45f;

        [Header("Events")]
        [SerializeField] private UnityEvent onKiss;

        [Header("Durations")]
        [SerializeField] private float kissCooldownDuration = 5f;

        [Tooltip("Adjust to match the kiss animation clip length.")]
        [SerializeField] private float kissAnimationDuration = 2f;

        [Header("Misc")]
        [SerializeField] private bool doUpdateSaveAfterKiss = true;

        // Components
        private NavMeshAgent _agent;

        // Ray-select gating (count because multiple rays could select)
        private int _raySelectCount;

        // State
        private bool _isKissing;
        private float _cooldownTimer;

        // Animator Hashes
        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
        private static readonly int KissTriggerHash = Animator.StringToHash("Kiss");

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();

            if (interactable == null)
                interactable = GetComponent<XRSimpleInteractable>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (illusionFace == null)
                illusionFace = transform;

            if (playerHead == null && Camera.main != null)
                playerHead = Camera.main.transform;

            ValidateDistances();
            HookInteractableEvents();
        }

        private void OnDestroy()
        {
            UnhookInteractableEvents();
        }

        private void ValidateDistances()
        {
            if (kissRange < 0.05f) kissRange = 0.05f;
            if (minFollowRange < kissRange) minFollowRange = kissRange + 0.1f;
        }

        private void HookInteractableEvents()
        {
            if (interactable == null) return;

            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
        }

        private void UnhookInteractableEvents()
        {
            if (interactable == null) return;

            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (IsRayInteractor(args.interactorObject))
                _raySelectCount++;
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (IsRayInteractor(args.interactorObject))
                _raySelectCount = Mathf.Max(0, _raySelectCount - 1);
        }

        private static bool IsRayInteractor(IXRSelectInteractor interactor)
        {
            if (interactor == null) return false;

            // We intentionally only consider ray-based selection.
            // This allows you to gate "follow" behind holding a ray on her + grab/select.
            var ray = interactor.transform.GetComponentInParent<XRRayInteractor>();
            return ray != null;
        }

        private void Update()
        {
            if (!playerHead || illusionFace == null || animator == null || _agent == null)
                return;

            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                StopMovement();
                UpdateAnimator();
                return;
            }

            if (_isKissing)
            {
                StopMovement();
                UpdateAnimator();
                return;
            }

            float d = DistanceToPlayer();

            // Rule: if d < r1 -> attempt kiss (angle-gated), no forced facing.
            if (d < kissRange)
            {
                StopMovement();

                if (CheckKissAngles())
                    StartCoroutine(PerformKissRoutine());

                UpdateAnimator();
                return;
            }

            // Rule: if r1 < d <= r2 -> stay completely still.
            if (d <= minFollowRange)
            {
                StopMovement();
                UpdateAnimator();
                return;
            }

            // Rule: if d > r2 -> approach ONLY if a ray is selecting (grab held).
            if (_raySelectCount > 0)
                FollowPlayer();
            else
                StopMovement();

            UpdateAnimator();
        }

        private void FollowPlayer()
        {
            if (_agent.isStopped)
                _agent.isStopped = false;

            // Keep destination current; NavMeshAgent will pathfind.
            _agent.SetDestination(playerHead.position);
        }

        private void StopMovement()
        {
            if (!_agent.isStopped)
                _agent.isStopped = true;

            // Optional: clears lingering path cornering (prevents micro-jitter on resume in some setups).
            if (_agent.hasPath)
                _agent.ResetPath();
        }

        private bool CheckKissAngles()
        {
            // Direction from Illusion to Player
            Vector3 dirToPlayer = (playerHead.position - illusionFace.position).normalized;

            // Player forward direction
            Vector3 playerLookDir = playerHead.forward;

            // 1) Player within Illusion's forward cone (Illusion does NOT rotate in this refactor)
            float angleToPlayer = Vector3.Angle(illusionFace.forward, dirToPlayer);

            // 2) Illusion within Player's forward cone
            float angleToIllusion = Vector3.Angle(playerLookDir, -dirToPlayer);

            return angleToPlayer < fieldOfViewAngle && angleToIllusion < fieldOfViewAngle;
        }

        private IEnumerator PerformKissRoutine()
        {
            _isKissing = true;
            StopMovement();

            if (doUpdateSaveAfterKiss)
                SaveManager.SetCompleted(GameProgress.SeenIllusion);

            onKiss?.Invoke();

            // IMPORTANT per your rule: do NOT rotate Illusion to face the player.
            // No LookAt / RotateTowardsPlayer here.

            animator.SetTrigger(KissTriggerHash);

            yield return new WaitForSeconds(kissAnimationDuration);

            _cooldownTimer = kissCooldownDuration;
            _isKissing = false;
        }

        private void UpdateAnimator()
        {
            bool moving = !_agent.isStopped && _agent.velocity.sqrMagnitude > 0.1f;
            animator.SetBool(IsWalkingHash, moving);
        }

        private float DistanceToPlayer()
        {
            return Vector3.Distance(illusionFace.position, playerHead.position);
        }

        private void OnDrawGizmos()
        {
            if (illusionFace == null) return;

            Vector3 center = illusionFace.position;

            // r1 - kiss (red)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, kissRange);

            // r2 - min follow range (yellow)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, minFollowRange);

            // Field of view rays (cyan), scaled to kissRange for quick tuning
            Gizmos.color = Color.cyan;
            Vector3 forward = illusionFace.forward;
            Vector3 up = Vector3.up;
            Vector3 leftAxis = -illusionFace.right;

            Quaternion leftRayRotation = Quaternion.AngleAxis(-fieldOfViewAngle, up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(fieldOfViewAngle, up);
            Quaternion topRayRotation = Quaternion.AngleAxis(fieldOfViewAngle, leftAxis);
            Quaternion bottomRayRotation = Quaternion.AngleAxis(-fieldOfViewAngle, leftAxis);

            Gizmos.DrawRay(center, leftRayRotation * forward * kissRange);
            Gizmos.DrawRay(center, rightRayRotation * forward * kissRange);
            Gizmos.DrawRay(center, topRayRotation * forward * kissRange);
            Gizmos.DrawRay(center, bottomRayRotation * forward * kissRange);
        }
    }
}
