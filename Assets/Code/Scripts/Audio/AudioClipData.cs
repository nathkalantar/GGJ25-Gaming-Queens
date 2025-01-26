using UnityEngine;

public enum AudioType
{
    Music,
    SFX
}

[CreateAssetMenu(fileName = "NewAudioClipData", menuName = "Audio/AudioClipData")]
public class AudioClipData : ScriptableObject
{
    public AudioType audioType; // Tipo de audio (Música o SFX)
    public AudioClip clip;      // Clip de audio
    [Range(0f, 1f)] public float volume = 1f; // Volumen predeterminado
    [Range(-3f, 3f)] public float pitch = 1f; // Pitch predeterminado
}
