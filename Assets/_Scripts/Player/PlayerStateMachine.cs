using UnityEngine;

public enum PlayerState
{
    Idle,
    Walking,
    Running,
    Jumping,
    Falling,
    WallSliding,
    WallJumping,
    Dashing,
    AirDashing
}

[System.Serializable]
public class PlayerStateMachine
{
    private PlayerController _controller;
    private PlayerMovement _movement;
    private PlayerJump _jump;
    private PlayerWallInteraction _wallInteraction;
    private PlayerDash _dash;

    public PlayerState currentState { get; private set; }
    public PlayerState previousState { get; private set; }

    public PlayerStateMachine(PlayerController controller, PlayerMovement movement, PlayerJump jump, PlayerWallInteraction wallInteraction, PlayerDash dash)
    {
        _controller = controller;
        _movement = movement;
        _jump = jump;
        _wallInteraction = wallInteraction;
        _dash = dash;

        currentState = PlayerState.Idle;
        previousState = PlayerState.Idle;

        // Set up dependencies between components
        _movement.SetDashComponent(_dash);
        _wallInteraction.SetDependencies(_movement, _jump);
        _dash.SetDependencies(_movement, _jump, _wallInteraction);
    }

    public void UpdateState()
    {
        PlayerState newState = DetermineState();

        if (newState != currentState)
        {
            OnStateExit(currentState);
            previousState = currentState;
            currentState = newState;
            OnStateEnter(currentState);
        }

        OnStateUpdate(currentState);
    }

    private PlayerState DetermineState()
    {
        // Priority order matters here - more specific states should be checked first

        if (_dash.isDashing)
        {
            return _dash.isAirDashing ? PlayerState.AirDashing : PlayerState.Dashing;
        }

        if (_wallInteraction.isWallSliding)
        {
            return PlayerState.WallSliding;
        }

        if (_wallInteraction.isWallJumping)
        {
            return PlayerState.WallJumping;
        }

        if (_jump.isJumping && _jump.verticalVelocity > 0f)
        {
            return PlayerState.Jumping;
        }

        if (_jump.isFalling || (_jump.isJumping && _jump.verticalVelocity < 0f))
        {
            return PlayerState.Falling;
        }

        // Ground-based states
        if (Mathf.Abs(InputManager.MoveInput.x) > 0f)
        {
            return InputManager.RunIsHeld ? PlayerState.Running : PlayerState.Walking;
        }

        return PlayerState.Idle;
    }

    private void OnStateEnter(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.Walking:
                break;
            case PlayerState.Running:
                break;
            case PlayerState.Jumping:
                break;
            case PlayerState.Falling:
                break;
            case PlayerState.WallSliding:
                break;
            case PlayerState.WallJumping:
                break;
            case PlayerState.Dashing:
                break;
            case PlayerState.AirDashing:
                break;
        }
    }

    private void OnStateUpdate(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                // Handle idle-specific logic
                break;
            case PlayerState.Walking:
                // Handle walking-specific logic
                break;
            case PlayerState.Running:
                // Handle running-specific logic
                break;
            case PlayerState.Jumping:
                // Handle jump-specific logic
                break;
            case PlayerState.Falling:
                // Handle fall-specific logic
                break;
            case PlayerState.WallSliding:
                // Handle wall slide-specific logic
                break;
            case PlayerState.WallJumping:
                // Handle wall jump-specific logic
                break;
            case PlayerState.Dashing:
                // Handle dash-specific logic
                break;
            case PlayerState.AirDashing:
                // Handle air dash-specific logic
                break;
        }
    }

    private void OnStateExit(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.Walking:
                break;
            case PlayerState.Running:
                break;
            case PlayerState.Jumping:
                break;
            case PlayerState.Falling:
                break;
            case PlayerState.WallSliding:
                break;
            case PlayerState.WallJumping:
                break;
            case PlayerState.Dashing:
                break;
            case PlayerState.AirDashing:
                break;
        }
    }

    public bool IsInState(PlayerState state)
    {
        return currentState == state;
    }

    public bool WasInState(PlayerState state)
    {
        return previousState == state;
    }

    public bool IsInAnyState(params PlayerState[] states)
    {
        foreach (PlayerState state in states)
        {
            if (currentState == state)
                return true;
        }
        return false;
    }
}