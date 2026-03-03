using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Expedition0.MainMenu.Settings
{
    public class ServerSettingsController : MonoBehaviour
    {
        public enum Status
        {
            Idle,
            Error,
            Success,
            Waiting
        }
        
        [Header("UI References")]
        [SerializeField] private TMP_InputField hostInput;
        [SerializeField] private TMP_InputField portInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Status line colors")]
        [SerializeField] private Color idleColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private Color successColor = new Color(0.5803f, 0.7373f, 0.0549f);
        [SerializeField] private Color errorColor = new Color(0.8392f, 0f, 0.1176f);
        [SerializeField] private Color testingColor = new Color(0.9752f, 0.7804f, 0.0941f);

        private const string HostKey = "ServerHost";
        private const string PortKey = "ServerPort";

        private void Start()
        {
            // Load existing values
            hostInput.text = PlayerPrefs.GetString(HostKey, "http://127.0.0.1");
            portInput.text = PlayerPrefs.GetString(PortKey, "8080");

            // Add listeners for real-time validation
            hostInput.onValueChanged.AddListener(_ => OnInputChanged());
            portInput.onValueChanged.AddListener(_ => OnInputChanged());

            // Initial validation check
            ValidateInputs();
        }

        private void OnInputChanged()
        {
            if (ValidateInputs())
            {
                SaveSettings();
            }
        }

        private bool ValidateInputs()
        {
            bool isHostValid = !string.IsNullOrWhiteSpace(hostInput.text);
            bool isPortValid = int.TryParse(portInput.text, out int port) && port > 0 && port <= 65535;

            bool isValid = isHostValid && isPortValid;

            submitButton.interactable = isValid;
            if (isValid)
            {
                SetStatusString("");
            }
            else
            {
                SetStatusString("Invalid host or port", Status.Error);
            }

            return isValid;
        }

        private void SaveSettings()
        {
            Debug.Log($"<b><color=cyan>[ConnectionSettingsController]</color></b>Saved new host: {hostInput.text}, port: {portInput.text}");
            PlayerPrefs.SetString(HostKey, hostInput.text);
            PlayerPrefs.SetString(PortKey, portInput.text);
            PlayerPrefs.Save();
        }

        public string GetFullAddress() => $"{hostInput.text}:{portInput.text}";

        public void SetStatusString(string message, Status status = Status.Idle)
        {
            statusText.color = status switch
            {
                Status.Waiting => testingColor,
                Status.Success => successColor,
                Status.Error => errorColor,
                _  => idleColor
            };
            statusText.text = message;
        }
    }
}