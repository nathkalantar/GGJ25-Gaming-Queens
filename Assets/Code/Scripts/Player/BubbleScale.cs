using UnityEngine;

public class BubbleScale : MonoBehaviour
{
    // Referencia al objeto Bubble
    public GameObject bubble;

    // Valores máximos y mínimos de la escala
    private const float maxScale = 2f;
    private const float minScale = 0.4f;
    private const float midScale = 0.93f; // Escala inicial y punto medio

    // Referencias a las barras de stat
    public HUDManager hudManager;

    private void Start()
    {
        // Configurar la escala inicial al punto medio
        if (bubble != null)
        {
            bubble.transform.localScale = Vector3.one * midScale;
        }
    }

    private void Update()
    {
        if (bubble != null && hudManager != null)
        {
            // Calcular el promedio de las barras
            float averageStat = (hudManager.health + hudManager.happiness + hudManager.imagination) / 3;

            // Normalizar el promedio a un rango de 0 a 1, donde el promedio inicial corresponde a midScale
            float normalizedStat = Mathf.InverseLerp(0, 100, averageStat);

            // Ajustar la escala basada en el promedio y el rango
            float bubbleScale = Mathf.Lerp(minScale, maxScale, normalizedStat);

            // Actualizar la escala del objeto Bubble
            bubble.transform.localScale = Vector3.one * bubbleScale;
        }
    }
}
