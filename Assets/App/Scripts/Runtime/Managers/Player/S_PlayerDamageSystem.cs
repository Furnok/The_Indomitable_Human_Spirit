using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

public class S_PlayerDamageSystem : MonoBehaviour
{
    [TabGroup("Reference")]
    [Title("Audio")]
    [SerializeField] private EventReference _damageSound;

    [TabGroup("Reference")]
    [Title("Particle")]
    [SerializeField] private GameObject _playerDamageVFX;

    [TabGroup("Reference")]
    [SerializeField] private GameObject _propLantern;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerHit _rseOnPlayerHit;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerHealthReduced rseOnPlayerHealthReduced;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationTriggerValueChange rseOnAnimationTriggerValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerGettingHit _rseOnPlayerGettingHit;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerAddState _onPlayerAddState;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnRumbleRequested _rseOnRumbleRequested;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnRumbleStopChannel _rseOnRumbleStopChannel;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentState _playerCurrentState;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_ConsoleCheats _debugPlayer;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStateTransitions _playerStateTransitions;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats _playerStats;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_RumbleData _gettingHitRumbleData;

    private Coroutine _hitReactCoroutine = null;

    private bool _isInvicible = false;

    private void OnEnable()
    {
        _rseOnPlayerHit.action += TakeDamage;
    }
    


    private void OnDisable()
    {
        _rseOnPlayerHit.action -= TakeDamage;
    }

    private void TakeDamage(S_StructAttackContact attackContact)
    {
        if (_playerStateTransitions.Value.CanTransition(_playerCurrentState.Value, S_EnumPlayerState.HitReact) == true && _isInvicible == false && _debugPlayer.Value.cantGetttingHit == false)
        {
            var attackData = attackContact.data;

            if (_hitReactCoroutine != null) StopCoroutine(_hitReactCoroutine);


            RuntimeManager.PlayOneShot(_damageSound);
            Instantiate(_playerDamageVFX, _propLantern.transform.position, Quaternion.identity, gameObject.transform);

            rseOnAnimationTriggerValueChange.Call("isHit");
            _onPlayerAddState.Call(S_EnumPlayerState.HitReact);
            _isInvicible = true;

            _rseOnRumbleStopChannel.Call(S_EnumRumbleChannel.Hit);
            _rseOnRumbleRequested.Call(_gettingHitRumbleData.Value);

            _hitReactCoroutine = StartCoroutine(S_Utils.Delay(attackData.knockbackHitDuration, () =>
            {
                if (_playerStateTransitions.Value.CanTransition(_playerCurrentState.Value, S_EnumPlayerState.None) == true)
                {
                    _onPlayerAddState.Call(S_EnumPlayerState.None);
                }
            }));

            StartCoroutine(S_Utils.Delay(attackData.invicibilityDuration, () =>
            {
                _isInvicible = false;
            }));

            rseOnPlayerHealthReduced.Call(attackData.damage);
            _rseOnPlayerGettingHit.Call();
        }
    }
}