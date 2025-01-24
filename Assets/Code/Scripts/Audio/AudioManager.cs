using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private Dictionary<string, AudioClipData> audioClipDataMap;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSourcePrefab; // Prefab para fuentes adicionales
    [SerializeField] private List<AudioClipData> audioClipDataList; // Lista de ScriptableObjects

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
}
