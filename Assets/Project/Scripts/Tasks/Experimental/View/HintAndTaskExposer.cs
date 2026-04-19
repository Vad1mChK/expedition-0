using System;
using System.Collections.Generic;
using System.Linq;
using Expedition0.Tasks.Experimental.Hint;
using UnityEngine;

namespace Expedition0.Tasks.Experimental.View
{
    public class HintAndTaskExposer : MonoBehaviour
    {
        [Serializable]
        public class HintTaskPair
        {
            public HintClient hintClient;
            public LogicTaskView taskView;
        }

        [SerializeField] private List<HintTaskPair> hintTaskPairs = new();

        // Counting tasks only in the current level

        public int TotalTaskCount { get; private set;  }
        public int CompletedTaskCount { get; private set; }
        public int HintCount { get; private set;  }

        private void Awake()
        {
            foreach (var hintTaskPair in hintTaskPairs)
            {
                if (hintTaskPair.taskView != null)
                {
                    // Add the listener only on first correct solution of the task
                    ++TotalTaskCount;
                    hintTaskPair.taskView.onFirstCorrect?.AddListener(MarkCompleted);
                }

                if (hintTaskPair.hintClient != null)
                {
                    ++HintCount;
                }
            }
        }

        public HintClient GetNearestHintClientToPlayer(GameObject player)
        {
            if (hintTaskPairs.Count == 0) return null;

            return hintTaskPairs
                .Select(pair => pair.hintClient)
                .OrderBy(hintClient => (hintClient.transform.position - player.transform.position).magnitude)
                .FirstOrDefault();
        }

        private void MarkCompleted()
        {
            CompletedTaskCount = Math.Clamp(CompletedTaskCount + 1, 0, TotalTaskCount);
        }

        private void OnDestroy()
        {
            foreach (var hintTaskPair in hintTaskPairs)
            {
                hintTaskPair.taskView.onFirstCorrect?.RemoveListener(MarkCompleted);
            }
        }
    }
}