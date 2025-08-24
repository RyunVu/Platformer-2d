using System;
using UnityEngine;

[System.Serializable]
public class PlayerCollisionDeteror 
{
    private PlayerController _controller;
    private PlayerDataSO _moveStats;

    // Collision states
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private RaycastHit2D _wallHit;
    private RaycastHit2D _lastWallHit;

    public bool isGrounded { get; private set; }
    public bool isHeadBumped { get; private set; }
    public bool isTouchingWall { get; private set; }
    public RaycastHit2D lastWallHit => _lastWallHit;

    // Timers
    public float jumpBufferTimer { get; private set; }
    public float coyoteTimer { get; private set; }
    public float wallJumpPostBufferTimer { get; private set; }
    public float dashOnGroundTimer { get; private set; }

    public PlayerCollisionDeteror(PlayerController controller, PlayerDataSO moveStats)
    {
        _controller = controller;
        _moveStats = moveStats;
        coyoteTimer = _moveStats.jumpCoyoteTime;
    }

    public void PerformCollisionChecks()
    {
        CheckIsGrounded();
        CheckBumpedHead();
        CheckIsTouchingWall();
    }

    public void UpdateTimers()
    {
        jumpBufferTimer -= Time.deltaTime;

        if(!isGrounded)
            coyoteTimer -= Time.deltaTime;
        else
            coyoteTimer = _moveStats.jumpCoyoteTime;

        if (!ShouldApplyPostWallJumpBuffer())
            wallJumpPostBufferTimer -= Time.deltaTime;  

        if(isGrounded)
            dashOnGroundTimer -= Time.deltaTime;
    }

    public void SetJumpBufferTimer(float timer)
    {
        jumpBufferTimer = timer;
    }

    public void SetWallJumpPostBufferTimer(float timer)
    {
        wallJumpPostBufferTimer = timer;
    }

    public void SetDashOnGroundTimer(float timer)
    {
        dashOnGroundTimer = timer;
    }

    public bool ShouldApplyPostWallJumpBuffer()
    {
        return !isGrounded && isTouchingWall;
    }

    private void CheckIsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_controller.feetCollider.bounds.center.x, _controller.feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_controller.feetCollider.bounds.size.x, _moveStats.groundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, _moveStats.groundDetectionRayLength, _moveStats.groundLayer);

        isGrounded = _groundHit.collider != null;

        #region Debug Visualization
        if (_moveStats.debugShowIsGroundedBox)
        {
            Color rayColor = isGrounded ? Color.green : Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * _moveStats.groundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * _moveStats.groundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - _moveStats.groundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion
    }

    private void CheckBumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_controller.feetCollider.bounds.center.x, _controller.bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_controller.feetCollider.bounds.size.x * _moveStats.headWidth, _moveStats.groundDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, _moveStats.headDetectionRayLength, _moveStats.groundLayer);

        isHeadBumped = _headHit.collider != null;

        #region Debug Visualization
        if (_moveStats.debugShowHeadBumpBox)
        {
            Color rayColor = isHeadBumped ? Color.green : Color.red;
            float headWidth = _moveStats.headWidth;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * _moveStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * _moveStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + _moveStats.headDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion
    }

    private void CheckIsTouchingWall()
    {
        float originEndpoint = _controller.isFacingRight ?
            _controller.bodyCollider.bounds.max.x :
            _controller.bodyCollider.bounds.min.x;

        float adjustHeight = _controller.bodyCollider.bounds.size.y * _moveStats.wallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originEndpoint, _controller.bodyCollider.bounds.center.y);
        Vector2 boxCastSize = new Vector2(_moveStats.wallDetectRayLength, adjustHeight);

        _wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, _controller.transform.right, _moveStats.wallDetectRayLength, _moveStats.groundLayer);

        if (_wallHit.collider != null)
        {
            _lastWallHit = _wallHit;
            isTouchingWall = true;
        }
        else
        {
            isTouchingWall = false;
        }

        #region Debug Visualization
        if (_moveStats.debugShowWallHitBox)
        {
            Color rayColor = isTouchingWall ? Color.green : Color.red;

            Vector2 boxBottomLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxBottomRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxTopLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);
            Vector2 boxTopRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);

            Debug.DrawLine(boxBottomLeft, boxBottomRight, rayColor);
            Debug.DrawLine(boxBottomRight, boxTopRight, rayColor);
            Debug.DrawLine(boxTopRight, boxTopLeft, rayColor);
            Debug.DrawLine(boxTopLeft, boxBottomLeft, rayColor);
        }
        #endregion
    }


}

