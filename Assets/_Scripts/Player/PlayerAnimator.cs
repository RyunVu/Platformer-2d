using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    #region ANIMATOR STATES
    public const string PLAYER_IDLE = "PlayerIdle";
    public const string PLAYER_RUN = "PlayerRun";
    public const string PLAYER_JUMP = "PlayerJump";
    public const string PLAYER_FALL = "PlayerFall";
    public const string PLAYER_DASH = "PlayerDash";
    public const string PLAYER_LAND = "PlayerLand";
    public const string PLAYER_WALL_SLIDE = "PlayerWallSlide";
    #endregion

    [Header("Components")]
    [SerializeField] private Animator _animator;

    [Header("Animation Settings")]
    [SerializeField] private float _crossFadeDuration = 0.1f;
    [SerializeField] private bool _useCrossFade = true;

    [Header("Debug")]
    [SerializeField] private bool _debugMode = false;

    private string _currentState;
    private string _previousState;

    #region UNITY CALLBACKS
    private void Awake()
    {
        // Try to get Animator from this GameObject first
        if (_animator == null)
            _animator = GetComponent<Animator>();

        // If still null, try to find it in children
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        // Log warning if still not found
        if (_animator == null)
            Debug.LogWarning($"No Animator found on {gameObject.name}!");
    }

    private void Start()
    {
        // Set initial state
        if (_animator != null && string.IsNullOrEmpty(_currentState))
        {
            ChangeAnimationState(PLAYER_IDLE);
        }
    }
    #endregion

    #region ANIMATION METHODS
    /// <summary>
    /// Changes the animation state with optional cross-fade
    /// </summary>
    /// <param name="newState">The new animation state to play</param>
    /// <param name="forceChange">Force the change even if it's the same state</param>
    public void ChangeAnimationState(string newState, bool forceChange = false)
    {
        // Null checks
        if (_animator == null || string.IsNullOrEmpty(newState))
            return;

        // Don't change if it's the same state (unless forced)
        if (_currentState == newState && !forceChange)
            return;

        // Debug logging
        if (_debugMode)
            Debug.Log($"Changing animation from '{_currentState}' to '{newState}'");

        // Store previous state
        _previousState = _currentState;
        _currentState = newState;

        // Play animation with or without crossfade
        if (_useCrossFade && _crossFadeDuration > 0)
        {
            _animator.CrossFade(newState, _crossFadeDuration);
        }
        else
        {
            _animator.Play(newState);
        }
    }

    /// <summary>
    /// Changes animation state with custom cross-fade duration
    /// </summary>
    public void ChangeAnimationState(string newState, float customCrossFadeDuration)
    {
        if (_animator == null || string.IsNullOrEmpty(newState))
            return;

        if (_currentState == newState)
            return;

        if (_debugMode)
            Debug.Log($"Changing animation from '{_currentState}' to '{newState}' with {customCrossFadeDuration}s crossfade");

        _previousState = _currentState;
        _currentState = newState;

        _animator.CrossFade(newState, customCrossFadeDuration);
    }

    /// <summary>
    /// Revert to the previous animation state
    /// </summary>
    public void RevertToPreviousState()
    {
        if (!string.IsNullOrEmpty(_previousState))
        {
            string temp = _currentState;
            ChangeAnimationState(_previousState);
            _previousState = temp;
        }
    }

    /// <summary>
    /// Check if a specific animation is currently playing
    /// </summary>
    public bool IsAnimationPlaying(string stateName)
    {
        if (_animator == null) return false;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(stateName);
    }

    /// <summary>
    /// Check if current animation has finished playing
    /// </summary>
    public bool IsCurrentAnimationComplete()
    {
        if (_animator == null) return true;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime >= 1.0f && !_animator.IsInTransition(0);
    }

    /// <summary>
    /// Get the normalized time of the current animation (0-1)
    /// </summary>
    public float GetCurrentAnimationTime()
    {
        if (_animator == null) return 0f;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime;
    }

    /// <summary>
    /// Set animator parameters (for blend trees, triggers, etc.)
    /// </summary>
    public void SetFloat(string parameterName, float value)
    {
        if (_animator != null && _animator.parameters != null)
        {
            _animator.SetFloat(parameterName, value);
        }
    }

    public void SetBool(string parameterName, bool value)
    {
        if (_animator != null && _animator.parameters != null)
        {
            _animator.SetBool(parameterName, value);
        }
    }

    public void SetTrigger(string parameterName)
    {
        if (_animator != null && _animator.parameters != null)
        {
            _animator.SetTrigger(parameterName);
        }
    }

    /// <summary>
    /// Set the animation speed
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        if (_animator != null)
        {
            _animator.speed = speed;
        }
    }
    #endregion

    #region GETTERS
    public string CurrentState => _currentState;
    public string PreviousState => _previousState;
    public Animator AnimatorComponent => _animator;
    public bool IsInTransition => _animator != null ? _animator.IsInTransition(0) : false;
    #endregion

    #region VALIDATION
    private void OnValidate()
    {
        // Auto-assign animator in editor if not set
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }
    #endregion
}