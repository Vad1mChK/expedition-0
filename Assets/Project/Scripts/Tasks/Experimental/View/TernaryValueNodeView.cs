using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public class TernaryValueNodeView : TernaryLogicNodeView
    {
        [Header("Initial Value")]
        public Trit initialValue = Trit.Neutral;

        protected override void BuildModelInternal()
        {
            Model = new TernaryValueNode
            {
                currentValue = initialValue,
                locked = locked
            };
        }
    }
}