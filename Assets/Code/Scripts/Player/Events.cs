using System.Collections;
using UnityEngine;

public class Events : MonoBehaviour
{
    public GameObject dialogueBubble;      // Referencia al globo de di�logo en la escena
    public GameObject healthIcon;         // Referencia al �cono de Health en la escena
    public GameObject happinessIcon;      // Referencia al �cono de Happiness en la escena
    public GameObject imaginationIcon;    // Referencia al �cono de Imagination en la escena

    public HUDManager hudManager;         // Referencia al HUDManager

    private bool eventInProgress = false;

    private void Start()
    {
        // Inicializar los globos e �conos como inactivos
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
            // Esperar hasta que sea el D�a 3 o m�s y que sea cada 2 d�as (D�a 3, 5, 7, etc.)
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

        // Activar el globo de di�logo
        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(true);
        }

        // Activar el �cono correspondiente sin modificar su posici�n
        switch (selectedStat)
        {
            case "health":
                ActivateIcon(healthIcon);
                hudManager.healthDecreaseSpeed *= 2; // Duplicar velocidad de disminuci�n
                break;
            case "happiness":
                ActivateIcon(happinessIcon);
                hudManager.happinessDecreaseSpeed *= 2; // Duplicar velocidad de disminuci�n
                break;
            case "imagination":
                ActivateIcon(imaginationIcon);
                hudManager.imaginationDecreaseSpeed *= 2; // Duplicar velocidad de disminuci�n
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

        // Desactivar el globo de di�logo y los �conos
        if (dialogueBubble != null) dialogueBubble.SetActive(false);
        if (healthIcon != null) healthIcon.SetActive(false);
        if (happinessIcon != null) happinessIcon.SetActive(false);
        if (imaginationIcon != null) imaginationIcon.SetActive(false);

        eventInProgress = false;
        Debug.Log("Evento Bob Needy terminado.");
    }

    private string SelectRandomStat()
    {
        // Generar un �ndice aleatorio para seleccionar una stat
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
            // No modificar posici�n, ya que se mantiene la configuraci�n de escena
        }
    }
}
