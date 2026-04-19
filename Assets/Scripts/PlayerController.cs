using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[Header("Gravity")]
	[HideInInspector] public float gravityStrength; //Downwards force (gravity) needed for the desired jumpHeight and jumpTimeToApex.
	[HideInInspector] public float gravityScale; //Strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D).
										  //Also the value the player's rigidbody2D.gravityScale is set to.
	[Space(5)]
	public float fallGravityMult; //Multiplier to the player's gravityScale when falling.
	public float maxFallSpeed; //Maximum fall speed (terminal velocity) of the player when falling.
	[Space(5)]
	public float fastFallGravityMult; //Larger multiplier to the player's gravityScale when they are falling and a downwards input is pressed.
									  //Seen in games such as Celeste, lets the player fall extra fast if they wish.
	public float maxFastFallSpeed; //Maximum fall speed(terminal velocity) of the player when performing a faster fall.
	
	[Space(20)]

    [Header("Run")]
	public float runMaxSpeed; //Target speed we want the player to reach.
	public float runAcceleration; //Time (approx.) time we want it to take for the player to accelerate from 0 to the runMaxSpeed.
	[HideInInspector] public float runAccelAmount; //The actual force (multiplied with speedDiff) applied to the player.
	public float runDecceleration; //Time (approx.) we want it to take for the player to accelerate from runMaxSpeed to 0.
	[HideInInspector] public float runDeccelAmount; //Actual force (multiplied with speedDiff) applied to the player .
	[Space(10)]
	[Range(0.01f, 1)] public float accelInAir; //Multipliers applied to acceleration rate when airborne.
	[Range(0.01f, 1)] public float deccelInAir;
	public bool doConserveMomentum;

	[Space(20)]

	[Header("Jump")]
	public float jumpHeight; //Height of the player's jump
	public float jumpTimeToApex; //Time between applying the jump force and reaching the desired jump height. These values also control the player's gravity and jump force.
	[HideInInspector] public float jumpForce; //The actual force applied (upwards) to the player when they jump.

	public float jumpCutGravityMult; //Multiplier to increase gravity if the player releases thje jump button while still jumping
	[Range(0f, 1)] public float jumpHangGravityMult; //Reduces gravity while close to the apex (desired max height) of the jump
	public float jumpHangTimeThreshold; //Speeds (close to 0) where the player will experience extra "jump hang". The player's velocity.y is closest to 0 at the jump's apex (think of the gradient of a parabola or quadratic function)
	[Space(0.5f)]
	public float jumpHangAccelerationMult; 
	public float jumpHangMaxSpeedMult; 				

    #region COMPONENTS
    public Rigidbody2D rb2d { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }
    public Animator animator { get; private set; }
    [Header("Animators")]
    public RuntimeAnimatorController animatorControllerNormal;
    public RuntimeAnimatorController animatorControllerBloqueado;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip resetClip; 

	#endregion
    [Header("Assists")]
	[Range(0.01f, 0.5f)] public float coyoteTime; //Grace period after falling off a platform, where you can still jump
	[Range(0.01f, 0.5f)] public float jumpInputBufferTime; //Grace period after pressing jump where a jump will be automatically performed once the requirements (eg. being grounded) are met.

	[Space(20)]

    [Header("Bloques")]
    public int maxBloques = 3; // Límite inicial de bloques
    public int bloquesIniciales = 3; // Bloques Iniciales
    private int bloquesRestantes;
    private bool puedeColocarBloques = true;

    [Header("Spawn")]
    public GameObject plataformaPrefab;
    public float distanciaSpawn = 3f;
    public float centroX = 0f;
	
	#region STATE PARAMETERS
	//Variables control the various actions the player can perform at any time.
	//These are fields which can are public allowing for other sctipts to read them
	//but can only be privately written to.
	public bool IsFacingRight { get; private set; }
	public bool IsJumping { get; private set; }
	public float LastOnGroundTime { get; private set; }
	
	//Jump
	private bool _isJumpCut;
	private bool _isJumpFalling;

	#endregion

	#region INPUT PARAMETERS
    private Vector2 _moveInput;

	public float LastPressedJumpTime { get; private set; }
	#endregion

    [Header("VisualBurst")]
    public GameObject visualBurstPrefab;

	#region CHECK PARAMETERS
	//Set all of these up in the inspector
	[Header("Checks")] 
	[SerializeField] private Transform _groundCheckPoint;
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
	#endregion

	#region LAYERS & TAGS
    [Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
	#endregion

    private InputAction moveAction;
	private InputAction jumpAction;
	private InputAction spawnAction;
	private InputAction resetStageAction;

    void Start()
    {
		IsFacingRight = true;
		IsJumping = false;
        bloquesRestantes = bloquesIniciales;
    }

    void Awake()
    {
		InputManagement();
        rb2d = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		
    }

	private void OnValidate()
    {
		//Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
		gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
		//Calculate the rigidbody's gravity scale (ie: gravity strength relative to unity's gravity value, see project settings/Physics2D)
		gravityScale = gravityStrength / Physics2D.gravity.y;

		//Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
		runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
		runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

		//Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
		jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

		#region Variable Ranges
		runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
		runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
		#endregion
	}

	private void InputManagement()
	{
		// Busca la acción "Move" dentro del InputSystem_Actions
        moveAction = InputSystem.actions.FindAction("Move");
		jumpAction = InputSystem.actions.FindAction("Jump");
		spawnAction = InputSystem.actions.FindAction("Spawn");
		resetStageAction = InputSystem.actions.FindAction("RestartLevel");
	}

    // Update is called once per frame
    void Update()
    {
		#region TIMERS
        LastOnGroundTime -= Time.deltaTime;

		LastPressedJumpTime -= Time.deltaTime;
		#endregion

		#region INPUT HANDLER
        _moveInput = moveAction.ReadValue<Vector2>();
		

		if (_moveInput.x != 0)
			CheckDirectionToFace(_moveInput.x > 0);
		#endregion

		if (jumpAction.WasPressedThisFrame())
		{
			OnJumpInput();
		}
		if (jumpAction.WasReleasedThisFrame())
		{
			OnJumpUpInput();
		}
		if (spawnAction.WasPerformedThisFrame())
		{
        	SpawnPlataforma();
		}
		if (resetStageAction.WasPerformedThisFrame())
		{
        	ResetStage();
		}

		#region COLLISION CHECKS
		if (!IsJumping)
		{
			//Ground Check
			if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer)) //checks if set box overlaps with ground
			{
				LastOnGroundTime = coyoteTime; //if so sets the lastGrounded to coyoteTime
            }
		}
		#endregion
		#region JUMP CHECKS
		if (IsJumping && rb2d.linearVelocityY < 0)
		{
			IsJumping = false;

			_isJumpFalling = true;
		}
		if (LastOnGroundTime > 0 && !IsJumping)
        {
			_isJumpCut = false;

			_isJumpFalling = false;
		}
		if (CanJump() && LastPressedJumpTime > 0)
		{
			IsJumping = true;
			_isJumpCut = false;
			_isJumpFalling = false;
			Jump();

			//AnimHandler.startedJumping = true;
		}
		#endregion
		#region GRAVITY
		//Higher gravity if we've released the jump input or are falling
		if (rb2d.linearVelocity.y < 0 && _moveInput.y < 0)
		{
			//Much higher gravity if holding down
			SetGravityScale(gravityScale * fastFallGravityMult);
			//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
			rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, Mathf.Max(rb2d.linearVelocityY, -maxFastFallSpeed));
		}
		else if (_isJumpCut)
		{
			//Higher gravity if jump button released
			SetGravityScale(gravityScale * jumpCutGravityMult);
			rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, Mathf.Max(rb2d.linearVelocityY, -maxFallSpeed));
		}
		else if ((IsJumping || _isJumpFalling) && Mathf.Abs(rb2d.linearVelocityY) < jumpHangTimeThreshold)
		{
			SetGravityScale(gravityScale * jumpHangGravityMult);
		}
		else if (rb2d.linearVelocityY < 0)
		{
			//Higher gravity if falling
			SetGravityScale(gravityScale * fallGravityMult);
			//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
			rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, Mathf.Max(rb2d.linearVelocity.y, -maxFallSpeed));
		}
		else
		{
			//Default gravity if standing on a platform or moving upwards
			SetGravityScale(gravityScale);
		}
		#endregion


		HandleAnimations();
    }

    void FixedUpdate()
    {
        Run();

		if(jumpAction.IsPressed() && !IsJumping && LastOnGroundTime > 0)
		{
			Jump();
		}
    }
	
	//Methods which whandle input detected in Update()
    public void OnJumpInput()
	{
		LastPressedJumpTime = jumpInputBufferTime;
	}

	public void OnJumpUpInput()
	{
		if (CanJumpCut())
			_isJumpCut = true;
	}

    public void SetGravityScale(float scale)
	{
		rb2d.gravityScale = scale;
	}

	//MOVEMENT METHODS
    #region RUN METHODS
    private void Run()
    {
		//Calculate the direction we want to move in and our desired velocity
		float targetSpeed = _moveInput.x * runMaxSpeed;

		#region Calculate AccelRate
		float accelRate;

		//Gets an acceleration value based on if we are accelerating (includes turning) 
		//or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
		if (LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount * accelInAir : runDeccelAmount * deccelInAir;
		#endregion

		//Not used since no jump implemented here, but may be useful if you plan to implement your own

		#region Add Bonus Jump Apex Acceleration
		//Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
		if ((IsJumping || _isJumpFalling) && Mathf.Abs(rb2d.linearVelocity.y) < jumpHangTimeThreshold)
		{
			accelRate *= jumpHangAccelerationMult;
			targetSpeed *= jumpHangMaxSpeedMult;
		}
		#endregion

		//Calculate difference between current linearVelocity and desired linearVelocity
		float speedDif = targetSpeed - rb2d.linearVelocityX;
		//Calculate force along x-axis to apply to thr player

		float movement = speedDif * accelRate;
        
		//Convert this to a vector and apply to rigidbody
		// rb2d.AddForce( Vector2.right, ForceMode2D.Force);
		rb2d.AddForceX(movement, ForceMode2D.Force);
    }

	private void Jump()
	{
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;

		float force = jumpForce;
		IsJumping = true;
		if (rb2d.linearVelocityY < 0)
			force -= rb2d.linearVelocityY; 

		rb2d.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        //playeranim.SetTrigger("jump");
	}

	private void Turn()
	{
		//stores scale and flips the player along the x axis, 
		Vector3 scale = transform.localScale; 
		scale.x *= -1;
		transform.localScale = scale;

		IsFacingRight = !IsFacingRight;
	}
    #endregion


    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != IsFacingRight)
			Turn();
	}
    private bool CanJump()
    {
		return LastOnGroundTime > 0 && !IsJumping;
    }

	private bool CanJumpCut()
    {
		return IsJumping && rb2d.linearVelocityY > 0;
    }

    #endregion

	#region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
	}
    #endregion

    void HandleAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(_moveInput.x));
        animator.SetBool("Grounded", LastOnGroundTime > 0);
        animator.SetBool("Crouch", _moveInput.y < -0.5f && LastOnGroundTime > 0);
        animator.SetBool("FastFall", !(LastOnGroundTime > 0) && _moveInput.y < -0.5f);
        animator.SetFloat("YVelocity", rb2d.linearVelocityY);

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
    public void ResetStage()
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
