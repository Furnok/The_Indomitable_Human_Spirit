using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Parsing;

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
    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnStopAttract rseOnStopAttract;

    private List<GameObject> spawnedAttracts = new List<GameObject>();
    private void OnEnable()
    {
        rseOnAttractParticle.action += InstantiatFireAttract;
        rseOnStopAttract.action += ResetAttract;
    }
    private void OnDisable()
    {
        rseOnAttractParticle.action -= InstantiatFireAttract;
        rseOnStopAttract.action -= ResetAttract;
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
            spawnedAttracts.Add(attract.gameObject);
            attract.InitializeTransform(_targetAttract);
        }

    }

    private void ResetAttract()
    {
        foreach (var item in spawnedAttracts)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        spawnedAttracts.Clear();
    }
}
