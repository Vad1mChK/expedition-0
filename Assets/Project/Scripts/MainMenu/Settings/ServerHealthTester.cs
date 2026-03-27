using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace Expedition0.MainMenu.Settings
{
    public class ServerHealthTester : MonoBehaviour
    {
        [SerializeField] private ServerSettingsController formController;

        public void RunHealthCheck()
        {
            StartCoroutine(TestConnectionRoutine(formController.GetFullAddress()));
        }

        private IEnumerator TestConnectionRoutine(string baseUrl)
        {
            string url = $"{baseUrl}/api/health";
            Debug.Log($"Connecting to address: {url}");
            formController.SetStatusString($"Connecting to {url}", ServerSettingsController.Status.Waiting);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // Set a short timeout for responsiveness
                request.timeout = 5;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    formController.SetStatusString("Connection successful", ServerSettingsController.Status.Success);
                    Debug.Log("Health check passed!");
                }
                else
                {
                    formController.SetStatusString("Connection failed", ServerSettingsController.Status.Error);
                    Debug.LogWarning($"Health check failed: {request.error}");
                }
            }
        }
    }
}