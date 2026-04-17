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
        public ItemRegistry itemRegistry;

        [Header("State")]
        [SerializeField] private PlaythroughSaveData _currentData;
        public PlaythroughSaveData CurrentData => _currentData;
        
        private string _currentLevelId;
        private Vector3 _respawnPosition;
        private Quaternion _respawnRotation;
        
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
        
        public void RegisterCurrentLevel(string id, Vector3 pos, Quaternion rot)
        {
            _currentLevelId = id;
            _respawnPosition = pos;
            _respawnRotation = rot;
        }

        public void LoadLevel(string levelId)
        {
            string sceneName = levelRegistry.GetItem(levelId);
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"[<b>PlaythroughLifecycleManager</b>] LoadLevel: Failed. Level ID {levelId} not found in registry.");
                return;
            }

            BeforeChangeScene();
            SceneManager.LoadScene(sceneName);
        }
        
        public void LoadRespawnLevel()
        {
            string respawnLevelId = _currentData.respawnLevel;
            
            ResetHealthAndIncrementDeath();
            if (_currentLevelId == respawnLevelId)
            {
                Debug.Log("[<b>PlaythroughLifecycleManager</b>] LoadRespawnLevel: Same scene respawn. Warping player.");
                WarpPlayerWithinSameLevel();
            }
            else
            {
                LoadLevel(respawnLevelId);
            }
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
            Debug.Log($"[<b>PlaythroughLifecycleManager</b>] Current respawn level set to {levelId}");
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
        
        private void WarpPlayerWithinSameLevel()
        {
            // Assuming there is only one XR Origin in scene
            var origin = FindFirstObjectByType<XROrigin>();
            if (origin != null)
            {
                // Move the whole rig back to the saved coordinates
                origin.transform.position = _respawnPosition;
                origin.transform.rotation = _respawnRotation;
            }
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