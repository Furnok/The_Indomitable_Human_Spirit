using UnityEngine;
using System.Collections.Generic;

public class S_TargetManager : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    [Header("Input")]
    [SerializeField] RSE_OnEnemyEnterTargetingRange _onEnemyEnterTargetingRange;
    [SerializeField] RSE_OnEnemyExitTargetingRange _onEnemyExitTargetingRange;

    //[Header("Output")]

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
            Debug.Log("Enemy in range: " + enemy.name);
        }
    }

    private void EnemyExitRange(GameObject enemy)
    {
        if (_enemiesInRange.Contains(enemy))
        {
            _enemiesInRange.Remove(enemy);
            Debug.Log("Enemy out of range: " + enemy.name);
        }
    }
}