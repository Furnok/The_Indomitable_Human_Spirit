using Sirenix.OdinInspector;
using UnityEngine;

public class S_ParticlesAttract : MonoBehaviour
{
    [TabGroup("Settings")]
    [Title("Settings")]
    [SuffixLabel("s", Overlay = true)]
    [SerializeField, Range(0f, 1f)] private float attractStartLife = 0.1f;           // at 70% of lifetime atrtract to the target transform

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

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerGainConviction _onPlayerGainConviction;

    private ParticleSystem.Particle[] _particles;
    private float _ammountTotalConvictionGain = 0f;
    private int _totalParticles = 0;

    private float _convictionPerParticle => _totalParticles > 0 ? _ammountTotalConvictionGain / _totalParticles : 0;

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
            //Destroy(gameObject);
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
                _onPlayerGainConviction.Call(_convictionPerParticle);
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

    public void InitializeTransform(Transform transformToAttract, float ammountConvictionGain)
    {
        _ps.Play();
        _ammountTotalConvictionGain = ammountConvictionGain;

        var emission = _ps.emission;

        if (emission.burstCount == 0) _totalParticles = 0;

        var bursts = new ParticleSystem.Burst[emission.burstCount];
        emission.GetBursts(bursts);

        int total = 0;

        foreach (var b in bursts)
        {
            int cycles = Mathf.Max(1, b.cycleCount);
            total += (int)b.count.constant * cycles;
        }

        _totalParticles = total;

        //Debug.Log("Total Particles Emitted: " + _totalParticles);

        target = transformToAttract;
    }

    private void ResetTarget()
    {
        target = null;
    }
}