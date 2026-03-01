namespace Expedition0.Tasks.Experimental
{
    public abstract class NonaryLogicNodeView : LogicNodeView
    {
        public sealed override int EvaluateInt() => ((NonaryLogicNode)Model).EvaluateInt();

        public override void Click()
        {
            if (locked || Model.locked) return;
            Model.Cycle();
            UpdateView();
            onClick?.Invoke();
        }
    }
}