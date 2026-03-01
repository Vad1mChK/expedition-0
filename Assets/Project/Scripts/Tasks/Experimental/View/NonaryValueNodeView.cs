using UnityEditor;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public class NonaryValueNodeView : NonaryLogicNodeView
    {
        [Header("Initial Value")]
        [SerializeField] protected int initialValue = 0;

        protected NonaryValueNode Node => (NonaryValueNode)Model;

        protected override void BuildModelInternal()
        {
            Model = new NonaryValueNode
            {
                currentValue = Mathf.Clamp(initialValue, 0, 8)
            };
        }
    }
}