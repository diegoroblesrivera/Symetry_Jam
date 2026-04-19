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

    [Header("Animators")]
    public RuntimeAnimatorController animatorControllerNormal;
    public RuntimeAnimatorController animatorControllerBloqueado;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip resetClip; 

    [Header("Coyote Time")]
    public float coyoteTime = 0.15f; // Duración del coyote time en segundos

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool isGrounded;
    private int bloquesRestantes;
    private bool puedeColocarBloques = true;
    private float coyoteTimeCounter;

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

        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (jumpPressed && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimeCounter = 0f; // Evita saltos dobles durante el coyote time
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

        ActualizarTransparencia();
    }

    // Llama este método desde MiddleLine para bloquear la colocación
    public void BloquearColocacionBloques()
    {
        puedeColocarBloques = false;
        if (animator != null && animatorControllerBloqueado != null)
        {
            animator.runtimeAnimatorController = animatorControllerBloqueado;
        }
        bloquesRestantes = 3; // Resetea los bloques restantes al límite inicial
        ActualizarTransparencia();
    }

    // Modifica SpawnPlataforma para respetar el límite y el bloqueo
    void SpawnPlataforma()
    {
        if (!puedeColocarBloques || bloquesRestantes <= 0)
            return;

        GameObject plataformasDinamicas = GameObject.Find("Plataformas_Dinamicas");
        if (plataformasDinamicas == null)
        {
            Debug.LogWarning("No se encontró el objeto Plataformas_Dinamicas en la escena.");
            return;
        }

        float direccion = spriteRenderer.flipX ? 1 : -1;
        Vector3 posicion = transform.position + Vector3.down * 0.6f;
        float xEspejo = 2 * centroX - posicion.x;
        Vector3 posicionEspejo = new Vector3(xEspejo, posicion.y, posicion.z);

        GameObject bloque = Instantiate(plataformaPrefab, posicion, Quaternion.identity, plataformasDinamicas.transform);
        GameObject bloqueEspejo = Instantiate(plataformaPrefab, posicionEspejo, Quaternion.identity, plataformasDinamicas.transform);
        CambiarColorABlanco(bloqueEspejo);

        bloquesRestantes--;
        ActualizarTransparencia();
    }

    // Método auxiliar para cambiar el color a blanco
    private void CambiarColorABlanco(GameObject bloque)
    {
        var sr = bloque.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.white;
    }

    public void SpawnVisualBurst()
    {
        Instantiate(visualBurstPrefab, transform.position, Quaternion.identity, null);
        Destroy(visualBurstPrefab,5f);
    }
    public void OnResetStage()
    {
        SpawnVisualBurst();
        StartCoroutine(DelayedReset());

    }

    private System.Collections.IEnumerator DelayedReset()
    {
        yield return null; // Espera un frame
        LevelManager.Instance.RestartCurrentLevel();
    }

    private void ActualizarTransparencia()
    {
        float alpha = 1f;
        if (bloquesRestantes >= 3)
            alpha = 1f;
        else if (bloquesRestantes == 2)
            alpha = 0.90f;
        else if (bloquesRestantes == 1)
            alpha = 0.80f;
        else if (bloquesRestantes == 0)
            alpha = 0.40f;

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
    }
}