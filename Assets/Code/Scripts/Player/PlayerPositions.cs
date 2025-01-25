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


    // M�todo para mover al jugador a la posici�n de salud
    public void MoveToHealthPosition()
    {
        if (player != null && healthPosition != null)
        {
            player.transform.position = healthPosition.position;
            Debug.Log("Jugador movido a la posici�n de Salud.");
        }
    }

    // M�todo para mover al jugador a la posici�n de felicidad
    public void MoveToHappinessPosition()
    {
        if (player != null && happinessPosition != null)
        {
            player.transform.position = happinessPosition.position;
            Debug.Log("Jugador movido a la posici�n de Felicidad.");
        }
    }

    // M�todo para mover al jugador a la posici�n de imaginaci�n
    public void MoveToImaginationPosition()
    {
        if (player != null && imaginationPosition != null)
        {
            player.transform.position = imaginationPosition.position;
            Debug.Log("Jugador movido a la posici�n de Imaginaci�n.");
        }
    }
}

