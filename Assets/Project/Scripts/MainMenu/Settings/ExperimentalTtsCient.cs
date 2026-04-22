using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Expedition0.Util;

namespace Expedition0.MainMenu.Settings
{
    [RequireComponent(typeof(AudioSource))]
    public class ExperimentalTtsClient : MonoBehaviour
    {
        [Serializable]
        public class ExperimentalTtsRequest
        {
            public string text;
            public string provider;
            public string voice;
        }
        
        private AudioSource audioSource;
        
        private const string HostKey = "ServerHost";
        private const string PortKey = "ServerPort";
        
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void GenerateAndPlay(string textToSpeak)
        {
            StartCoroutine(PostTTSRequest(textToSpeak));
        }

        public void GenerateAndPlayRandom()
        {
            var textToSpeak = RandomUtils.Choice(
                new[]
                {
                    "Добро пожаловать в Экспедицию Ноль",
                    "Добро пожаловать на Станцию Пульсар",
                    "Помоги нашей космической миссии",
                    "Слава Советскому народу, покорителю космоса!",
                    "Спаси три артефакта"
                }
            );
            
            StartCoroutine(PostTTSRequest(textToSpeak));
        }
        
        IEnumerator PostTTSRequest(string text)
        {
            // 1. Create the JSON payload
            ExperimentalTtsRequest payload = new ExperimentalTtsRequest
            {
                text = text,
                provider = "silero",
                voice = "eugene"
            };
            string json = JsonUtility.ToJson(payload);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            var urlHost = PlayerPrefs.GetString(HostKey, "http://127.0.0.1");
            var urlPort = PlayerPrefs.GetString(PortKey, "8080");
            var url = $"{urlHost}:{urlPort}/api/tts/generate";
            
            // 2. Setup the request
            // Note: We use a standard UnityWebRequest to POST, 
            // but we'll manually handle the audio download
            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"Sending request to TTS, url: {url}, text: {text}");

            // 3. Send and wait
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
            }
            else
            {
                // 4. Extract the AudioClip and play it
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log("TTS Playback started!");
                }
            }
        }
    }
}