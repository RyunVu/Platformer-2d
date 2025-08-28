using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAudioManager : MonoBehaviour
{
    [SerializeField] private PlayerAudioClips _audioClips;
    [SerializeField] private AudioSource _jumpAudioSource;
    [SerializeField] private AudioSource _landingAudioSource;
    [SerializeField] private AudioSource _effectsAudioSource;

    // Auto-create audio sources if not assigned
    private void Awake()
    {
        if (_jumpAudioSource == null)
            _jumpAudioSource = CreateAudioSource("Jump Audio Source");

        if (_landingAudioSource == null)
            _landingAudioSource = CreateAudioSource("Landing Audio Source");

        if (_effectsAudioSource == null)
            _effectsAudioSource = CreateAudioSource("Effects Audio Source");
    }

    private AudioSource CreateAudioSource(string name)
    {
        GameObject audioObject = new GameObject(name);
        audioObject.transform.SetParent(transform);
        audioObject.transform.localPosition = Vector3.zero;
        return audioObject.AddComponent<AudioSource>();
    }

    public void PlayJumpSound(int jumpNumber = 1)
    {
        AudioClip[] soundArray = GetJumpSoundsForNumber(jumpNumber);
        if (soundArray != null && soundArray.Length > 0)
        {
            PlayRandomSound(_jumpAudioSource, soundArray, _audioClips.jumpVolume);
        }
    }

    public void PlayLandingSound(float fallDistance)
    {
        AudioClip[] soundArray = fallDistance >= _audioClips.hardLandingThreshold
            ? _audioClips.hardLandingSounds
            : _audioClips.softLandingSounds;

        if (soundArray != null && soundArray.Length > 0)
        {
            float volume = _audioClips.landingVolume;
            if (fallDistance >= _audioClips.hardLandingThreshold)
            {
                // Scale volume based on fall distance for hard landings
                volume *= Mathf.Clamp(fallDistance / (_audioClips.hardLandingThreshold * 2f), 1f, 1.5f);
            }

            PlayRandomSound(_landingAudioSource, soundArray, volume);
        }
    }

    public void PlayWallJumpSound()
    {
        if (_audioClips.wallJumpSounds != null && _audioClips.wallJumpSounds.Length > 0)
        {
            PlayRandomSound(_effectsAudioSource, _audioClips.wallJumpSounds, _audioClips.jumpVolume);
        }
    }

    public void PlayDashSound()
    {
        if (_audioClips.dashStartSounds != null && _audioClips.dashStartSounds.Length > 0)
        {
            PlayRandomSound(_effectsAudioSource, _audioClips.dashStartSounds, _audioClips.dashVolume);
        }

        // Start looping dash sound if available
        if (_audioClips.dashLoopSound != null && _effectsAudioSource != null)
        {
            StartDashLoop();
        }
    }

    public void PlayDashEndSound()
    {
        // Stop looping sound first
        StopDashLoop();

        // Play dash end sound if available
        if (_audioClips.dashEndSounds != null && _audioClips.dashEndSounds.Length > 0)
        {
            PlayRandomSound(_effectsAudioSource, _audioClips.dashEndSounds, _audioClips.dashVolume * 0.7f);
        }
    }

    private void StartDashLoop()
    {
        if (_audioClips.dashLoopSound != null && _effectsAudioSource != null)
        {
            _effectsAudioSource.clip = _audioClips.dashLoopSound;
            _effectsAudioSource.volume = _audioClips.dashVolume * 0.8f;
            _effectsAudioSource.pitch = 1f + Random.Range(-_audioClips.pitchVariation * 0.5f, _audioClips.pitchVariation * 0.5f);
            _effectsAudioSource.loop = true;
            _effectsAudioSource.Play();
        }
    }

    private void StopDashLoop()
    {
        if (_effectsAudioSource != null && _effectsAudioSource.loop && _effectsAudioSource.isPlaying)
        {
            _effectsAudioSource.loop = false;
            _effectsAudioSource.Stop();
        }
    }

    private AudioClip[] GetJumpSoundsForNumber(int jumpNumber)
    {
        return jumpNumber switch
        {
            1 => _audioClips.jumpSounds,
            2 => _audioClips.doubleJumpSounds?.Length > 0 ? _audioClips.doubleJumpSounds : _audioClips.jumpSounds,
            _ => _audioClips.jumpSounds
        };
    }

    private void PlayRandomSound(AudioSource audioSource, AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;

        AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];
        if (clipToPlay != null)
        {
            audioSource.clip = clipToPlay;
            audioSource.volume = volume;
            audioSource.pitch = 1f + Random.Range(-_audioClips.pitchVariation, _audioClips.pitchVariation);
            audioSource.Play();
        }
    }

    // Method to stop all sounds (useful for special cases)
    public void StopAllSounds()
    {
        _jumpAudioSource?.Stop();
        _landingAudioSource?.Stop();
        StopDashLoop(); // Use the dash-specific stop method
        _effectsAudioSource?.Stop();
    }
}