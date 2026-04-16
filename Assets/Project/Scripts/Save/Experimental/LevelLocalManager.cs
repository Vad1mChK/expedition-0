using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Expedition0.Save.Experimental
{
    public class LevelLocalManager : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private string levelId; // e.g., "e0:bridge_corridor"
        [SerializeField] private bool completeLevelBeforeChange;
        [SerializeField] private bool autoUnlockThisLevelGamewide;
        
        [Header("Events")]
        [Tooltip("Triggered when this level starts and finds the Lifecycle Manager")]
        public UnityEvent<string> onLevelInitialized;
        // public UnityEvent onBeforeChangeScene;

        private void Start()
        {
            onLevelInitialized?.Invoke(levelId);
            
            if (PlaythroughLifecycleManager.Instance != null)
            {
                // Subscribe a local method to the global "Before Change" event
                PlaythroughLifecycleManager.Instance.onBeforeChangeScene.AddListener(OnManagerPreparingToLeave);
            }
            else
            {
                Debug.LogWarning("[<b>LevelLocalManager</b>] PlaythroughLifecycleManager instance is null");
            }
            
            if (autoUnlockThisLevelGamewide) UnlockThisLevelGamewide();
        }

        // Methods for scene-objects to call via UnityEvents or direct ref
        
        public void SetAsRespawnLevel() => 
            PlaythroughLifecycleManager.Instance.SetRespawnLevel(levelId);

        public void CommitSave() =>
            PlaythroughLifecycleManager.Instance.SavePlaythroughProgress();

        public void LoadRespawnLevel() => 
            PlaythroughLifecycleManager.Instance.LoadRespawnLevel();

        public void LoadLevel(string newLevelId) =>
            PlaythroughLifecycleManager.Instance.LoadLevel(newLevelId);

        public void IncrementTaskSolvedCount() => 
            ++PlaythroughLifecycleManager.Instance.CurrentData.taskSolvedCount;

        public void IncrementTaskMistakeCount() =>
            ++PlaythroughLifecycleManager.Instance.CurrentData.taskMistakeCount;

        public void IncrementEnemiesDefeatedCount() =>
            ++PlaythroughLifecycleManager.Instance.CurrentData.enemyDefeatedCount;

        public void SetInventory(List<PlaythroughInventoryEntry> newInventory)
        {
            PlaythroughLifecycleManager.Instance.CurrentData.inventory.Clear();
            PlaythroughLifecycleManager.Instance.CurrentData.inventory.AddRange(newInventory);
        }
        
        public void UpdateCurrentHealth(float newHealth) => 
            PlaythroughLifecycleManager.Instance.CurrentData.currentHealth = newHealth;

        public void CompleteThisLevel()
        {
            Debug.Log($"[<b>LevelLocalManager</b>] CompleteThisLevel: Completing level {levelId}");
            if (!PlaythroughLifecycleManager.Instance.CurrentData.completedLevels.Contains(levelId))
            {
                PlaythroughLifecycleManager.Instance.CurrentData.completedLevels.Add(levelId);
            }
        }

        public void UnlockThisLevelGamewide()
        {
            GamewideLifecycleManager.Instance?.UnlockLevel(levelId);
        }

        public void UnlockMusicGamewide(string musicId)
        {
            GamewideLifecycleManager.Instance?.UnlockMusic(musicId);
        }
        
        private void OnManagerPreparingToLeave()
        {
            // Do last minute cleanup or data pushing
            Debug.Log($"Level {levelId} is cleaning up before the scene changes.");
    
            // Example: Save current state to the data object before the file is written
            if (completeLevelBeforeChange) CompleteThisLevel();
            // onBeforeChangeScene?.Invoke();
        }
        
        private void OnDestroy()
        {
            if (PlaythroughLifecycleManager.Instance != null)
            {
                PlaythroughLifecycleManager.Instance.onBeforeChangeScene.RemoveListener(OnManagerPreparingToLeave);
            }
        }
    }
}