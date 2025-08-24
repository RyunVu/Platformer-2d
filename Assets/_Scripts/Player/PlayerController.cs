using Assets._Scripts.Player;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerJump))]
[RequireComponent(typeof(PlayerWallInteraction))]
[RequireComponent(typeof(PlayerDash))]
[RequireComponent(typeof(PlayerCollisionDeteror))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public PlayerDataSO playerData;
    [SerializeField] private PlayerAnimator _playerAnimator;
    [SerializeField] private Collider2D _bodyColl;
    [SerializeField] private Collider2D _feetColl;

    // Components
    private PlayerMovement _movement;
    private PlayerJump _jump;
    private PlayerWallInteraction _wallInteraction;
    private PlayerDash _dash;
    private PlayerCollisionDeteror _collisionDetector;
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
        _collisionDetector = GetComponent<PlayerCollisionDeteror>();
        _movement = GetComponent<PlayerMovement>();
        _jump = GetComponent<PlayerJump>();
        _wallInteraction = GetComponent<PlayerWallInteraction>();
        _dash = GetComponent<PlayerDash>();
        _stateMachine = GetComponent<PlayerStateMachine>();
    }
}

