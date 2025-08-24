using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementData_", menuName = "ScriptableObject/Player/Movement")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Walk")]
    [Range(0f, 1f)]     public float moveThreshold = .25f;                          // Minimum input to start moving
    [Range(1f, 100f)]   public float maxWalkSpeed = 12.5f;
    [Range(.25f, 50f)]  public float groundAcceleration = 12.5f;
    [Range(.25f, 50f)]  public float groundDeceleration = 12.5f;
    [Range(.25f, 50f)]  public float airAcceleration = 12.5f;
    [Range(.25f, 50f)]  public float airDeceleration = 12.5f;
    [Range(.25f, 50f)]  public float wallJumpAcceleration = 12.5f;
    [Range(.25f, 50f)]  public float wallJumpDeceleration = 12.5f;

    [Header("Run")]
    [Range(1f, 100f)]   public float maxRunSpeed = 20f;

    [Header("Grounded/Collision Checks")]
    public LayerMask groundLayer;
    public float groundDetectionRayLength = .02f;
    public float headDetectionRayLength = .75f;
    [Range(0f, 1f)]     public float headWidth = .75f;
    public float wallDetectRayLength = .125f;
    [Range(0f, 1f)]     public float wallDetectionRayHeightMultiplier = .9f;

    [Header("Jump")]
    public float jumpHeight = 6.5f;
    [Range(1f, 1.1f)]   public float jumpHeightCompensationFactor = 1.054f;            // Compensates for float precision errors
    public float timeTillJumpApex = .35f;
    [Range(.01f, 5f)]   public float gravityOnReleaseMultiplier = 2f;  
    public float maxFallSpeed = 26f;
    [Range(1, 5)]       public int numberOfJumpsAllowed = 2;

    [Header("Reset Jump Option")]
    public bool resetJumpOnWallSlide = true;

    [Header("Jump Cut")]
    [Range(.02f, .3f)]  public float timeForUpwardsCancel = .027f;                   // Time after jump input where releasing jump will cut upwards momentum

    [Header("Jump Apex")]
    [Range(.5f, 1f)]    public float apexThreshold = .97f;                           // Percentage of jump apex velocity to be considered "at apex"    
    [Range(.01f, 1f)]   public float apexHangTime = .075f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)]     public float jumpBuffTime = .125f;

    [Header("Jump Coyote Time")]
    [Range(0f, 1f)]     public float jumpCoyoteTime = .1f;

    [Header("Wall Slide")]
    [Min(.01f)]         public float wallSlideSpeed = 5f;
    [Range(0f, 1f)]     public float wallSlideDecelerationSpeed = 50f;

    [Header("Wall Jump")]
    public Vector2 wallJumpDirection = new Vector2(-20f, 6.5f);
    [Range(0f, 1f)]     public float wallJumpBufferTime = .125f;
    [Range(.01f, 5f)]   public float wallJumpGravityOnReleaseMultiplier = 1f;

    [Header("Dash")]
    [Range(0f, 1f)] public float dashTime = .11f;
    [Range(1f, 200f)] public float dashSpeed = 40f;
    [Range(0f, 1f)] public float timeBtwDashesOnGround = .225f;
    public bool resetDashOnWallSlide = true;
    [Range(0, 5)] public int numberOfDashes = 2;
    [Range(0f, .5f)] public float dashDiagonallyBias = .4f;

    [Header("Dash Cancel Time")]
    [Range(.01f, 5f)] public float dashGravityOnReleaseMultiplier = 1f;
    [Range(.02f, .3f)] public float dashTimeForUpwardsCancel = .027f;

    [Header("Debug")]
    public bool debugShowIsGroundedBox;
    public bool debugShowHeadBumpBox;
    public bool debugShowWallHitBox;

    [Header("JumpVisualization Tool")]
    public bool showWalkJumpArc = false;
    public bool showRunJumpArc = false;
    public bool stopOnCollision = false;
    public bool drawRight = false;
    [Range(5, 100)] public int arcResolution = 20;
    [Range(0, 100)] public int visualizationSteps = 90;

    public readonly Vector2[] dashDirections = new Vector2[]
    {
        new Vector2(0, 0),				// Nothing
		new Vector2(1, 0),				// Right
		new Vector2(1, 1).normalized,	// Top-Right
		new Vector2(0, 1),				// Up
		new Vector2(-1, 1).normalized,	// Top-Left
		new Vector2(-1, 0),				// Left
		new Vector2(-1, -1).normalized,	// Bottom-Left
		new Vector2(0, -1),				// Down
		new Vector2(1, -1).normalized,	// Bottom-Right
	};

    // Jump
    public float gravity { get; private set; }
    public float initialJumpVelocity { get; private set; }
    public float adjustedJumpHeight { get; private set; }

    // Wall Jump
    public float wallJumpGravity { get; private set; }
    public float initialWallJumpVelocity { get; private set; }
    public float adjustedWallJumpHeight { get; private set; }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        // Jump
        adjustedJumpHeight = jumpHeight * jumpHeightCompensationFactor;
        gravity = -(2f * adjustedJumpHeight) / Mathf.Pow(timeTillJumpApex, 2f);
        initialJumpVelocity = Mathf.Abs(gravity) * timeTillJumpApex;

        // Wall Jump
        adjustedWallJumpHeight = wallJumpDirection.y * jumpHeightCompensationFactor;
        wallJumpGravity = -(2f * adjustedWallJumpHeight) / Mathf.Pow(timeTillJumpApex, 2f);
        initialWallJumpVelocity = Mathf.Abs(wallJumpGravity) * timeTillJumpApex;
    }
}

