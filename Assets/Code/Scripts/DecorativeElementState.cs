using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DecorativeElementState : MonoBehaviour
{
    public Sprite neutralSprite;
    public Sprite happySprite;
    public Sprite sadSprite;

    private SpriteRenderer spriteRenderer;
    private HUDManager hudManager;

    private Vector3 originalScale; // Para guardar la escala original del objeto
    private Sprite currentSprite; // Para evitar repetir la misma animación si no hay cambio

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        hudManager = FindObjectOfType<HUDManager>();
        originalScale = transform.localScale; // Guardar la escala original
        currentSprite = spriteRenderer.sprite; // Guardar el sprite actual
        UpdateSpriteWithAnimation(); // Llamar a la actualización al inicio
    }

    private void Update()
    {
        UpdateSpriteWithAnimation();
    }

    private void UpdateSpriteWithAnimation()
    {
        if (hudManager == null || spriteRenderer == null) return;

        // Calcular el promedio de las barras
        float averageStat = (hudManager.health + hudManager.happiness + hudManager.imagination) / 3;

        Sprite newSprite;

        // Determinar el sprite según el promedio
        if (averageStat >= 70)
        {
            newSprite = happySprite;
        }
        else if (averageStat <= 30)
        {
            newSprite = sadSprite;
        }
        else
        {
            newSprite = neutralSprite;
        }

        // Si el sprite cambia, actualizamos y ejecutamos la animación
        if (newSprite != currentSprite)
        {
            currentSprite = newSprite;
            spriteRenderer.sprite = newSprite;

            // Realizar la animación de bounce con DoTween
            transform.DOKill(); // Cancelar cualquier animación en curso
            transform.localScale = originalScale; // Resetear la escala
            transform.DOScale(originalScale * 1.2f, 0.3f).SetEase(Ease.OutBounce) // Escala hacia afuera
                     .OnComplete(() =>
                     {
                         transform.DOScale(originalScale, 0.2f).SetEase(Ease.InBounce); // Regresar a la escala original
                     });
        }
    }
}

