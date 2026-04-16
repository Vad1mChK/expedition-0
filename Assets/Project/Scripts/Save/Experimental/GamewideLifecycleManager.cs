using System.Linq;
using Expedition0.Save.Experimental;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Expedition0.Save.Experimental
{
    public class GamewideLifecycleManager : MonoBehaviour
    {
        public static GamewideLifecycleManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GamewideSaveData _currentData;
        public GamewideSaveData CurrentData => _currentData;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _currentData = NewSaveSystem.LoadGamewide();
        }

        public void SaveGamewideProgress()
        {
            NewSaveSystem.SaveGamewide(_currentData);
        }
        
        public void UnlockMusic(string musicId)
        {
            if (!_currentData.musicUnlocked.Contains(musicId)) 
                _currentData.musicUnlocked.Add(musicId);
            SaveGamewideProgress();
        }
        
        public void UnlockLevel(string levelId)
        {
            if (!_currentData.levelsUnlocked.Contains(levelId)) 
                _currentData.levelsUnlocked.Add(levelId);
            SaveGamewideProgress();
        }
        
        [ContextMenu("Print Current Save")]
        public void PrintCurrentSave()
        {
            var message = JsonUtility.ToJson(_currentData, prettyPrint: true);
            Debug.Log("[<b>GamewideLifecycleManager</b>] Current save:" + message);
        }
    }
}