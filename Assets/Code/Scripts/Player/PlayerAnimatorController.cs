using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        // Obtener la referencia al Animator
        animator = GetComponent<Animator>();
    }

    public void PlayHealthAnimation()
    {
        animator.SetTrigger("PlayHealth");
    }

    public void PlayHappinessAnimation()
    {
        animator.SetTrigger("PlayHappiness");
    }

    public void PlayImaginationAnimation()
    {
        animator.SetTrigger("PlayImagination");
    }
}

