using System.IO;
using UnityEngine;

namespace Expedition0.Save.Experimental
{
    public static class NewSaveSystem
    {
        private const string GamewideFileName = "gamewideSave.json";
        private const string PlaythroughFileName = "playthroughSave.json";
        private static string BasePath => Application.persistentDataPath;

        public static void SaveGamewide(GamewideSaveData data) =>
            SaveFile(data, GamewideFileName);

        public static void SavePlaythrough(PlaythroughSaveData data) =>
            SaveFile(data, PlaythroughFileName);

        public static GamewideSaveData LoadGamewide() =>
            LoadFile<GamewideSaveData>(GamewideFileName) ?? GamewideSaveData.Default;

        public static PlaythroughSaveData LoadPlaythrough() =>
            LoadFile<PlaythroughSaveData>(PlaythroughFileName) ?? PlaythroughSaveData.Default;

        public static void DeletePlaythrough()
        {
            string path = Path.Combine(BasePath, PlaythroughFileName);
            if (File.Exists(path)) File.Delete(path);
        }

        private static void SaveFile<T>(T data, string fileName)
        {
            string fullPath = Path.Combine(BasePath, fileName);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(fullPath, json);
        }

        private static T LoadFile<T>(string fileName) where T : class
        {
            string fullPath = Path.Combine(BasePath, fileName);
            if (!File.Exists(fullPath)) return null;

            string json = File.ReadAllText(fullPath);
            return JsonUtility.FromJson<T>(json);
        }
    }
}