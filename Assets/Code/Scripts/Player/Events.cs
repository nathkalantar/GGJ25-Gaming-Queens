using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Events : MonoBehaviour
{
    public GameObject dialogueBubble;      // Referencia al globo de diálogo en la escena
    public GameObject healthIcon;         // Referencia al ícono de Health en la escena
    public GameObject happinessIcon;      // Referencia al ícono de Happiness en la escena
    public GameObject imaginationIcon;    // Referencia al ícono de Imagination en la escena

    public HUDManager hudManager;         // Referencia al HUDManager

    [Header("Escala del Globo de Diálogo")]
    public float dialogueStartScale = 0f; // Escala inicial del globo de diálogo
    public float dialogueEndScale = 1.2f; // Escala final del globo de diálogo

    [Header("Escala de los Íconos")]
    public float iconStartScale = 0f;     // Escala inicial de los íconos
    public float iconEndScale = 0.8f;     // Escala final de los íconos

    private bool eventInProgress = false;

    private void Start()
    {
        // Inicializar los globos e íconos como inactivos
        if (dialogueBubble != null) dialogueBubble.SetActive(false);
        if (healthIcon != null) healthIcon.SetActive(false);
        if (happinessIcon != null) happinessIcon.SetActive(false);
        if (imaginationIcon != null) imaginationIcon.SetActive(false);

        // Comenzar a verificar los eventos
        StartCoroutine(EventLoop());
    }

    private IEnumerator EventLoop()
    {
        while (true)
        {
            // Esperar hasta que sea el Día 3 o más y que sea cada 2 días (Día 3, 5, 7, etc.)
            yield return new WaitUntil(() => hudManager.Day >= 3 && hudManager.Day % 2 == 1 && !eventInProgress);

            // Iniciar el evento Bob Needy
            StartCoroutine(BobNeedyEvent());
        }
    }

    private IEnumerator BobNeedyEvent()
    {
        eventInProgress = true;

        // Elegir una stat al azar
        string selectedStat = SelectRandomStat();
        Debug.Log($"Evento Bob Needy: {selectedStat} seleccionado.");

        // Activar el globo de diálogo con animación
        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(true);
            dialogueBubble.transform.localScale = Vector3.one * dialogueStartScale; // Escala inicial
            dialogueBubble.transform.DOScale(dialogueEndScale, 0.5f).SetEase(Ease.OutBounce); // Escala final ajustada
        }

        // Activar el ícono correspondiente con animación
        switch (selectedStat)
        {
            case "health":
                ActivateIcon(healthIcon);
                hudManager.healthDecreaseSpeed *= 2; // Duplicar velocidad de disminución
                break;
            case "happiness":
                ActivateIcon(happinessIcon);
                hudManager.happinessDecreaseSpeed *= 2; // Duplicar velocidad de disminución
                break;
            case "imagination":
                ActivateIcon(imaginationIcon);
                hudManager.imaginationDecreaseSpeed *= 2; // Duplicar velocidad de disminución
                break;
        }

        // Esperar 8 segundos antes de finalizar el evento
        yield return new WaitForSeconds(8f);

        // Restaurar la velocidad de la stat seleccionada
        switch (selectedStat)
        {
            case "health":
                hudManager.healthDecreaseSpeed /= 2; // Restaurar velocidad original
                break;
            case "happiness":
                hudManager.happinessDecreaseSpeed /= 2; // Restaurar velocidad original
                break;
            case "imagination":
                hudManager.imaginationDecreaseSpeed /= 2; // Restaurar velocidad original
                break;
        }

        // Desactivar el globo de diálogo y los íconos
        if (dialogueBubble != null) dialogueBubble.SetActive(false);
        if (healthIcon != null) healthIcon.SetActive(false);
        if (happinessIcon != null) happinessIcon.SetActive(false);
        if (imaginationIcon != null) imaginationIcon.SetActive(false);

        eventInProgress = false;
        Debug.Log("Evento Bob Needy terminado.");
    }

    private string SelectRandomStat()
    {
        // Generar un índice aleatorio para seleccionar una stat
        int randomIndex = Random.Range(0, 3);
        switch (randomIndex)
        {
            case 0:
                return "health";
            case 1:
                return "happiness";
            case 2:
                return "imagination";
        }

        return "health"; // Valor por defecto
    }

    private void ActivateIcon(GameObject icon)
    {
        if (icon != null)
        {
            icon.SetActive(true);
            icon.transform.localScale = Vector3.one * iconStartScale; // Escala inicial

            // Ajustar escala final del ícono
            icon.transform.DOScale(iconEndScale, 0.5f).SetEase(Ease.OutBounce);
        }
    }
}
