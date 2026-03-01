using Expedition0.Tasks.Experimental.Json;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public class LogicEqualityTaskView : LogicTaskView
    {
        public LogicNodeView leftRoot;
        public LogicNodeView rightRoot;
        public void Awake()
        {
            BuildTaskGraph();
        }

        private void BuildTaskGraph()
        {
            leftRoot.BuildModel();
            rightRoot.BuildModel();
        }

        protected override bool ValidateTaskInternal()
        {
            var left = leftRoot.EvaluateInt();
            var right = rightRoot.EvaluateInt();
            return left == right;
        }
        
        [ContextMenu("Test/JSON serialize left")]
        public void TestSerializeLeft()
        {
            if (leftRoot == null || leftRoot.Model == null)
            {
                Debug.LogWarning("Left root model is not initialized");
            }

            var model = leftRoot.Model;
            var dto = TaskSerializer.SerializeTask(model);
            var json = JsonUtility.ToJson(dto);
            Debug.Log($"<b><color=cyan>Left root model: {json}</color></b>");
        }
        
        [ContextMenu("Test/JSON serialize right")]
        public void TestSerializeRight()
        {
            if (rightRoot == null || rightRoot.Model == null)
            {
                Debug.LogWarning("Right root model is not initialized");
            }

            var model = rightRoot.Model;
            var dto = TaskSerializer.SerializeTask(model);
            var json = JsonUtility.ToJson(dto);
            Debug.Log($"<b><color=cyan>Right root model: {json}</color></b>");
        }
    }
}