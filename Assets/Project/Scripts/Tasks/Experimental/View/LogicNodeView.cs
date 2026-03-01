using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Expedition0.Tasks.Experimental
{
    public abstract class LogicNodeView : MonoBehaviour
    {
        protected HashSet<LogicNodeView> parents = new(); // leave empty for root nodes
        public LogicNode Model { get; protected set; }

        public bool locked;

        [SerializeField] protected UnityEvent onClick;

        public virtual void BuildModel()
        {
            if (Model != null) return;
            BuildModelInternal();
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
    }
}