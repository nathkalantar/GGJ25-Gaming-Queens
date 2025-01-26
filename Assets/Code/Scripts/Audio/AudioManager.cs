using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> musicClips; // Lista de clips de música
    [SerializeField] private List<AudioClip> sfxClips;   // Lista de clips de efectos de sonido
    [SerializeField] private List<AudioClip> ambientClips; // Lista de clips de sonido ambiental


    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource ambientSource; // Nuevo AudioSource para sonido ambiental


    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f; // Volumen global
    [Range(0f, 1f)] public float musicVolume = 1f;  // Volumen de música
    [Range(0f, 1f)] public float sfxVolume = 1f;    // Volumen de efectos de sonido
    [Range(0f, 1f)] public float ambientVolume = 0.75f; // Volumen de sonido ambiental


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        // Crear el AudioSource para música
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // Crear el AudioSource para efectos de sonido
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
    }

    // Reproducir música desde la lista
    public void PlayMusic(string clipName)
    {
        AudioClip clip = musicClips.Find(c => c.name == clipName);
        if (clip == null)
        {
            Debug.LogWarning($"No se encontró música con el nombre '{clipName}'.");
            return;
        }

        musicSource.clip = clip;
        musicSource.volume = musicVolume * masterVolume; // Aplicar volúmenes
        musicSource.Play();
    }

    public void PlayAmbient(string clipName)
    {
        AudioClip clip = ambientClips.Find(c => c.name == clipName);
        if (clip == null)
        {
            Debug.LogWarning($"No se encontró sonido ambiental con el nombre '{clipName}'.");
            return;
        }

        ambientSource.clip = clip;
        ambientSource.volume = ambientVolume * masterVolume; // Aplicar volúmenes
        ambientSource.Play();
    }

    // Reproducir efecto de sonido desde la lista
    public void PlaySFX(string clipName)
    {
        AudioClip clip = sfxClips.Find(c => c.name == clipName);
        if (clip == null)
        {
            Debug.LogWarning($"No se encontró efecto de sonido con el nombre '{clipName}'.");
            return;
        }

        sfxSource.volume = sfxVolume * masterVolume; // Aplicar volúmenes
        sfxSource.PlayOneShot(clip);
    }

    // Métodos para ajustar volúmenes
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        musicSource.volume = musicVolume * masterVolume;
        sfxSource.volume = sfxVolume * masterVolume;
        ambientSource.volume = ambientVolume * masterVolume;
    }
    public void SetAmbientVolume(float value)
    {
        ambientVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }
    public void StopAmbient()
    {
        ambientSource.Stop();
    }
}
