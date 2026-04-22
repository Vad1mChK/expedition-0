using System.Collections.Generic;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public class NonaryOperatorNodeView : NonaryLogicNodeView
    {
        [Header("Inputs")]
        [SerializeField] private LogicNodeView leftInputView;

        [SerializeField] private LogicNodeView rightInputView;

        [Header("Operator")]
        [SerializeField] private NonaryOperatorType initialOperator;
        
        [Tooltip("If assigned, overrides the default operator cycle order.")]
        public List<NonaryOperatorType> customCycleOrder;

        protected NonaryOperatorNode Node => (NonaryOperatorNode)Model;

        protected override void BuildModelInternal()
        {
            leftInputView.BuildModel();
            rightInputView.BuildModel();

            leftInputView.RegisterParent(this);
            rightInputView.RegisterParent(this);

            Model = new NonaryOperatorNode
            {
                leftInput = leftInputView.Model as NonaryLogicNode,
                rightInput = rightInputView.Model as NonaryLogicNode,
                op = initialOperator,
                locked = locked
            };
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