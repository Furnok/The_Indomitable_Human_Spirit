using System.Collections.Generic;
using UnityEngine;

public class S_TargetsDebug : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Color _gizmoColor = Color.green;
    [SerializeField] Color _gizmoTargetColor = Color.red;
    [SerializeField][Min(0.1f)] float _gizmoRadius = 0.1f;
    [SerializeField][Min(0.1f)] float _gizmoTargetRadius = 0.3f;
    [SerializeField] float _gizmoHeightOffset = 1.5f;
    [SerializeField] bool _drawGizmos = true;

    //[Header("References")]

    [Header("Input")]
    [SerializeField] RSE_OnEnemyEnterTargetingRange _onEnemyEnterTargetingRange;
    [SerializeField] RSE_OnEnemyExitTargetingRange _onEnemyExitTargetingRange;
    [SerializeField] RSE_OnNewTargeting _onNewTargeting;
    [SerializeField] RSE_OnPlayerCancelTargeting _onPlayerCancelTargeting;

    //[Header("Output")]

    [Header("RSO")]
    [SerializeField] RSO_PlayerIsTargeting _playerIsTargeting;


    HashSet<Transform> _targets = new HashSet<Transform>();

    bool _canDrawTarget = false;
    Transform _target = null;

    private void OnEnable()
    {
        _onEnemyEnterTargetingRange.action += AddTarget;
        _onEnemyExitTargetingRange.action += RemoveTarget;
        _onNewTargeting.action += OnNewTargeting;
        _onPlayerCancelTargeting.action += OnCancelTargeting;
    }

    private void OnDisable()
    {
        _onEnemyEnterTargetingRange.action -= AddTarget;
        _onEnemyExitTargetingRange.action -= RemoveTarget;
        _onNewTargeting.action -= OnNewTargeting;
        _onPlayerCancelTargeting.action -= OnCancelTargeting;
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

        if (_playerIsTargeting.Value == true && _canDrawTarget == true)
        {
            DrawTarget();
        }
    }

    void DrawAll()
    {
        var count = _targets.Count;

        foreach (var target in _targets)
        {
            if (_playerIsTargeting.Value == true && _canDrawTarget == true && target == _target) continue;

            Gizmos.color = _gizmoColor;
            Vector3 pos = new Vector3(target.position.x, target.position.y + _gizmoHeightOffset, target.position.z);
            Gizmos.DrawSphere(pos, _gizmoRadius);
        }
    }

    void DrawTarget()
    {
        Gizmos.color = _gizmoTargetColor;
        Vector3 pos = new Vector3(_target.position.x, _target.position.y + _gizmoHeightOffset, _target.position.z);
        Gizmos.DrawSphere(pos, _gizmoTargetRadius);
    }

    void OnNewTargeting(GameObject target)
    {
        _target = target.transform;
        _canDrawTarget = true;
    }

    void OnCancelTargeting(GameObject target)
    {
        _canDrawTarget = false;
        _target = null;
    }
}