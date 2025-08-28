using UnityEngine;

public class QuickTest : MonoBehaviour
{
    private PlayerAudioManager _audioManager;
    private PlayerEffectsManager _effectManager;

    private void Start()
    {
        _audioManager = GetComponent<PlayerAudioManager>();
        _effectManager = GetComponent<PlayerEffectsManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("Testing jump effect");
            _effectManager.CreateSimpleJumpEffect(transform.position, Color.yellow);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Testing landing effect");
            _effectManager.CreateSimpleLandingEffect(transform.position, Color.brown, 1.5f);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Testing dash effect");
            _effectManager.CreateSimpleDashEffect(transform.position, transform.right);
        }
    }
}

