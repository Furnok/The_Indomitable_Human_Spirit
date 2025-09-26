using UnityEngine;

public class S_PlayerProjectileManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] int _initialPoolSize = 10;

    [Header("References")]
    [SerializeField] Transform _spawnProjectileParent;
    [SerializeField] S_PlayerProjectile _projectilePrefab;

    S_ObjectPool<S_PlayerProjectile> _projectilePool;

    private void Awake()
    {
        _projectilePool = new S_ObjectPool<S_PlayerProjectile>(_projectilePrefab, _initialPoolSize, _spawnProjectileParent);
    }
}