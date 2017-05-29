using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

[ExecuteInEditMode]
public class ParticlePerlinTurbulence : MonoBehaviour {

    private ParticleSystem system;
    private ParticleSystem.Particle[] particles;

    [Range(0.0f, 50.0f)]
    public float Force = 10.0f;

    [Range(0.01f, 100.0f)]
    public float Frequency = 4.0f;

    public bool UseRandomSeed = true;

    public AnimationCurve OverLifetime = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

    public void Start()
    {
        OnScriptReload();
    }

    public void OnScriptReload()
    {
        system = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[1024];
    }

#if UNITY_EDITOR
    public void OnEnable()
    {
        OnScriptReload();
    }
#endif

    public void Update()
    {
        var particleCount = system.GetParticles(particles);
        var dt = Time.deltaTime;
        var seedFactor = UseRandomSeed ? 1.0f : 0.0f;

#if UNITY_EDITOR
        dt = 1.0f / 60.0f; // approximates 60hz refresh rate in editor
#endif

        for (int i = 0; i < particleCount; ++i)
        {
            var pos = particles[i].position;
            var force = Force;
            var freq = Frequency;

            pos += Vector3.one * (float)(system.randomSeed % 1024) * seedFactor;

            var noiseX = Mathf.PerlinNoise(pos.x * freq, pos.y * freq);
            var noiseY = Mathf.PerlinNoise(pos.z * freq, pos.x * freq);
            var noiseZ = Mathf.PerlinNoise(pos.y * freq, pos.z * freq);

            noiseX = noiseX * 2.0f - 1.0f;
            noiseY = noiseY * 2.0f - 1.0f;
            noiseZ = noiseZ * 2.0f - 1.0f;

            var targetVelocity = new Vector3(noiseX, noiseY, noiseZ) * force;

            var t = 1.0f - particles[i].lifetime / particles[i].startLifetime;
            var apply = OverLifetime.Evaluate(t);

            particles[i].velocity += targetVelocity * dt * apply;
        }

        system.SetParticles(particles, particleCount);
    }
}