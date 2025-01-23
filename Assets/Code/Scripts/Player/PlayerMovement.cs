using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
     // Velocidad del jugador
    public float speed = 5f;

    // Tiempo de desaceleración (Coyote Time)
    public float coyoteTimeDuration = 0.05f;

    // Referencia al Rigidbody2D
    private Rigidbody2D rb;

    // Inputs y velocidad
    private Vector2 movementInput;
    private Vector2 currentVelocity;

    // Temporizador del coyote time
    private float coyoteTimeCounter;

    private void Awake()
    {
        // Obtener el componente Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Capturar el input del jugador
        movementInput.x = Input.GetAxis("Horizontal");
        movementInput.y = Input.GetAxis("Vertical");

        // Normalizar el input para que no se mueva más rápido en diagonal
        movementInput = movementInput.normalized;

        // Contar el tiempo para el coyote time
        if (movementInput.magnitude > 0)
        {
            coyoteTimeCounter = coyoteTimeDuration;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        // Calcular la velocidad del jugador considerando el coyote time
        if (coyoteTimeCounter > 0)
        {
            // Suavizar la transición entre la velocidad actual y el input del jugador
            currentVelocity = Vector2.Lerp(currentVelocity, movementInput * speed, 0.2f);
        }
        else
        {
            // Si no hay input, detener al jugador lentamente
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, 0.1f);
        }

        // Aplicar movimiento al Rigidbody2D
        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
    }
}
