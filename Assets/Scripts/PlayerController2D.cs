using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 7.0f;
    public float blocksToJump = 3.0f;
    public float gravityForce = 35.0f;
    private float jumpVelocity;

    [Header("Components")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("Spawn")]
    public GameObject plataformaPrefab;
    public float distanciaSpawn = 3f;
    public float centroX = 0f;

    public bool isJumpPressed;
    private Vector2 moveInput;
    private bool isGrounded;

    void Start()
    {
        jumpVelocity = Mathf.Sqrt(2 * gravityForce * blocksToJump);
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (!isGrounded && moveInput.y < -0.5f)
        {
            rb.linearVelocity += Vector2.down * 50f * Time.deltaTime;
        }

        HandleAnimations();
        FlipSprite();
    }

    void FixedUpdate()
    {
        if (!isGrounded)
        {
            rb.linearVelocity += Vector2.down * gravityForce * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -0.1f);
        }

        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) isJumpPressed = true;
        if (context.canceled) isJumpPressed = false;

        if (context.started && isGrounded)
        {
            if (moveInput.y < -0.5f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity * 0.6f);
            else
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
        }
    }

    // 🔥 NUEVO INPUT PARA SPAWN
    public void OnSpawn(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        SpawnPlataforma();
    }

    void SpawnPlataforma()
    {
        // Dirección según hacia dónde mira el personaje
        float direccion = spriteRenderer.flipX ? 1 : -1;

        Vector3 posicion = transform.position + new Vector3(direccion * distanciaSpawn, 0, 0);

        // Spawn original
        Instantiate(plataformaPrefab, posicion, Quaternion.identity);

        // Calcular espejo
        float xEspejo = 2 * centroX - posicion.x;
        Vector3 posicionEspejo = new Vector3(xEspejo, posicion.y, posicion.z);

        // Spawn espejo
        Instantiate(plataformaPrefab, posicionEspejo, Quaternion.identity);
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