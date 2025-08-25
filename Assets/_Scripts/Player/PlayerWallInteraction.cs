using UnityEngine;

[System.Serializable]
public class PlayerWallInteraction
{
    private PlayerController _controller;
    private PlayerDataSO _moveStats;
    private PlayerCollisionDetector _collisionDetector;
    private PlayerMovement _movement;
    private PlayerJump _jump;

    // Wall slide
    public bool isWallSliding { get; private set; }
    public bool isWallSlideFalling { get; private set; }

    // Wall jump
    public bool isWallJumping { get; private set; }
    public float verticalVelocity { get; private set; }

    private float _wallJumpTime;
    private bool _isWallJumpFastFalling;
    private bool _isWallJumpFalling;
    private float _wallJumpFastFallTime;
    private float _wallJumpFastFallReleaseSpeed;

    private float _wallJumpApexPoint;
    private float _timePastWallJumpApexThreshold;
    private bool _isPastWallJumpApexThreshold;

    public PlayerWallInteraction(PlayerController controller, PlayerDataSO moveStats, PlayerCollisionDetector collisionDetector)
    {
        _controller = controller;
        _moveStats = moveStats;
        _collisionDetector = collisionDetector;

        Debug.Log("PlayerWallInteraction initialized successfully");
    }

    public void SetDependencies(PlayerMovement movement, PlayerJump jump)
    {
        _movement = movement;
        _jump = jump;
    }

    public void HandleInput()
    {
        CheckWallSlide();
        HandleWallJumpInput();
    }

    public void UpdatePhysics()
    {
        UpdateWallSlide();
        UpdateWallJump();
    }

    public void ResetWallJumpValues()
    {
        isWallSlideFalling = false;
        isWallJumping = false;
        _isWallJumpFastFalling = false;
        _isWallJumpFalling = false;
        _isPastWallJumpApexThreshold = false;
        _wallJumpFastFallTime = 0f;
        _wallJumpTime = 0f;

        if (_movement != null)
        {
            _movement.SetUseWallJumpMoveStats(false);
        }
    }

    public void StopWallSlide()
    {
        if (isWallSliding)
        {
            if (_jump != null)
            {
                _jump.AddJumpUsed(1);
            }
            isWallSliding = false;
        }
    }

    private void CheckWallSlide()
    {
        if (_collisionDetector.isTouchingWall && !_collisionDetector.isGrounded)
        {
            if (verticalVelocity < 0f && !isWallSliding)
            {
                _jump?.ResetJumpValues();
                ResetWallJumpValues();

                if (_moveStats.resetJumpOnWallSlide)
                {
                    _jump?.ResetJumpCount();
                }

                isWallSlideFalling = false;
                isWallSliding = true;
            }
        }
        else if (isWallSlideFalling && !_collisionDetector.isTouchingWall && !_collisionDetector.isGrounded && !isWallSlideFalling)
        {
            isWallSlideFalling = true;
            StopWallSlide();
        }
        else
        {
            StopWallSlide();
        }
    }

    private void HandleWallJumpInput()
    {
        if (ShouldApplyPostWallJumpBuffer())
        {
            _collisionDetector.SetWallJumpPostBufferTimer(_moveStats.wallJumpBufferTime);
        }

        // Wall jump fast falling
        if (InputManager.JumpWasReleased && !isWallSliding && !_collisionDetector.isTouchingWall && isWallJumping)
        {
            if (verticalVelocity > 0f)
            {
                if (_isPastWallJumpApexThreshold)
                {
                    _isPastWallJumpApexThreshold = false;
                    _isWallJumpFastFalling = true;
                    _wallJumpFastFallTime = _moveStats.timeForUpwardsCancel;
                    verticalVelocity = 0f;
                }
                else
                {
                    _isWallJumpFastFalling = true;
                    _wallJumpFastFallReleaseSpeed = verticalVelocity;
                }
            }
        }

        // Actual jump with post wall jump buffer time
        if (InputManager.JumpWasPressed && _collisionDetector.wallJumpPostBufferTimer > 0f)
        {
            InitiateWallJump();
        }
    }

