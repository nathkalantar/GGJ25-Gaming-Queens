using UnityEngine;

public enum AudioType
{
    Music,
    OneShot,
    LoopingSFX
}

[CreateAssetMenu(fileName = "NewAudioClipData", menuName = "Audio/AudioClipData")]
public class AudioClipData : ScriptableObject
{
    [Header("Audio Settings")]
    public AudioClip clip; // El clip de audio
    [HideInInspector] public string id; // ID único (generado automáticamente)

    [Tooltip("ID personalizado (sobrescribirá el generado automáticamente)")]
    public string customId;

    public AudioType audioType = AudioType.OneShot; // Tipo de audio

    [Header("Default Settings")]
    [Range(0f, 1f)] public float volume = 1f; // Volumen predeterminado
    [Range(-3f, 3f)] public float pitch = 1f; // Pitch predeterminado
    public bool loop = false; // Si el sonido debe repetirse (solo para efectos en loop)

    private void OnValidate()
    {
        // Generar ID automáticamente al asignar un clip
        if (clip != null && string.IsNullOrEmpty(customId))
        {
            id = clip.name + "_" + GetHashCode();
        }
        else if (!string.IsNullOrEmpty(customId))
        {
            id = customId; // Usar el ID personalizado si está configurado
        }

        // Ajustar la configuración de loop según el tipo de audio
        if (audioType == AudioType.Music)
        {
            loop = true; // La música siempre estará en loop
        }
    }
}
