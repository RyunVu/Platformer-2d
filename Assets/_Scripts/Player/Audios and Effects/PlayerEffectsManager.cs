using System.Data.Common;
using Unity.Hierarchy;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerEffectsManager : SingletonMonobehaviour<PlayerEffectsManager>
{
    [SerializeField] private PlayerEffectsPrefab _effectPrefab;
    [SerializeField] private Transform _jumpEffectSpawnPoint;
    [SerializeField] private Transform _landingEffectSpawnPoint;

    private GameObject _currentDashEffect;

    private void Start()
    {
        if (_jumpEffectSpawnPoint == null)
            _jumpEffectSpawnPoint = transform;

        if (_landingEffectSpawnPoint == null)
            _landingEffectSpawnPoint = transform;
    }

    public void PlayJumpEffect(int jumpNumber = 1, Vector3? customPosition = null)
    {
        GameObject effectPrefab = GetJumpEffectForNumber(jumpNumber);
        if (effectPrefab != null)
        {
            Vector3 spawnPoint = customPosition ?? (_jumpEffectSpawnPoint.position + _effectPrefab.jumpEffectOffset);
            SpawnEffect(effectPrefab, spawnPoint);
        }
    }

    public void PlayLandingEffect(float fallDistance, Vector3? customPosition = null)
    {
        GameObject effectPrefab = fallDistance >= _effectPrefab.hardLandingThreshold
            ? _effectPrefab.hardLandingEffect
            : _effectPrefab.softLandingEffect;

        if (effectPrefab != null)
        {
            Vector3 spawnPosition = customPosition ?? (_landingEffectSpawnPoint.position + _effectPrefab.landingEffectOffset);
            GameObject effect = SpawnEffect(effectPrefab, spawnPosition);

            if (fallDistance >= _effectPrefab.hardLandingThreshold && _effectPrefab != null)
            {
                float scale = Mathf.Clamp(fallDistance / (_effectPrefab.hardLandingThreshold * 2f), 1f, 1.8f);
                effect.transform.localScale *= scale;
            }
        }
    }

    public void PlayWallJumpEffect(Vector3? customPosition = null)
    {
        if (_effectPrefab.wallJumpEffect != null)
        {
            Vector3 spawnPosition = customPosition ?? (_jumpEffectSpawnPoint.position + _effectPrefab.jumpEffectOffset);
            SpawnEffect(_effectPrefab.wallJumpEffect, spawnPosition);
        }
    }

    public void StartDashEffect(Vector2 dashDirection)
    {
        if (_effectPrefab.dashEffect != null)
        {
            if (_currentDashEffect != null) Destroy(_currentDashEffect);

            _currentDashEffect = Instantiate(_effectPrefab.dashEffect, transform.position, Quaternion.identity);
            _currentDashEffect.transform.SetParent(transform);

            if (dashDirection != Vector2.zero)
            {
                float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
                _currentDashEffect.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
        else
            CreateSimpleDashEffect(transform.position, dashDirection);
    }

    public void StopDashEffect()
    {
        if (_currentDashEffect != null)
        {
            Destroy(_currentDashEffect);
            _currentDashEffect = null;
        }
    }

    private GameObject GetJumpEffectForNumber(int jumpNumber)
    {
        return jumpNumber switch
        {
            1 => _effectPrefab.jumpEffect,
            2 => _effectPrefab.doubleJumpEffect ?? _effectPrefab.jumpEffect,
            _ => _effectPrefab.jumpEffect,
        };
    }

    private GameObject SpawnEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab == null) return null;

        GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);

        Destroy(effect, _effectPrefab.effectLifeTime);
        return effect;
    }
    public void CreateSimpleJumpEffect(Vector3 position, Color color)
    {
        GameObject effectObject = new GameObject("Simple Jump Effect");
        effectObject.transform.position = position;

        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = color;
        main.startSize = 0.1f;
        main.startSpeed = 2f;
        main.maxParticles = 10;
        main.startLifetime = 0.5f;

        var emission = particles.emission;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 10)
        });

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        Destroy(effectObject, 2f);
    }

    public void CreateSimpleLandingEffect(Vector3 position, Color color, float intensity = 1f)
    {
        GameObject effectObject = new GameObject("Simple Landing Effect");
        effectObject.transform.position = position;

        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = color;
        main.startSize = 0.15f * intensity;
        main.startSpeed = 3f * intensity;
        main.maxParticles = (int)(15 * intensity);
        main.startLifetime = 0.7f;

        var emission = particles.emission;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, (short)(15 * intensity))
        });

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.5f * intensity;

        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-2f * intensity);

        Destroy(effectObject, 3f);
    }

    public void CreateSimpleDashEffect(Vector3 position, Vector2 direction)
    {
        GameObject effectObject = new GameObject("Simple Dash Effect");
        effectObject.transform.position = position;
        effectObject.transform.SetParent(transform);

        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = new Color(1f, 1f, 1f, 0.8f); // White with transparency
        main.startSize = 0.1f;
        main.startSpeed = 0.5f;
        main.maxParticles = 50;
        main.startLifetime = 0.3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = 100;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        // Set velocity opposite to dash direction to create trail effect
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-direction.x * 2f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-direction.y * 2f);

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;

        _currentDashEffect = effectObject;
    }
}

