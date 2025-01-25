using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer; // Referencia al Audio Mixer
    public static AudioManager Instance;

    private Dictionary<string, AudioClipData> audioClipDataMap;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSourcePrefab; // Prefab para fuentes adicionales
    [SerializeField] private List<AudioClipData> audioClipDataList; // Lista de ScriptableObjects

    [Header("Volume Settings")]
    [Range(0.0001f, 1f)] [SerializeField] private float defaultGlobalVolume = 1f;
    [Range(0.0001f, 1f)] [SerializeField] private float defaultMusicVolume = 1f;
    [Range(0.0001f, 1f)] [SerializeField] private float defaultSFXVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioClipDataMap = new Dictionary<string, AudioClipData>();
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;

        LoadClips();

        // Asegurar AudioSources
        EnsureAudioSources();

        // Configurar volúmenes iniciales
        SetGlobalVolume(defaultGlobalVolume);
        SetMusicVolume(defaultMusicVolume);
        SetSFXVolume(defaultSFXVolume);
    }

    private void EnsureAudioSources()
    {
        // Configurar AudioSources para música y efectos si no existen
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
    }


    private void LoadClips()
    {
        foreach (var audioClipData in audioClipDataList)
        {
            if (audioClipData.clip != null && !audioClipDataMap.ContainsKey(audioClipData.id))
            {
                audioClipDataMap.Add(audioClipData.id, audioClipData);
            }
        }
    }

    public void PlayMusic(string clipId)
    {
        if (audioClipDataMap.TryGetValue(clipId, out AudioClipData data) && data.audioType == AudioType.Music)
        {
            musicSource.clip = data.clip;
            musicSource.volume = data.volume;
            musicSource.pitch = data.pitch;
            musicSource.loop = data.loop;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"No se encontró música con el ID '{clipId}' o el tipo no es música.");
        }
    }

    public void PlayOneShot(string clipId)
    {
        if (audioClipDataMap.TryGetValue(clipId, out AudioClipData data) && data.audioType == AudioType.OneShot)
        {
            sfxSource.PlayOneShot(data.clip, data.volume);
        }
        else
        {
            Debug.LogWarning($"No se encontró efecto de sonido 'OneShot' con el ID '{clipId}'.");
        }
    }

    public void PlayLoopingSFX(string clipId, Vector3 position)
    {
        if (audioClipDataMap.TryGetValue(clipId, out AudioClipData data) && data.audioType == AudioType.LoopingSFX)
        {
            AudioSource tempSource = Instantiate(audioSourcePrefab, position, Quaternion.identity);
            tempSource.clip = data.clip;
            tempSource.volume = data.volume;
            tempSource.pitch = data.pitch;
            tempSource.loop = true; // Forzar a que esté en loop
            tempSource.Play();
        }
        else
        {
            Debug.LogWarning($"No se encontró efecto de sonido en loop con el ID '{clipId}'.");
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void StopAllLoopingSFX()
    {
        foreach (var source in FindObjectsOfType<AudioSource>())
        {
            if (source != musicSource && source != sfxSource)
            {
                Destroy(source.gameObject); // Eliminar todas las fuentes de audio temporales
            }
        }
    }

    // Métodos para ajustar el volumen global
    public void SetGlobalVolume(float volume)
    {
        float volumeInDecibels = Mathf.Log10(volume) * 20; // Convertir a decibelios
        audioMixer.SetFloat("MasterVolume", volumeInDecibels);
    }

    // Métodos para ajustar el volumen de música
    public void SetMusicVolume(float volume)
    {
        float volumeInDecibels = Mathf.Log10(volume) * 20;
        audioMixer.SetFloat("MusicVolume", volumeInDecibels);
    }

    // Métodos para ajustar el volumen de SFX
    public void SetSFXVolume(float volume)
    {
        float volumeInDecibels = Mathf.Log10(volume) * 20;
        audioMixer.SetFloat("SFXVolume", volumeInDecibels);
    }

    // Métodos para obtener el volumen actual
    public float GetGlobalVolume()
    {
        audioMixer.GetFloat("MasterVolume", out float volumeInDecibels);
        return Mathf.Pow(10, volumeInDecibels / 20);
    }

    public float GetMusicVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float volumeInDecibels);
        return Mathf.Pow(10, volumeInDecibels / 20);
    }

    public float GetSFXVolume()
    {
        audioMixer.GetFloat("SFXVolume", out float volumeInDecibels);
        return Mathf.Pow(10, volumeInDecibels / 20);
    }
}
