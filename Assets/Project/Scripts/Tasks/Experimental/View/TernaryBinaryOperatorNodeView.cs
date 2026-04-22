using System.Collections.Generic;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public class TernaryBinaryOperatorNodeView : TernaryLogicNodeView
    {
        [Header("Inputs")]
        public LogicNodeView leftInputView;
        public LogicNodeView rightInputView;

        [Header("Operator Settings")]
        public TernaryBinaryOperatorType initialOperator;

        [Tooltip("If assigned, overrides the default operator cycle order.")]
        public List<TernaryBinaryOperatorType> customCycleOrder;

        protected TernaryBinaryOperatorNode Node;

        protected override void BuildModelInternal()
        {
            leftInputView.BuildModel();
            rightInputView.BuildModel();
            
            leftInputView.RegisterParent(this);
            rightInputView.RegisterParent(this);
            
            Node = new TernaryBinaryOperatorNode
            {
                leftInput = leftInputView.Model as TernaryLogicNode,
                rightInput = rightInputView.Model as TernaryLogicNode,
                op = initialOperator,
                locked = locked
            };

            Model = Node;
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