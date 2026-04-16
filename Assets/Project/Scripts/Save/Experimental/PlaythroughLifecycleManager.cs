using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Expedition0.Save.Registries;
using JetBrains.Annotations;
using Unity.XR.CoreUtils;
using UnityEngine.Events;

namespace Expedition0.Save.Experimental
{
    public class PlaythroughLifecycleManager : MonoBehaviour
    {
        public static PlaythroughLifecycleManager Instance { get; private set; }

        [Header("Events")]
        public UnityEvent onBeforeChangeScene;

        [Header("Registries")]
        public LevelRegistry levelRegistry;

        [Header("State")]
        [SerializeField] private PlaythroughSaveData _currentData;
        public PlaythroughSaveData CurrentData => _currentData;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _currentData = NewSaveSystem.LoadPlaythrough();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void LoadLevel(string levelId)
        {
            string sceneName = levelRegistry.GetItem(levelId);
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"Respawn failed: Level ID {levelId} not found in registry.");
                return;
            }

            BeforeChangeScene();
            SceneManager.LoadScene(sceneName);
        }
        
        public void LoadRespawnLevel()
        {
            string respawnLevelId = _currentData.respawnLevel;
            LoadLevel(respawnLevelId);
        }

        public void ResetHealthAndIncrementDeath()
        {
            _currentData.currentHealth = PlaythroughSaveData.DefaultHealth;
            ++_currentData.deathCount;
        }

        public void RespawnAndLoadRespawnLevel()
        {
            ResetHealthAndIncrementDeath();
            LoadRespawnLevel();
        }

        public void SetRespawnLevel(string levelId)
        {
            _currentData.respawnLevel = levelId;
        }

        public void SavePlaythroughProgress()
        {
            NewSaveSystem.SavePlaythrough(_currentData);
        }

        public void ResetPlaythroughProgress()
        {
            NewSaveSystem.DeletePlaythrough();
            _currentData = PlaythroughSaveData.Default;
        }

        private void BeforeChangeScene()
        {
            onBeforeChangeScene?.Invoke();
            SavePlaythroughProgress();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"Entered {scene.name}, data synced.");
        }

        [ContextMenu("Print Current Save")]
        public void PrintCurrentSave()
        {
            var message = JsonUtility.ToJson(_currentData, prettyPrint: true);
            Debug.Log("[<b>PlaythroughLifecycleManager</b>] Current save:" + message);
        }
    }
}