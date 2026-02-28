using System;
using UnityEngine;
using UnityEngine.Events;

namespace Expedition0.Save
{
    public class ProgressUpdater: MonoBehaviour
    {
        [SerializeField] private GameProgressEvent onUpdateProgress;
        
        public void SetCompleted(GameProgress progress)
        {
            SaveManager.SetCompleted(progress);
            onUpdateProgress?.Invoke(SaveManager.LoadProgress());
        }

        public void ResetProgress()
        {
            SaveManager.ResetSave();
            onUpdateProgress?.Invoke((GameProgress)0);
        }
    }
}