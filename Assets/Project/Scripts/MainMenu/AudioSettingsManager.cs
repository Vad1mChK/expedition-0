using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro; 
public class AudioSettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    
    [Header("Volume Parameters")]
    public string masterVolumeParam = "MasterVolume";
    public string musicVolumeParam = "MusicVolume";
    public string voiceVolumeParam = "VoiceVolume";
    public string sfxVolumeParam = "SfxVolume";
    
    // [Header("Volume Step")]
    // [Range(5, 20)]
    // public float volumeStep = 15f; 
    
    [Header("UI Elements - Text Labels")]
    public TMP_Text masterVolumeText; 
    public TMP_Text musicVolumeText; 
    public TMP_Text voiceVolumeText; 
    public TMP_Text sfxVolumeText;

    [Header("Volume Values")]
    private const float DefaultMasterVolume = 80f;
    private const float DefaultMusicVolume = 70f;
    private const float DefaultVoiceVolume = 90f;
    private const float DefaultSfxVolume = 80f;
    
    [Range(0, 100)]
    private float masterVolume = DefaultMasterVolume;
    [Range(0, 100)]
    private float musicVolume = DefaultMusicVolume;
    [Range(0, 100)]
    private float voiceVolume = DefaultVoiceVolume;
    [Range(0, 100)]
    private float sfxVolume = DefaultSfxVolume;

    private void Start()
    {
        FindTextMeshProElements();
        LoadVolumeSettings();
        ApplyAllVolumes();
        UpdateAllVolumeTexts();
    }

    private void FindTextMeshProElements()
    {
        if (masterVolumeText == null)
            masterVolumeText = GameObject.Find("MasterVolumeText")?.GetComponent<TMP_Text>();
        
        if (musicVolumeText == null)
            musicVolumeText = GameObject.Find("MusicVolumeText")?.GetComponent<TMP_Text>();
            
        if (voiceVolumeText == null)
            voiceVolumeText = GameObject.Find("VoiceVolumeText")?.GetComponent<TMP_Text>();
            
        if (sfxVolumeText == null)
            sfxVolumeText = GameObject.Find("SFXVolumeText")?.GetComponent<TMP_Text>();

        if (masterVolumeText == null || musicVolumeText == null || voiceVolumeText == null || sfxVolumeText == null)
        {
            TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
            foreach (TMP_Text text in allTexts)
            {
                if (text.name.Contains("Master") && masterVolumeText == null)
                    masterVolumeText = text;
                else if (text.name.Contains("Music") && musicVolumeText == null)
                    musicVolumeText = text;
                else if (text.name.Contains("Voice") && voiceVolumeText == null)
                    voiceVolumeText = text;
                else if (text.name.Contains("SFX") && sfxVolumeText == null)
                    sfxVolumeText = text;
            }
        }
    }

    public void ChangeMasterVolume(float changeAmount)
    {
        masterVolume = Mathf.Clamp(masterVolume + changeAmount, 0f, 100f);
        SetMasterVolume(masterVolume / 100f);
        SaveVolumeSetting(masterVolumeParam, masterVolume / 100f);
        UpdateVolumeText(masterVolumeText, masterVolume);
        Debug.Log($"Master Volume: {masterVolume}%");
    }

    public void ChangeMusicVolume(float changeAmount)
    {
        musicVolume = Mathf.Clamp(musicVolume + changeAmount, 0f, 100f);
        SetMusicVolume(musicVolume / 100f);
        SaveVolumeSetting(musicVolumeParam, musicVolume / 100f);
        UpdateVolumeText(musicVolumeText, musicVolume);
        Debug.Log($"Music Volume: {musicVolume}%");
    }

    public void ChangeVoiceVolume(float changeAmount)
    {
        voiceVolume = Mathf.Clamp(voiceVolume + changeAmount, 0f, 100f);
        SetVoiceVolume(voiceVolume / 100f);
        SaveVolumeSetting(voiceVolumeParam, voiceVolume / 100f);
        UpdateVolumeText(voiceVolumeText, voiceVolume);
        Debug.Log($"Voice Volume: {voiceVolume}%");
    }

    public void ChangeSfxVolume(float changeAmount)
    {
        sfxVolume = Mathf.Clamp(sfxVolume + changeAmount, 0f, 100f);
        SetSfxVolume(sfxVolume / 100f);
        SaveVolumeSetting(sfxVolumeParam, sfxVolume / 100f);
        UpdateVolumeText(sfxVolumeText, sfxVolume);
        Debug.Log($"SFX Volume: {sfxVolume}%");
    }

    public void SetMasterVolume(float volume)
    {
        SetVolume(masterVolumeParam, volume);
    }

    public void SetMusicVolume(float volume)
    {
        SetVolume(musicVolumeParam, volume);
    }

    public void SetVoiceVolume(float volume)
    {
        SetVolume(voiceVolumeParam, volume);
    }

    public void SetSfxVolume(float volume)
    {
        SetVolume(sfxVolumeParam, volume);
    }

    private void SetVolume(string parameter, float volume)
    {
        if (audioMixer != null)
        {
            float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat(parameter, dB);
        }
    }

    private void UpdateVolumeText(TMP_Text volumeText, float volumePercent)
    {
        if (volumeText != null)
        {
            volumeText.text = $"{Mathf.RoundToInt(volumePercent)}%";
        }
    }

    private void UpdateAllVolumeTexts()
    {
        UpdateVolumeText(masterVolumeText, masterVolume);
        UpdateVolumeText(musicVolumeText, musicVolume);
        UpdateVolumeText(voiceVolumeText, voiceVolume);
        UpdateVolumeText(sfxVolumeText, sfxVolume);
    }

    private void SaveVolumeSetting(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(masterVolumeParam, DefaultMasterVolume / 100f) * 100f;
        musicVolume = PlayerPrefs.GetFloat(musicVolumeParam, DefaultMusicVolume / 100f) * 100f;
        voiceVolume = PlayerPrefs.GetFloat(voiceVolumeParam, DefaultVoiceVolume / 100f) * 100f;
        sfxVolume = PlayerPrefs.GetFloat(sfxVolumeParam, DefaultSfxVolume / 100f) * 100f;
    }

    private void ApplyAllVolumes()
    {
        SetMasterVolume(masterVolume / 100f);
        SetMusicVolume(musicVolume / 100f);
        SetVoiceVolume(voiceVolume / 100f);
        SetSfxVolume(sfxVolume / 100f);
    }

    public void ResetToDefaults()
    {
        masterVolume = DefaultMasterVolume;
        musicVolume = DefaultMusicVolume;
        voiceVolume = DefaultVoiceVolume;
        sfxVolume = DefaultSfxVolume;

        ApplyAllVolumes();
        UpdateAllVolumeTexts();

        SaveVolumeSetting(masterVolumeParam, masterVolume / 100f);
        SaveVolumeSetting(musicVolumeParam, musicVolume / 100f);
        SaveVolumeSetting(voiceVolumeParam, voiceVolume / 100f);
        SaveVolumeSetting(sfxVolumeParam, sfxVolume / 100f);

        Debug.Log("Audio settings reset to defaults");
    }

    [ContextMenu("Check TextMeshPro References")]
    public void CheckTextReferences()
    {
        Debug.Log($"Master TMP_Text: {masterVolumeText != null}");
        Debug.Log($"Music TMP_Text: {musicVolumeText != null}");
        Debug.Log($"Voice TMP_Text: {voiceVolumeText != null}");
        Debug.Log($"SFX TMP_Text: {sfxVolumeText != null}");
    }

    [ContextMenu("Print Current Volumes")]
    public void PrintCurrentVolumes()
    {
        Debug.Log($"Master: {masterVolume}%, Music: {musicVolume}, Voice: {voiceVolume}, SFX: {sfxVolume}");
    }
}