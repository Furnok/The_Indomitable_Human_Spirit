using Sirenix.OdinInspector;
using UnityEngine;

public class S_FireManager : MonoBehaviour
{
    [TabGroup("Settings")]
    [SerializeField] private float _spawnRadius = 1f;
    [TabGroup("Settings")]
    [SerializeField] private float _spawnHeight = 1f;

    [TabGroup("Settings")]
    [SerializeField] private float nmbParticle = 1f;

    [TabGroup("References")]
    [SerializeField] private Transform _targetAttract;

    [TabGroup("References")]
    [SerializeField] private S_FireAttract fireAttract;

    [TabGroup("References")]
    [Title("Spawn Center")]
    [SerializeField] private Transform bossTransform;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnAttractParticle rseOnAttractParticle;

    //[Header("Outputs")]

    private void OnEnable()
    {
        rseOnAttractParticle.action += InstantiatFireAttract;
    }
    private void OnDisable()
    {
        rseOnAttractParticle.action -= InstantiatFireAttract;
    }
    private void InstantiatFireAttract()
    {
        
        Transform spawnCenter = bossTransform != null ? bossTransform : transform;

        int count = Mathf.Max(0, Mathf.FloorToInt(nmbParticle));
        for (int i = 0; i < count; i++)
        {
            Debug.Log("Instantiate Fire Attract");
            Vector2 rnd = Random.insideUnitCircle * _spawnRadius;
            Vector3 spawnPos = spawnCenter.position + new Vector3(rnd.x, _spawnHeight, rnd.y);

            var attract = Instantiate(fireAttract, spawnPos, Quaternion.identity);
            attract.InitializeTransform(_targetAttract);
        }

    }
}
