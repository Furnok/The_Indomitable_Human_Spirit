using System.Collections.Generic;
using UnityEngine;

public class S_TargetingManager : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    [Header("Input")]
    [SerializeField] RSE_OnEnemyEnterTargetingRange _onEnemyEnterTargetingRange;
    [SerializeField] RSE_OnEnemyExitTargetingRange _onEnemyExitTargetingRange;

    [SerializeField] RSE_OnTargetsInRangeChange _onTargetsInRangeChange;

    [SerializeField] RSE_OnPlayerTargeting _onPlayerTargeting;
    [SerializeField] RSE_OnPlayerTargetingCancel _onPlayerTargetingCancel;
    [SerializeField] RSE_OnPlayerSwapTarget _onPlayerSwapTarget;

    //[Header("Output")]

    GameObject _currentTarget;
    HashSet<GameObject> _targetsPosible = new HashSet<GameObject>();


    private void OnEnable()
    {
        _onTargetsInRangeChange.action += OnChangeTargetsPosible;
    }

    private void OnDisable()
    {
        _onTargetsInRangeChange.action -= OnChangeTargetsPosible;
    }

    void OnChangeTargetsPosible(HashSet<GameObject> targetsList)
    {
        _targetsPosible = targetsList;
    }
}