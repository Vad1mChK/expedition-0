using UnityEngine;
using UnityEngine.Events;

namespace Expedition0.Environment.Artifacts
{
    public class ArtifactStandController : MonoBehaviour
    {
        [Header("Open/Closed")] 
        [SerializeField] private Animator animator;
        [SerializeField] private string openPropertyName = "IsOpen";
        [SerializeField] private UnityEvent onSuccessfulOpenToggle;
        [SerializeField] private UnityEvent onUnlock;
        [SerializeField] private bool locked = true;

        private int _openPropertyHash = -1;

        public bool Open
        {
            get => animator.GetBool(_openPropertyHash);
            set => SetOpen(value);
        }

        public bool Locked
        {
            get => locked;
            set => SetLocked(value);
        }
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if (!animator) animator = GetComponent<Animator>(); 
            _openPropertyHash = Animator.StringToHash(openPropertyName);
        }

        public void SetOpen(bool newOpen)
        {
            if (locked) return;
            animator.SetBool(_openPropertyHash, newOpen);
            onSuccessfulOpenToggle?.Invoke();
        }

        public void Toggle()
        {
            SetOpen(!Open);
        }

        public void SetLocked(bool newLocked)
        {
            locked = newLocked;
            if (!locked) onUnlock?.Invoke();
        }
    }
}