using UnityEngine; 
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Para usar TextMeshPro
using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;

public class HUDManager : MonoBehaviour
{
    public int Day = 1;
    public float dayTimer;

    // Máximos y mínimos de las barras
    private const int maxStatValue = 100;
    private const int minStatValue = 0;

    // Valores actuales de las barras
    public float health = 50;
    public float happiness = 50;
    public float imagination = 50;

    // Velocidades de decremento (editables desde el Inspector)
    public float healthDecreaseSpeed = 1f;
    public float happinessDecreaseSpeed = 1.5f;
    public float imaginationDecreaseSpeed = 2f;

    // Referencias opcionales a sliders o imágenes para mostrar las barras visualmente
    public Slider healthBar;
    public Slider happinessBar;
    public Slider imaginationBar;

    // Referencias a TextMeshPro para mostrar los valores en pantalla
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI imaginationText;

    public GameObject floatingTextPrefab;

    // Referencias a los paneles UI
    public GameObject panelBadEnding;
    public GameObject panelEndingDelulu;

    [Header("Botónes")]
    public List<Animator> btnAnimations = new List<Animator>();

    private PlayerPositions playerPositions;
    private PlayerAnimationController playerAnimationController;

    public TextMeshProUGUI timerText;

    [Header("Botones para móviles")]
    public Button healthButton; // Botón para la barra de salud
    public Button happinessButton; // Botón para la barra de felicidad
    public Button imaginationButton; // Botón para la barra de imaginación
    public Button healthButton2; // Botón para la barra de salud
    public Button happinessButton2; // Botón para la barra de felicidad
    public Button imaginationButton2; // Botón para la barra de imaginación

    // Texto para el día actual
    public TextMeshProUGUI dayText;
    public float countdown;

    // Estados de congelación
    private bool isHealthFrozen = false;
    private bool isHappinessFrozen = false;
    private bool isImaginationFrozen = false;

    private void Start()
    {
        #if UNITY_ANDROID
        ConfigureMobileButtons();
        #else
        // Desactiva los botones si no es Android
        healthButton.gameObject.SetActive(true);
        healthButton2.gameObject.SetActive(true);
        happinessButton.gameObject.SetActive(false);
        imaginationButton.gameObject.SetActive(false);
        imaginationButton2.gameObject.SetActive(false);
        happinessButton2.gameObject.SetActive(false);
        
        #endif

        happinessBar.gameObject.SetActive(false);
        imaginationBar.gameObject.SetActive(false);

        btnAnimations[0].gameObject.SetActive(false);
        btnAnimations[2].gameObject.SetActive(false);
        btnAnimations[3].gameObject.SetActive(false);
        btnAnimations[4].gameObject.SetActive(false);

        countdown = 10f;
        DaySituation();

        Day = 1;
        dayTimer = 0;
        StartCoroutine(DayTimer()); // Iniciar el temporizador de días

        ShowDayAnimation(Day);

        // Configura los sliders si están asignados
        if (healthBar != null)
        {
            healthBar.maxValue = maxStatValue;
            healthBar.value = health;
        }

        if (happinessBar != null)
        {
            happinessBar.maxValue = maxStatValue;
            happinessBar.value = happiness;
        }

        if (imaginationBar != null)
        {
            imaginationBar.maxValue = maxStatValue;
            imaginationBar.value = imagination;
        }

        // Asegurarse de que los paneles están desactivados al inicio
        if (panelBadEnding != null)
            panelBadEnding.SetActive(false);

        if (panelEndingDelulu != null)
            panelEndingDelulu.SetActive(false);

        // Obtén la instancia del InputManager
        playerPositions = FindAnyObjectByType<PlayerPositions>();
        playerAnimationController = FindObjectOfType<PlayerAnimationController>();

        // Actualizar textos iniciales
        UpdateTextValues();
    }

    private void Update()
    {
        // Reducir los valores de las barras con base en su velocidad si no están congeladas
        ReduceBars();

        // Actualizar los sliders si están asignados
        UpdateSliders();

        // Manejar el input para incrementar barras
        HandleInput();

        // Actualizar textos de valores
        UpdateTextValues();

        float averageStats = (health + happiness + imagination) / 3;

        if (averageStats < 33)
        {
            playerAnimationController.SetState(0); // Sad
        }
        else if (averageStats >= 33 && averageStats < 66)
        {
            playerAnimationController.SetState(1); // Neutral
        }
        else
        {
            playerAnimationController.SetState(2); // Delulu
        }

        // Verificar condiciones de fin de juego
        CheckGameEndings();
    }

