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

        [Tooltip("The exact point on Illusion where distance is measured (e.g., eyes/lips).")]
        [SerializeField] private Transform illusionFace;

        [Tooltip("The animator that controls Illusion's animations.")]
        [SerializeField] private Animator animator;

        [Header("Kiss Parameters")]
        [Tooltip("Single radius: within this range the kiss can trigger (if mutual-facing).")]
        [SerializeField] private float kissRange = 0.5f;

        [Header("Angle Parameters")]
        [Tooltip("Max angle for mutual facing (both must be within this angle).")]
        [Range(0f, 90f)]
        [SerializeField] private float fieldOfViewAngle = 45f;

        [Header("Events")]
        [SerializeField] private UnityEvent onKiss;

        [Header("Durations")]
        [SerializeField] private float kissCooldownDuration = 5f;

        [Tooltip("Match this to the kiss animation clip length.")]
        [SerializeField] private float kissAnimationDuration = 2f;

        [Header("Save")]
        [SerializeField] private bool doUpdateSaveAfterKiss = true;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private enum AIState
        {
            Idle,
            Approaching,
            Kissing,
            Cooldown
        }

        [SerializeField] private AIState currentState;

        private NavMeshAgent _agent;
        private XRSimpleInteractable _interactable;

        private float _cooldownTimer;

        private int _rayHoverCount;
        private int _raySelectCount;

        private bool _kissRoutineRunning;

        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
        private static readonly int KissTriggerHash = Animator.StringToHash("Kiss");

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _interactable = GetComponent<XRSimpleInteractable>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (illusionFace == null)
                illusionFace = transform;

            if (playerHead == null && Camera.main != null)
                playerHead = Camera.main.transform;

            kissRange = Mathf.Max(0.1f, kissRange);

            HookInteractableEvents();
        }

        private void OnDestroy()
        {
            UnhookInteractableEvents();
        }

        private void HookInteractableEvents()
        {
            if (_interactable == null)
                return;

            _interactable.hoverEntered.AddListener(OnHoverEntered);
            _interactable.hoverExited.AddListener(OnHoverExited);
            _interactable.selectEntered.AddListener(OnSelectEntered);
            _interactable.selectExited.AddListener(OnSelectExited);
        }

        private void UnhookInteractableEvents()
        {
            if (_interactable == null)
                return;

            _interactable.hoverEntered.RemoveListener(OnHoverEntered);
            _interactable.hoverExited.RemoveListener(OnHoverExited);
            _interactable.selectEntered.RemoveListener(OnSelectEntered);
            _interactable.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (IsFarRayLikeInteractor(args.interactorObject))
            {
                _rayHoverCount++;
            }
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            if (IsFarRayLikeInteractor(args.interactorObject))
            {
                _rayHoverCount = Mathf.Max(0, _rayHoverCount - 1);
            }
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            // Treat "grab button down" as selecting the interactable with a ray.
            if (IsFarRayLikeInteractor(args.interactorObject))
            {
                _raySelectCount++;
            }
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (IsFarRayLikeInteractor(args.interactorObject))
            {
                _raySelectCount = Mathf.Max(0, _raySelectCount - 1);
            }
        }

        private void Update()
        {
            if (playerHead == null || illusionFace == null || animator == null)
                return;

            float d = DistanceToPlayer();

            switch (currentState)
            {
                case AIState.Idle:
                    HandleIdle(d);
                    break;

                case AIState.Approaching:
                    HandleApproaching(d);
                    break;

                case AIState.Kissing:
                    // Coroutine-driven
                    break;

                case AIState.Cooldown:
                    HandleCooldown(d);
                    break;
            }

            UpdateAnimator();
        }

        private void HandleIdle(float d)
        {
            _agent.isStopped = true;

            if (currentState == AIState.Cooldown || currentState == AIState.Kissing)
                return;

            if (d < kissRange)
            {
                TryStartKiss();
                return;
            }

            // New rule: only approach if ray is held on her AND grab/select is down
            if (ShouldApproach() && d >= kissRange)
            {
                currentState = AIState.Approaching;
            }
        }

        private void HandleApproaching(float d)
        {
            if (!ShouldApproach())
            {
                _agent.isStopped = true;
                currentState = AIState.Idle;
                return;
            }

            if (d < kissRange)
            {
                _agent.isStopped = true;
                currentState = AIState.Idle;
                TryStartKiss();
                return;
            }

            _agent.isStopped = false;
            _agent.SetDestination(playerHead.position);
        }

        private void HandleCooldown(float d)
        {
            _agent.isStopped = true;

            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer > 0f)
                return;

            currentState = AIState.Idle;

            // After cooldown, do not auto-approach.
            // She only moves again if guided by ray+grab.
            if (d < kissRange)
                TryStartKiss();
        }

        private bool ShouldApproach()
        {
            // "one ray is held on her (and grab button is down)"
            // Here: ray hovering + ray selecting (grab held)
            return _raySelectCount > 0 && currentState != AIState.Cooldown && currentState != AIState.Kissing;
        }

        private void TryStartKiss()
        {
            if (currentState == AIState.Cooldown || currentState == AIState.Kissing)
                return;

            if (_kissRoutineRunning)
                return;

            if (!IsWithinKissRange())
                return;

            // New rule: she does NOT turn to face player; mutual-facing required
            if (!CheckMutualFacing())
                return;

            StartCoroutine(PerformKissRoutine());
        }

        private bool IsWithinKissRange()
        {
            return DistanceToPlayer() < kissRange;
        }

        private bool CheckMutualFacing()
        {
            // Direction from Illusion -> Player
            Vector3 dirToPlayer = (playerHead.position - illusionFace.position).normalized;

            // Illusion's forward should point toward player (within angle)
            float illusionToPlayerAngle = Vector3.Angle(illusionFace.forward, dirToPlayer);

            // Player's forward should point toward Illusion (within angle)
            Vector3 dirToIllusion = -dirToPlayer;
            float playerToIllusionAngle = Vector3.Angle(playerHead.forward, dirToIllusion);

            return illusionToPlayerAngle < fieldOfViewAngle && playerToIllusionAngle < fieldOfViewAngle;
        }

        private IEnumerator PerformKissRoutine()
        {
            _kissRoutineRunning = true;
            currentState = AIState.Kissing;
            _agent.isStopped = true;

            if (doUpdateSaveAfterKiss)
            {
                SaveManager.SetCompleted(GameProgress.SeenIllusion);
            }

            onKiss?.Invoke();

            animator.SetTrigger(KissTriggerHash);

            yield return new WaitForSeconds(kissAnimationDuration);

            _cooldownTimer = kissCooldownDuration;
            currentState = AIState.Cooldown;

            _kissRoutineRunning = false;
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
            if (!drawGizmos)
                return;

            if (illusionFace == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(illusionFace.position, Mathf.Max(0.1f, kissRange));
        }
        
        private static bool IsFarRayLikeInteractor(IXRInteractor interactorObject)
        {
            return interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor
                   || interactorObject is XRRayInteractor;
        }
    }
}
