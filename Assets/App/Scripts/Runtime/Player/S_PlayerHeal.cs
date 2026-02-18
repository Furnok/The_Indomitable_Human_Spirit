using Sirenix.OdinInspector;
using UnityEngine;

public class S_PlayerHeal : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Animation")]
    [SerializeField, S_AnimationName] private string _healParam;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerHealInput rseOnPlayerHeal;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerGettingHit _rseOnPlayerGettingHit;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerHealPerformed rseOnPlayerHealPerformed;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerAddState _onPlayerAddState;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationBoolValueChange rseOnAnimationBoolValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnHealStart _onHealStart;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnDisplayError _onDisplayError;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentState _playerCurrentState;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentHealth _playerCurrentHealth;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentConviction _playerCurrentConviction;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStateTransitions _playerStateTransitions;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats ssoPlayerStats;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerConvictionData _playerConvictionData;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_AnimationTransitionDelays _animationTransitionDelays;

    private Coroutine healCoroutine = null;

    private void OnEnable()
    {
        rseOnPlayerHeal.action += TryHeal;
        _rseOnPlayerGettingHit.action += CancelHeal;
    }

    private void OnDisable()
    {
        rseOnPlayerHeal.action -= TryHeal;
        _rseOnPlayerGettingHit.action -= CancelHeal;
    }

    private void TryHeal()
    {
        if (_playerStateTransitions.Value.CanTransition(_playerCurrentState.Value, S_EnumPlayerState.Healing) == false) return;

        if (_playerCurrentHealth.Value >= ssoPlayerStats.Value.maxHealth) return;

        if (_playerCurrentConviction.Value < _playerConvictionData.Value.healCost)
        {
            _onDisplayError.Call();
            return;
        }

        _onPlayerAddState.Call(S_EnumPlayerState.Healing);
        rseOnAnimationBoolValueChange.Call(_healParam, true);
        _onHealStart.Call();

        healCoroutine = StartCoroutine(S_Utils.Delay(_animationTransitionDelays.Value.healStartupDelay, () =>
        {
            healCoroutine = StartCoroutine(S_Utils.Delay(ssoPlayerStats.Value.delayBeforeHeal, () =>
            {
                rseOnPlayerHealPerformed.Call();

                healCoroutine = StartCoroutine(S_Utils.Delay(_animationTransitionDelays.Value.healRecoveryDelay, () =>
                {
                    _onPlayerAddState.Call(S_EnumPlayerState.None);
                    rseOnAnimationBoolValueChange.Call(_healParam, false);
                }));
            }));
        }));
    }

    private void CancelHeal()
    {
        if (healCoroutine == null) return;

        StopCoroutine(healCoroutine);
        rseOnAnimationBoolValueChange.Call(_healParam, false);
    }
}