    private IEnumerator DayTimer()
    {
        while (true)
        {
            // Inicializa el temporizador según el día actual
            DaySituation();

            // Mostrar el texto del día
            ShowDayAnimation(Day);

            // Temporizador del día
            while (countdown > 0)
            {
                // Solo decrementar el temporizador si el estado del juego es Gameplay
                if (GameManager.Instance.CurrentGameState == GameStates.Gameplay)
                {
                    if (Mathf.Approximately(countdown, 1f)) // Falta 1 segundo para cambiar de día
                    {
                        AnimateDayTextOut(); // Animar la salida del texto del día
                    }

                    countdown -= Time.deltaTime; // Reducir el tiempo restante
                    UpdateTimerText(countdown); // Actualizar el texto del temporizador
                }

                yield return null; // Esperar al siguiente frame
            }

            // Incrementar el día solo si se está en Gameplay
            if (GameManager.Instance.CurrentGameState == GameStates.Gameplay)
            {
                Day++;
                Debug.Log($"Día incrementado a: {Day}");
            }
        }
    }


    private void AnimateDayTextOut()
    {
        if (dayText != null)
        {
            dayText.DOFade(0, 1f) // Fade out en 1 segundo
                   .OnComplete(() =>
                   {
                       dayText.transform.localScale = Vector3.zero; // Escala inicial después de desaparecer
                   });
        }
    }


    private void ReduceBars()
    {
        if (GameManager.Instance.CurrentGameState == GameStates.Gameplay)
        {
            if (!isHealthFrozen && healthBar.IsActive())
            {
                health = Mathf.Max(health - DayDecreased(healthDecreaseSpeed) * Time.deltaTime, minStatValue);
                CheckFreezeState(ref health, ref isHealthFrozen, healthBar);
            }

            if (!isHappinessFrozen && happinessBar.IsActive())
            {
                happiness = Mathf.Max(happiness - DayDecreased(happinessDecreaseSpeed) * Time.deltaTime, minStatValue);
                CheckFreezeState(ref happiness, ref isHappinessFrozen, happinessBar);
            }

            if (!isImaginationFrozen && imaginationBar.IsActive())
            {
                imagination = Mathf.Max(imagination - DayDecreased(imaginationDecreaseSpeed) * Time.deltaTime, minStatValue);
                CheckFreezeState(ref imagination, ref isImaginationFrozen, imaginationBar);
            }
        }
    }

