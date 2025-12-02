using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Expedition0.Environment
{
    public class DoorController : MonoBehaviour
    {
        [SerializeField] private bool useTrigger = true;
        [SerializeField] private bool locked = true;
        [SerializeField] private Animator animator;
        [SerializeField] private string openPropertyName = "IsOpen";
        [SerializeField] private bool startOpen = false;
        
        // Optional: State events
        public UnityEvent onDoorOpened;
        public UnityEvent onDoorClosed;
        public UnityEvent onDoorLocked;
        public UnityEvent onDoorUnlocked;
        
        private int _openPropertyHash = -1;
        
        private void Awake()
        {
            if (!animator) animator = GetComponent<Animator>();
            
            // Set initial state
            IsOpen = startOpen;
            if (animator)
            {
                _openPropertyHash = Animator.StringToHash(openPropertyName);
                animator.SetBool(_openPropertyHash, startOpen);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!useTrigger) return;
            if (other.CompareTag("Player"))
            {
                TryOpenDoor();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!useTrigger) return;
            if (other.CompareTag("Player"))
            {
                TryCloseDoor();
            }
        }

        // Public methods for interaction
        public void TryToggleDoor()
        {
            if (locked)
            {
                onDoorLocked?.Invoke();
                return;
            }
            
            if (IsOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
        
        public void TryOpenDoor()
        {
            if (locked || IsOpen) return;
            OpenDoor();
        }
        
        public void TryCloseDoor()
        {
            if (locked || !IsOpen) return;
            CloseDoor();
        }
        
        // Manual lock control
        public void UnlockDoor()
        {
            locked = false;
            onDoorUnlocked?.Invoke();
        }
        
        public void LockDoor()
        {
            locked = true;
            onDoorLocked?.Invoke();
        }
        
        public void SetLockedState(bool shouldLock)
        {
            locked = shouldLock;
            if (locked)
                onDoorLocked?.Invoke();
            else
                onDoorUnlocked?.Invoke();
        }
        
        private void OpenDoor()
        {
            if (!animator) return;

            IsOpen = true;
            onDoorOpened?.Invoke();
        }
        
        private void CloseDoor()
        {
            if (!animator) return;

            IsOpen = false;
            onDoorClosed?.Invoke();
        }
        
        public bool IsOpen
        {
            get => animator.GetBool(_openPropertyHash);
            set => animator.SetBool(_openPropertyHash, value);
        }
        public bool IsLocked => locked;
        
        private void OnDrawGizmos()
        {
            if (locked)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            }
            else
            {
                Gizmos.color = startOpen ? Color.green : Color.yellow;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            }
        }
    }
}