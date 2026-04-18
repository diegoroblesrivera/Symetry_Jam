using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
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
    public Rigidbody2D rb2d { get; private set; }

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
	//Script to handle all player animations, all references can be safely removed if you're importing into your own project.
	#endregion

	#region STATE PARAMETERS
	//Variables control the various actions the player can perform at any time.
	//These are fields which can are public allowing for other sctipts to read them
	//but can only be privately written to.
	public bool IsFacingRight { get; private set; }
	public float LastOnGroundTime { get; private set; }
	#endregion

	#region INPUT PARAMETERS
    private Vector2 _moveInput;
	#endregion

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

    void Start()
    {
		IsFacingRight = true;
    }

    void Awake()
    {
        // Busca la acción "Move" dentro del InputSystem_Actions
        moveAction = InputSystem.actions.FindAction("Move");
        rb2d = GetComponent<Rigidbody2D>();
    }

	private void OnValidate()
    {
		//Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
		runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
		runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

		#region Variable Ranges
		runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
		runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
		#endregion
	}

    void OnEnable()
    {
        moveAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        _moveInput = moveAction.ReadValue<Vector2>();
		#region TIMERS
        LastOnGroundTime -= Time.deltaTime;
		#endregion

		#region INPUT HANDLER
		if (_moveInput.x != 0)
			CheckDirectionToFace(_moveInput.x > 0);
		#endregion

		#region COLLISION CHECKS
		//Ground Check
		if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer)) //checks if set box overlaps with ground
			LastOnGroundTime = 0.1f;
		#endregion
    }

    void FixedUpdate()
    {
        Run();
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
		print(runAccelAmount);

		//Not used since no jump implemented here, but may be useful if you plan to implement your own

		// #region Add Bonus Jump Apex Acceleration
		// //Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
		// if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < jumpHangTimeThreshold)
		// {
		// 	accelRate *= jumpHangAccelerationMult;
		// 	targetSpeed *= jumpHangMaxSpeedMult;
		// }
		// #endregion

		//Calculate difference between current linearVelocity and desired linearVelocity
		float speedDif = targetSpeed - rb2d.linearVelocityX;
		//Calculate force along x-axis to apply to thr player

		float movement = speedDif * accelRate;
        
		//Convert this to a vector and apply to rigidbody
		// rb2d.AddForce( Vector2.right, ForceMode2D.Force);
		rb2d.AddForceX(movement, ForceMode2D.Force);
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
    #endregion
}
