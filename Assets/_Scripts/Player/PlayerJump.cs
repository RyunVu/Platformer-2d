using System;
using UnityEngine;

[System.Serializable]
public class PlayerJump
{
    private PlayerController _controller;
    private PlayerDataSO _moveStats;
    private PlayerCollisionDetector _collisionDetector;

    public float verticalVelocity { get; private set; }
    public bool isJumping { get; private set; }
    public bool isFastFalling { get; private set; }
    public bool isFalling { get; private set; }

    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;
    private bool _jumpReleaseedDuringBuffer;

    // Apex vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    public PlayerJump(PlayerController controller, PlayerDataSO moveStats, PlayerCollisionDetector collisionDeteror)
    {
        _controller = controller;
        _moveStats = moveStats;
        _collisionDetector = collisionDeteror;

        //Debug.Log("PlayerJump initialized successfully");
    }

    public void HandleInput()
    {
        if (PlayerInput.Instance.jumpWasPressed)
        {
            //Debug.Log("Jump input detected!");
            _collisionDetector.SetJumpBufferTimer(_moveStats.jumpBuffTime);
            _jumpReleaseedDuringBuffer = false;
        }

        if (PlayerInput.Instance.jumpWasReleased)
        {
            if (_collisionDetector.jumpBufferTimer > 0f)
                _jumpReleaseedDuringBuffer = true;

            if (isJumping && verticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    isFastFalling = true;
                    _fastFallTime = _moveStats.timeForUpwardsCancel;
                    verticalVelocity = 0f;
                }
                else
                {
                    isFastFalling = true;
                    _fastFallReleaseSpeed = verticalVelocity;
                }
            }
        }

        CheckJumpCondition();
    }

    public void UpdatePhysics()
    {
        CheckLanding();
        ApplyJumpPhysics();
        HandleFalling();
    }

    public void ResetJumpValues()
    {
        isJumping = false;
        isFalling = false;
        isFastFalling = false;
        _fastFallTime = 0f;
        _isPastApexThreshold = false;
        _numberOfJumpsUsed = 0;
    }

    public void ResetJumpCount()
    {
        _numberOfJumpsUsed = 0;
    }

    public void AddJumpUsed(int count = 1)
    {
        _numberOfJumpsUsed += count;
    }

    private void CheckJumpCondition()
    {
        // Debug the jump conditions
        //if (_collisionDetector.jumpBufferTimer > 0f)
        //{
            //Debug.Log($"Jump buffer active: {_collisionDetector.jumpBufferTimer}");
            //Debug.Log($"Is grounded: {_collisionDetector.isGrounded}");
            //Debug.Log($"Coyote timer: {_collisionDetector.coyoteTimer}");
            //Debug.Log($"Is jumping: {isJumping}");
            //Debug.Log($"Number of jumps used: {_numberOfJumpsUsed}");
            //Debug.Log($"Max jumps allowed: {_moveStats.numberOfJumpsAllowed}");
        //}

        // Ground jump or coyote jump
        if (_collisionDetector.jumpBufferTimer > 0f && !isJumping && (_collisionDetector.isGrounded || _collisionDetector.coyoteTimer > 0f))
        {
            //Debug.Log("Initiating ground/coyote jump!");
            InitiateJump(1);

            if (_jumpReleaseedDuringBuffer)
            {
                isFastFalling = true;
                _fastFallReleaseSpeed = verticalVelocity;
            }
        }
        // Multi-jump while jumping
        else if (_collisionDetector.jumpBufferTimer > 0f && (isJumping || isFalling) && _numberOfJumpsUsed < _moveStats.numberOfJumpsAllowed)
        {
            //Debug.Log("Initiating multi-jump while jumping!");
            isFastFalling = true;
            InitiateJump(1);
        }
        // Air jump after coyote time lapse
        else if (_collisionDetector.jumpBufferTimer > 0f && isFalling && _numberOfJumpsUsed < _moveStats.numberOfJumpsAllowed - 1)
        {
            //Debug.Log("Initiating air jump!");
            InitiateJump(2);
            isFastFalling = false;
        }
    }

    public void InitiateJump(int jumpsToAdd)
    {
        //Debug.Log($"InitiateJump called with {jumpsToAdd} jumps to add");
        //Debug.Log($"Initial jump velocity: {_moveStats.initialJumpVelocity}");

        if (!isJumping)
            isJumping = true;

        _collisionDetector.SetJumpBufferTimer(0f);
        _numberOfJumpsUsed += jumpsToAdd;
        verticalVelocity = _moveStats.initialJumpVelocity;

        //Debug.Log($"Jump initiated! Vertical velocity set to: {verticalVelocity}");
    }

    private void ApplyJumpPhysics()
    {
        if (isJumping)
        {
            if (_collisionDetector.isHeadBumped)
                isFastFalling = true;

            if (verticalVelocity > 0f)
            {
                // Apex controls
                _apexPoint = Mathf.InverseLerp(_moveStats.initialJumpVelocity, 0f, verticalVelocity);

                if (_apexPoint > _moveStats.apexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < _moveStats.apexHangTime)
                            verticalVelocity = 0f;
                        else
                            verticalVelocity = -.01f;
                    }
                }
                // Gravity on ascending but not past apex threshold
                else if (!isFastFalling)
                {
                    verticalVelocity += _moveStats.gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                        _isPastApexThreshold = false;
                }
            }
            // Gravity on descending
            else if (!isFastFalling)
            {
                verticalVelocity += _moveStats.gravity * _moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (verticalVelocity < 0f)
            {
                if (!isFalling)
                    isFalling = true;
            }
        }

        // Jump cut
        if (isFastFalling)
        {
            if (_fastFallTime >= _moveStats.timeForUpwardsCancel)
            {
                verticalVelocity += _moveStats.gravity * _moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < _moveStats.timeForUpwardsCancel)
            {
                verticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / _moveStats.timeForUpwardsCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }
    }

    private void HandleFalling()
    {
        if (!_collisionDetector.isGrounded && !isJumping && !isFalling)
            isFalling = true;

        if (isFalling && !isJumping)
            verticalVelocity += _moveStats.gravity * _moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
        
    }

    private void CheckLanding()
    {
        if ((isJumping || isFalling) && _collisionDetector.isGrounded && verticalVelocity <= 0f)
        {
            //Debug.Log("Landing detected - resetting jump values");
            ResetJumpValues();
            verticalVelocity = Physics2D.gravity.y;
        }
    }
}