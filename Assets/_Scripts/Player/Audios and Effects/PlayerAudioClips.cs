using UnityEngine;

[System.Serializable]
public class PlayerAudioClips 
{
    [Header("Jump Sounds")]
    public AudioClip[] jumpSounds;
    public AudioClip[] doubleJumpSounds;

    [Header("Landing Sounds")]
    public AudioClip[] softLandingSounds;
    public AudioClip[] hardLandingSounds;

    [Header("Dash Sounds")]
    public AudioClip[] dashStartSounds;
    public AudioClip[] dashEndSounds;
    public AudioClip dashLoopSound;

    [Header("Other Sounds")]
    public AudioClip[] wallJumpSounds;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float jumpVolume = .7f;
    [Range(0f, 1f)] public float landingVolume = .8f;
    [Range(0f, 1f)] public float dashVolume = .8f;
    [Range(0f, 1f)] public float pitchVariation = .1f;
    public float hardLandingThreshold = 3f;
}

