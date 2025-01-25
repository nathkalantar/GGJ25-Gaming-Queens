using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositions : MonoBehaviour
{
    public GameObject player;

    // Posiciones para las acciones
    public Transform healthPosition;
    public Transform happinessPosition;
    public Transform imaginationPosition;


    // Método para mover al jugador a la posición de salud
    public void MoveToHealthPosition()
    {
        if (player != null && healthPosition != null)
        {
            player.transform.position = healthPosition.position;
            Debug.Log("Jugador movido a la posición de Salud.");
        }
    }

    // Método para mover al jugador a la posición de felicidad
    public void MoveToHappinessPosition()
    {
        if (player != null && happinessPosition != null)
        {
            player.transform.position = happinessPosition.position;
            Debug.Log("Jugador movido a la posición de Felicidad.");
        }
    }

    // Método para mover al jugador a la posición de imaginación
    public void MoveToImaginationPosition()
    {
        if (player != null && imaginationPosition != null)
        {
            player.transform.position = imaginationPosition.position;
            Debug.Log("Jugador movido a la posición de Imaginación.");
        }
    }
}

