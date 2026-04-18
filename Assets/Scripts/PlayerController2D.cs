using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 7.0f;
    public float blocksToJump = 3.0f; // Altura deseada en bloques
    public float gravityForce = 35.0f; // Tu gravedad manual
    private float jumpVelocity; // Se calculará sola


    [Header("Components")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public bool isJumpPressed;
    private Vector2 moveInput;
    private bool isGrounded;

    void Start()
    {
        // Fórmula física: Velocidad = Raíz Cuadrada de (2 * Gravedad * Altura)
        jumpVelocity = Mathf.Sqrt(2 * gravityForce * blocksToJump);
    }
    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Lógica de "bajar rápido" en el aire
        if (!isGrounded && moveInput.y < -0.5f)
        {
            // Sumamos un extra de caída si presionamos abajo (como tu script de Godot)
            rb.linearVelocity += Vector2.down * 50f * Time.deltaTime;
        }

        HandleAnimations();
        FlipSprite();
    }

    void FixedUpdate()
    {
        // 1. APLICAR GRAVEDAD MANUAL (Como en Godot: velocity += gravity * delta)
        if (!isGrounded)
        {
            rb.linearVelocity += Vector2.down * gravityForce * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y < 0)
        {
            // Evita que la gravedad se acumule infinitamente cuando estás en el suelo
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -0.1f);
        }

        // 2. MOVIMIENTO HORIZONTAL
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // Detectamos si el botón está presionado o soltado
        if (context.started) isJumpPressed = true;
        if (context.canceled) isJumpPressed = false;

        if (context.started && isGrounded)
        {
            if (moveInput.y < -0.5f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity * 0.6f);
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            }
        }
    }

    void HandleAnimations()
    {
        bool isMovingDown = moveInput.y < -0.5f;
        bool isMovingUp = moveInput.y > 0.5f;

        if (isGrounded)
        {
            if (Mathf.Abs(moveInput.x) < 0.1f)
                animator.Play(isMovingDown ? "crawl" : "idle");
            else
                animator.Play("run");
        }
        else
        {
            // Lógica de animaciones aire (tu script original)
            if (rb.linearVelocity.y > -2f)
                animator.Play(isMovingDown ? "crawl" : "jump");
            else
                animator.Play(isMovingUp ? "jump" : "crawl");
        }
    }

    void FlipSprite()
    {
        if (moveInput.x > 0) spriteRenderer.flipX = true;
        else if (moveInput.x < 0) spriteRenderer.flipX = false;
    }
}