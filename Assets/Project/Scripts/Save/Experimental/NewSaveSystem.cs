using System;
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
            LoadFile<GamewideSaveData>(GamewideFileName, GamewideSaveData.SerialVersionId) ?? GamewideSaveData.Default;

        public static PlaythroughSaveData LoadPlaythrough() =>
            LoadFile<PlaythroughSaveData>(PlaythroughFileName, PlaythroughSaveData.SerialVersionId) ?? PlaythroughSaveData.Default;

        public static void DeletePlaythrough() => DeleteFile(PlaythroughFileName);

        public static void DeleteGamewide() => DeleteFile(GamewideFileName);

        private static void SaveFile<T>(T data, string fileName)
        {
            string fullPath = Path.Combine(BasePath, fileName);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(fullPath, json);
        }

        private static void DeleteFile(string fileName)
        {
            string path = Path.Combine(BasePath, fileName);
            if (File.Exists(path)) File.Delete(path);
        }

        private static T LoadFile<T>(string fileName, int? expectedVersion = null) where T : ExpeditionBaseSaveData<T>, new()
        {
            string fullPath = Path.Combine(BasePath, fileName);
            if (!File.Exists(fullPath)) return null;

            try
            {
                string json = File.ReadAllText(fullPath);
                T data = JsonUtility.FromJson<T>(json);

                if (expectedVersion != null && data.saveVersion != expectedVersion)
                {
                    Debug.LogWarning($"Version mismatch for {fileName} (expected {expectedVersion}, got {data.saveVersion}). Dropping save.");
                    return null;
                }
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load {fileName}: {e.Message}");
                return null;
            }
        }
    }
}