using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameStates{MainMenu, Pause, Gameplay}
public enum InputType { Keyboard, Mouse, MouseKeyboard, Gamepad, Automatic }

public class GameManager : MonoBehaviour
{
    #region Variables
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    public GameStates CurrentGameState;

    [Header("Input Settings")]
    public InputType CurrentInputType;

    // Configuración gráfica
    [Header("Graphics Settings")]
    public int TargetFrameRate = 60; // Predeterminado
    public FullScreenMode ScreenMode = FullScreenMode.FullScreenWindow;
    public Resolution CurrentResolution;
    public event Action<InputType> OnInputTypeChanged;

    #endregion

    #region Running Instances
    private void Awake() 
    { 
        if (Instance != null) 
        { 
            Debug.Log("Found more than one GameManager. Destroying the newest one.");
            Destroy(this);
            return; 
        } 
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        LoadSettings(); // Cargar configuraciones al iniciar
        
    }
    #endregion

    #region Settings methods
    /// <summary>
    /// Obtiene el tipo de entrada actual.
    /// </summary>
    public InputType GetInputType()
    {
        return CurrentInputType;
    }

    /// <summary>
    /// Cambia y guarda el tipo de entrada actual.
    /// </summary>
    public void SetInputType(InputType inputType)
    {
        CurrentInputType = inputType;
        PlayerPrefs.SetInt("InputType", (int)inputType);
        PlayerPrefs.Save();

        OnInputTypeChanged?.Invoke(inputType); // Disparar el evento
        Debug.Log($"InputType set to: {inputType}");
    }

    /// <summary>
    /// Devuelve una lista de opciones de resolución disponibles y el índice de la resolución actual.
    /// </summary>
    public List<string> GetResolutionOptions(out int currentResolutionIndex)
    {
        Resolution[] availableResolutions = Screen.resolutions;
        List<string> resolutionOptions = new List<string>();
        currentResolutionIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            resolutionOptions.Add($"{availableResolutions[i].width}x{availableResolutions[i].height} @ {availableResolutions[i].refreshRateRatio.value}Hz");

            if (availableResolutions[i].width == CurrentResolution.width &&
                availableResolutions[i].height == CurrentResolution.height &&
                Mathf.Approximately((float)availableResolutions[i].refreshRateRatio.value, (float)CurrentResolution.refreshRateRatio.value))
            {
                currentResolutionIndex = i;
            }
        }

        return resolutionOptions;
    }

    /// <summary>
    /// Cambia la tasa de fotogramas objetivo.
    /// </summary>
    public void SetTargetFrameRate(int fps)
    {
        TargetFrameRate = fps;
        Application.targetFrameRate = fps;
        SaveSettings();
    }

    /// <summary>
    /// Cambia el modo de pantalla (Completa, Ventana, etc.).
    /// </summary>
    public void SetScreenMode(FullScreenMode mode)
    {
        ScreenMode = mode;
        Screen.fullScreenMode = mode;
        SaveSettings();
    }

    /// <summary>
    /// Cambia la resolución actual.
    /// </summary>
    public void SetResolution(Resolution newResolution)
    {
        CurrentResolution = newResolution;
        Screen.SetResolution(
            newResolution.width,
            newResolution.height,
            ScreenMode,
            newResolution.refreshRateRatio
        );
        SaveSettings();
    }

    /// <summary>
    /// Aplica las configuraciones actuales a la pantalla y gráficos.
    /// </summary>
    public void ApplySettings()
    {
        Application.targetFrameRate = TargetFrameRate;
        Screen.fullScreenMode = ScreenMode;
        Screen.SetResolution(
            CurrentResolution.width,
            CurrentResolution.height,
            ScreenMode,
            CurrentResolution.refreshRateRatio
        );
    }
    #endregion

    #region Persistence Methods

    /// <summary>
    /// Guarda las configuraciones en PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("TargetFrameRate", TargetFrameRate);
        PlayerPrefs.SetInt("ScreenMode", (int)ScreenMode);
        PlayerPrefs.SetInt("ResolutionWidth", CurrentResolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", CurrentResolution.height);
        PlayerPrefs.SetFloat("RefreshRate", (float)CurrentResolution.refreshRateRatio.value);
        PlayerPrefs.SetInt("InputType", (int)CurrentInputType);
        PlayerPrefs.SetFloat("GlobalVolume", AudioManager.Instance.GetGlobalVolume());
        PlayerPrefs.SetFloat("MusicVolume", AudioManager.Instance.GetMusicVolume());
        PlayerPrefs.SetFloat("SFXVolume", AudioManager.Instance.GetSFXVolume());
        PlayerPrefs.Save();
        Debug.Log("Settings saved.");
    }

    /// <summary>
    /// Carga las configuraciones desde PlayerPrefs.
    /// </summary>
    public void LoadSettings()
    {
        TargetFrameRate = PlayerPrefs.GetInt("TargetFrameRate", 60);
        ScreenMode = (FullScreenMode)PlayerPrefs.GetInt("ScreenMode", (int)FullScreenMode.FullScreenWindow);

        int width = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
        int height = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);
        float refreshRate = PlayerPrefs.GetFloat("RefreshRate", (float)Screen.currentResolution.refreshRateRatio.value);

        CurrentResolution = new Resolution
        {
            width = width,
            height = height,
            refreshRateRatio = Screen.currentResolution.refreshRateRatio
        };

        CurrentInputType = (InputType)PlayerPrefs.GetInt("InputType");

        ApplySettings();
        Debug.Log("Settings loaded.");
    }
    #endregion

}
