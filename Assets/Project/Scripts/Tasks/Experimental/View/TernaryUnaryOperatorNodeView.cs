using System.Collections.Generic;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public class TernaryUnaryOperatorNodeView : TernaryLogicNodeView
    {
        [Header("Inputs")]
        public LogicNodeView inputView;

        [Header("Operator Settings")]
        public TernaryUnaryOperatorType initialOperator;

        [Tooltip("If assigned, overrides the default operator cycle order.")]
        public List<TernaryUnaryOperatorType> customCycleOrder;

        protected TernaryUnaryOperatorNode Node;

        protected override void BuildModelInternal()
        {
            inputView.BuildModel();
            inputView.RegisterParent(this);
            
            Node = new TernaryUnaryOperatorNode
            {
                input = inputView.Model as TernaryLogicNode,
                op = initialOperator,
                locked = locked
            };

            Model = Node;

            UpdateView();
        }

        public override void Click()
        {
            CycleOperator();
            UpdateView();
            onClick?.Invoke();
        }

        private void CycleOperator()
        {
            if (customCycleOrder != null && customCycleOrder.Count > 0)
            {
                // use custom
                int idx = customCycleOrder.IndexOf(Node.op);
                if (idx < 0) idx = 0;
                idx = (idx + 1) % customCycleOrder.Count;
                Node.op = customCycleOrder[idx];
            }
            else
            {
                // fallback to default cycle
                Node.op = Node.op.Next();
            }
        }
    }
}