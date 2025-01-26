using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SurvivalDaysText : MonoBehaviour
{
    [Header("Configuración")]
    public TextMeshProUGUI survivalText; // Referencia al TextMeshPro en la escena
    public HUDManager hudManager;       // Referencia al HUDManager para obtener el día actual

    private void Start()
    {
        UpdateSurvivalText();
    }

    public void UpdateSurvivalText()
    {
        if (survivalText != null && hudManager != null)
        {
            // Actualizar el texto con el número de días que sobrevivió el jugador
            survivalText.text = $"Bob survived {hudManager.Day} days";
        }
    }
}

