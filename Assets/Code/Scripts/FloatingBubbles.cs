using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingBubbles : MonoBehaviour
{
    
    [Header("Configuraci�n de Flotaci�n")]
    public float floatAmplitude = 0.5f; // Amplitud del movimiento vertical
    public float floatDuration = 1.5f; // Duraci�n del movimiento completo (arriba y abajo)

    private Vector3 initialPosition;

    private void Start()
    {
        // Guardar la posici�n inicial de la burbuja
        initialPosition = transform.position;

        // Comenzar la animaci�n de flotaci�n
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
