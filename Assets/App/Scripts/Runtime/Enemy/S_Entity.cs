using UnityEngine;
using Unity.Behavior;
using UnityEngine.AI;

public class S_Entity : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BehaviorGraphAgent agent;
    [SerializeField] NavMeshAgent enemyNavMesh;
    [SerializeField] S_EnemyRangeDetection S_EnemyRangeDetection;

    void SetTarget(GameObject Target)
    {
        agent.SetVariableValue<GameObject>("Player", Target);
        if(Target != null)
        {
            agent.SetVariableValue<S_EnumEnemyState>("EnemyState", S_EnumEnemyState.Chase);
        }
        else
        {
            agent.SetVariableValue<S_EnumEnemyState>("EnemyState", S_EnumEnemyState.Patrol);
        }
    }

    private void Update()
    {
        agent.SetVariableValue<float>("StopDistance", enemyNavMesh.stoppingDistance);
    }

    private void OnEnable()
    {
        S_EnemyRangeDetection.onTargetDetected.AddListener(SetTarget);
    }

    private void OnDisable()
    {
        S_EnemyRangeDetection.onTargetDetected.RemoveListener(SetTarget);
    }
}
