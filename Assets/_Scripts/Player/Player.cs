using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour
{
    [HideInInspector] public PlayerController playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerController>();
    }


}

