using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class S_PlayerHitResolver : MonoBehaviour
{
    [TabGroup("Settings")]
    [Title("Camera")]
    [SerializeField] private S_ClassCameraShake _cameraShake;

    [TabGroup("Settings")]
    [Title("PitchParrySFX")]
    [SerializeField] private float _pitchParryMinSFX = 0f;

    [TabGroup("Settings")]
    [SerializeField] private float _pitchParryMaxSFX = 20f;

    [TabGroup("Settings")]
    [SerializeField] private float _pitchParryGainEachParry = 2f;

    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference _parrySoundEffect;

    [TabGroup("References")]
    [SerializeField] private EventReference _parryReverbSoundEffect;

    [TabGroup("References")]
    [Title("Motor")]
    [SerializeField] private GameObject _playerMotorGO;
    
    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnAttackCollide _onAttackCollide;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnParrySuccess _rseOnParrySuccess;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerHit _rseOnPlayerHit;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerGainConviction _onPlayerGainConviction;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCameraShake _onCameraShake;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnRumbleRequested _rseOnRumbleRequested;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnRumbleStopChannel _rseOnRumbleStopChannel;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCameraFOV rseOnCameraFOV;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CanParry _canParry;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_ParryStartTime _parryStartTime;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_AttackDataInDodgeableArea _attackDataInDodgeableArea;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_AttackCanHitPlayer _attackCanHitPlayer;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerConvictionData _playerConvictionData;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats _playerStats;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_RumbleData _parryRumbleData;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_CameraData ssoCameraData;

    private float _currentPitchParrySFX = 0f;

    private Coroutine coroutineParry = null;

    private void Awake()
    {
        _currentPitchParrySFX = _pitchParryMinSFX;
    }

    private void OnEnable()
    {
        _onAttackCollide.action += ResolveHit;
    }

    private void OnDisable()
    {
        _onAttackCollide.action -= ResolveHit;
    }

    private void ResolveHit(S_StructAttackContact contact)
    {
        var attackData = contact.data;

        if (attackData.attackType == S_EnumEnemyAttackType.Parryable)
        {
            StartCoroutine(IsWithinParryWindowCoroutine((canParry, data) =>
            {
                    if (canParry)
                    {
                        _rseOnParrySuccess.Call(data);
                        //_onPlayerGainConviction.Call(attackData.convictionParryGain);
                        _onCameraShake.Call(_cameraShake);

                        _rseOnRumbleStopChannel.Call(S_EnumRumbleChannel.Parry);
                        _rseOnRumbleRequested.Call(_parryRumbleData.Value);
                        Debug.Log("Parried!");

                        bool isLastAttack = attackData.lastAttack;

                        if (!isLastAttack)
                        {
                            EventInstance _parryEventInstanceSFX;

                            _currentPitchParrySFX += _pitchParryGainEachParry;
                            _currentPitchParrySFX = Mathf.Clamp(_currentPitchParrySFX, _pitchParryMinSFX, _pitchParryMaxSFX);
                            _parryEventInstanceSFX = RuntimeManager.CreateInstance(_parrySoundEffect);
                            _parryEventInstanceSFX.setParameterByName("AttackParried", _currentPitchParrySFX);
                            _parryEventInstanceSFX.start();

                            S_ClassCameraFOV fov = new S_ClassCameraFOV();
                            fov.value = ssoCameraData.Value.fovParry;
                            fov.time = ssoCameraData.Value.fovParrySwitchTime;

                            rseOnCameraFOV.Call(fov);
                        }
                        else
                        {
                            EventInstance _parryReverbEventInstanceSFX;

                            _currentPitchParrySFX += _pitchParryGainEachParry;
                            _currentPitchParrySFX = Mathf.Clamp(_currentPitchParrySFX, _pitchParryMinSFX, _pitchParryMaxSFX);
                            _parryReverbEventInstanceSFX = RuntimeManager.CreateInstance(_parryReverbSoundEffect);
                            _parryReverbEventInstanceSFX.start();
                        }

                        if (coroutineParry != null)
                        {
                            StopCoroutine(coroutineParry);
                            coroutineParry = null;
                        }

                        coroutineParry = StartCoroutine(ResetParryAudio());
                    }
                    else
                    {
                        _rseOnPlayerHit.Call(data);

                        _currentPitchParrySFX = _pitchParryMinSFX;

                        if (coroutineParry != null)
                        {
                            StopCoroutine(coroutineParry);
                            coroutineParry = null;
                        }
                    }
            }, contact));
        }
        else if (attackData.attackType == S_EnumEnemyAttackType.Dodgeable)
        {
            var canHit = _attackCanHitPlayer.Value.ContainsKey(attackData.goSourceId);

            if (canHit) _rseOnPlayerHit.Call(contact);
            else Debug.Log("Dont hit cause dodge perfect");
        }
        else if (attackData.attackType == S_EnumEnemyAttackType.Projectile)
        {
            StartCoroutine(IsWithinParryWindowCoroutine((canParry, data) =>
            {
                if (canParry)
                {
                    _rseOnParrySuccess.Call(contact);
                    //_onPlayerGainConviction.Call(attackData.convictionParryGain);

                    _rseOnRumbleStopChannel.Call(S_EnumRumbleChannel.Parry);
                    _onCameraShake.Call(_cameraShake);

                    _rseOnRumbleRequested.Call(_parryRumbleData.Value);

                    TryReflectProjectile(contact.source);
                    Debug.Log("Parried!");

                    EventInstance _parryEventInstanceSFX;

                    _currentPitchParrySFX += _pitchParryGainEachParry;
                    _currentPitchParrySFX = Mathf.Clamp(_currentPitchParrySFX, _pitchParryMinSFX, _pitchParryMaxSFX);
                    _parryEventInstanceSFX = RuntimeManager.CreateInstance(_parrySoundEffect);
                    _parryEventInstanceSFX.setParameterByName("AttackParried", _currentPitchParrySFX);
                    _parryEventInstanceSFX.start();

                    S_ClassCameraFOV fov = new S_ClassCameraFOV();
                    fov.value = ssoCameraData.Value.fovParry;
                    fov.time = ssoCameraData.Value.fovParrySwitchTime;

                    rseOnCameraFOV.Call(fov);

                    if (coroutineParry != null)
                    {
                        StopCoroutine(coroutineParry);
                        coroutineParry = null;
                    }

                    coroutineParry = StartCoroutine(ResetParryAudio());
                }
                else
                {
                    if (contact.source != null && contact.source.gameObject != null)
                    {
                        Destroy(contact.source.gameObject);
                        _rseOnPlayerHit.Call(contact);

                        _currentPitchParrySFX = _pitchParryMinSFX;

                        if (coroutineParry != null)
                        {
                            StopCoroutine(coroutineParry);
                            coroutineParry = null;
                        }
                    }
                }
            }, contact));
        }
    }

    private IEnumerator ResetParryAudio()
    {
        yield return new WaitForSeconds(0.8f);

        S_ClassCameraFOV fov = new S_ClassCameraFOV();
        fov.value = -ssoCameraData.Value.fovParry;
        fov.time = ssoCameraData.Value.fovParrySwitchTime;

        rseOnCameraFOV.Call(fov);

        _currentPitchParrySFX = _pitchParryMinSFX;
    }

    private IEnumerator IsWithinParryWindowCoroutine(System.Action<bool, S_StructAttackContact> callback, S_StructAttackContact enemyAttackData)
    {
        float t = Time.time; //moment getting hit
        float start = _parryStartTime.Value; //when parry started
        float duration = _playerStats.Value.parryDuration;
        float tolBefore = enemyAttackData.data.parryToleranceBeforeHit;
        float tolAfter = enemyAttackData.data.parryToleranceAfterHit;

        if (t <= start + duration + tolBefore)
        {
            callback(true, enemyAttackData);
            yield break;
        }

        yield return new WaitForSeconds(tolAfter);

        start = _parryStartTime.Value;
        bool valid = t >= start - tolAfter && t <= start + duration + tolBefore;
        callback(valid, enemyAttackData);
    }

    private void TryReflectProjectile(Collider source)
    {
        if (source == null) return;

        if (source.TryGetComponent<I_ReflectableProjectile>(out var proj)) proj.Reflect(_playerMotorGO.transform);
        else
        {
            var p = source.GetComponentInParent<I_ReflectableProjectile>();
            if (p != null) p.Reflect(_playerMotorGO.transform);
        }
    }
}