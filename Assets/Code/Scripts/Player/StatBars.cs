using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Necesario si usas barras de tipo UI Slider

public class StatBars : MonoBehaviour
{
    // M�ximos y m�nimos de las barras
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

    // Referencias opcionales a sliders o im�genes para mostrar las barras visualmente
    public Slider healthBar;
    public Slider happinessBar;
    public Slider imaginationBar;

    private void Start()
    {
        // Configura los sliders si est�n asignados
        if (healthBar != null)
            healthBar.maxValue = maxStatValue;

        if (happinessBar != null)
            happinessBar.maxValue = maxStatValue;

        if (imaginationBar != null)
            imaginationBar.maxValue = maxStatValue;
    }

    private void Update()
    {
        // Reducir los valores de las barras con base en su velocidad
        health = Mathf.Clamp(health - healthDecreaseSpeed * Time.deltaTime, minStatValue, maxStatValue);
        happiness = Mathf.Clamp(happiness - happinessDecreaseSpeed * Time.deltaTime, minStatValue, maxStatValue);
        imagination = Mathf.Clamp(imagination - imaginationDecreaseSpeed * Time.deltaTime, minStatValue, maxStatValue);

        // Actualizar los sliders si est�n asignados
        if (healthBar != null)
            healthBar.value = health;

        if (happinessBar != null)
            happinessBar.value = happiness;

        if (imaginationBar != null)
            imaginationBar.value = imagination;

        // Manejar el input para incrementar barras
        HandleInput();
    }

    private void HandleInput()
    {
        Vector2 moveDirection = InputManager.GetInstance().GetMoveDirection();

        if (moveDirection == Vector2.left) // Bot�n izquierdo
        {
            Debug.Log("Izquierdo");
            AddHealth(1);
        }
        else if (moveDirection == Vector2.up) // Bot�n arriba
        {
            Debug.Log("Arriba");
            AddHappiness(1);
        }
        else if (moveDirection == Vector2.right) // Bot�n derecho
        {
            Debug.Log("Derecho");
            AddImagination(1);
        }
    }

    // M�todos para agregar valores a las barras
    public void AddHealth(float amount)
    {
        health = Mathf.Clamp(health + amount, minStatValue, maxStatValue);
    }

    public void AddHappiness(float amount)
    {
        happiness = Mathf.Clamp(happiness + amount, minStatValue, maxStatValue);
    }

    public void AddImagination(float amount)
    {
        imagination = Mathf.Clamp(imagination + amount, minStatValue, maxStatValue);
    }
}


