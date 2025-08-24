using UnityEngine;

[System.Serializable]
public class PlayerMovement
{
    private PlayerController _controller;
    private PlayerDataSO _moveStats;
    private PlayerCollisionDeteror _collisionDetector;

    public bool isFacingRight { get; private set; } = true;
}

