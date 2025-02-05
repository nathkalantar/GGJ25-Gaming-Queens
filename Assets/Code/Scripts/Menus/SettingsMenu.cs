using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    #region Variables 

    [Header("Feature Toggles")]
    public bool enableResolutionSettings = true;
    public bool enableScreenModeSettings = true;
    public bool enableFrameRateSettings = true;
    public bool enableInputTypeSettings = true;
    public bool enableAudioSettings = true;

    [Header("Dropdowns")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown;
    public TMP_Dropdown frameRateDropdown;
    public TMP_Dropdown inputTypeDropdown;
    public Toggle vSyncToggle;

    [Header("Audio Settings")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider ambientSlider;

    private Resolution[] availableResolutions;

    #endregion

    #region Running Instances

    private void Start()
    {
        availableResolutions = Screen.resolutions;

        if (enableResolutionSettings) InitializeResolutionOptions();
        if (enableScreenModeSettings) InitializeScreenModeOptions();
        if (enableFrameRateSettings) InitializeFrameRateOptions();
        if (enableInputTypeSettings) InitializeInputTypeOptions();
        if (enableAudioSettings) InitializeAudioSettings();

        LoadSettings();
    }

    #endregion

    #region Settings Methods

    private void InitializeInputTypeOptions()
    {
        if (!enableInputTypeSettings) return;
        inputTypeDropdown.ClearOptions();
        inputTypeDropdown.AddOptions(new List<string> { "Keyboard", "Mouse", "MouseKeyboard", "Gamepad", "Automatic" });
        inputTypeDropdown.value = (int)GameManager.Instance.GetInputType();
    }

    private void InitializeResolutionOptions()
    {
        if (!enableResolutionSettings) return;
        resolutionDropdown.ClearOptions();
        int currentResolutionIndex;
        List<string> resolutionOptions = GameManager.Instance.GetResolutionOptions(out currentResolutionIndex);
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
    }

    private void InitializeScreenModeOptions()
    {
        if (!enableScreenModeSettings) return;
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(new List<string> { "Completa", "Ventana", "Ventana sin bordes" });
        screenModeDropdown.value = (int)Screen.fullScreenMode;
    }

    private void InitializeFrameRateOptions()
    {
        if (!enableFrameRateSettings) return;
        frameRateDropdown.ClearOptions();
        frameRateDropdown.AddOptions(new List<string> { "15", "30", "60", "120", "240", "Sin límite" });
        frameRateDropdown.value = frameRateDropdown.options.Count - 1;
    }

    public void OnResolutionChanged(int index)
    {
        if (!enableResolutionSettings) return;
        Resolution selectedResolution = Screen.resolutions[index];
        GameManager.Instance.SetResolution(selectedResolution);
        GameManager.Instance.SaveSettings();
    }

    public void OnScreenModeChanged(int index)
    {
        if (!enableScreenModeSettings) return;
        FullScreenMode mode = (FullScreenMode)index;
        GameManager.Instance.SetScreenMode(mode);
        GameManager.Instance.SaveSettings();
    }

    public void OnFrameRateChanged(int index)
    {
        if (!enableFrameRateSettings) return;
        int targetFPS = index switch
        {
            0 => 15,
            1 => 30,
            2 => 60,
            3 => 120,
            4 => 240,
            5 => -1,
            _ => 60
        };
        GameManager.Instance.SetTargetFrameRate(targetFPS);
        GameManager.Instance.SaveSettings();
    }

    public void OnInputTypeChanged(int index)
    {
        if (!enableInputTypeSettings) return;
        InputType selectedInputType = (InputType)index;
        GameManager.Instance.SetInputType(selectedInputType);
    }

    public void OnVSyncToggled(bool isEnabled)
    {
        QualitySettings.vSyncCount = isEnabled ? 1 : 0;
        GameManager.Instance.SaveSettings();
    }

    private void LoadSettings()
    {
        if (GameManager.Instance != null)
        {
            if (enableFrameRateSettings)
            {
                frameRateDropdown.value = GameManager.Instance.TargetFrameRate == -1
                    ? frameRateDropdown.options.Count - 1
                    : frameRateDropdown.options.FindIndex(option => option.text == GameManager.Instance.TargetFrameRate.ToString());
            }
            if (enableScreenModeSettings) screenModeDropdown.value = (int)Screen.fullScreenMode;
            if (enableResolutionSettings) resolutionDropdown.value = GetCurrentResolutionIndex();
            if (enableInputTypeSettings) inputTypeDropdown.value = (int)GameManager.Instance.GetInputType();
            vSyncToggle.isOn = QualitySettings.vSyncCount > 0;
        }
        else
        {
            Debug.LogError("GameManager.Instance es null. No se pueden cargar las configuraciones.");
        }
    }

    public void InitializeAudioSettings()
    {
        if (!enableAudioSettings) return;

        masterSlider.value = AudioManager.Instance.masterVolume;
        musicSlider.value = AudioManager.Instance.musicVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;
        ambientSlider.value = AudioManager.Instance.ambientVolume;

        masterSlider.onValueChanged.AddListener((value) => AudioManager.Instance.SetMasterVolume(value));
        musicSlider.onValueChanged.AddListener((value) => AudioManager.Instance.SetMusicVolume(value));
        sfxSlider.onValueChanged.AddListener((value) => AudioManager.Instance.SetSFXVolume(value));
        ambientSlider.onValueChanged.AddListener((value) => AudioManager.Instance.SetAmbientVolume(value));
    }

    private int GetCurrentResolutionIndex()
    {
        if (availableResolutions == null || availableResolutions.Length == 0)
        {
            Debug.LogWarning("availableResolutions no está inicializado o no contiene resoluciones.");
            return 0;
        }

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height &&
                Mathf.Approximately((float)availableResolutions[i].refreshRateRatio.value, (float)Screen.currentResolution.refreshRateRatio.value))
            {
                return i;
            }
        }
        return 0;
    }

    #endregion
}
