using UnityEngine;
using UnityEngine.UI; // Necesario si usas barras de tipo UI Slider
using TMPro; // Para usar TextMeshPro

public class StatBars : MonoBehaviour
{
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

    // Referencias a los paneles UI
    public GameObject panelBadEnding;
    public GameObject panelEndingDelulu;

    private PlayerPositions playerPositions;

    // Estados de congelación
    private bool isHealthFrozen = false;
    private bool isHappinessFrozen = false;
    private bool isImaginationFrozen = false;

    private void Start()
    {
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
        if (GameManager.Instance.CurrentGameState == GameStates.Gameplay)
        {
            if (!isHealthFrozen)
            {
                health = Mathf.Max(health - healthDecreaseSpeed * Time.deltaTime, minStatValue);
                CheckFreezeState(ref health, ref isHealthFrozen, healthBar);
               
            }

            if (!isHappinessFrozen)
            {
                happiness = Mathf.Max(happiness - happinessDecreaseSpeed * Time.deltaTime, minStatValue);
                CheckFreezeState(ref happiness, ref isHappinessFrozen, happinessBar);
            }

            if (!isImaginationFrozen)
            {
                imagination = Mathf.Max(imagination - imaginationDecreaseSpeed * Time.deltaTime, minStatValue);
                CheckFreezeState(ref imagination, ref isImaginationFrozen, imaginationBar);
            }
        }

        // Actualizar los sliders si están asignados
        UpdateSliders();

        // Manejar el input para incrementar barras
        HandleInput();

        // Actualizar textos de valores
        UpdateTextValues();

        // Verificar condiciones de fin de juego
        CheckGameEndings();
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
            if (moveDirection == Vector2.up && !isHealthFrozen) // Botón izquierdo
            {
                health += 1;
                playerPositions.MoveToHealthPosition();
            }
            else if (moveDirection == Vector2.right && !isHappinessFrozen) // Botón arriba
            {
                happiness += 1;
                playerPositions.MoveToHappinessPosition();
            }
            else if (moveDirection == Vector2.left && !isImaginationFrozen) // Botón derecho
            {
                imagination += 1;
                playerPositions.MoveToImaginationPosition();
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
