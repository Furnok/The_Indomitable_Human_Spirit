using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_DodgeableAreaDetector : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Filter")]
    [SerializeField, S_TagName] private string tagHit;

    [Title("RSO")]
    [SerializeField] RSO_ListTutoStepFinished _tutoStepsFinished;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_AttackDataInDodgeableArea _attackDataInDodgeableArea;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_AttackCanHitPlayer _attackCanHitPlayer;

    [TabGroup("Outputs")]
    [SerializeField] RSE_OnRequestStartTutorialStep _onRequestStartTutorialStep;

    [TabGroup("Inputs")]
    [SerializeField] RSO_SettingsSaved _rsoSettingsSaved;



    private Dictionary<I_AttackProvider, Collider> _tempAttackDataInDodgeableArea = new();
    bool _hasParryedProjectile = false;
    GameObject _projectileGameObject;

    private void Awake()
    {
        _attackDataInDodgeableArea.Value = new S_SerializableDictionary<int, S_StructAttackData>();
        _attackDataInDodgeableArea.Value.Clear();

        _attackCanHitPlayer.Value = new S_SerializableDictionary<int, S_StructAttackData>();
        _attackCanHitPlayer.Value.Clear();
        _tempAttackDataInDodgeableArea.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagHit) && other.TryGetComponent(out I_AttackProvider attack) && other.enabled == true)
        {
            var goId = other.gameObject.GetInstanceID();
            ref var attackData = ref attack.GetAttackData();
            attackData.goSourceId = goId;

            if (_tempAttackDataInDodgeableArea.ContainsKey(attack) == false)
            {
                _tempAttackDataInDodgeableArea.Add(attack, other);
            }

            if ( attackData.attackType == S_EnumAttackType.Projectile && _hasParryedProjectile == false && _rsoSettingsSaved.Value.activateTuto == true)
            {
                if (_tutoStepsFinished.Value != null && _tutoStepsFinished.Value.Count > 0)
                {
                    var tutoStep = _tutoStepsFinished.Value.Find(x => x.Step == S_EnumTutorialStep.ParryProjectile && x.IsFinished == false);
                    if (tutoStep != null)
                    {
                        _projectileGameObject = other.gameObject;
                    }
                }
            }

            if (_attackDataInDodgeableArea.Value == null || _attackDataInDodgeableArea.Value.ContainsKey(goId) || attackData.attackType != S_EnumAttackType.Dodgeable)
            {

            }
            else
            {
                _attackDataInDodgeableArea.Value.Add(goId, attack.GetAttackData());
            }


            if (_attackCanHitPlayer.Value == null || _attackCanHitPlayer.Value.ContainsKey(goId) || attackData.attackType != S_EnumAttackType.Dodgeable)
            {

            }
            else
            {
                _attackCanHitPlayer.Value.Add(goId, attack.GetAttackData());

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagHit) && other.TryGetComponent(out I_AttackProvider attack))
        {
            var goId = other.gameObject.GetInstanceID();
            if (_attackDataInDodgeableArea.Value.ContainsKey(goId) == true)
            {
                _attackDataInDodgeableArea.Value.Remove(goId);
            }

            if (_attackCanHitPlayer.Value.ContainsKey(goId) == true)
            {
                _attackCanHitPlayer.Value.Remove(goId);
            }

            if (_tempAttackDataInDodgeableArea.ContainsKey(attack) == true)
            {
                _tempAttackDataInDodgeableArea.Remove(attack);
            }
        }
    }

    private void LateUpdate()
    {
        if (_tempAttackDataInDodgeableArea.Count <= 0) return;

        var toRemove = new List<I_AttackProvider>();

        foreach (var kvp in _tempAttackDataInDodgeableArea)
        {
            if (kvp.Value == null || kvp.Value.enabled == false)
            {
                int? goId = null;

                if (kvp.Value != null)
                {
                    goId = kvp.Value.gameObject.GetInstanceID();
                }
                else if (kvp.Key != null)
                {
                    try
                    {
                        ref var attackData = ref kvp.Key.GetAttackData();
                        goId = attackData.goSourceId;
                    }
                    catch
                    {
                        goId = null;
                    }
                }

                if (goId.HasValue)
                {
                    if (_attackDataInDodgeableArea.Value != null && _attackDataInDodgeableArea.Value.ContainsKey(goId.Value))
                    {
                        _attackDataInDodgeableArea.Value.Remove(goId.Value);
                    }

                    if (_attackCanHitPlayer.Value != null && _attackCanHitPlayer.Value.ContainsKey(goId.Value))
                    {
                        _attackCanHitPlayer.Value.Remove(goId.Value);
                    }
                }

                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            _tempAttackDataInDodgeableArea.Remove(key);
        }
    }

    private void FixedUpdate()
    {
        if(_projectileGameObject != null && _hasParryedProjectile == false)
        {
            if(Vector3.Distance(_projectileGameObject.transform.position, this.transform.position) < 2f)
            {
                _onRequestStartTutorialStep.Call(S_EnumTutorialStep.ParryProjectile);
                _projectileGameObject = null;
                _hasParryedProjectile = true;

                //StartCoroutine(S_Utils.Delay(1f, () =>
                //{
                //    _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Conviction);
                //}));

                //StartCoroutine(S_Utils.Delay(1.5f, () =>
                //{
                //    _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Attack);
                //}));
            }
        }
    }
}