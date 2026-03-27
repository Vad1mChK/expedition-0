using System.IO;
using UnityEditor;
using UnityEngine;
using Expedition0.Save.Experimental;

namespace Expedition0.EditorTools
{
    public class SaveSystemTools
    {
        private const string MENU_ROOT = "E0 Tools/New Save System/";
        private const string GamewideFileName = "gamewideSave.json";
        private const string PlaythroughFileName = "playthroughSave.json";
        
        // This works in Editor and points to AppData/LocalLow on Windows
        private static string BasePath => Application.persistentDataPath;

        [MenuItem(MENU_ROOT + "Show Save Folder in Explorer")]
        public static void OpenExplorer()
        {
            if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);
            EditorUtility.RevealInFinder(BasePath);
        }
        
        [MenuItem(MENU_ROOT + "Ensure saves created")]
        public static void EnsureSavesCreated()
        {
            var gamewideSave = NewSaveSystem.LoadGamewide() ?? GamewideSaveData.Default;
            NewSaveSystem.SaveGamewide(gamewideSave);

            var playthroughSave = NewSaveSystem.LoadPlaythrough() ?? PlaythroughSaveData.Default;
            NewSaveSystem.SavePlaythrough(playthroughSave);
            
            Debug.Log("<b><color=green>[New Save System]</color></b> Saves created if don't exist already");
        }

        [MenuItem(MENU_ROOT + "Print Current Playthrough")]
        public static void PrintCurrentPlaythroughSave()
        {
            string path = Path.Combine(BasePath, PlaythroughFileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                Debug.Log($"<b><color=cyan>[Playthrough Save]</color></b> Path: {path}\n{json}");
            }
            else
            {
                Debug.LogWarning("New Save System: No Playthrough save found at " + path);
            }
        }

        [MenuItem(MENU_ROOT + "Print Current Gamewide")]
        public static void PrintCurrentGamewide()
        {
            string path = Path.Combine(BasePath, GamewideFileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                Debug.Log($"<b><color=orange>[Gamewide Save]</color></b> Path: {path}\n{json}");
            }
            else
            {
                Debug.LogWarning("New Save System: No Gamewide save found at " + path);
            }
        }

        [MenuItem(MENU_ROOT + "Wipe All Local Saves")]
        public static void WipeSaves()
        {
            if (EditorUtility.DisplayDialog("Wipe Saves?", "Delete all local gamewide and playthrough JSONs?", "Okay", "Cancel"))
            {
                if (File.Exists(Path.Combine(BasePath, GamewideFileName))) 
                    File.Delete(Path.Combine(BasePath, GamewideFileName));
                if (File.Exists(Path.Combine(BasePath, PlaythroughFileName))) 
                    File.Delete(Path.Combine(BasePath, PlaythroughFileName));
                
                Debug.Log("<color=red>Saves wiped successfully.</color>");
            }
        }
        
        [MenuItem(MENU_ROOT + "Juxtapose Gamewide vs Perfect")]
        public static void Juxtapose()
        {
            var current = NewSaveSystem.LoadGamewide();
            var perfect = GamewideSaveData.Perfect; // Loads from Resources

            Debug.Log($"<b>Progress:</b> " +
                      $"{current.charactersUnlocked.Count}/{perfect.charactersUnlocked.Count} Characters, " +
                      $"{current.endingsUnlocked.Count}/{perfect.endingsUnlocked.Count} Endings.");
    
            // Check for "Illegal" entries (things in save that aren't in perfect)
            foreach (var id in current.charactersUnlocked)
            {
                if (!perfect.charactersUnlocked.Contains(id))
                    Debug.LogWarning($"Unknown character ID in save: {id}");
            }
        }
    }
}