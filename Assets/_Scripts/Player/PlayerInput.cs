using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : SingletonMonobehaviour<PlayerInput>
{
    [Header("Input Actions Asset")]
    [SerializeField] private InputActionAsset inputActions;

    // Input Actions
    private InputAction _moveAction;
    private InputAction _runAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _interactAction;

    // Input Values
    public Vector2 moveInput { get; private set; }
    public bool runIsHeld { get; private set; }
    public bool jumpWasPressed { get; private set; }
    public bool jumpWasReleased { get; private set; }
    public bool jumpIsHeld { get; private set; }
    public bool dashWasPressed { get; private set; }
    public bool interactWasPressed { get; private set; }

    protected override void Awake()
    {   
        base.Awake();

        var actionMap = inputActions.FindActionMap("Player");

        _moveAction = actionMap.FindAction("Movement");
        _runAction = actionMap.FindAction("Run");
        _jumpAction = actionMap.FindAction("Jump");
        _dashAction = actionMap.FindAction("Dash");
        _interactAction = actionMap.FindAction("Interact");
    }

    private void OnEnable()
    {
        // Enable all actions and subscribe to events
        _moveAction.Enable();
        _runAction.Enable();
        _jumpAction.Enable();
        _dashAction.Enable();
        _interactAction.Enable();

        // Subscribe to input events
        _jumpAction.performed += OnJumpPerformed;
        _jumpAction.canceled += OnJumpCanceled;
        _dashAction.performed += OnDashPerformed;
        _interactAction.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        // Disable all actions and subscribe to events
        _moveAction.Disable();
        _runAction.Disable();
        _jumpAction.Disable();
        _dashAction.Disable();
        _interactAction.Disable();

        // Subscribe to input events
        _jumpAction.performed -= OnJumpPerformed;
        _jumpAction.canceled -= OnJumpCanceled;
        _dashAction.performed -= OnDashPerformed;
        _interactAction.performed -= OnInteractPerformed;
    }

    private void Update()
    {
        // Read input values every frame
        moveInput = _moveAction.ReadValue<Vector2>();
        runIsHeld = _runAction.IsPressed();
        jumpIsHeld = _jumpAction.IsPressed();

        // Reset one-frame input flags at the end of frame
        if (jumpWasPressed) jumpWasPressed = false;
        if (jumpWasReleased) jumpWasReleased = false;
        if (dashWasPressed) dashWasPressed = false;
        if (interactWasPressed) interactWasPressed = false;
    }

    private void LateUpdate()
    {
        // Reset one-frame flags after all Update calls
        jumpWasPressed = false;
        jumpWasReleased = false;
        dashWasPressed = false;
        interactWasPressed = false;
    }

    #region Input Event Handlers
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpWasPressed = true;
    }
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpWasReleased = true;
    }
    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        dashWasPressed = true;
    }
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        interactWasPressed = true;
    }

    #endregion

    #region Public Helper Methods
    
    public bool IsMovingHorizontally()
    {
        return Mathf.Abs(moveInput.x) > 0.1f;
    }

    public bool IsMovingVertically()
    {
        return Mathf.Abs(moveInput.y) > 0.1f;
    }

    public float GetHorizontalInput()
    {
        float horizontal = moveInput.x;
        if (horizontal > 0.5f) return 1f;
        if (horizontal < -0.5f) return -1f;
        return 0f;
    }

    public float GetVerticalInput()
    {
        float vertical = moveInput.y;
        if (vertical > 0.5f) return 1f;
        if (vertical < -0.5f) return -1f;
        return 0f;
    }
    #endregion
}

