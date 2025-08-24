using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerJump))]
[RequireComponent(typeof(PlayerWallInteraction))]
[RequireComponent(typeof(PlayerDash))]
[RequireComponent(typeof(PlayerCollisionDetector))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public PlayerDataSO moveStats;
    [SerializeField] private PlayerAnimator _playerAnimator;
    [SerializeField] private Collider2D _bodyColl;
    [SerializeField] private Collider2D _feetColl;

    // Components
    private PlayerMovement _movement;
    private PlayerJump _jump;
    private PlayerWallInteraction _wallInteraction;
    private PlayerDash _dash;
    private PlayerCollisionDetector _collisionDetector;
    private PlayerStateMachine _stateMachine;

    // Shared ref
    private Rigidbody2D _rb;

    public Rigidbody2D rb => _rb;
    public Collider2D bodyCollider => _bodyColl;
    public Collider2D feetCollider => _feetColl;
    public PlayerAnimator playerAnimator => _playerAnimator;
    public bool isFacingRight => _movement.isFacingRight;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // Initialize components
        _collisionDetector = new PlayerCollisionDetector(this, moveStats);
        _movement = new PlayerMovement(this, moveStats, _collisionDetector);
        _jump = new PlayerJump(this, moveStats, _collisionDetector);
        _wallInteraction = new PlayerWallInteraction(this, moveStats, _collisionDetector);
        _dash = new PlayerDash(this, moveStats, _collisionDetector);
        _stateMachine = new PlayerStateMachine(this, _movement, _jump, _wallInteraction, _dash);
    }

    private void Update()
    {
        _collisionDetector.UpdateTimers();

        // Handle input and state logic
        _jump.HandleInput();
        _wallInteraction.HandleInput();
        _dash.HandleInput();

        _stateMachine.UpdateState();
        //UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        _collisionDetector.PerformCollisionChecks();

        // Update physics
        _jump.UpdatePhysics();
        _wallInteraction.UpdatePhysics();
        _dash.UpdatePhysics();
        _movement.UpdateMovement();

        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        Vector2 velocity = new Vector2(_movement.horizontalVelocity, GetVerticalVelocity());

        // Clamp velocities
        if (!_dash.isDashing)
        {
            velocity.y = Mathf.Clamp(velocity.y, -moveStats.maxFallSpeed, 50f);
        }
        else
        {
            velocity.y = Mathf.Clamp(velocity.y, -50f, 50f);
        }

        _rb.linearVelocity = velocity;
    }

    private float GetVerticalVelocity()
    {
        if (_dash.isDashing) return _dash.verticalVelocity;
        if (_jump.isJumping || _jump.isFalling) return _jump.verticalVelocity;
        if (_wallInteraction.isWallJumping) return _wallInteraction.verticalVelocity;
        return _rb.linearVelocity.y;
    }

    //private void UpdateAnimationState()
    //{
    //    if (_dash.isDashing)
    //        _playerAnimator.ChangeAnimationState(PlayerAnimator.PLAYER_Dash);
    //    else if (_jump.isJumping && _jump.verticalVelocity >= 0f)
    //        _playerAnimator.ChangeAnimationState(PlayerAnimator.PLAYER_Jump);
    //    else if ((_jump.isJumping || _jump.isFalling) && _jump.verticalVelocity < 0f)
    //        _playerAnimator.ChangeAnimationState(PlayerAnimator.PLAYER_Fall);
    //    else if (Mathf.Abs(InputManager.MoveInput.x) > 0f && _collisionDetector.isGrounded)
    //        _playerAnimator.ChangeAnimationState(PlayerAnimator.PLAYER_RUN);
    //    else
    //        _playerAnimator.ChangeAnimationState(PlayerAnimator.PLAYER_IDLE);
    //}

    #region Component Access Methods (for Player manager)

    public PlayerMovement GetMovement() => _movement;
    public PlayerJump GetJump() => _jump;
    public PlayerWallInteraction GetWallInteraction() => _wallInteraction;
    public PlayerDash GetDash() => _dash;
    public PlayerCollisionDetector GetCollisionDetector() => _collisionDetector;
    public PlayerStateMachine GetStateMachine() => _stateMachine;

    #endregion

    private void OnDrawGizmos()
    {
        if (moveStats.showWalkJumpArc)
        {
            DrawJumpArc(moveStats.maxWalkSpeed, Color.white);
        }

        if (moveStats.showRunJumpArc)
        {
            DrawJumpArc(moveStats.maxRunSpeed, Color.red);
        }
    }

    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        if (_feetColl == null) return;

        Vector2 startPosition = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed = moveStats.drawRight ? moveSpeed : -moveSpeed;
        Vector2 velocity = new Vector2(speed, moveStats.initialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * moveStats.timeTillJumpApex / moveStats.arcResolution;

        for (int i = 0; i < moveStats.visualizationSteps; i++)
        {
            float simulationtime = i * timeStep;
            Vector2 displacement;

            if (simulationtime < moveStats.timeTillJumpApex)
            {
                displacement = velocity * simulationtime + 0.5f * new Vector2(0, moveStats.gravity) * simulationtime * simulationtime;
            }
            else if (simulationtime < moveStats.timeTillJumpApex + moveStats.apexHangTime)
            {
                float apextime = simulationtime - moveStats.timeTillJumpApex;
                displacement = velocity * moveStats.timeTillJumpApex + 0.5f * new Vector2(0, moveStats.gravity) * moveStats.timeTillJumpApex * moveStats.timeTillJumpApex;
                displacement += new Vector2(speed, 0) * apextime;
            }
            else
            {
                float descendtime = simulationtime - (moveStats.timeTillJumpApex + moveStats.apexHangTime);
                displacement = velocity * moveStats.timeTillJumpApex + 0.5f * new Vector2(0, moveStats.gravity) * moveStats.timeTillJumpApex * moveStats.timeTillJumpApex;
                displacement += new Vector2(speed, 0) * moveStats.apexHangTime;
                displacement += new Vector2(speed, 0) * descendtime + 0.5f * new Vector2(0, moveStats.gravity) * descendtime * descendtime;
            }

            Vector2 drawPoint = startPosition + displacement;

            if (moveStats.stopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint), moveStats.groundLayer);
                if (hit.collider != null)
                {
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }
}

