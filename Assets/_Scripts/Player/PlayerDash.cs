using UnityEngine;

[System.Serializable]
public class PlayerDash
{
    private PlayerController _controller;
    private PlayerDataSO _moveStats;
    private PlayerCollisionDetector _collisionDetector;
    private PlayerMovement _movement;
    private PlayerJump _jump;
    private PlayerWallInteraction _wallInteraction;

    public bool isDashing { get; private set; }
    public bool isAirDashing { get; private set; }
    public float verticalVelocity { get; private set; }

    private float _dashTimer;
    private int _numberOfDashesUsed;
    private Vector2 _dashDirection;
    private bool _isDashFastFalling;
    private float _dashFastFallTime;
    private float _dashFastFallReleaseSpeed;


    public PlayerDash(PlayerController controller, PlayerDataSO moveStats, PlayerCollisionDetector collisionDetector)
    {
        _controller = controller;
        _moveStats = moveStats;
        _collisionDetector = collisionDetector;

        //Debug.Log("PlayerDash initialized successfully");
    }

    public void SetDependencies(PlayerMovement movement, PlayerJump jump, PlayerWallInteraction wallInteraction)
    {
        _movement = movement;
        _jump = jump;
        _wallInteraction = wallInteraction;
    }

    public void HandleInput()
    {
        if (InputManager.DashWasPressed)
        {
            // Ground dash
            if (_collisionDetector.isGrounded && _collisionDetector.dashOnGroundTimer < 0f && !isDashing)
            {
                InitiateDash();
            }
            // Air dash
            else if (!_collisionDetector.isGrounded && !isDashing && _numberOfDashesUsed < _moveStats.numberOfDashes)
            {
                isAirDashing = true;
                InitiateDash();

                // You left a wallslide but dashed within the wall jump post buffer timer
                if (_collisionDetector.wallJumpPostBufferTimer > 0f)
                {
                    _jump?.AddJumpUsed(-1);
                    // Ensure it doesn't go below 0
                    if (_jump != null && _numberOfDashesUsed < 0)
                    {
                        _jump.ResetJumpCount();
                    }
                }
            }
        }
    }

    public void UpdatePhysics()
    {
        UpdateDash();
        HandleDashFastFall();

        // Check for landing
        if (_collisionDetector.isGrounded)
        {
            ResetDashes();
            isAirDashing = false;
            isDashing = false;
        }
    }

    public void ResetDashValues()
    {
        _isDashFastFalling = false;
    }

    public void ResetDashes()
    {
        _numberOfDashesUsed = 0;
    }

    private void InitiateDash()
    {
        if (!isDashing)
            isDashing = true;

        _dashDirection = InputManager.MoveInput;

        Vector2 closestDirection = GetClosestDashDirection(_dashDirection);

        if (closestDirection == Vector2.zero)
        {
            closestDirection = _movement.isFacingRight ? Vector2.right : Vector2.left;
        }

        _dashDirection = closestDirection;

        _numberOfDashesUsed++;
        _isDashFastFalling = true;
        _dashTimer = 0f;
        _collisionDetector.SetDashOnGroundTimer(_moveStats.timeBtwDashesOnGround);

        _jump?.ResetJumpValues();
        _wallInteraction?.ResetWallJumpValues();
        _wallInteraction?.StopWallSlide();
    }

    private Vector2 GetClosestDashDirection(Vector2 inputDirection)
    {
        Vector2 closestDirection = Vector2.zero;
        float minDistance = Vector2.Distance(inputDirection, _moveStats.dashDirections[0]);

        for (int i = 0; i < _moveStats.dashDirections.Length; i++)
        {
            // Skip if we hit it bang on
            if (inputDirection == _moveStats.dashDirections[i])
            {
                closestDirection = inputDirection;
                break;
            }

            float distance = Vector2.Distance(inputDirection, _moveStats.dashDirections[i]);

            // Check if this is a diagonal direction and apply bias
            bool isDiagonal = (Mathf.Abs(_moveStats.dashDirections[i].x) > 0.5f && Mathf.Abs(_moveStats.dashDirections[i].y) > 0.5f);
            if (isDiagonal)
            {
                distance -= _moveStats.dashDiagonallyBias;
            }

            if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = _moveStats.dashDirections[i];
            }
        }

        return closestDirection;
    }

    private void UpdateDash()
    {
        if (isDashing)
        {
            // Stop the dash after the timer
            _dashTimer += Time.fixedDeltaTime;
            if (_dashTimer >= _moveStats.dashTime)
            {
                if (_collisionDetector.isGrounded)
                {
                    ResetDashes();
                }

                isAirDashing = false;
                isDashing = false;

                if (!_jump.isJumping && !_wallInteraction.isWallJumping)
                {
                    _dashFastFallTime = 0f;
                    _dashFastFallReleaseSpeed = verticalVelocity;

                    if (!_collisionDetector.isGrounded)
                    {
                        _isDashFastFalling = true;
                    }
                }
                return;
            }

            // Apply dash velocity
            _movement?.SetHorizontalVelocity(_moveStats.dashSpeed * _dashDirection.x);
            if (_dashDirection.y != 0f || isAirDashing)
            {
                verticalVelocity = _moveStats.dashSpeed * _dashDirection.y;
            }
        }
    }

    private void HandleDashFastFall()
    {
        // Handle dash cut time
        if (_isDashFastFalling && !isDashing)
        {
            if (verticalVelocity > 0f)
            {
                if (_dashFastFallTime < _moveStats.dashTimeForUpwardsCancel)
                {
                    verticalVelocity = Mathf.Lerp(_dashFastFallReleaseSpeed, 0f, (_dashFastFallTime / _moveStats.dashTimeForUpwardsCancel));
                }
                else if (_dashFastFallTime >= _moveStats.dashTimeForUpwardsCancel)
                {
                    verticalVelocity += _moveStats.gravity * _moveStats.dashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }

                _dashFastFallTime += Time.fixedDeltaTime;
            }
            else
            {
                verticalVelocity += _moveStats.gravity * _moveStats.dashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            // Check if dash fast fall should end
            if (_collisionDetector.isGrounded)
            {
                ResetDashValues();
                _isDashFastFalling = false;
            }
        }
    }
}