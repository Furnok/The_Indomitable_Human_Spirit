using UnityEngine;
using Sirenix.OdinInspector;

public class S_FireAttract : MonoBehaviour
{
    [TabGroup("Settings")]
    [Title("Settings")]
    [SuffixLabel("s", Overlay = true)]
    [SerializeField, Range(0f, 1f)] private float attractStartLife = 0.1f;

    [TabGroup("Settings")]
    [SerializeField] private float attractionStrength = 8f;

    [TabGroup("Settings")]
    [SerializeField] private float attractionLerp = 10f;

    [TabGroup("Settings")]
    [SerializeField] private float killRadius = 0.1f;

    [TabGroup("References")]
    [Title("Target")]
    [SerializeField] private Transform target;

    [TabGroup("References")]
    [Title("Particle")]
    [SerializeField] private ParticleSystem _ps;

    private ParticleSystem.Particle[] _particles;
    private void LateUpdate()
    {
        if (target == null) return;

        int max = _ps.main.maxParticles;
        if (_particles == null || _particles.Length < max)
        {
            _particles = new ParticleSystem.Particle[max];
        }

        int count = _ps.GetParticles(_particles);
        if (count == 0)
        {
            ResetTarget();
            return;
        }

        bool worldSim = _ps.main.simulationSpace == ParticleSystemSimulationSpace.World;

        for (int i = 0; i < count; i++)
        {
            var p = _particles[i];

            float life01 = 1f - (p.remainingLifetime / p.startLifetime);

            if (life01 < attractStartLife)
            {
                _particles[i] = p;
                continue;
            }

            Vector3 worldPos = worldSim ? p.position : transform.TransformPoint(p.position);

            Vector3 toTarget = target.position - worldPos;
            float dist = toTarget.magnitude;
            if (dist < killRadius)
            {
                p.remainingLifetime = 0f;
                _particles[i] = p;
                continue;
            }

            toTarget /= Mathf.Max(dist, 0.0001f);

            Vector3 velWorld = worldSim
                ? p.velocity
                : transform.TransformDirection(p.velocity);

            Vector3 desiredVel = toTarget * attractionStrength;
            velWorld = Vector3.Lerp(velWorld, desiredVel, Time.deltaTime * attractionLerp);

            p.velocity = worldSim
                ? velWorld
                : transform.InverseTransformDirection(velWorld);

            _particles[i] = p;
        }

        _ps.SetParticles(_particles, count);
    }
    public void InitializeTransform(Transform transformToAttract)
    {
        _ps.Play();

        var emission = _ps.emission;

        var bursts = new ParticleSystem.Burst[emission.burstCount];
        emission.GetBursts(bursts);

        int total = 0;

        foreach (var b in bursts)
        {
            int cycles = Mathf.Max(1, b.cycleCount);
            total += (int)b.count.constant * cycles;
        }

        target = transformToAttract;
    }

    private void ResetTarget()
    {
        target = null;
    }
}