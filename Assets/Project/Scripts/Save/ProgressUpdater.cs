using System;
using Expedition0.Save.Experimental;
using UnityEngine;

namespace Expedition0.Save
{
    [Serializable]
    public class PlaythroughDataEvent : UnityEngine.Events.UnityEvent<PlaythroughSaveData> { }

    public class ProgressUpdater : MonoBehaviour
    {
        [SerializeField] private PlaythroughDataEvent onUpdateProgress;

        public void MarkLevelComplete(string levelId)
        {
            var data = PlaythroughLifecycleManager.Instance.CurrentData;
            if (!data.completedLevels.Contains(levelId))
            {
                data.completedLevels.Add(levelId);
                PlaythroughLifecycleManager.Instance.SavePlaythroughProgress();
                onUpdateProgress?.Invoke(data);
            }
        }

        public void ForceTriggerUpdate()
        {
            onUpdateProgress?.Invoke(PlaythroughLifecycleManager.Instance.CurrentData);
        }
    }
}