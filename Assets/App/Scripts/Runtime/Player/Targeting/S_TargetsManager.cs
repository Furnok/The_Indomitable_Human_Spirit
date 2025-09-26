using UnityEngine;
using System.Collections.Generic;

public class S_TargetManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] RSE_OnEnemyEnterTargetingRange _onEnemyEnterTargetingRange;
    [SerializeField] RSE_OnEnemyExitTargetingRange _onEnemyExitTargetingRange;

    [Header("Output")]
    [SerializeField] RSE_OnTargetsInRangeChange _onTargetsInRangeChange;

    HashSet<GameObject> _enemiesInRange = new HashSet<GameObject>();

    private void OnEnable()
    {
        _onEnemyEnterTargetingRange.action += EnemyEnterRange;
        _onEnemyExitTargetingRange.action += EnemyExitRange;
    }

    private void OnDisable()
    {
        _onEnemyEnterTargetingRange.action -= EnemyEnterRange;
        _onEnemyExitTargetingRange.action -= EnemyExitRange;
    }

    private void EnemyEnterRange(GameObject enemy)
    {
        if (!_enemiesInRange.Contains(enemy))
        {
            _enemiesInRange.Add(enemy);

            _onTargetsInRangeChange.Call(_enemiesInRange);
        }
    }

    private void EnemyExitRange(GameObject enemy)
    {
        if (_enemiesInRange.Contains(enemy))
        {
            _enemiesInRange.Remove(enemy);

            _onTargetsInRangeChange.Call(_enemiesInRange);
        }
    }
}