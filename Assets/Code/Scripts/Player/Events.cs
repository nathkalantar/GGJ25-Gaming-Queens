using System.Collections;
using UnityEngine;

public class Events : MonoBehaviour
{
    public GameObject dialogueBubblePrefab; // Prefab del globo de di�logo
    public GameObject healthIconPrefab;    // Prefab del �cono de Health
    public GameObject happinessIconPrefab; // Prefab del �cono de Happiness
    public GameObject imaginationIconPrefab; // Prefab del �cono de Imagination

    public HUDManager hudManager; // Referencia al HUDManager
    public GameObject player; // Referencia al GameObject Player

    private GameObject activeDialogueBubble; // Referencia al globo de di�logo instanciado
    private GameObject activeIcon; // Referencia al �cono instanciado
    private bool eventInProgress = false;

    private void Start()
    {
        // Comenzar a verificar los eventos
        StartCoroutine(EventLoop());
    }

    private IEnumerator EventLoop()
    {
        while (true)
        {
            // Esperar 2 d�as antes de intentar el pr�ximo evento
            yield return new WaitUntil(() => hudManager.Day % 2 == 0 && !eventInProgress);

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

        // Instanciar el globo de di�logo
        if (dialogueBubblePrefab != null)
        {
            activeDialogueBubble = Instantiate(dialogueBubblePrefab, player.transform.position + new Vector3(0, 2f, 0), Quaternion.identity);
        }

        // Instanciar el �cono correspondiente
        switch (selectedStat)
        {
            case "health":
                activeIcon = Instantiate(healthIconPrefab, activeDialogueBubble.transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
                hudManager.healthDecreaseSpeed *= 2; // Duplicar velocidad de disminuci�n
                break;
            case "happiness":
                activeIcon = Instantiate(happinessIconPrefab, activeDialogueBubble.transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
                hudManager.happinessDecreaseSpeed *= 2; // Duplicar velocidad de disminuci�n
                break;
            case "imagination":
                activeIcon = Instantiate(imaginationIconPrefab, activeDialogueBubble.transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
                hudManager.imaginationDecreaseSpeed *= 2; // Duplicar velocidad de disminuci�n
                break;
        }

        // Esperar 8 segundos antes de finalizar el evento
        yield return new WaitForSeconds(8f);

        // Restaurar la velocidad de la stat seleccionada
        switch (selectedStat)
        {
            case "health":
                hudManager.healthDecreaseSpeed /= 2;
                break;
            case "happiness":
                hudManager.happinessDecreaseSpeed /= 2;
                break;
            case "imagination":
                hudManager.imaginationDecreaseSpeed /= 2;
                break;
        }

        // Destruir el globo de di�logo y el �cono
        if (activeDialogueBubble != null) Destroy(activeDialogueBubble);
        if (activeIcon != null) Destroy(activeIcon);

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
}
