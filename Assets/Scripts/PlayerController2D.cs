using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum PlayerAbilityType
{
    None,
    PlaceBlock,
    Dash,
    DoubleJump,
    Invulnerability
}

[System.Serializable]
public class AbilityPalette
{
    public PlayerAbilityType ability;
    public Color color0 = Color.white;   // Más claro
    public Color color1 = Color.gray;    // Gris claro
    public Color color2 = Color.gray;    // Gris oscuro
    public Color color3 = Color.black;   // Más oscuro
}

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 7f;
    public float jumpForce = 12f;
    public float coyoteTime = 0.15f;
    public float fastFallMultiplier = 2f;

    [Header("Chequeo de suelo")]
    public Transform groundCheck;
    public float groundRadius = 0.25f;
    public LayerMask groundLayer;
    public int groundRayCount = 3;
    public float groundRayLength = 0.1f;

    [Header("Paletas de habilidades")]
    public List<AbilityPalette> abilityPalettes;
    public AbilityPalette defaultPalette;

    [Header("Componentes")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    // Habilidades
    private Dictionary<PlayerAbilityType, int> habilidades = new();
    private List<PlayerAbilityType> habilidadesOrden = new();
    private int habilidadSeleccionada = 0;

    public PlayerAbilityType HabilidadActual =>
        (habilidadesOrden.Count > 0) ? habilidadesOrden[habilidadSeleccionada] : PlayerAbilityType.None;

    // Estado
    private bool jumpPressed;
    private bool isGrounded;
    private bool canDoubleJump;
    private float coyoteTimeCounter;
    private Vector2 moveInput;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Start()
    {
        canDoubleJump = false;
        ActualizarColorPersonaje();
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
        BetterFall();
    }

    #region Movimiento y Suelo

    void CheckGround()
    {
        // OverlapCircle para área general
        bool overlap = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Múltiples rayos desde el círculo
        bool rayHit = false;
        for (int i = 0; i < groundRayCount; i++)
        {
            float t = (groundRayCount == 1) ? 0.5f : (float)i / (groundRayCount - 1);
            float xOffset = Mathf.Lerp(-groundRadius, groundRadius, t);
            Vector2 origin = (Vector2)groundCheck.position + Vector2.right * xOffset;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundLayer);
            Debug.DrawRay(origin, Vector2.down * groundRayLength, hit.collider ? Color.green : Color.red, 0.02f);
            if (hit.collider != null)
            {
                rayHit = true;
                break;
            }
        }

        isGrounded = overlap || rayHit;

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
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

    #endregion

    #region Input

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            jumpPressed = true;

        if (context.canceled)
            jumpPressed = false;
    }

    public void OnUseAbility(InputAction.CallbackContext context)
    {
        if (context.performed)
            UsarHabilidadActual();
    }

    public void OnNextAbility(InputAction.CallbackContext context)
    {
        if (context.performed)
            CambiarHabilidad(1);
    }

    public void OnPrevAbility(InputAction.CallbackContext context)
    {
        if (context.performed)
            CambiarHabilidad(-1);
    }

    public void OnResetStage()
    {
        LevelManager.Instance.RestartCurrentLevel();
    }

    #endregion

    void LateUpdate()
    {
        HandleJump();
    }

    void HandleJump()
    {
        if (jumpPressed)
        {
            // Salto normal o coyote time
            if (coyoteTimeCounter > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                coyoteTimeCounter = 0f;
                canDoubleJump = habilidades.ContainsKey(PlayerAbilityType.DoubleJump);
            }
            // Doble salto si tiene la habilidad y no está en el suelo
            else if (canDoubleJump && habilidades.ContainsKey(PlayerAbilityType.DoubleJump))
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canDoubleJump = false;
                GastarHabilidad(PlayerAbilityType.DoubleJump);
            }
            jumpPressed = false;
        }
    }

    #region Habilidades

    public void OtorgarHabilidad(PlayerAbilityType tipo, int usos)
    {
        if (habilidades.ContainsKey(tipo))
        {
            habilidades[tipo] += usos;
        }
        else
        {
            habilidades.Add(tipo, usos);
            habilidadesOrden.Add(tipo);
            habilidadSeleccionada = habilidadesOrden.Count - 1; // Se equipa automáticamente
        }
        ActualizarColorPersonaje();
    }

    public void CambiarHabilidad(int direccion)
    {
        if (habilidadesOrden.Count <= 1) return;
        habilidadSeleccionada = (habilidadSeleccionada + direccion + habilidadesOrden.Count) % habilidadesOrden.Count;
        ActualizarColorPersonaje();
    }

    public void UsarHabilidadActual()
    {
        var tipo = HabilidadActual;
        if (tipo == PlayerAbilityType.None || !habilidades.ContainsKey(tipo) || habilidades[tipo] <= 0) return;

        switch (tipo)
        {
            case PlayerAbilityType.PlaceBlock:
                SpawnPlataforma();
                GastarHabilidad(tipo);
                break;
            case PlayerAbilityType.Dash:
                EjecutarDash();
                GastarHabilidad(tipo);
                break;
            case PlayerAbilityType.Invulnerability:
                ActivarInvulnerabilidad();
                GastarHabilidad(tipo);
                break;
            // El doble salto se maneja en el salto
        }
    }

    private void GastarHabilidad(PlayerAbilityType tipo)
    {
        if (!habilidades.ContainsKey(tipo)) return;
        habilidades[tipo]--;
        if (habilidades[tipo] <= 0)
        {
            habilidades.Remove(tipo);
            habilidadesOrden.Remove(tipo);
            habilidadSeleccionada = Mathf.Clamp(habilidadSeleccionada, 0, habilidadesOrden.Count - 1);
            ActualizarColorPersonaje();
        }
    }

    #endregion

    #region Plataforma y Dash

    void SpawnPlataforma()
    {
        // Implementa aquí la lógica de instanciación de bloques
    }

    private void EjecutarDash()
    {
        // Implementa aquí la lógica de dash
    }

    private void ActivarInvulnerabilidad()
    {
        // Implementa aquí la lógica de invulnerabilidad temporal
    }

    #endregion

    #region Visual y Animación

    void FlipSprite()
    {
        if (moveInput.x > 0.1f)
            spriteRenderer.flipX = false;
        else if (moveInput.x < -0.1f)
            spriteRenderer.flipX = true;
    }

    void HandleAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("Grounded", isGrounded);
        animator.SetBool("Crouch", moveInput.y < -0.5f && isGrounded);
        animator.SetBool("FastFall", !isGrounded && moveInput.y < -0.5f);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
    }

    public void ActualizarColorPersonaje()
    {
        AbilityPalette palette = defaultPalette;
        var tipo = HabilidadActual;
        foreach (var ap in abilityPalettes)
        {
            if (ap.ability == tipo)
            {
                palette = ap;
                break;
            }
        }
        if (spriteRenderer != null)
        {
            var mat = spriteRenderer.material;
            mat.SetColor("_Color0", palette.color0);
            mat.SetColor("_Color1", palette.color1);
            mat.SetColor("_Color2", palette.color2);
            mat.SetColor("_Color3", palette.color3);
        }
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
            // Dibuja los rayos
            for (int i = 0; i < groundRayCount; i++)
            {
                float t = (groundRayCount == 1) ? 0.5f : (float)i / (groundRayCount - 1);
                float xOffset = Mathf.Lerp(-groundRadius, groundRadius, t);
                Vector2 origin = (Vector2)groundCheck.position + Vector2.right * xOffset;
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(origin, origin + Vector2.down * groundRayLength);
            }
        }
    }

    #endregion
}