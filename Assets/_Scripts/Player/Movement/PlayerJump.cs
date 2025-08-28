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

    // Audio and Effects
    private PlayerAudioManager _audioManager;
    private PlayerEffectsManager _effectsManager;
    private float _fallStartHeight;
    private bool _trackingFallHeight;

    public PlayerJump(PlayerController controller, PlayerDataSO moveStats, PlayerCollisionDetector collisionDeteror)
    {
        _controller = controller;
        _moveStats = moveStats;
        _collisionDetector = collisionDeteror;

        // Get audio and effects managers
        _audioManager = _controller.GetComponentInChildren<PlayerAudioManager>();
        _effectsManager = _controller.GetComponentInChildren<PlayerEffectsManager>();

        if (_audioManager == null)
            Debug.LogWarning("PlayerAudioManager not found on " + _controller.name);
        if (_effectsManager == null)
            Debug.LogWarning("PlayerEffectsManager not found on " + _controller.name);
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

        // Play jump sound and effects
        PlayJumpAudioAndEffects();
        //Debug.Log($"Jump initiated! Vertical velocity set to: {verticalVelocity}");
    }

    private void PlayJumpAudioAndEffects()
    {
        // Play jump sound
        _audioManager?.PlayJumpSound(_numberOfJumpsUsed);

        // Play jump effects
        _effectsManager?.PlayJumpEffect(_numberOfJumpsUsed);

        // Add screen shake for higher jumps
        if (_numberOfJumpsUsed > 1)
        {
            // You can implement screen shake here if you have a camera shake system
            // CameraShake.Instance?.ShakeCamera(0.1f, 0.1f);
        }
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
            float fallDistance = 0f;
            if (_trackingFallHeight)
            {
                fallDistance = _fallStartHeight - _controller.transform.position.y;
            }

            // Play landing sound and effects
            PlayLandingAudioAndEffects(fallDistance);

            //Debug.Log("Landing detected - resetting jump values");
            ResetJumpValues();
            verticalVelocity = Physics2D.gravity.y;
        }
    }

    private void PlayLandingAudioAndEffects(float fallDistance)
    {
        // Only play effects if we fell a reasonable distance
        if (fallDistance > 0.5f)
        {
            // Play landing sound
            _audioManager?.PlayLandingSound(fallDistance);

            // Play landing effects
            _effectsManager?.PlayLandingEffect(fallDistance);

            // Add screen shake for hard landings
            if (fallDistance > 4f)
            {
                //CameraShake.Instance?.ShakeCamera(0.2f, 0.3f);
            }
        }
    }

    public float GetCurrentFallDistance()
    {
        if (_trackingFallHeight)
        {
            return _fallStartHeight - _controller.transform.position.y;
        }
        return 0f;
    }
}