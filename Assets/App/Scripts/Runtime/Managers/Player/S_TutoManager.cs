using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class S_TutoManager : MonoBehaviour
{
    //[Header("Settings")]
    //[SerializeField] float _looseHealthStartTuto = 30f;

    [Header("References")]
    [SerializeField] S_SerializableDictionary<S_EnumTutorialStep, GameObject> _tutoPrefabToEnumDictionary;
    [SerializeField] RSO_ListTutoStepFinished _tutoStepsFinished;
    [SerializeField] RectTransform _filterConvictionBar;

    //[SerializeField] List<GameObject> _tutoStepPrefabs;
    [Header("Inputs")]
    [SerializeField] RSE_OnRequestStartTutorialStep _onRequestStartTutorialStep;
    [SerializeField] RSE_OnTutorialStepCompleted _onTutorialStepCompleted;
    [SerializeField] RSE_OnRequestAcceptedTutorialStep _onRequestAcceptedTutorialStep;
    [SerializeField] RSO_SettingsSaved _rsoSettingsSaved;
    [SerializeField] RSO_Device _rsoDevice;
    [SerializeField] RSO_HasEnterAreaTuto _rsoHasEnterAreaTuto;

    [SerializeField] private RSE_OnTargetsInRangeChange rseOnTargetsInRangeChange;
    [SerializeField] private SSO_PlayerAttackSteps _playerAttackSteps;
    [SerializeField] private RSO_PlayerCurrentConviction _playerCurrentConviction;

    [Header("Outputs")]
    [SerializeField] private RSE_OnGamePause _onGamePause;
    [SerializeField] RSE_OnChangeHighlightTarget _onChangeHighlightTarget;
    [SerializeField] RSE_OnChangeActiveStatePanelsFilters _onChangeActiveStatePanelsFilters;
    [SerializeField] RSE_OnPlayerHealthReduced _onPlayerHealthReduced;
    [SerializeField] private RSE_OnPlayerGainConviction _onPlayerGainConviction;
    [SerializeField] private RSE_OnGameInputEnabled rseOnGameActionInputEnabled;

    [SerializeField] private RSE_OnPlayerTargeting rseOnPlayerTargeting;
    [SerializeField] private RSO_CurrentTarget rsoCurrentTarget;


    private HashSet<GameObject> targetsPossible = new();

    private Dictionary<S_EnumTutorialStep, TutoStepData> _tutorials = new Dictionary<S_EnumTutorialStep, TutoStepData>();

    private int _parryCountDupplicate = 0;
    private int _dodgeCountDupplicate = 0;
    private int _targetCountDupplicate = 0;


    private void OnEnable()
    {
        _onRequestStartTutorialStep.action += StartTutorialStep;
        _onTutorialStepCompleted.action += EndTutorialStep;

        _playerCurrentConviction.onValueChanged += OnConvictionChange;
        rseOnTargetsInRangeChange.action += OnChangeTargetsPosible;

        _rsoHasEnterAreaTuto.Value = false;
    }
    private void OnDisable()
    {
        _onRequestStartTutorialStep.action -= StartTutorialStep;
        _onTutorialStepCompleted.action -= EndTutorialStep;

        _playerCurrentConviction.onValueChanged -= OnConvictionChange;
        rseOnTargetsInRangeChange.action -= OnChangeTargetsPosible;

        if (_tutoStepsFinished.Value != null)
            _tutoStepsFinished.Value.Clear();
    }

    private void Awake()
    {
        _tutoStepsFinished.Value = new List<TutoStepFinish>();
        _tutoStepsFinished.Value.Clear();

        foreach (var tuto in _tutoPrefabToEnumDictionary)
        {
            _tutorials.Add(tuto.Key, new TutoStepData { IsFinished = false });

            _tutoStepsFinished.Value.Add(new TutoStepFinish
            {
                Step = tuto.Key,
                IsFinished = false
            });
        }
    }

    void OnConvictionChange(float conviction)
    {
        //if(_rsoSettingsSaved.Value.activateTuto == false) return;

        //S_StructPlayerAttackStep stepConvition = _playerAttackSteps.Value.Where(x => x.step == 1).FirstOrDefault();

        //if (!stepConvition.Equals(default(S_StructPlayerAttackStep)) && _playerCurrentConviction.Value >= stepConvition.ammountConvitionNeeded)
        //{
        //    if(targetsPossible.Count > 0)
        //    {
        //        if(rsoCurrentTarget.Value == null)
        //        {
        //            rseOnPlayerTargeting.Call();
        //            _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Attack);
        //        }
        //        else
        //        {
        //            _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Attack);
        //        }
        //    }
        //}
    }

    private void OnChangeTargetsPosible(HashSet<GameObject> targetsList)
    {
        targetsPossible = targetsList;
    }

    private void Start()
    {
        //if (_rsoSettingsSaved.Value.activateTuto == true)
        //{
        //    _onPlayerHealthReduced.Call(_looseHealthStartTuto);
        //}
    }

    void StartTutorialStep(S_EnumTutorialStep tutoStep)
    {
        StartTuto(tutoStep);
    }

    void EndTutorialStep(S_EnumTutorialStep tutoStep)
    {
        _onGamePause.Call(false);
        if (_tutorials.ContainsKey(tutoStep))
        {
            _tutorials.TryGetValue(tutoStep, out TutoStepData stepData);
            stepData.IsFinished = true;

            _tutoStepsFinished.Value.Find(x => x.Step == tutoStep).IsFinished = true;
            StepFinished(tutoStep);
        }
        else
        {
            _tutorials.Add(tutoStep, new TutoStepData { IsFinished = true });
        }

        DisableTutoGO();
    }

    void StartTuto(S_EnumTutorialStep tutoStep)
    {
        if(_rsoHasEnterAreaTuto.Value == false && tutoStep == S_EnumTutorialStep.ParryProjectile) return;
        if (_rsoSettingsSaved.Value.activateTuto == false) return;
        //Debug.Log("Start tuto step : " + tutoStep);

        if (tutoStep == S_EnumTutorialStep.Parry || tutoStep == S_EnumTutorialStep.Dodge)
        {
            _tutorials.TryGetValue(tutoStep, out TutoStepData stepData);
            if (rsoCurrentTarget.Value == null && targetsPossible.Count > 0 && (stepData.IsFinished == false))
            {
                rseOnPlayerTargeting.Call();
            }
        }

        if (_tutorials.ContainsKey(tutoStep))
        {
            _tutorials.TryGetValue(tutoStep, out TutoStepData stepData);
            if (stepData.IsFinished == true || tutoStep == S_EnumTutorialStep.Parry && _parryCountDupplicate >= 3)
            {
                // Parry tuto already finished, do not show it again
                return;
            }
            else if (_tutoPrefabToEnumDictionary.ContainsKey(tutoStep))
            {
                if(tutoStep == S_EnumTutorialStep.Parry && _parryCountDupplicate >= 1)
                {
                    _tutoPrefabToEnumDictionary[S_EnumTutorialStep.ParryDuplicate].SetActive(true);
                }
                else if(tutoStep == S_EnumTutorialStep.Dodge && _dodgeCountDupplicate >= 1)
                {
                    _tutoPrefabToEnumDictionary[S_EnumTutorialStep.DodgeDuplicate].SetActive(true);
                }
                else
                {
                    _tutoPrefabToEnumDictionary[tutoStep].SetActive(true);
                }

                _onGamePause.Call(true);
                _onRequestAcceptedTutorialStep.Call(tutoStep);
            }
        }
        else if(tutoStep == S_EnumTutorialStep.None)
        {
            _onRequestAcceptedTutorialStep.Call(tutoStep);
        }
    }

    void DisableTutoGO()
    {
        foreach (var tuto in _tutoPrefabToEnumDictionary)
        {
            tuto.Value.SetActive(false);
        }
    }

    void StepFinished(S_EnumTutorialStep tutoStep)
    {
        var tuto = _tutoStepsFinished.Value.Find(x => x.Step == tutoStep && x.IsFinished == true);

        _onChangeHighlightTarget.Call(_filterConvictionBar);

        //if (tuto != null && tutoStep == S_EnumTutorialStep.ParryProjectile)
        //{
        //    StartCoroutine(S_Utils.Delay(1.0f, () =>
        //    {
        //        _onChangeHighlightTarget.Call(_filterConvictionBar);
        //        _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Conviction);
        //        _onChangeActiveStatePanelsFilters.Call();
        //    }));

        //    StartCoroutine(S_Utils.Delay(1.5f, () =>
        //    {
        //        _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Attack);
        //    }));
        //}

        //if (tuto != null && tutoStep == S_EnumTutorialStep.Conviction)
        //{
        //    _onChangeActiveStatePanelsFilters.Call();
        //}

        //if (tuto != null && tutoStep == S_EnumTutorialStep.Attack)
        //{

        //    StartCoroutine(S_Utils.Delay(1.0f, () =>
        //    {
        //        _onPlayerGainConviction.Call(40f);

        //        _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Heal);
        //    }));
        //}

        if (tuto != null && tutoStep == S_EnumTutorialStep.Heal)
        {
            StartCoroutine(S_Utils.Delay(0.2f, () =>
            {
                rseOnGameActionInputEnabled.Call();
            }));
        }

        if (tuto != null && tutoStep == S_EnumTutorialStep.Parry)
        {
            _parryCountDupplicate++;
            if (_parryCountDupplicate >= 3)
            {
                _tutorials.TryGetValue(tutoStep, out TutoStepData stepData);

                stepData.IsFinished = true;
            }
            else
            {
                _tutorials.TryGetValue(tutoStep, out TutoStepData stepData);
                _onRequestAcceptedTutorialStep.Call(tutoStep);

                stepData.IsFinished = false;
            }
        }

        if (tuto != null && tutoStep == S_EnumTutorialStep.Dodge)
        {
            _dodgeCountDupplicate++;
            if (_dodgeCountDupplicate >= 2)
            {
                _tutorials.TryGetValue(tutoStep, out TutoStepData stepData);

                stepData.IsFinished = true;
            }
            else
            {
                _tutorials.TryGetValue(tutoStep, out TutoStepData stepData);

                stepData.IsFinished = false;
                _onRequestAcceptedTutorialStep.Call(tutoStep);


            }
        }

        if (tuto != null && tutoStep == S_EnumTutorialStep.Parry && _parryCountDupplicate >= 3)
        {
            StartCoroutine(S_Utils.Delay(0.5f, () =>
            {
                _onChangeActiveStatePanelsFilters.Call();

                if (targetsPossible.Count > 0)
                {
                    if (rsoCurrentTarget.Value == null)
                    {
                        rseOnPlayerTargeting.Call();
                        _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Attack);
                    }
                    else
                    {
                        _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Attack);
                    }
                }
            }));
        }

        if (tuto != null && tutoStep == S_EnumTutorialStep.Attack)
        {
            _onChangeActiveStatePanelsFilters.Call();
        }

        //if (tuto != null && tutoStep == S_EnumTutorialStep.Parry)
        //{
        //    StartCoroutine(S_Utils.Delay(0.5f, () =>
        //    {
        //        _onChangeActiveStatePanelsFilters.Call();

        //        _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Conviction);
        //    }));
        //}

        if (tuto != null && tutoStep == S_EnumTutorialStep.Conviction)
        {
            _onChangeActiveStatePanelsFilters.Call();
        }

        if (tuto != null && tutoStep == S_EnumTutorialStep.Targeting)
        {
            _targetCountDupplicate++;
            if (_targetCountDupplicate >= 2)
            {
                _tutorials.TryGetValue(tutoStep, out TutoStepData stepData);

                stepData.IsFinished = true;
            }
            else
            {
                _tutorials.TryGetValue(tutoStep, out TutoStepData stepData);

                stepData.IsFinished = false;
            }

            StartCoroutine(S_Utils.Delay(0.1f, () =>
            {
                if (rsoCurrentTarget.Value == null && targetsPossible.Count > 0)
                {
                    rseOnPlayerTargeting.Call();
                }
            }));
        }

        //if (tuto != null && tutoStep == S_EnumTutorialStep.Targeting)
        //{
        //    StartCoroutine(S_Utils.Delay(0.1f, () =>
        //    {
        //        _onRequestStartTutorialStep.Call(S_EnumTutorialStep.SwapTarget);
        //    }));
        //}
    }
}

public class TutoStepData
{
    public string Title;
    public string Description;
    public bool IsFinished;
    public VideoClip TutorialVideo;
}