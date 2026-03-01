using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public abstract class TernaryLogicNodeView : LogicNodeView
    {
        [Header("Wires")]
        [SerializeField] protected bool usesWires = true;
        [ShowIf(nameof(usesWires))]
        [SerializeField] protected List<TernaryWireView> connectedWires;
        
        public virtual Trit EvaluateTrit() => (Trit)Model.EvaluateInt();

        public override void UpdateView()
        {
            base.UpdateView();
            
            var currentValue = EvaluateTrit();
            if (connectedWires != null && connectedWires.Count > 0)
            {
                UpdateWires(currentValue);
            }
        }
        
        protected virtual void UpdateWires(Trit value)
        {
            foreach (var wire in connectedWires)
            {
                wire.TritValue = value;
            }
        }
    }
}