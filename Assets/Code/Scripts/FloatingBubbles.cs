using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingBubbles : MonoBehaviour
{
    
    [Header("Configuración de Flotación")]
    public float floatAmplitude = 0.5f; // Amplitud del movimiento vertical
    public float floatDuration = 1.5f; // Duración del movimiento completo (arriba y abajo)

    private Vector3 initialPosition;

    private void Start()
    {
        // Guardar la posición inicial de la burbuja
        initialPosition = transform.position;

        // Comenzar la animación de flotación
        StartFloating();
    }

    private void StartFloating()
    {
        // Movimiento hacia arriba
        transform.DOMoveY(initialPosition.y + floatAmplitude, floatDuration)
            .SetEase(Ease.InOutSine) // Movimiento fluido
            .SetLoops(-1, LoopType.Yoyo); // Repetir infinitamente con ida y vuelta
    }

}
