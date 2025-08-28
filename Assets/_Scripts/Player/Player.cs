using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerAnimator))]
public class Player : MonoBehaviour
{
    [Header("Player Components")]
    [HideInInspector] public PlayerController controller;
    [HideInInspector] public PlayerAnimator animator;
    [HideInInspector] public PlayerAudioManager audioManager;
    [HideInInspector] public PlayerEffectsManager effectManager;

    // Properties to access the internal components through the controller
    public PlayerMovement movement => controller?.GetMovement();
    public PlayerJump jump => controller?.GetJump();
    public PlayerWallInteraction wallInteraction => controller?.GetWallInteraction();
    public PlayerDash dash => controller?.GetDash();
    public PlayerCollisionDetector collisionDetector => controller?.GetCollisionDetector();
    public PlayerStateMachine stateMachine => controller?.GetStateMachine();

    // Quick access properties for common queries
    public bool isGrounded => collisionDetector?.isGrounded ?? false;
    public bool isJumping => jump?.isJumping ?? false;
    public bool isDashing => dash?.isDashing ?? false;
    public bool isWallSliding => wallInteraction?.isWallSliding ?? false;
    public bool isFacingRight => movement?.isFacingRight ?? true;
    public Vector2 Velocity => controller?.rb?.linearVelocity ?? Vector2.zero;

    private void Awake()
    {
        // Get required components
        controller = GetComponent<PlayerController>();
        animator = GetComponent<PlayerAnimator>();
        audioManager = GetComponentInChildren<PlayerAudioManager>();
        effectManager = GetComponentInChildren<PlayerEffectsManager>();

        // Validate components
        if (controller == null)
        {
            Debug.LogError($"PlayerController not found on {gameObject.name}!");
        }

        if (animator == null)
        {
            Debug.LogWarning($"PlayerAnimator not found on {gameObject.name}. Animations may not work.");
        }
    }

    private void Start()
    {
        // Initialize any player-specific logic here
        InitializePlayer();
    }

    /// <summary>
    /// Initialize player-specific settings and configurations
    /// </summary>
    private void InitializePlayer()
    {
        // Set up any initial player state
        // Configure player settings
        // Initialize health, stats, etc.
    }

    #region Public Methods - Player Actions

    /// <summary>
    /// Force the player to jump (useful for external triggers)
    /// </summary>
    public void ForceJump()
    {
        jump?.InitiateJump(1);
    }

    /// <summary>
    /// Reset player to a specific position
    /// </summary>
    public void ResetPosition(Vector3 position)
    {
        transform.position = position;
        if (controller != null && controller.rb != null)
        {
            controller.rb.linearVelocity = Vector2.zero;
        }

        // Reset all movement states
        jump?.ResetJumpValues();
        wallInteraction?.ResetWallJumpValues();
        dash?.ResetDashValues();
    }

    /// <summary>
    /// Enable or disable player input
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        controller.enabled = enabled;
    }

    /// <summary>
    /// Get the current player state as a string (useful for debugging)
    /// </summary>
    public string GetCurrentStateString()
    {
        return stateMachine?.currentState.ToString() ?? "Unknown";
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Print current player status to console (useful for debugging)
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugPlayerStatus()
    {
        Debug.Log($"Player Status:\n" +
                  $"State: {GetCurrentStateString()}\n" +
                  $"Grounded: {isGrounded}\n" +
                  $"Velocity: {Velocity}\n" +
                  $"Facing Right: {isFacingRight}");
    }

    /// <summary>
    /// Draw debug information in the scene view
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (controller?.moveStats == null) return;

        // Draw player center
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        // Draw facing direction
        Gizmos.color = isFacingRight ? Color.green : Color.red;
        Vector3 direction = isFacingRight ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, direction * 0.5f);
    }

    #endregion

    #region Events (Optional - for other systems to listen to)

    // You can add events here for other systems to subscribe to
    // public event System.Action OnPlayerLanded;
    // public event System.Action OnPlayerJumped;
    // public event System.Action OnPlayerDied;

    #endregion


}

