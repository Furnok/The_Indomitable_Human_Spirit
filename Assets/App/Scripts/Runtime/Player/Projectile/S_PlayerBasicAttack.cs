using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class S_PlayerBasicAttack : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Animation")]
    [SerializeField, S_AnimationName] private string _attackParam;

    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference _convictionAccumulationSound;

    [TabGroup("References")]
    [SerializeField] private EventReference _attackPerformedSound;

    [TabGroup("References")]
    [SerializeField] private GameObject _weaponHand;

    [TabGroup("References")]
    [SerializeField] private GameObject _weaponBack;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerAttackInput rseOnPlayerAttack;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerGettingHit _rseOnPlayerGettingHit;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerAttackInputCancel _onPlayerAttackInputCancel;

    [TabGroup("Inputs")]
    [SerializeField] private RSO_GameInPause _rsoGameInPause;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerAttackUpgradeInput _rseOnPlayerAttackUpgradeInput;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnSpawnProjectile rseOnSpawnProjectile;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerAddState _onPlayerAddState;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationBoolValueChange rseOnAnimationBoolValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerAttackCancel _onPlayerAttackCancel;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAttackStartPerformed _onAttackStartPerformed;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnSendConsoleMessage rseOnSendConsoleMessage;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnRumbleRequested _rseOnRumbleRequested;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnRumbleStopChannel _rseOnRumbleStopChannel;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerIsTargeting rsoPlayerIsTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentState _playerCurrentState;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentConviction _playerCurrentConviction;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PreconsumedConviction _preconsumedConviction;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CurrentChargeStep _rsoCurrentChargeStep;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStateTransitions _playerStateTransitions;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerConvictionData _playerConvictionData;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats _playerStats;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerAttackSteps _playerAttackSteps;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_AnimationTransitionDelays _animationTransitionDelays;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_RumbleData _chargeAttackRumbleData;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_RumbleData _chargeAttackBetweenStepRumbleData;

    private Coroutine _attackChargeCoroutine = null;

    private bool _isHolding = false;
    private bool _wasCanceled = false;

    private List<S_StructPlayerAttackStep> _steps = new();
    private List<float> _stepTimes = new();
    private List<float> _stepConvThresholds = new();
    private float _reservedConviction = 0;
    private int _lastCompletedStep = 0;

    EventInstance _convictionAccumulationInstance;

    private Coroutine _weaponAttackCoroutine = null;

    private int _currenStepAttack = 0;

    private void Awake()
    {
        _preconsumedConviction.Value = 0;
        _rsoCurrentChargeStep.Value = 0;

        _steps = _playerAttackSteps.Value?.OrderBy(s => s.step).ToList() ?? new List<S_StructPlayerAttackStep>();
        if (_steps.Count == 0)
        {
            Debug.LogWarning("No attack steps configured");
            return;
        }

        var dup = _steps.GroupBy(s => s.step).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (dup.Count > 0) Debug.LogError("There is duplicate step in the attack steps SSO ");

        foreach (var s in _steps)
        {
            _stepTimes.Add(s.timeHoldingInput);
            _stepConvThresholds.Add(s.ammountConvitionNeeded);
        }

        _weaponHand.SetActive(false);
        _weaponBack.SetActive(true);
    }

    private void OnEnable()
    {
        rseOnPlayerAttack.action += OnPlayerAttackInput;
        _rseOnPlayerGettingHit.action += OnHitCancel;
        _onPlayerAttackInputCancel.action += OnAttackReleased;
        _rseOnPlayerAttackUpgradeInput.action += OnPlayerAttackUpgradeInput;

        _rsoGameInPause.onValueChanged += OnGamePause;
    }

    private void OnDisable()
    {
        rseOnPlayerAttack.action -= OnPlayerAttackInput;
        _rseOnPlayerGettingHit.action -= OnHitCancel;
        _onPlayerAttackInputCancel.action -= OnAttackReleased;
        _rseOnPlayerAttackUpgradeInput.action -= OnPlayerAttackUpgradeInput;

        _rsoGameInPause.onValueChanged -= OnGamePause;

        _rsoCurrentChargeStep.Value = 0;
    }

    private void OnGamePause(bool newPauseValue)
    {
        if (newPauseValue == true)
        {
            OnAttackReleased();
        }
    }

    private void OnPlayerAttackUpgradeInput()
    {
        if(_isHolding == false) return;


        S_StructPlayerAttackStep stepConvition = _playerAttackSteps.Value.Where(x => x.step == _currenStepAttack + 1).FirstOrDefault();

        if (!stepConvition.Equals(default(S_StructPlayerAttackStep)) && _playerCurrentConviction.Value >= stepConvition.ammountConvitionNeeded)
        {
            _currenStepAttack ++;
            _rsoCurrentChargeStep.Value = _currenStepAttack;

            _reservedConviction = stepConvition.ammountConvitionNeeded;

            PublishPreconsume(_reservedConviction);

            var rumbleBetweenStepData = _chargeAttackBetweenStepRumbleData.Value;
            _rseOnRumbleRequested.Call(rumbleBetweenStepData);
        }
    }

    private void OnPlayerAttackInput()
    {
        if (_playerStateTransitions.Value.CanTransition(_playerCurrentState.Value, S_EnumPlayerState.Attacking) == false ||_playerCurrentConviction.Value < 1) return;

        _weaponHand.SetActive(true);
        _weaponBack.SetActive(false);

        _onPlayerAddState.Call(S_EnumPlayerState.Attacking);
        _onAttackStartPerformed.Call();
        rseOnAnimationBoolValueChange.Call(_attackParam, true);

        /*
        _attackCoroutine = StartCoroutine(S_Utils.Delay(_animationTransitionDelays.Value.attackStartupDelay, () =>
        {
            if (CanGoUpperState() == true)
            {
                StartStepDuration();
            }
        }));
        */

        _isHolding = true;
        _wasCanceled = false;
        _reservedConviction = 0f;
        _lastCompletedStep = 0;
        _rsoCurrentChargeStep.Value = 0;
        _currenStepAttack = 0;

        PublishPreconsume(_reservedConviction);

        //if (_attackChargeCoroutine != null) StopCoroutine(_attackChargeCoroutine);
        //_attackChargeCoroutine = StartCoroutine(ChargeRoutine());

        var rumbleData = _chargeAttackRumbleData.Value;
        rumbleData.Duration = 300f;
        _rseOnRumbleRequested.Call(rumbleData);

        if (!_convictionAccumulationSound.IsNull)
        {
            _convictionAccumulationInstance = RuntimeManager.CreateInstance(_convictionAccumulationSound);
            _convictionAccumulationInstance.setParameterByName("ConvictionAccumulated", _reservedConviction / _playerConvictionData.Value.maxConviction);
            _convictionAccumulationInstance.start();
        }

        rseOnSendConsoleMessage.Call("Player Start Attacking!");
    }

    private void OnAttackReleased()
    {
        _isHolding = false;
        rseOnAnimationBoolValueChange.Call(_attackParam, false);

        FinalizeAttack();

        rseOnSendConsoleMessage.Call("Player Launch Attack!");

        _rseOnRumbleStopChannel.Call(S_EnumRumbleChannel.ChargeAttack);

        if (_weaponAttackCoroutine != null) StopCoroutine(_weaponAttackCoroutine);

        _weaponAttackCoroutine = StartCoroutine(S_Utils.Delay(0.3f, () =>
        {
            _weaponHand.SetActive(false);
            _weaponBack.SetActive(true);
        }));
    }

    /*
    private void StartStepDuration()
    {
        float timeWait = _playerAttackSteps.Value.Find(x => x.step == _currenStepAttack + 1).timeHoldingInput - _playerAttackSteps.Value.Find(x => x.step == _currenStepAttack).timeHoldingInput;

        _attackChargeCoroutine = StartCoroutine(S_Utils.Delay(timeWait, () =>
        {
            _currenStepAttack++;

            if (CanGoUpperState() == true)
            {
                StartStepDuration();
            }
        }));
    }

    private bool CanGoUpperState()
    {
        var lastStep = _playerAttackSteps.Value.OrderByDescending(x => x.step).First().step;
        bool existNextStep = _playerAttackSteps.Value.Exists(x => x.step == _currenStepAttack + 1);
        bool canGoUpperState = false;

        if (existNextStep == true)
        {
            var nextStep = _playerAttackSteps.Value.Find(x => x.step == _currenStepAttack + 1);
            canGoUpperState = _playerCurrentConviction.Value >= nextStep.ammountConvitionNeeded;

            if (_currenStepAttack < lastStep && canGoUpperState == true)
            {
                return true;
            }
        }

        return false;
    }
    */

    private IEnumerator ChargeRoutine()
    {
        yield return new WaitForSeconds(_animationTransitionDelays.Value.attackStartupDelay);

        if (!_convictionAccumulationSound.IsNull)
        {
            _convictionAccumulationInstance = RuntimeManager.CreateInstance(_convictionAccumulationSound);
            _convictionAccumulationInstance.setParameterByName("ConvictionAccumulated", _reservedConviction / _playerConvictionData.Value.maxConviction);
            _convictionAccumulationInstance.start();
        }

        float started = Time.time;
        float pauseCarry = 0f;

        _rseOnRumbleStopChannel.Call(S_EnumRumbleChannel.ChargeAttack);

        var rumbleData = _chargeAttackRumbleData.Value;
        rumbleData.Duration = _steps[_lastCompletedStep].timeHoldingInput;
        _rseOnRumbleRequested.Call(rumbleData);

        // i go in each attackSteps
        for (int i = 1; i < _stepTimes.Count; i++)
        {
            if (!_isHolding || _wasCanceled) break;

            float tStart = _stepTimes[i - 1];
            float tEnd = _stepTimes[i];

            float convStart = _stepConvThresholds[i - 1];
            float convEnd = _stepConvThresholds[i];

            float cap = _playerCurrentConviction.Value;
            float targetEnd = Mathf.Min(convEnd, cap);

            _rseOnRumbleStopChannel.Call(S_EnumRumbleChannel.ChargeAttack);

            rumbleData = _chargeAttackRumbleData.Value;
            rumbleData.Duration = _steps[_lastCompletedStep + 1].timeHoldingInput;
            _rseOnRumbleRequested.Call(rumbleData);

            _rsoCurrentChargeStep.Value = _lastCompletedStep + 1;

            if (targetEnd <= convStart + 0.0001f)
            {
                _reservedConviction = Mathf.Min(cap, Mathf.Max(_reservedConviction, convStart));
                PublishPreconsume(_reservedConviction);
                while (_isHolding && !_wasCanceled) yield return null;
                break;
            }

            while (_isHolding && !_wasCanceled && _reservedConviction < targetEnd - 0.0001f)
            {
                float elapsed = (Time.time - started - pauseCarry);
                float segT = Mathf.InverseLerp(tStart, tEnd, Mathf.Clamp(elapsed, tStart, tEnd));

                _reservedConviction = Mathf.Lerp(convStart, targetEnd, segT);
                PublishPreconsume(_reservedConviction);

                yield return null;
            }

            if (!_isHolding || _wasCanceled) break;

            _reservedConviction = Mathf.Min(_reservedConviction, targetEnd);
            PublishPreconsume(_reservedConviction);

            bool fullStepReached = Mathf.Abs(_reservedConviction - convEnd) <= 0.0001f;
            if (fullStepReached)
            {
                _lastCompletedStep = i;

                int lastIndex = _stepTimes.Count - 1;
                bool isFirstBoundaryFromZero = i == 1 && Mathf.Approximately(_stepTimes[0], 0f) && Mathf.Approximately(_stepConvThresholds[0], 0f);

                bool isLastBoundary = (i == lastIndex);

                float pause = (/*!isFirstBoundaryFromZero && */!isLastBoundary) ? Mathf.Max(0f, _playerStats.Value.timeWaitBetweenSteps) : 0f;


                if (pause > 0f)
                {
                    _rseOnRumbleStopChannel.Call(S_EnumRumbleChannel.ChargeAttack);

                    var rumbleBetweenStepData = _chargeAttackBetweenStepRumbleData.Value;
                    rumbleBetweenStepData.Duration = _playerStats.Value.timeWaitBetweenSteps;
                    _rseOnRumbleRequested.Call(rumbleData);

                    float endPause = Time.time + pause;
                    while (Time.time < endPause)
                    {
                        if (!_isHolding || _wasCanceled) break;
                        yield return null;
                    }
                    pauseCarry += pause;
                }
            }
        }

        if (!_wasCanceled)
        {
            _rseOnRumbleStopChannel.Call(S_EnumRumbleChannel.ChargeAttack);

            rumbleData = _chargeAttackRumbleData.Value;
            rumbleData.Duration = 300f;
            _rseOnRumbleRequested.Call(rumbleData);

            while (_isHolding && !_wasCanceled)
                yield return null;

            if (!_wasCanceled)
                FinalizeAttack();
        }
    }

    private void FinalizeAttack()
    {
        if (_convictionAccumulationInstance.isValid())
        {
            _convictionAccumulationInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _convictionAccumulationInstance.release();
            _convictionAccumulationInstance = default;
        }

        _attackChargeCoroutine = StartCoroutine(S_Utils.Delay(_playerStats.Value.delayBeforeCastAttack, () =>
        {
            _rseOnRumbleStopChannel.Call(S_EnumRumbleChannel.ChargeAttack);

            if(_attackPerformedSound.IsNull == false && _currenStepAttack != 0)
            {
                EventInstance instance = RuntimeManager.CreateInstance(_attackPerformedSound);
                instance.start();
            }

            rseOnAnimationBoolValueChange.Call(_attackParam, false);
            var value = Mathf.FloorToInt(_reservedConviction);

            rseOnSpawnProjectile.Call(value);

            _reservedConviction = 0f;
            PublishPreconsume(_reservedConviction);

            _attackChargeCoroutine = StartCoroutine(S_Utils.Delay(_animationTransitionDelays.Value.attackRecoveryDelay, () =>
            {
                _onPlayerAddState.Call(S_EnumPlayerState.None);
                if (_attackChargeCoroutine == null) return;
                _attackChargeCoroutine = null;
            }));
        }));
    }

    private void OnHitCancel()
    {
        var currentConviction = _playerCurrentConviction.Value;
        var stepsUpperCurrentConviction = _playerAttackSteps.Value.FindAll(x => x.ammountConvitionNeeded <= currentConviction);
        var currentStep = stepsUpperCurrentConviction.OrderByDescending(x => x.ammountConvitionNeeded).First().step;

        _wasCanceled = true;
        _isHolding = false;

        _rsoCurrentChargeStep.Value = 0;

        if (_attackChargeCoroutine != null)
        {
            StopCoroutine(_attackChargeCoroutine);
            _onPlayerAttackCancel.Call(currentStep);
        }

        rseOnAnimationBoolValueChange.Call(_attackParam, false);

        _reservedConviction = 0f;

        if (_convictionAccumulationInstance.isValid())
        {
            _convictionAccumulationInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _convictionAccumulationInstance.release();
            _convictionAccumulationInstance = default;
        }

        _rseOnRumbleStopChannel.Call(S_EnumRumbleChannel.ChargeAttack);

        PublishPreconsume(_reservedConviction);
    }

    private void PublishPreconsume(float value)
    {
        if (_preconsumedConviction != null)
        {
            _preconsumedConviction.Value = value;
        }
    }
}