using Sirenix.OdinInspector;
using UnityEngine;

public class S_PlayerParry : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Animation")]
    [SerializeField, S_AnimationName] string _parryParam;

    [TabGroup("References")]
    [SerializeField] private GameObject _weaponHand;

    [TabGroup("References")]
    [SerializeField] private GameObject _weaponBack;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerParryInput rseOnPlayerParry;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerGettingHit _rseOnPlayerGettingHit;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerAddState _onPlayerAddState;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationBoolValueChange rseOnAnimationBoolValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnSendConsoleMessage rseOnSendConsoleMessage;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CanParry _canParry;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_ParryStartTime _parryStartTime;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentState _playerCurrentState;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats _playerStats;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStateTransitions _playerStateTransitions;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_AnimationTransitionDelays _animationTransitionDelays;

    private float _parryDuration => _playerStats.Value.parryDuration;

    private Coroutine _parryCoroutine = null;

    private bool _parryUp = true;

    private void OnEnable()
    {
        _canParry.Value = false;
        _parryStartTime.Value = 0f;

        rseOnPlayerParry.action += TryParry;
        _rseOnPlayerGettingHit.action += CancelParry;
    }

    private void OnDisable()
    {
        rseOnPlayerParry.action -= TryParry;
        _rseOnPlayerGettingHit.action -= CancelParry;
    }

    private void TryParry()
    {
        if (_playerStateTransitions.Value.CanTransition(_playerCurrentState.Value, S_EnumPlayerState.Parrying) == false || _parryUp == false) return;

        _weaponHand.SetActive(true);
        _weaponBack.SetActive(false);

        _onPlayerAddState.Call(S_EnumPlayerState.Parrying);

        rseOnSendConsoleMessage.Call("Player Pary!");

        if (_parryCoroutine != null)  StopCoroutine(_parryCoroutine);

        _parryUp = false;
        StartCoroutine(S_Utils.Delay(_playerStats.Value.parryCooldown, () =>
        {
            _parryUp = true;
        }));

        rseOnAnimationBoolValueChange.Call(_parryParam, true);

        _parryCoroutine = StartCoroutine(S_Utils.Delay(_animationTransitionDelays.Value.parryStartupDelay, () =>
        {
            _parryStartTime.Value = Time.time;
            _canParry.Value = true;

            _parryCoroutine = StartCoroutine(S_Utils.Delay(_parryDuration, () =>
            {
                _canParry.Value = false;

                if (_parryCoroutine != null) StopCoroutine(_parryCoroutine);

                rseOnAnimationBoolValueChange.Call(_parryParam, false);

                _parryCoroutine = StartCoroutine(S_Utils.Delay(_animationTransitionDelays.Value.parryRecoveryDelay, () =>
                {
                    if (_parryCoroutine != null) StopCoroutine(_parryCoroutine);

                    _onPlayerAddState.Call(S_EnumPlayerState.None);

                    _weaponHand.SetActive(false);
                    _weaponBack.SetActive(true);
                }));
            }));
        }));
    }

    private void CancelParry()
    {
        if (_parryCoroutine == null) return;

        StopCoroutine(_parryCoroutine);

        ResetValue();

        _weaponHand.SetActive(false);
        _weaponBack.SetActive(true);
    }

    private void ResetValue()
    {
        _canParry.Value = false;
        rseOnAnimationBoolValueChange.Call(_parryParam, false);
    }
}