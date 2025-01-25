using UnityEngine;
using UnityEngine.UI; // Necesario si usas barras de tipo UI Slider
using TMPro;
using System.Collections.Generic; // Para usar TextMeshPro
using DG.Tweening;
using System.Collections;

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
    public TextMeshProUGUI timerText;

    // Texto para el día actual
    public TextMeshProUGUI dayText;
    public float countdown;

    // Estados de congelación
    private bool isHealthFrozen = false;
    private bool isHappinessFrozen = false;
    private bool isImaginationFrozen = false;

    private void Start()
    {
        happinessBar.gameObject.SetActive(false);
        imaginationBar.gameObject.SetActive(false);

        btnAnimations[0].gameObject.SetActive(false);
        btnAnimations[2].gameObject.SetActive(false);

        countdown = 10f;
        DaySituation();

        Day = 1;
        dayTimer= 0;
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

        // Verificar condiciones de fin de juego
        CheckGameEndings();
    }

    private IEnumerator DayTimer()
    {
        while (true)
        {
            // Inicializa el temporizador según el día actual
            DaySituation();

            while (countdown > 0)
            {
                countdown -= Time.deltaTime; // Reducir el tiempo restante
                UpdateTimerText(countdown); // Actualizar el texto del temporizador
                yield return null; // Esperar al siguiente frame
            }

            // Incrementar el día cuando el temporizador llegue a 0
            Day++;
            ShowDayAnimation(Day); // Mostrar animación para el nuevo día
            Debug.Log($"Día incrementado a: {Day}");
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
            dayText.text = $"Día {currentDay}";
            dayText.alpha = 0; // Asegurarse de que el texto esté invisible al inicio

            // Animación con DoTween
            dayText.DOFade(1, 1f) // Fade in
                   .OnComplete(() =>
                   {
                       dayText.DOFade(0, 1f).SetDelay(2f); // Fade out con delay
                   });

            dayText.transform.localScale = Vector3.zero; // Escala inicial
            dayText.transform.DOScale(1.5f, 1f).SetEase(Ease.OutBounce); // Animación de escala
        }
    }
    private float DayDecreased(float Statvalue)
    {
        switch (Day)
        {
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                Statvalue = Statvalue * 3;
                break;
            case 6:
                Statvalue = Statvalue * 10;
                break;
            default:
                Statvalue =  Statvalue * 1.1f;
                break;
            
        }
        return Statvalue;
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
            imaginationBar.gameObject.SetActive(true); // Activa la barra de imaginación
            countdown = 10f;
            break;
        case 3:
            btnAnimations[2].gameObject.SetActive(true);
            happinessBar.gameObject.SetActive(true); // Activa la barra de felicidad
            countdown = 15f;
            break;
        case 4:
            countdown = 15f;
            break;
        case 5:
            countdown = 20f;
            break;
        case 6:
            countdown = 20f;
            break;
        default:
            countdown = 30f; // Valor por defecto para días posteriores
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
                health += 1;
                playerPositions.MoveToHealthPosition();
                btnAnimations[1].SetTrigger("Pressed");
                ShowFloatingText(healthBar.transform, "+1");

            }
            else if (moveDirection == Vector2.right && !isHappinessFrozen && btnAnimations[2].gameObject.activeSelf)
            {
                happiness += 1;
                playerPositions.MoveToHappinessPosition();
                btnAnimations[2].SetTrigger("Pressed");
                ShowFloatingText(happinessBar.transform, "+1");
            }
            else if (moveDirection == Vector2.left && !isImaginationFrozen && btnAnimations[0].gameObject.activeSelf)
            {
                imagination += 1;
                playerPositions.MoveToImaginationPosition();
                btnAnimations[0].SetTrigger("Pressed");
                ShowFloatingText(imaginationBar.transform, "+1");
            }
        }
    }

    private void ShowFloatingText(Transform target, string text)
    {
        if (floatingTextPrefab != null)
        {
            // Instanciar el texto flotante cerca del objetivo
            GameObject floatingText = Instantiate(floatingTextPrefab, target.position, Quaternion.identity, target);
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
        // Verificar si todas las barras están en 0
        if (health == minStatValue && happiness == minStatValue && imagination == minStatValue)
        {
            if (panelBadEnding != null)
                panelBadEnding.SetActive(true);
        }

        // Verificar si todas las barras están en 100
        if (health >= maxStatValue && happiness >= maxStatValue && imagination >= maxStatValue)
        {
            if (panelEndingDelulu != null)
                panelEndingDelulu.SetActive(true);
        }
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
}
