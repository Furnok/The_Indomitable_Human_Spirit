using System.Collections.Generic;
using UnityEngine;

public class S_TargetsDebug : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Color _gizmoColor = Color.green;
    [SerializeField][Min(0.1f)] float _gizmoRadius = 0.5f;
    [SerializeField] float _gizmoHeightOffset = 1.5f;
    [SerializeField] bool _drawGizmos = true;

    //[Header("References")]

    [Header("Input")]
    [SerializeField] RSE_OnEnemyEnterTargetingRange _onEnemyEnterTargetingRange;
    [SerializeField] RSE_OnEnemyExitTargetingRange _onEnemyExitTargetingRange;

    //[Header("Output")]

    HashSet<Transform> _targets = new HashSet<Transform>();

    private void OnEnable()
    {
        _onEnemyEnterTargetingRange.action += AddTarget;
        _onEnemyExitTargetingRange.action += RemoveTarget;
    }

    private void OnDisable()
    {
        _onEnemyEnterTargetingRange.action -= AddTarget;
        _onEnemyExitTargetingRange.action -= RemoveTarget;
    }

    private void AddTarget(GameObject target)
    {
        if (!_targets.Contains(target.transform))
        {
            _targets.Add(target.transform);
        }
    }

    private void RemoveTarget(GameObject target)
    {
        if (_targets.Contains(target.transform))
        {
            _targets.Remove(target.transform);
        }
    }

    private void OnDrawGizmos()
    {
        if (!enabled || !_drawGizmos) return;
        DrawAll();
    }

    void DrawAll()
    {
        var count = _targets.Count;

        foreach (var target in _targets)
        {
            Gizmos.color = _gizmoColor;
            Vector3 pos = new Vector3(target.position.x, target.position.y + _gizmoHeightOffset, target.position.z);
            Gizmos.DrawSphere(pos, _gizmoRadius);
        }
    }
}