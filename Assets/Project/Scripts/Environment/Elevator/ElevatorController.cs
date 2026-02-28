using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Expedition0.Save;
using Expedition0.Visuals;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Expedition0.Environment.Elevator
{
    public enum ElevatorLevel
    {
        Lobby,
        Greenhouse,
        OuterSkeleton,
        MachineHall,
        Death,
        SpaceCombat,
        IllusionRoom,
        Victory
    }

    public enum ElevatorKind
    {
        StartOfLevel,
        EndOfLevel
    }

    [Serializable]
    public class ElevatorLevelIconEntry
    {
        public ElevatorLevel level;
        [Header("Unlocked")]
        public Sprite leftUnlockedSprite;
        public Sprite rightUnlockedSprite;
        [Header("Locked")]
        public Sprite leftLockedSprite;
        public Sprite rightLockedSprite;
    }

    [RequireComponent(typeof(Animator))]
    public class ElevatorController : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator animator;
        [SerializeField] private string openPropertyName = "IsOpen";
        [SerializeField] private UnityEvent<bool> onSuccessfulDoorToggle;

        [Header("Elevator State")]
        [SerializeField] private ElevatorKind kind;
        [SerializeField] private ElevatorLevel elevatorLevel;
        [SerializeField] private string sceneToLoad;
        [SerializeField] private List<ProgressBasedConditional<bool>> lockedConditions;
        [SerializeField] private bool defaultLocked = false;
        [SerializeField] private float delayUntilTransport = 2.5f;
        [SerializeField] private GameProgress progressBeforeTransport;
        [SerializeField] private VisualEffectsController vfx;

        [Header("Elevator Status Icon")]
        [SerializeField] private SpriteRenderer leftSpriteRenderer;
        [SerializeField] private SpriteRenderer rightSpriteRenderer;
        [SerializeField] private Material unlockedIconMaterial;
        [SerializeField] private Material lockedIconMaterial;
        [SerializeField] private List<ElevatorLevelIconEntry> icons;

        private int _openPropertyHash = -1;
        private bool _locked;
        private bool _transportQueued;

        /// <summary>Animator bool (IsOpen by default).</summary>
        public bool Open
        {
            get => animator.GetBool(_openPropertyHash);
            private set => animator.SetBool(_openPropertyHash, value);
        }

        public bool Locked => _locked;
        public ElevatorKind Kind => kind;
        public float DelayUntilTransport => delayUntilTransport;

        void Reset()
        {
            animator = GetComponent<Animator>();
        }

        void Awake()
        {
            if (!animator) animator = GetComponent<Animator>();
            _openPropertyHash = Animator.StringToHash(openPropertyName);
        }

        void Start()
        {
            if (kind == ElevatorKind.StartOfLevel)
            {
                // Start-of-level elevator: open and unlocked when player spawns inside.
                SetLocked(false);
                OpenDoor();
            }
            else
            {
                // End-of-level elevator: lock state depends on progress.
                ReevaluateLockState();
                if (Locked)
                    CloseDoor();
            }
        }

        // ----- Lock state evaluation -----

        public void ReevaluateLockState()
        {
            bool locked = defaultLocked;
            var progress = SaveManager.LoadProgress();

            foreach (var cond in lockedConditions)
            {
                if (cond != null && cond.SatisfiedFor(progress))
                {
                    // Last matching condition wins
                    locked = cond.outcome;
                }
            }
            
            Debug.Log($"Lock state of end-level elevator {gameObject.name} evaluated to {locked}");

            SetLocked(locked);
        }

        public void SetLocked(bool newLocked)
        {
            bool prev = _locked;
            _locked = newLocked;
            if (prev != newLocked)
            {
                UpdateVisualizationColor();
                UpdateVisualizationIcons();
            }

            // If we lock the elevator, ensure doors are closed.
            if (_locked && Open)
                CloseDoor();
        }

        // ----- Public door API -----

        public void OpenDoor()
        {
            if (Locked || Open) return;
            Open = true;
            onSuccessfulDoorToggle?.Invoke(true);
        }

        public void CloseDoor()
        {
            if (!Open) return;
            Open = false;
            onSuccessfulDoorToggle?.Invoke(false);
        }

        // Called by trigger when player steps fully inside an unlocked end-of-level elevator.
        public void BeginTransportIfPossible()
        {
            if (kind != ElevatorKind.EndOfLevel || Locked || _transportQueued)
                return;

            _transportQueued = true;
            EnsureObjectiveCompleted(progressBeforeTransport);
            CloseDoor();
            StartCoroutine(TransportAfterDelay());
        }

        private IEnumerator TransportAfterDelay()
        {
            yield return new WaitForSeconds(delayUntilTransport);
            
            if (vfx) // reference to VisualEffectsController
                yield return StartCoroutine(Fade01(vfx, 0f, 1f, 1f));
            
            if (!string.IsNullOrEmpty(sceneToLoad))
                SceneManager.LoadScene(sceneToLoad);
            
            // Wait one frame so XR rig is placed
            yield return null;

            // Fade in
            if (vfx)
                yield return StartCoroutine(Fade01(vfx, 1f, 0f, 1f));
        }
        
        private IEnumerator Fade01(VisualEffectsController vfx, float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                float value = Mathf.Lerp(from, to, k);
                vfx.SetFade01(value);
                yield return null;
            }
            vfx.SetFade01(to);
        }

        // ----- Icons & materials -----

        private void UpdateVisualizationIcons()
        {
            if (!leftSpriteRenderer || !rightSpriteRenderer) return;

            var icon = icons?.FirstOrDefault(e => e.level == elevatorLevel);
            if (icon == null) return;

            if (_locked)
            {
                leftSpriteRenderer.sprite = icon.leftLockedSprite;
                rightSpriteRenderer.sprite = icon.rightLockedSprite;
            }
            else
            {
                leftSpriteRenderer.sprite = icon.leftUnlockedSprite;
                rightSpriteRenderer.sprite = icon.rightUnlockedSprite;
            }
        }

        private void UpdateVisualizationColor()
        {
            if (!leftSpriteRenderer || !rightSpriteRenderer) return;

            var material = _locked ? lockedIconMaterial : unlockedIconMaterial;
            leftSpriteRenderer.material = material;
            rightSpriteRenderer.material = material;
        }

        private void EnsureObjectiveCompleted(GameProgress progress)
        {
            SaveManager.SetCompleted(progress);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!animator)
                animator = GetComponent<Animator>();

            UpdateVisualizationIcons();
            UpdateVisualizationColor();
        }
#endif
    }
}
