using Sirenix.OdinInspector;
using UnityEngine;

public class S_EnemyMaxTravelZone : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Filters")]
    [SerializeField, S_TagName] private string playerTag;

    [TabGroup("References")]
    [Title("Colliders")]
    [SerializeField] private SphereCollider detectionCollider;

    [TabGroup("References")]
    [Title("Scripts")]
    [SerializeField] private S_Enemy enemy;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) enemy.SetTargetInMaxTravelZone(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            enemy.SetTargetInMaxTravelZone(null);
            enemy.SetTarget(null);
        }
    }

    public void Setup(SSO_EnemyData enemyData)
    {
        detectionCollider.radius = enemyData.Value.detectionAggroRangeMax;
    }
}