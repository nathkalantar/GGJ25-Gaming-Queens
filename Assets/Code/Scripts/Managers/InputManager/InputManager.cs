using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// This script acts as a single point for all other scripts to get
// the current input from. It uses Unity's new Input System and
// functions should be mapped to their corresponding controls
// using a PlayerInput component with Unity Events.

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    private Vector2 interactionDirection = Vector2.zero;
    private bool moveTapped = false; // Nueva variable para detectar el "tap"

    private bool interactPressed = false;
    private bool submitPressed = false;
    private bool pausePressed = false;

    private static InputManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Input Manager in the scene.");
        }
        instance = this;
    }

    public static InputManager GetInstance() 
    {
        return instance;
    }

    public void MovePressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Registrar la dirección solo si es un "tap"
            interactionDirection = context.ReadValue<Vector2>();
            moveTapped = true;
        }
        else if (context.canceled)
        {
            interactionDirection = Vector2.zero; // Resetear la dirección cuando se cancela
        }
    }

    public void InteractButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactPressed = true;
        }
        else if (context.canceled)
        {
            interactPressed = false;
        } 
    }

    public void SubmitPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            submitPressed = true;
        }
        else if (context.canceled)
        {
            submitPressed = false;
        } 
    }
    public void PausePressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            pausePressed = true;
        }
        else if (context.canceled)
        {
            pausePressed = false;
        } 
    }

    public Vector2 GetMoveDirection() 
    {
        if (moveTapped)
        {
            // Retorna la dirección solo una vez
            moveTapped = false;
            Vector2 result = interactionDirection;
            interactionDirection = Vector2.zero; // Resetear después del tap
            return result;
        }

        return Vector2.zero; // Si no hubo "tap", retorna cero
    }

    // for any of the below 'Get' methods, if we're getting it then we're also using it,
    // which means we should set it to false so that it can't be used again until actually
    // pressed again.

    public bool GetInteractPressed() 
    {
        bool result = interactPressed;
        interactPressed = false;
        return result;
    }
    public bool GetPausePressed() 
    {
        bool result = pausePressed;
        pausePressed = false;
        return result;
    }

    public bool GetSubmitPressed() 
    {
        bool result = submitPressed;
        submitPressed = false;
        return result;
    }

    public void RegisterSubmitPressed() 
    {
        submitPressed = false;
    }

}
