using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMultipartParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace Expedition0.Command
{
    public sealed class CommandClient : MonoBehaviour
    {
        private const string HostKey = "ServerHost";
        private const string PortKey = "ServerPort";

        // Using camelCase resolver to match your Pydantic models
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public async Task<(CommandResponseDto dto, byte[] audioBytes)> SendCommandAsync(byte[] wavData, CommandRequestDto metadata)
        {
            string jsonPayload = JsonConvert.SerializeObject(metadata, JsonSettings);

            var formData = new System.Collections.Generic.List<IMultipartFormSection>
            {
                new MultipartFormFileSection("audio", wavData, "command.wav", "audio/wav"),
                new MultipartFormDataSection("data", jsonPayload)
            };
            
            string url = GetApiUrl("/api/command/recognize_command");

            using (UnityWebRequest www = UnityWebRequest.Post(url, formData))
            {
                var operation = www.SendWebRequest();
                while (!operation.isDone) await Task.Yield();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[CommandClient] HTTP Error: {www.error}");
                    return (null, null);
                }

                // Parse the multipart response
                using (var stream = new MemoryStream(www.downloadHandler.data))
                {
                    var parser = MultipartFormDataParser.Parse(stream);

                    // 1. Extract JSON DTO from 'data' field
                    var jsonParameter = parser.Parameters.FirstOrDefault(p => p.Name == "data");
                    if (jsonParameter == null)
                    {
                        Debug.LogError("[CommandClient] Part 'data' missing in response");
                        return (null, null);
                    }

                    var responseDto = JsonConvert.DeserializeObject<CommandResponseDto>(jsonParameter.Data, JsonSettings);

                    // 2. Extract Audio from 'audio' field
                    var audioFile = parser.Files.FirstOrDefault(f => f.Name == "audio");
                    byte[] audioBytes = ExtractBytesFromStream(audioFile?.Data);

                    return (responseDto, audioBytes);
                }
            }
        }
        
        private byte[] ExtractBytesFromStream(Stream source)
        {
            if (source == null) return null;

            // If it's already a MemoryStream, we can skip the copy
            if (source is MemoryStream ms)
            {
                return ms.ToArray();
            }

            using (var memoryStream = new MemoryStream())
            {
                source.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        
        private string GetApiUrl(string suffix)
        {
            var host = PlayerPrefs.GetString(HostKey, "http://localhost");
            var port = PlayerPrefs.GetString(PortKey, "5000");
            return $"{host}:{port}{suffix}";
        }
    }
}