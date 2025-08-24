using System;
using UnityEngine;

[System.Serializable]
public class PlayerMovement
{
    private PlayerController _controller;
    private PlayerDataSO _moveStats;
    private PlayerCollisionDetector _collisionDetector;

    public float horizontalVelocity { get; private set; }
    public bool isFacingRight { get; private set; } = true;

    private bool _useWallJumpMoveStats;

    public PlayerMovement(PlayerController controller, PlayerDataSO moveStats, PlayerCollisionDetector collisionDeteror)
    {
        _controller = controller;
        _moveStats = moveStats;
        _collisionDetector = collisionDeteror;
    }

    public void UpdateMovement()
    {
        Vector2 moveInput = PlayerInput.Instance.moveInput;

        if (_collisionDetector.isGrounded)
            Move(_moveStats.groundAcceleration, _moveStats.groundDeceleration, moveInput);
        else
        {
            if(_useWallJumpMoveStats)
                Move(_moveStats.wallJumpAcceleration, _moveStats.wallJumpDeceleration, moveInput);
            else
                Move(_moveStats.airAcceleration, _moveStats.airDeceleration, moveInput);
        }
    }

    public void SetUseWallJumpMoveStats(bool useWallJumpStats)
    {
        _useWallJumpMoveStats = useWallJumpStats;
    }

    public void SetHorizontalVelocity(float velocity)
    {
        horizontalVelocity = velocity;
    }

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (GetDashComponent().isDashing)
            return;

        if (Mathf.Abs(moveInput.x) >= _moveStats.moveThreshold)
        {
            TurnCheck(moveInput);

            float targetVelocity = InputManager.RunIsHeld ?
                moveInput.x * _moveStats.maxRunSpeed :
                moveInput.x * _moveStats.maxWalkSpeed;

            horizontalVelocity = Mathf.Lerp(horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);
        } else if (Mathf.Abs(moveInput.x) < _moveStats.moveThreshold){
            horizontalVelocity = Mathf.Lerp(horizontalVelocity, 0, deceleration * Time.deltaTime);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !isFacingRight)
            Turn();
    }

    private void Turn()
    {
        Vector3 scale = _controller.transform.localScale;
        scale.x *= -1;
        _controller.transform.localScale = scale;

        isFacingRight = !isFacingRight;
    }

    // This will be properly injected by the PlayerController
    private PlayerDash _dashComponent;
    public void SetDashComponent(PlayerDash dashComponent)
    {
        _dashComponent = dashComponent;
    }
    private PlayerDash GetDashComponent()
    {
        return _dashComponent;
    }
}

