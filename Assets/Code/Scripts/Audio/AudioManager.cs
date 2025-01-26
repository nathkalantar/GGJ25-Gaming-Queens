using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer; // Referencia al Audio Mixer

    private Dictionary<string, AudioClipData> audioClipDataMap;
    [SerializeField] private List<AudioClipData> audioClipDataList; // Lista de ScriptableObjects para almacenar datos de clips


    private AudioSource musicSource;
    private AudioSource sfxSource;

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

        // Cargar clips y volúmenes iniciales
        LoadClips();
        LoadVolumes(); // Cargar volúmenes guardados

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
        musicSource.loop = true;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
    }

    private void Start()
    {
        LoadClips();
    }

    private void LoadClips()
    {
        audioClipDataMap = new Dictionary<string, AudioClipData>();

        foreach (var audioClipData in audioClipDataList)
        {
            if (audioClipData.clip != null)
            {
                // Usa el ID del AudioClipData como clave
                string id = string.IsNullOrEmpty(audioClipData.customId) ? audioClipData.id : audioClipData.customId;

                if (!audioClipDataMap.ContainsKey(id))
                {
                    audioClipDataMap.Add(id, audioClipData);
                }
                else
                {
                    Debug.LogWarning($"Ya existe un AudioClipData con el ID '{id}'.");
                }
            }
            else
            {
                Debug.LogWarning("AudioClipData no tiene asignado un clip de audio.");
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
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.clip = data.clip;
            tempSource.volume = data.volume;
            tempSource.pitch = data.pitch;
            tempSource.loop = true;
            tempSource.spatialBlend = 1f; // Para simular efectos 3D
            tempSource.transform.position = position;
            tempSource.Play();

            StartCoroutine(DestroyAudioSourceWhenFinished(tempSource));
        }
        else
        {
            Debug.LogWarning($"No se encontró efecto de sonido en loop con el ID '{clipId}'.");
        }
    }

    private IEnumerator DestroyAudioSourceWhenFinished(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length + 0.1f);
        Destroy(source);
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
                Destroy(source);
            }
        }
    }

    // Métodos para ajustar volúmenes con transiciones suaves
    public void SetGlobalVolume(float targetVolume, float duration = 1f)
    {
        StartCoroutine(SmoothVolumeChange("MasterVolume", targetVolume, duration));
    }

    public void SetMusicVolume(float targetVolume, float duration = 1f)
    {
        StartCoroutine(SmoothVolumeChange("MusicVolume", targetVolume, duration));
    }

    public void SetSFXVolume(float targetVolume, float duration = 1f)
    {
        StartCoroutine(SmoothVolumeChange("SFXVolume", targetVolume, duration));
    }
    public float GetGlobalVolume()
    {
        audioMixer.GetFloat("MasterVolume", out float volumeInDecibels);
        return Mathf.Pow(10, volumeInDecibels / 20); // Convertir de decibelios a un rango de 0 a 1
    }

    public float GetMusicVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float volumeInDecibels);
        return Mathf.Pow(10, volumeInDecibels / 20); // Convertir de decibelios a un rango de 0 a 1
    }

    public float GetSFXVolume()
    {
        audioMixer.GetFloat("SFXVolume", out float volumeInDecibels);
        return Mathf.Pow(10, volumeInDecibels / 20); // Convertir de decibelios a un rango de 0 a 1
    }


    private IEnumerator SmoothVolumeChange(string parameterName, float targetVolume, float duration)
    {
        audioMixer.GetFloat(parameterName, out float currentVolumeInDecibels);
        float currentVolume = Mathf.Pow(10, currentVolumeInDecibels / 20);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(currentVolume, targetVolume, elapsedTime / duration);
            float newVolumeInDecibels = Mathf.Log10(Mathf.Max(newVolume, 0.0001f)) * 20;
            audioMixer.SetFloat(parameterName, newVolumeInDecibels);
            yield return null;
        }

        float finalVolumeInDecibels = Mathf.Log10(Mathf.Max(targetVolume, 0.0001f)) * 20;
        audioMixer.SetFloat(parameterName, finalVolumeInDecibels);
    }

    private void LoadVolumes()
    {
        float globalVolume = PlayerPrefs.GetFloat("GlobalVolume", defaultGlobalVolume);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);

        SetGlobalVolume(globalVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
}
