using System;
using UnityEngine;

namespace Expedition0.Save.Experimental
{
    [Serializable]
    public abstract class ExpeditionBaseSaveData<T> where T : class, new()
    {
        protected static T LoadTemplateFromResources(string fileName)
        {
            var jsonFile = Resources.Load<TextAsset>($"SaveSystem/{fileName}");
            if (jsonFile != null)
            {
                try
                {
                    return JsonUtility.FromJson<T>(jsonFile.text);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse {fileName}: {e.Message}");
                }
            }

            Debug.LogWarning($"Resource {fileName} not found in SaveSystem folder. Returning empty instance.");
            return new T();
        }
    }
}