using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 7f;
    public float jumpForce = 12f;
    public float fastFallMultiplier = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Components")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("Spawn")]
    public GameObject plataformaPrefab;
    public float distanciaSpawn = 3f;
    public float centroX = 0f;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool isGrounded;

    void Update()
    {
        CheckGround();

        HandleAnimations();
        FlipSprite();
    }

    void FixedUpdate()
    {
        Move();
        Jump();
        BetterFall();
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        jumpPressed = false;
    }

    void BetterFall()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                (fastFallMultiplier - 1) *
                Time.fixedDeltaTime;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            jumpPressed = true;
    }

    void FlipSprite()
    {
        if (moveInput.x > 0.1f)
            spriteRenderer.flipX = true;
        else if (moveInput.x < -0.1f)
            spriteRenderer.flipX = false;
    }

    void HandleAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("Grounded", isGrounded);
        animator.SetBool("Crouch", moveInput.y < -0.5f && isGrounded);
        animator.SetBool("FastFall", !isGrounded && moveInput.y < -0.5f);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);

    }

    public void OnSpawn(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        SpawnPlataforma();
    }

    void SpawnPlataforma()
    {
        // Dirección según hacia dónde mira el personaje
        float direccion = spriteRenderer.flipX ? 1 : -1;

        Vector3 posicion = transform.position + Vector3.down * 0.6f;

        // Spawn original
        Instantiate(plataformaPrefab, posicion, Quaternion.identity);

        // Calcular espejo
        float xEspejo = 2 * centroX - posicion.x;
        Vector3 posicionEspejo = new Vector3(xEspejo, posicion.y, posicion.z);

        // Spawn espejo
        Instantiate(plataformaPrefab, posicionEspejo, Quaternion.identity);
    }

}