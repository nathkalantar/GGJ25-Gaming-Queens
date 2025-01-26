using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Para UI moderna

public class SettingsMenu : MonoBehaviour
{

    #region Variables 

    [Header("Dropdowns")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown;
    public TMP_Dropdown frameRateDropdown;
    public TMP_Dropdown inputTypeDropdown;
    public Toggle vSyncToggle;

    [Header("Audio Settings")]
    public Slider masterSlider; // Slider para el volumen de música
    public Slider musicSlider; // Slider para el volumen de música
    public Slider sfxSlider;   // Slider para el volumen de efectos de sonido
    public Slider ambientSlider;   // Slider para el volumen de efectos de sonido

    private Resolution[] availableResolutions;

    #endregion

    #region Running Instances

    private void Start()
    {
        availableResolutions = Screen.resolutions;

        InitializeResolutionOptions();
        InitializeScreenModeOptions();
        InitializeFrameRateOptions();
        InitializeInputTypeOptions();
        InitializeAudioSettings();
        LoadSettings();
    }

    #endregion

    #region Settings Methods

    private void InitializeInputTypeOptions()
    {
        inputTypeDropdown.ClearOptions();
        inputTypeDropdown.AddOptions(new List<string> { "Keyboard", "Mouse", "MouseKeyboard", "Gamepad", "Automatic" });
        inputTypeDropdown.value = (int)GameManager.Instance.GetInputType();
    }
    private void InitializeResolutionOptions()
    {
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex;
        List<string> resolutionOptions = GameManager.Instance.GetResolutionOptions(out currentResolutionIndex);

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
    }

    private int GetCurrentResolutionIndex()
    {
        if (availableResolutions == null || availableResolutions.Length == 0)
        {
            Debug.LogWarning("availableResolutions no está inicializado o no contiene resoluciones.");
            return 0; // Valor predeterminado
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

        return 0; // Predeterminado si no se encuentra la resolución actual
    }

    private void InitializeScreenModeOptions()
    {
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(new List<string> { "Completa", "Ventana", "Ventana sin bordes" });
        screenModeDropdown.value = (int)Screen.fullScreenMode;
    }

    private void InitializeFrameRateOptions()
    {
        frameRateDropdown.ClearOptions();
        frameRateDropdown.AddOptions(new List<string> { "15", "30", "60", "120", "240", "Sin límite" });
        frameRateDropdown.value = frameRateDropdown.options.Count - 1; // Sin límite por defecto
    }

    public void OnResolutionChanged(int index)
    {
        Resolution selectedResolution = Screen.resolutions[index];
        GameManager.Instance.SetResolution(selectedResolution);
        GameManager.Instance.SaveSettings();
        Debug.Log($"Resolution changed to: {selectedResolution.width}x{selectedResolution.height} @ {selectedResolution.refreshRateRatio.value}Hz");
    }

    public void OnScreenModeChanged(int index)
    {
        FullScreenMode mode = (FullScreenMode)index;
        GameManager.Instance.SetScreenMode(mode);
        GameManager.Instance.SaveSettings();
    }

    public void OnFrameRateChanged(int index)
    {
        int targetFPS = index switch
        {
            0 => 15,
            1 => 30,
            2 => 60,
            3 => 120,
            4 => 240,
            5 => -1, // Sin límite
            _ => 60
        };

        GameManager.Instance.SetTargetFrameRate(targetFPS);
        GameManager.Instance.SaveSettings();
    }

    public void OnInputTypeChanged(int index)
    {
        InputType selectedInputType = (InputType)index;
        GameManager.Instance.SetInputType(selectedInputType); // Actualiza en el GameManager
    }

    public void OnVSyncToggled(bool isEnabled)
    {
        QualitySettings.vSyncCount = isEnabled ? 1 : 0;
        GameManager.Instance.SaveSettings();
    }
    public void OnGlobalVolumeChanged(float sliderValue)
    {
        AudioManager.Instance.SetMasterVolume(sliderValue);
        GameManager.Instance.SaveSettings();
    }

    public void OnMusicVolumeChanged(float sliderValue)
    {
        AudioManager.Instance.SetMusicVolume(sliderValue);
        GameManager.Instance.SaveSettings();
    }

    public void OnSFXVolumeChanged(float sliderValue)
    {
        AudioManager.Instance.SetSFXVolume(sliderValue);
        GameManager.Instance.SaveSettings();
    }

    private void LoadSettings()
    {
        if (GameManager.Instance != null)
        {
            frameRateDropdown.value = GameManager.Instance.TargetFrameRate == -1
                ? frameRateDropdown.options.Count - 1
                : frameRateDropdown.options.FindIndex(option => option.text == GameManager.Instance.TargetFrameRate.ToString());

            screenModeDropdown.value = (int)Screen.fullScreenMode;
            resolutionDropdown.value = GetCurrentResolutionIndex();
            inputTypeDropdown.value = (int)GameManager.Instance.GetInputType();
            vSyncToggle.isOn = QualitySettings.vSyncCount > 0;
        }
        else
        {
            Debug.LogError("GameManager.Instance es null. No se pueden cargar las configuraciones.");
        }
    }

    public void InitializeAudioSettings()
    {
         // Sincronizar sliders con los valores iniciales
        masterSlider.value = AudioManager.Instance.masterVolume;
        musicSlider.value = AudioManager.Instance.musicVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;
        ambientSlider.value = AudioManager.Instance.ambientVolume;

        // Conectar sliders con el AudioManager
        masterSlider.onValueChanged.AddListener((value) =>
        {
            AudioManager.Instance.SetMasterVolume(value);
        });

        musicSlider.onValueChanged.AddListener((value) =>
        {
            AudioManager.Instance.SetMusicVolume(value);
        });

        sfxSlider.onValueChanged.AddListener((value) =>
        {
            AudioManager.Instance.SetSFXVolume(value);
        });
        ambientSlider.onValueChanged.AddListener((value) =>
        {
            AudioManager.Instance.SetAmbientVolume(value);
        });
    }

    private void SaveVolumeSetting(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }



    #endregion
}
