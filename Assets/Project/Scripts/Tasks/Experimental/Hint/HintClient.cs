using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expedition0.Audio;
using Expedition0.Tasks.Experimental.Json;
using HttpMultipartParser;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Expedition0.Tasks.Experimental.Hint
{
    public class HintClient : MonoBehaviour
    {
        [Header("Task details")]
        [SerializeField] private LogicEqualityTaskView taskView;
        [SerializeField] private LogicNodeView leftRoot;
        [SerializeField] private LogicNodeView rightRoot;
        [SerializeField] private LogicInterfaceType leftInterfaceType = LogicInterfaceType.TernaryEquation;
        [SerializeField] private LogicInterfaceType rightInterfaceType = LogicInterfaceType.TernaryEquation;
        
        [Header("Side effects")]
        [SerializeField] private AudioSource audioSource;
        
        [Header("Events")]
        public UnityEvent onHealthCheckSuccess;
        public UnityEvent onHealthCheckFailure;
        public UnityEvent onRequestStarted;
        public UnityEvent onRequestFailed;
        public UnityEvent<HintResponseMetadataDto> onHintReceived;
        
        private const string HostKey = "ServerHost";
        private const string PortKey = "ServerPort";

        private async void Start()
        {
            SetRoots();
            await CheckServerHealth();
        }

        public async void GetHint()
        {
            onRequestStarted?.Invoke();
            
            var requestDto = CreateRequestDto();
            if (requestDto == null)
            {
                Debug.LogError("Failed to create HintRequestDto. Check if roots are initialized.");
                onRequestFailed?.Invoke();
                return;
            }

            try
            {
                await SendHintRequest(requestDto);
            }
            catch (Exception e)
            {
                Debug.LogError($"Hint Request failed: {e.Message}");
                onRequestFailed?.Invoke();
            }
        }
        
        public async Task CheckServerHealth()
        {
            // Assuming health endpoint returns 200 OK
            string url = GetApiUrl("/api/health");
            
            using var request = UnityWebRequest.Get(url);
            var operation = request.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
                onHealthCheckSuccess?.Invoke();
            else
                onHealthCheckFailure?.Invoke();
        }

        private async Task SendHintRequest(HintRequestDto dto)
        {
            string url = GetApiUrl("/api/hint/generate_hint");
            string jsonBody = JsonConvert.SerializeObject(dto);
            Debug.Log($"Sending hint request to: {url}");
            
            using var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Extension method or custom awaiter to handle UnityWebRequest as Task
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error} | Response: {request.downloadHandler.text}");
                onRequestFailed?.Invoke();
                return;
            }

            ProcessMultipartResponse(request.downloadHandler.data);
        }

        private void ProcessMultipartResponse(byte[] data)
        {
            using var stream = new MemoryStream(data);
            var parser = MultipartFormDataParser.Parse(stream);

            // 1. Handle Metadata (JSON)
            var metadataPart = parser.Parameters.FirstOrDefault(p => p.Name == "metadata");
            if (metadataPart != null)
            {
                var metadata = JsonConvert.DeserializeObject<HintResponseMetadataDto>(metadataPart.Data);
                Debug.Log($"<color=green>Hint Received ({metadata.status}):</color> {metadata.text}");
                onHintReceived?.Invoke(metadata);
            }

            // 2. Handle Audio (WAV)
            var audioFile = parser.Files.FirstOrDefault(f => f.Name == "audio");
            if (audioFile != null)
            {
                using var audioStream = new MemoryStream();
                audioFile.Data.CopyTo(audioStream);
                PlayHintAudio(audioStream.ToArray());
            }
        }

        private void PlayHintAudio(byte[] wavData)
        {
            if (audioSource == null) return;

            // Simple WAV to AudioClip conversion
            // For production, consider a robust WAV utility or saving to a temp file
            // and using UnityWebRequestMultimedia.GetAudioClip for streaming support.
            AudioClip clip = WavUtility.ToAudioClip(wavData);
            
            audioSource.clip = clip;
            audioSource.Play();
        }

        private string GetApiUrl(string suffix)
        {
            var host = PlayerPrefs.GetString(HostKey, "http://localhost");
            var port = PlayerPrefs.GetString(PortKey, "5000");
            return $"{host}:{port}{suffix}";
        }

        private void SetRoots()
        {
            if (taskView != null)
            {
                if (leftRoot == null) leftRoot = taskView.leftRoot;
                if (rightRoot == null) rightRoot = taskView.rightRoot;
            }
        }

        private HintRequestDto CreateRequestDto()
        {
            SetRoots();
            
            var leftModel = leftRoot.Model;
            var rightModel = rightRoot.Model;

            if (leftModel == null || rightModel == null) return null;
            
            var leftSerialized = TaskSerializer.SerializeTask(leftModel);
            var rightSerialized = TaskSerializer.SerializeTask(rightModel);

            return new HintRequestDto
            {
                leftRoot = leftSerialized,
                rightRoot = rightSerialized,
                attemptCount = taskView?.AttemptsCount ?? 0,
                mistakeCount = taskView?.ErrorsCount ?? 0,
                leftInterfaceType = leftInterfaceType,
                rightInterfaceType = rightInterfaceType,
                balanced = taskView.balanced
            };
        }

        [ContextMenu("Test/Create hint request DTO")]
        public void TestCreateRequestDto()
        {
            if (leftRoot == null || rightRoot == null)
            {
                Debug.LogWarning("Roots are not initialized");
            }

            var requestDto = CreateRequestDto();
            var json = JsonConvert.SerializeObject(requestDto, Formatting.None);
            Debug.Log($"<b><color=cyan>Hint request model: {json}</color></b>");
        }
    }
}