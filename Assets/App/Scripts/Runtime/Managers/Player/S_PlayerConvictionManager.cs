using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

public class S_PlayerConvictionManager : MonoBehaviour
{
    [TabGroup("References")]
    [SerializeField] private float _soundDelayToMakeConvictionGain;

    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference _convictionGainSoundEffect;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDataLoad rseOnDataLoad;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnHealStart _onHealStart;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerAttackCancel _onPlayerAttackCancel;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerGainConviction _onPlayerGainConviction;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnSpawnProjectile _onSpawnProjectile;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnAttackStartPerformed _onAttackStartPerformed;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerHit _rseOnPlayerHit;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDisplayUIGame _rseOnDisplayUIGame;

    [TabGroup("Inputs")]
    [SerializeField] RSE_OnPlayerLooseConviction _rseOnPlayerLooseConviction;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerConvictionUpdate rseOnPlayerConvictionUpdate;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerAttackSteps _playerAttackSteps;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerConvictionData _playerConvictionData;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats _playerStats;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentConviction _playerCurrentConviction;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_ConsoleCheats _debugPlayer;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_DataSaved rsoDataSaved;

    private Coroutine _convictionConsumptionCoroutine = null;
    private Coroutine _convictionGainOrLossCoroutine = null;

    private float _timerConvictionGainSound = 0f;
    private bool _canPlayConvictionGainSound => _timerConvictionGainSound <= 0f;

    private void Awake()
    {
        _playerCurrentConviction.Value = _playerConvictionData.Value.startConviction;
    }

    private void OnEnable()
    {
        _onHealStart.action += ReduceConvictionOnHealPerformed;
        _onPlayerAttackCancel.action += ReduceConvictionOnAttackCancel;
        _onPlayerGainConviction.action += OnPlayerGainConviction;
        _onSpawnProjectile.action += ReductionConviction;
        _onAttackStartPerformed.action += StopComsuptioncoroutine;
        _rseOnDisplayUIGame.action += ManageConviction;

        rseOnDataLoad.action += SetValueFromData;

        _rseOnPlayerHit.action += ReductionConviction;

        _rseOnPlayerLooseConviction.action += ReductionConviction;
    }

    private void OnDisable()
    {
        _onHealStart.action -= ReduceConvictionOnHealPerformed;
        _onPlayerAttackCancel.action -= ReduceConvictionOnAttackCancel;
        _onPlayerGainConviction.action -= OnPlayerGainConviction;
        _onSpawnProjectile.action -= ReductionConviction;
        _onAttackStartPerformed.action -= StopComsuptioncoroutine;
        _rseOnDisplayUIGame.action -= ManageConviction;

        rseOnDataLoad.action -= SetValueFromData;

        _rseOnPlayerHit.action -= ReductionConviction;

        _rseOnPlayerLooseConviction.action -= ReductionConviction;
    }

    private void Start()
    {
        rseOnPlayerConvictionUpdate.Call(_playerCurrentConviction.Value);
    }

    private void Update()
    {
        if (_debugPlayer.Value.infiniteConviction == true && _playerCurrentConviction.Value != _playerConvictionData.Value.maxConviction)
        {
            StartCoroutine(S_Utils.Delay(0.3f, () =>
            {
                _playerCurrentConviction.Value = _playerConvictionData.Value.maxConviction;
                rseOnPlayerConvictionUpdate.Call(_playerConvictionData.Value.maxConviction);
            }));

            return;
        }

        if (_timerConvictionGainSound > 0f)
        {
            _timerConvictionGainSound -= Time.deltaTime;
        }
    }

    private void ManageConviction(bool value)
    {
        if (value) StartConvitionConsumption();
        else StopComsuptioncoroutine();
    }

    private void SetValueFromData()
    {
        _playerCurrentConviction.Value = rsoDataSaved.Value.conviction;
        rseOnPlayerConvictionUpdate.Call(_playerCurrentConviction.Value);
    }

    private void ReduceConvictionOnHealPerformed()
    {
        var newAmmount = Mathf.Clamp(_playerCurrentConviction.Value - _playerConvictionData.Value.healCost, 0, _playerConvictionData.Value.maxConviction);
        _playerCurrentConviction.Value = newAmmount;
        rseOnPlayerConvictionUpdate.Call(newAmmount);

        if (_debugPlayer.Value.infiniteConviction == true) return;

        DelayWhenConvictionLoss();
    }

