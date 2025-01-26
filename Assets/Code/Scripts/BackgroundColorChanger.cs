using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundColorChanger : MonoBehaviour
{
    [Header("HEX Colors for Background")]
    [SerializeField] private Color colorAtMin = Color.black; // Color para el valor mínimo (0)
    [SerializeField] private Color colorAtMid = Color.gray;  // Color para el valor medio (50)
    [SerializeField] private Color colorAtMax = Color.white; // Color para el valor máximo (100)

    [Header("HUD Reference")]
    [SerializeField] private HUDManager hudManager; // Referencia al HUDManager

    [Header("Background")]
    [SerializeField] private Camera mainCamera; // Referencia a la cámara o fondo

    private void Update()
    {
        if (hudManager != null && mainCamera != null)
        {
            // Calcular el promedio de los tres stats
            float averageStat = (hudManager.health + hudManager.happiness + hudManager.imagination) / 3;

            // Normalizar el promedio de 0 a 1 (0 = minStatValue, 1 = maxStatValue)
            float normalizedValue = Mathf.InverseLerp(0, 100, averageStat);

            // Interpolar entre los colores definidos
            Color targetColor;

            if (normalizedValue <= 0.5f)
            {
                // Interpolación entre colorAtMin y colorAtMid
                targetColor = Color.Lerp(colorAtMin, colorAtMid, normalizedValue * 2);
            }
            else
            {
                // Interpolación entre colorAtMid y colorAtMax
                targetColor = Color.Lerp(colorAtMid, colorAtMax, (normalizedValue - 0.5f) * 2);
            }

            // Asignar el color al fondo
            mainCamera.backgroundColor = targetColor;
        }
    }
}

