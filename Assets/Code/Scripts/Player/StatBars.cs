using UnityEngine;
using UnityEngine.UI; // Necesario si usas barras de tipo UI Slider
using TMPro; // Para usar TextMeshPro

public class StatBars : MonoBehaviour
{
    // Máximos y mínimos de las barras
    private const int maxStatValue = 100;
    private const int minStatValue = 0;

    // Valores actuales de las barras
    public float health = maxStatValue;
    public float happiness = maxStatValue;
    public float imagination = maxStatValue;

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

    private InputManager inputManager;

    // Estados de congelación
    private bool isHealthFrozen = false;
    private bool isHappinessFrozen = false;
    private bool isImaginationFrozen = false;

    private void Start()
    {
        // Configura los sliders si están asignados
        if (healthBar != null)
            healthBar.maxValue = maxStatValue;

        if (happinessBar != null)
            happinessBar.maxValue = maxStatValue;

        if (imaginationBar != null)
            imaginationBar.maxValue = maxStatValue;

        // Obtén la instancia del InputManager
        inputManager = InputManager.GetInstance();

        // Actualizar textos iniciales
        UpdateTextValues();
    }

    private void Update()
    {
        // Reducir los valores de las barras con base en su velocidad si no están congeladas
        if (!isHealthFrozen)
        {
            health = Mathf.Clamp(health - healthDecreaseSpeed * Time.deltaTime, minStatValue, maxStatValue);
            CheckFreezeState(ref health, ref isHealthFrozen, healthBar);
        }

        if (!isHappinessFrozen)
        {
            happiness = Mathf.Clamp(happiness - happinessDecreaseSpeed * Time.deltaTime, minStatValue, maxStatValue);
            CheckFreezeState(ref happiness, ref isHappinessFrozen, happinessBar);
        }

        if (!isImaginationFrozen)
        {
            imagination = Mathf.Clamp(imagination - imaginationDecreaseSpeed * Time.deltaTime, minStatValue, maxStatValue);
            CheckFreezeState(ref imagination, ref isImaginationFrozen, imaginationBar);
        }

        // Actualizar los sliders si están asignados
        if (healthBar != null)
            healthBar.value = health;

        if (happinessBar != null)
            happinessBar.value = happiness;

        if (imaginationBar != null)
            imaginationBar.value = imagination;

        // Manejar el input para incrementar barras
        HandleInput();

        // Actualizar textos de valores
        UpdateTextValues();
    }

    private void HandleInput()
    {
        Vector2 moveDirection = inputManager.GetMoveDirection();

        if (moveDirection == Vector2.left && !isHealthFrozen) // Botón izquierdo
        {
            AddHealth(1);
        }
        else if (moveDirection == Vector2.up && !isHappinessFrozen) // Botón arriba
        {
            AddHappiness(1);
        }
        else if (moveDirection == Vector2.right && !isImaginationFrozen) // Botón derecho
        {
            AddImagination(1);
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
        if (value == maxStatValue)
        {
            if (!isFrozen)
            {
                isFrozen = true;
                Debug.Log("Barra Freeze: Alcanzó el máximo");
                if (slider != null)
                {
                    ColorBlock colors = slider.colors;
                    colors.disabledColor = Color.gray;
                    slider.colors = colors;
                }
            }
        }
        else if (value == minStatValue)
        {
            if (!isFrozen)
            {
                isFrozen = true;
                Debug.Log("Barra Freeze: Alcanzó el mínimo");
                if (slider != null)
                {
                    ColorBlock colors = slider.colors;
                    colors.disabledColor = Color.gray;
                    slider.colors = colors;
                }
            }
        }
    }

    // Métodos para agregar valores a las barras
    public void AddHealth(float amount)
    {
        if (!isHealthFrozen)
        {
            health = Mathf.Clamp(health + amount, minStatValue, maxStatValue);
        }
    }

    public void AddHappiness(float amount)
    {
        if (!isHappinessFrozen)
        {
            happiness = Mathf.Clamp(happiness + amount, minStatValue, maxStatValue);
        }
    }

    public void AddImagination(float amount)
    {
        if (!isImaginationFrozen)
        {
            imagination = Mathf.Clamp(imagination + amount, minStatValue, maxStatValue);
        }
    }
}