    // Método para actualizar el texto del temporizador
    private void UpdateTimerText(float countdown)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(countdown); // Redondear hacia arriba para mostrar segundos completos
            timerText.text = $"Next Day: {seconds}s"; // Actualiza el texto del temporizador
        }
    }

    private void ShowDayAnimation(int currentDay)
    {
        if (dayText != null)
        {
            dayText.text = $"Day {currentDay}";
            dayText.alpha = 1; // Asegurarse de que el texto esté completamente visible
            dayText.transform.localScale = Vector3.one; // Escala normal al inicio del día

            // Animación inicial de aparición
            dayText.transform.localScale = Vector3.zero; // Escala inicial para el efecto de bounce
            dayText.transform.DOScale(1.5f, 1f).SetEase(Ease.OutBounce); // Escala de aparición
        }
    }

    private float DayDecreased(float Statvalue)
    {
        float multiplier = 1f;

        // Cada 5 días, incrementar el multiplicador en 2
        if (Day % 5 == 0)
        {
            multiplier += 2f; // Incremento adicional en los días divisibles por 5
        }
        // El día posterior al incremento (día divisible por 5 + 1), restablecer el multiplicador
        else if ((Day - 1) % 5 == 0)
        {
            multiplier = Mathf.Pow(1.2f, Day - 1); // Continuar con la progresión basada en el día anterior
        }
        else
        {
            // Progresión estándar exponencial
            multiplier = Mathf.Pow(1.2f, Day);
        }

        return Statvalue * multiplier;
    }

    private void DaySituation()
    {
        // Configura el tiempo del temporizador y otros cambios según el día
        switch (Day)
        {
            case 1:
                countdown = 10f;
                break;
            case 2:
                btnAnimations[0].gameObject.SetActive(true);
                btnAnimations[3].gameObject.SetActive(true);
                imaginationButton.gameObject.SetActive(true);
                imaginationButton2.gameObject.SetActive(true);
                imaginationBar.gameObject.SetActive(true); // Activa la barra de imaginación
                countdown = 10f;
                break;
            case 3:
                btnAnimations[2].gameObject.SetActive(true);
                btnAnimations[4].gameObject.SetActive(true);
                happinessButton.gameObject.SetActive(true);
                happinessButton2.gameObject.SetActive(true);
                happinessBar.gameObject.SetActive(true); // Activa la barra de felicidad
                countdown = 15f;
                break;
            default:
                countdown = 15f; // Valor por defecto para días posteriores
                break;
        }
    }

    private void UpdateSliders()
    {
        if (GameManager.Instance.CurrentGameState == GameStates.Gameplay)
            if (healthBar != null)
                healthBar.value = Mathf.Clamp(health, minStatValue, maxStatValue);

        if (happinessBar != null)
            happinessBar.value = Mathf.Clamp(happiness, minStatValue, maxStatValue);

        if (imaginationBar != null)
            imaginationBar.value = Mathf.Clamp(imagination, minStatValue, maxStatValue);
    }

    private void HandleInput()
    {
        Vector2 moveDirection = InputManager.GetInstance().GetMoveDirection();

        if (GameManager.Instance.CurrentGameState == GameStates.Gameplay)
        {
            if (moveDirection == Vector2.up && !isHealthFrozen && btnAnimations[1].gameObject.activeSelf)
            {
                AudioManager.Instance.PlaySFX("sfx_ui_page");
                health += 1;
                playerPositions.MoveToHealthPosition();
                btnAnimations[1].SetTrigger("Pressed");
                ShowFloatingText(healthBar.transform, "+1");
                playerAnimationController.PlayHealthAnimation();

            }
            else if (moveDirection == Vector2.right && !isHappinessFrozen && btnAnimations[2].gameObject.activeSelf)
            {
                AudioManager.Instance.PlaySFX("sfx_ui_button");
                happiness += 1;
                playerPositions.MoveToHappinessPosition();
                btnAnimations[2].SetTrigger("Pressed");
                ShowFloatingText(happinessBar.transform, "+1");
                playerAnimationController.PlayHappinessAnimation();
            }
            else if (moveDirection == Vector2.left && !isImaginationFrozen && btnAnimations[0].gameObject.activeSelf)
            {
                AudioManager.Instance.PlaySFX("Sfx_UI");
                imagination += 1;
                playerPositions.MoveToImaginationPosition();
                btnAnimations[0].SetTrigger("Pressed");
                ShowFloatingText(imaginationBar.transform, "+1");
                playerAnimationController.PlayImaginationAnimation();
            }
        }
    }

    private void ShowFloatingText(Transform target, string text)
    {
        if (floatingTextPrefab != null)
        {
            // Instanciar el texto flotante cerca del objetivo
            Vector3 offset = new Vector3(0, 25, 0); // Ajusta el valor Y para moverlo más arriba
            GameObject floatingText = Instantiate(floatingTextPrefab, target.position + offset, Quaternion.identity, target);
            TextMeshProUGUI textComponent = floatingText.GetComponent<TextMeshProUGUI>();

            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.alpha = 0;

                // Animación de DoTween
                floatingText.transform.localScale = Vector3.zero;
                floatingText.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBounce);
                textComponent.DOFade(1, 0.5f).OnComplete(() =>
                {
                    // Destruir el texto después de un tiempo
                    Destroy(floatingText, 1f);
                });
            }
        }
    }

    public void ReproducirAnimation(int boton)
    {
        btnAnimations[boton].SetTrigger("Pressed");
        
    }


    private void UpdateTextValues()
    {
        if (healthText != null)
            healthText.text = Mathf.FloorToInt(health).ToString();

        if (happinessText != null)
            happinessText.text = Mathf.FloorToInt(happiness).ToString();

        if (imaginationText != null)
            imaginationText.text = Mathf.FloorToInt(imagination).ToString();
    }

    private void CheckFreezeState(ref float value, ref bool isFrozen, Slider slider)
    {
        if (value >= maxStatValue && !isFrozen)
        {
            isFrozen = true;
            Debug.Log("Barra Freeze: Alcanzó o sobrepasó el máximo");
            FreezeSliderVisuals(slider);
        }
        else if (value == minStatValue && !isFrozen)
        {
            isFrozen = true;
            Debug.Log("Barra Freeze: Alcanzó el mínimo");
            FreezeSliderVisuals(slider);
        }
    }

    private void FreezeSliderVisuals(Slider slider)
    {
        if (slider != null)
        {
            var colors = slider.colors;
            colors.normalColor = Color.gray;
            colors.highlightedColor = Color.gray;
            colors.pressedColor = Color.gray;
            colors.selectedColor = Color.gray;
            slider.colors = colors;
        }
    }

    private void CheckGameEndings()
    {
        int slidersAtMax = 0;
        int slidersAtMin = 0;

        // Verificar cuántos sliders están en su valor máximo y mínimo
        if (health >= maxStatValue) slidersAtMax++;
        if (happiness >= maxStatValue) slidersAtMax++;
        if (imagination >= maxStatValue) slidersAtMax++;

        if (health <= minStatValue) slidersAtMin++;
        if (happiness <= minStatValue) slidersAtMin++;
        if (imagination <= minStatValue) slidersAtMin++;

        // Mostrar el panel de Delulu si 2 o más sliders están al máximo
        if (slidersAtMax >= 2 && panelEndingDelulu != null && !panelEndingDelulu.activeSelf)
        {
            panelEndingDelulu.SetActive(true);
            Debug.Log("Ending Delulu activado.");

            // Seleccionar el primer botón del panel
            SelectFirstButtonInPanel(panelEndingDelulu);
        }

        // Mostrar el panel de Bad Ending si 2 o más sliders están al mínimo
        if (slidersAtMin >= 2 && panelBadEnding != null && !panelBadEnding.activeSelf)
        {
            panelBadEnding.SetActive(true);
            Debug.Log("Bad Ending activado.");

            // Seleccionar el primer botón del panel
            SelectFirstButtonInPanel(panelBadEnding);
        }
    }
    private void SelectFirstButtonInPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogWarning("El panel es nulo. No se puede seleccionar el botón.");
            return;
        }

        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        if (buttons.Length > 0)
        {
            GameObject firstButton = buttons[0].gameObject;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
            Debug.Log($"Primer botón seleccionado en el panel: {firstButton.name}");
        }
        else
        {
            Debug.LogWarning($"No se encontraron botones en el panel {panel.name}.");
        }
    }






    public IEnumerator final()
    {
        yield return new WaitForSeconds(3f);


        // Actualiza el texto de "Bob survived X days"
        FindObjectOfType<SurvivalDaysText>()?.UpdateSurvivalText();
    }



    // Métodos para agregar valores a las barras
    public void AddHealth(float amount)
    {
        if (!isHealthFrozen)
        {
            health += amount;
        }
    }

    public void AddHappiness(float amount)
    {
        if (!isHappinessFrozen)
        {
            happiness += amount;
        }
    }

    public void AddImagination(float amount)
    {
        if (!isImaginationFrozen)
        {
            imagination += amount;
        }
    }

    private void ConfigureMobileButtons()
    {
        // Asegúrate de que los botones estén activos
        if (healthButton != null && healthButton2 != null)
        {
            healthButton.gameObject.SetActive(true);
            healthButton2.gameObject.SetActive(true);
            healthButton.onClick.AddListener(() => HandleHealthInput());
            healthButton2.onClick.AddListener(() => HandleHealthInput());
        }

        if (happinessButton != null && happinessButton2 != null)
        {
            happinessButton.gameObject.SetActive(true);
            happinessButton2.gameObject.SetActive(true);
            happinessButton.onClick.AddListener(() => HandleHappinessInput());
            happinessButton2.onClick.AddListener(() => HandleHappinessInput());
        }

        if (imaginationButton != null && imaginationButton2 != null)
        {
            imaginationButton.gameObject.SetActive(true);
            imaginationButton.gameObject.SetActive(true);
            imaginationButton.onClick.AddListener(() => HandleImaginationInput());
            imaginationButton2.onClick.AddListener(() => HandleImaginationInput());
        }
    }

    // Manejar el input para la barra de salud
    private void HandleHealthInput()
    {
        if (!isHealthFrozen && health < maxStatValue)
        {
            AudioManager.Instance.PlaySFX("sfx_ui_page");
            health += 1;
            playerPositions.MoveToHealthPosition();
            ShowFloatingText(healthBar.transform, "+1");
            playerAnimationController.PlayHealthAnimation();
            
        }
    }

    // Manejar el input para la barra de felicidad
    private void HandleHappinessInput()
    {
        if (!isHappinessFrozen && happiness < maxStatValue)
        {
            AudioManager.Instance.PlaySFX("sfx_ui_button");
            happiness += 1;
            playerPositions.MoveToHappinessPosition();
            ShowFloatingText(happinessBar.transform, "+1");
            playerAnimationController.PlayHappinessAnimation();
        }
    }

    // Manejar el input para la barra de imaginación
    private void HandleImaginationInput()
    {
        if (!isImaginationFrozen && imagination < maxStatValue)
        {
            AudioManager.Instance.PlaySFX("Sfx_UI");
            imagination += 1;
            playerPositions.MoveToImaginationPosition();
            ShowFloatingText(imaginationBar.transform, "+1");
            playerAnimationController.PlayImaginationAnimation();
        }
    }

    private void InitializeBars()
    {
        // Configura las barras y otras variables
        if (healthBar != null) healthBar.maxValue = maxStatValue;
        if (happinessBar != null) happinessBar.maxValue = maxStatValue;
        if (imaginationBar != null) imaginationBar.maxValue = maxStatValue;
    }
}