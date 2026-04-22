using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Expedition0.Tasks.Experimental
{
    public abstract class LogicNodeView : MonoBehaviour
    {
        protected HashSet<LogicNodeView> parents = new(); // leave empty for root nodes
        public LogicNode Model { get; protected set; }

        public bool locked;

        [Header("Interactions")]
        [SerializeField] protected UnityEvent onClick;
        [CanBeNull] [SerializeField] protected XRSimpleInteractable interactable;

        private void Awake()
        {
            TryFindInteractable();
        }

        private void Start()
        {
            if (interactable == null)
                TryFindInteractable();
        }

        private void TryFindInteractable()
        {
            if (interactable != null) return;

            // 1. Try local first
            interactable = GetComponent<XRSimpleInteractable>();

            // 2. Try children (even if disabled)
            if (interactable == null)
            {
                var found = GetComponentsInChildren<XRSimpleInteractable>(true);
                if (found.Length > 0)
                    interactable = found[0];
            }

            if (interactable == null)
                Debug.LogError($"{name}: No XRSimpleInteractable found!");
        }

        public virtual void BuildModel()
        {
            if (Model != null) return;
            BuildModelInternal();
            TryFindInteractable();
            ApplyLockState();
        }

        protected abstract void BuildModelInternal();
        
        public void RegisterParent(LogicNodeView parent)
        {
            parents.Add(parent);
        }

        public virtual int EvaluateInt() => Model.EvaluateInt();

        public virtual void Click()
        {
            if (Model.locked) return;
            
            Model.Cycle();
            UpdateView();
            onClick?.Invoke();
        }

        public virtual void UpdateView()
        {
            UpdateParentsViews();
        }

        public virtual void UpdateParentsViews()
        {
            foreach (var parent in parents)
            {
                parent.UpdateView();
            }
        }
        
        protected void ApplyLockState()
        {
            if (interactable == null)
                return;

            var manager = interactable.interactionManager;

            if (locked || Model.locked)
            {
                // Cancel current interactions
                if (interactable is IXRSelectInteractable si && manager != null)
                    manager.CancelInteractableSelection(si);

                if (interactable is IXRHoverInteractable hi && manager != null)
                    manager.CancelInteractableHover(hi);

                // Unregister completely
                if (interactable is IXRInteractable inter && manager != null)
                    manager.UnregisterInteractable(inter);

                // Disable collider
                foreach (var col in interactable.colliders)
                    col.enabled = false;

                interactable.enabled = false;
            }
            else
            {
                // Re-enable collider
                foreach (var col in interactable.colliders)
                    col.enabled = true;

                interactable.enabled = true;

                if (interactable is IXRInteractable inter && manager != null)
                    manager.RegisterInteractable(inter);
            }
        }
    }
}