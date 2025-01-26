using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Events : MonoBehaviour
{
    public GameObject dialogueBubble;      // Referencia al globo de diálogo en la escena
    public GameObject healthIcon;         // Referencia al ícono de Health en la escena
    public GameObject happinessIcon;      // Referencia al ícono de Happiness en la escena
    public GameObject imaginationIcon;    // Referencia al ícono de Imagination en la escena
    public GameObject arrowPrefab;        // Prefab de la flecha roja

    public HUDManager hudManager;         // Referencia al HUDManager

    [Header("Escala del Globo de Diálogo")]
    public float dialogueStartScale = 0f; // Escala inicial del globo de diálogo
    public float dialogueEndScale = 1.2f; // Escala final del globo de diálogo

    [Header("Escala de los Íconos")]
    public float iconStartScale = 0f;     // Escala inicial de los íconos
    public float iconEndScale = 0.8f;     // Escala final de los íconos

    [Header("Flecha Roja")]
    public float arrowOffsetY = 50f;      // Distancia hacia arriba para posicionar la flecha
    public float arrowOffsetX = 0f;       // Offset en el eje X para ajustar horizontalmente la flecha
    public float arrowStartScale = 0f;    // Escala inicial de la flecha roja
    public float arrowEndScale = 1f;      // Escala final de la flecha roja

    private GameObject currentArrow;      // Referencia a la flecha instanciada
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

        // Activar el ícono correspondiente y añadir la flecha roja
        switch (selectedStat)
        {
            case "health":
                ActivateIcon(healthIcon);
                SpawnArrowAbove(hudManager.healthBar.transform); // Flecha sobre la barra de salud
                hudManager.healthDecreaseSpeed *= 2; // Duplicar velocidad de disminución
                break;
            case "happiness":
                ActivateIcon(happinessIcon);
                SpawnArrowAbove(hudManager.happinessBar.transform); // Flecha sobre la barra de felicidad
                hudManager.happinessDecreaseSpeed *= 2; // Duplicar velocidad de disminución
                break;
            case "imagination":
                ActivateIcon(imaginationIcon);
                SpawnArrowAbove(hudManager.imaginationBar.transform); // Flecha sobre la barra de imaginación
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

        // Desactivar el globo de diálogo, los íconos y destruir la flecha roja
        if (dialogueBubble != null) dialogueBubble.SetActive(false);
        if (healthIcon != null) healthIcon.SetActive(false);
        if (happinessIcon != null) happinessIcon.SetActive(false);
        if (imaginationIcon != null) imaginationIcon.SetActive(false);
        if (currentArrow != null) Destroy(currentArrow);

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

    private void SpawnArrowAbove(Transform targetTransform)
    {
        if (arrowPrefab != null)
        {
            // Instanciar la flecha sobre el objeto de destino
            currentArrow = Instantiate(arrowPrefab, targetTransform.position, Quaternion.identity, targetTransform);

            // Ajustar la posición de la flecha con offsets en X y Y
            Vector3 offset = new Vector3(arrowOffsetX, arrowOffsetY, 0);
            currentArrow.transform.localPosition += offset;

            // Ajustar la escala inicial de la flecha
            currentArrow.transform.localScale = Vector3.one * arrowStartScale;

            // Animación de escala con DoTween Pro
            currentArrow.transform.DOScale(Vector3.one * arrowEndScale, 0.5f).SetEase(Ease.OutBounce);
        }
    }
}
