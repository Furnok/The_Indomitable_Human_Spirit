using UnityEngine;
using Unity.Behavior;
using UnityEngine.Events;
using System;

public class Entity : MonoBehaviour
{
    public BehaviorGraphAgent agent;
    [SerializeField] S_EnemyRangeDetection S_EnemyRangeDetection;

    void SetTarget(GameObject Target)
    {
        agent.SetVariableValue<GameObject>("Player", Target);
        if(Target != null)
        {
            agent.SetVariableValue<EnemyState>("EnemyState", EnemyState.Chase);
        }
        else
        {
            agent.SetVariableValue<EnemyState>("EnemyState", EnemyState.Patrol);
        }
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
