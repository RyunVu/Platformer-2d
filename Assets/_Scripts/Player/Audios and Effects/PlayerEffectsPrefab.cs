using UnityEngine;

[System.Serializable]
public class PlayerEffectsPrefab 
{
    [Header("Jump Effects")]
    public GameObject jumpEffect;
    public GameObject doubleJumpEffect;

    [Header("Landing Effects")]
    public GameObject softLandingEffect;
    public GameObject hardLandingEffect;

    [Header("Other Effects")]
    public GameObject wallJumpEffect;
    public GameObject dashEffect;

    [Header("Effect Settings")]
    public float effectLifeTime = 2f;
    public float hardLandingThreshold = 3f;
    public Vector3 jumpEffectOffset = Vector3.zero;
    public Vector3 landingEffectOffset = Vector3.zero;
}