    private void ReduceConvictionOnAttackCancel(int stepCancel)
    {
        if (_debugPlayer.Value.infiniteConviction == true) return;
        if (stepCancel == 0) return;

        var currentStep = _playerAttackSteps.Value.Find(x => x.step == stepCancel);

        if (currentStep.step != stepCancel)
        {
            Debug.LogError($"Didn't find the step {currentStep.step} & {stepCancel}");
            return;
        }

        var stepUnder = _playerAttackSteps.Value.Find(x => x.step == currentStep.step - 1);
        var stepUpper = _playerAttackSteps.Value.Find(x => x.step == currentStep.step + 1);

        var differenceWithUpper = Mathf.Abs(stepUpper.ammountConvitionNeeded - currentStep.ammountConvitionNeeded);
        var percentage = (_playerCurrentConviction.Value - currentStep.ammountConvitionNeeded) * 100 / differenceWithUpper;

        var differenceWithUnder = Mathf.Abs(stepUnder.ammountConvitionNeeded - currentStep.ammountConvitionNeeded);

        var newConvictionValue = stepUnder.ammountConvitionNeeded + differenceWithUnder / 100 * percentage;

        _playerCurrentConviction.Value = newConvictionValue;
        rseOnPlayerConvictionUpdate.Call(newConvictionValue);

        DelayWhenConvictionLoss();
    }

    private void OnPlayerGainConviction(float ammountGain)
    {
        var ammount = Mathf.Clamp(ammountGain + _playerCurrentConviction.Value, 0, _playerConvictionData.Value.maxConviction);
        _playerCurrentConviction.Value = ammount;
        rseOnPlayerConvictionUpdate.Call(ammount);

        if(_canPlayConvictionGainSound == true)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(_convictionGainSoundEffect);
            eventInstance.setParameterByName("CurrentConviction", _playerCurrentConviction.Value);
            eventInstance.start();
        }

        _timerConvictionGainSound = _soundDelayToMakeConvictionGain;

        DelayWhenConvictionGain();
    }

    private void StartConvitionConsumption()
    {
        if (_debugPlayer.Value.infiniteConviction == true) return;
        StopComsuptioncoroutine();

        _convictionConsumptionCoroutine = StartCoroutine(S_Utils.Delay(_playerConvictionData.Value.tickIntervalSec, () =>
        {
            ReductionConsumptionOnConsuption(_playerConvictionData.Value.ammountLostOverTick);
            StartConvitionConsumption();
        }));
    }

    private void ReductionConviction(float ammount)
    {
        var newAmmount = Mathf.Clamp(_playerCurrentConviction.Value - ammount, 0, _playerConvictionData.Value.maxConviction);
        _playerCurrentConviction.Value = newAmmount;
        rseOnPlayerConvictionUpdate.Call(newAmmount);

        if (_debugPlayer.Value.infiniteConviction == true) return;

        if (ammount >= 1) DelayWhenConvictionLoss();
        else StartConvitionConsumption();
    }

    private void ReductionConviction(S_StructAttackContact attacqueContactInfo)
    {   
        float ammount = attacqueContactInfo.data.convictionReduction;

        var newAmmount = Mathf.Clamp(_playerCurrentConviction.Value - ammount, 0, _playerConvictionData.Value.maxConviction);
        _playerCurrentConviction.Value = newAmmount;
        rseOnPlayerConvictionUpdate.Call(newAmmount);

        if (_debugPlayer.Value.infiniteConviction == true) return;

        if (ammount >= 1) DelayWhenConvictionLoss();
        else StartConvitionConsumption();
    }

    private void ReductionConsumptionOnConsuption(float ammount)
    {
        var newAmmount = Mathf.Clamp(_playerCurrentConviction.Value - ammount, 0, _playerConvictionData.Value.maxConviction);
        _playerCurrentConviction.Value = newAmmount;
        rseOnPlayerConvictionUpdate.Call(newAmmount);
    }

    private void DelayWhenConvictionLoss()
    {
        StopComsuptioncoroutine();
        if(_convictionGainOrLossCoroutine != null) StopCoroutine(_convictionGainOrLossCoroutine);

        _convictionGainOrLossCoroutine = StartCoroutine(S_Utils.Delay(_playerConvictionData.Value.pauseIntervalAfterLoss, () =>
        {
            if (_playerCurrentConviction.Value > 0) StartConvitionConsumption();
        }));
    }

    private void DelayWhenConvictionGain()
    {
        StopComsuptioncoroutine();
        if (_convictionGainOrLossCoroutine != null) StopCoroutine(_convictionGainOrLossCoroutine);

        _convictionGainOrLossCoroutine = StartCoroutine(S_Utils.Delay(_playerConvictionData.Value.pauseIntervalAfterGained, () =>
        {
            StartConvitionConsumption();
        }));
    }

    private void StopComsuptioncoroutine()
    {
        if (_convictionConsumptionCoroutine != null)
        {
            StopCoroutine(_convictionConsumptionCoroutine);
            _convictionConsumptionCoroutine = null;
        }

        if (_convictionGainOrLossCoroutine != null)
        {
            StopCoroutine(_convictionGainOrLossCoroutine);
            _convictionGainOrLossCoroutine = null;
        }
    }
}