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

    [Header("VisualBurst")]
    public GameObject visualBurstPrefab;

    [Header("Bloques")]
    public int maxBloques = 3; // Límite inicial de bloques

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool isGrounded;
    private int bloquesRestantes;
    private bool puedeColocarBloques = true;

    void Start()
    {
        bloquesRestantes = maxBloques;
    }

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

    // Llama este método para aumentar el límite desde un item
    public void AumentarLimiteBloques(int cantidad)
    {
        maxBloques += cantidad;
        bloquesRestantes += cantidad;
    }

    // Llama este método desde MiddleLine para bloquear la colocación
    public void BloquearColocacionBloques()
    {
        puedeColocarBloques = false;
    }

    // Modifica SpawnPlataforma para respetar el límite y el bloqueo
    void SpawnPlataforma()
    {
        if (!puedeColocarBloques || bloquesRestantes <= 0)
            return;

        // Buscar el objeto Plataformas_Dinamicas en la escena
        GameObject plataformasDinamicas = GameObject.Find("Plataformas_Dinamicas");
        if (plataformasDinamicas == null)
        {
            Debug.LogWarning("No se encontró el objeto Plataformas_Dinamicas en la escena.");
            return;
        }

        // Dirección según hacia dónde mira el personaje
        float direccion = spriteRenderer.flipX ? 1 : -1;

        Vector3 posicion = transform.position + Vector3.down * 0.6f;

        // Spawn original
        Instantiate(plataformaPrefab, posicion, Quaternion.identity, plataformasDinamicas.transform);

        // Calcular espejo
        float xEspejo = 2 * centroX - posicion.x;
        Vector3 posicionEspejo = new Vector3(xEspejo, posicion.y, posicion.z);

        // Spawn espejo
        Instantiate(plataformaPrefab, posicionEspejo, Quaternion.identity, plataformasDinamicas.transform);

        bloquesRestantes--;
    }

    public void SpawnVisualBurst()
    {
        Instantiate(visualBurstPrefab, transform.position, Quaternion.identity);
        Destroy(visualBurstPrefab,5f);
    }
    public void OnResetStage()
    {
        StartCoroutine(DelayedReset());
    }

    private System.Collections.IEnumerator DelayedReset()
    {
        yield return null; // Espera un frame
        LevelManager.Instance.RestartCurrentLevel();
    }

}