    private void InitiateWallJump()
    {
        if (!isWallJumping)
        {
            isWallJumping = true;
            _movement?.SetUseWallJumpMoveStats(true);
        }

        StopWallSlide();
        _jump?.ResetJumpValues();
        _wallJumpTime = 0f;

        verticalVelocity = _moveStats.initialWallJumpVelocity;

        float direction = Mathf.Sign(_controller.transform.position.x - _collisionDetector.lastWallHit.collider.ClosestPoint(_controller.bodyCollider.bounds.center).x);
        _movement?.SetHorizontalVelocity(Mathf.Abs(_moveStats.wallJumpDirection.x) * direction);
    }

    private void UpdateWallSlide()
    {
        if (isWallSliding)
        {
            verticalVelocity = Mathf.Lerp(verticalVelocity, -_moveStats.wallSlideSpeed, _moveStats.wallSlideDecelerationSpeed * Time.fixedDeltaTime);
        }
    }

    private void UpdateWallJump()
    {
        if (isWallJumping)
        {
            // Time to take over movement controls while wall jumping
            _wallJumpTime += Time.fixedDeltaTime;
            if (_wallJumpTime >= _moveStats.timeTillJumpApex)
            {
                _movement?.SetUseWallJumpMoveStats(false);
            }

            // Hit head
            if (_collisionDetector.isHeadBumped)
            {
                _isWallJumpFastFalling = true;
                _movement?.SetUseWallJumpMoveStats(false);
            }

            // Gravity on ascending
            if (verticalVelocity >= 0f)
            {
                // Apex controls
                _wallJumpApexPoint = Mathf.InverseLerp(_moveStats.wallJumpDirection.y, 0f, verticalVelocity);

                if (_wallJumpApexPoint > _moveStats.apexThreshold)
                {
                    if (!_isPastWallJumpApexThreshold)
                    {
                        _isPastWallJumpApexThreshold = true;
                        _timePastWallJumpApexThreshold = 0f;
                    }

                    if (_isPastWallJumpApexThreshold)
                    {
                        _timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                        if (_timePastWallJumpApexThreshold < _moveStats.apexHangTime)
                        {
                            verticalVelocity = 0f;
                        }
                        else
                        {
                            verticalVelocity = -.01f;
                        }
                    }
                }
                // Gravity on ascending but not past apex threshold
                else if (!_isWallJumpFastFalling)
                {
                    verticalVelocity += _moveStats.wallJumpGravity * Time.fixedDeltaTime;

                    if (_isPastWallJumpApexThreshold)
                    {
                        _isPastWallJumpApexThreshold = false;
                    }
                }
            }
            // Gravity on descending
            else if (!_isWallJumpFastFalling)
            {
                verticalVelocity += _moveStats.wallJumpGravity * Time.fixedDeltaTime;
            }
            else if (verticalVelocity < 0f)
            {
                if (!_isWallJumpFalling)
                    _isWallJumpFalling = true;
            }
        }

        // Handle wall jump cut time
        if (_isWallJumpFastFalling)
        {
            if (_wallJumpFastFallTime >= _moveStats.timeForUpwardsCancel)
            {
                verticalVelocity += _moveStats.wallJumpGravity * _moveStats.wallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_wallJumpFastFallTime < _moveStats.timeForUpwardsCancel)
            {
                verticalVelocity = Mathf.Lerp(_wallJumpFastFallReleaseSpeed, 0f, (_wallJumpFastFallTime / _moveStats.timeForUpwardsCancel));
            }

            _wallJumpFastFallTime += Time.fixedDeltaTime;
        }

        // Check for landing
        if (isWallJumping && _collisionDetector.isGrounded && verticalVelocity <= 0f)
        {
            ResetWallJumpValues();
        }
    }

    private bool ShouldApplyPostWallJumpBuffer()
    {
        return !_collisionDetector.isGrounded && (_collisionDetector.isTouchingWall || isWallSliding);
    }
}