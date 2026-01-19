using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class S_TutoManager : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] S_SerializableDictionary<S_EnumTutorialStep, GameObject> _tutoPrefabToEnumDictionary;
    [SerializeField] RSO_ListTutoStepFinished _tutoStepsFinished;
    [SerializeField] RectTransform _filterConvictionBar;

    //[SerializeField] List<GameObject> _tutoStepPrefabs;
    [Header("Inputs")]
    [SerializeField] RSE_OnRequestStartTutorialStep _onRequestStartTutorialStep;
    [SerializeField] RSE_OnTutorialStepCompleted _onTutorialStepCompleted;
    [SerializeField] RSE_OnRequestAcceptedTutorialStep _onRequestAcceptedTutorialStep;

    [Header("Outputs")]
    [SerializeField] private RSE_OnGamePause _onGamePause;
    [SerializeField] RSE_OnChangeHighlightTarget _onChangeHighlightTarget;
    [SerializeField] RSE_OnChangeActiveStatePanelsFilters _onChangeActiveStatePanelsFilters;

    private Dictionary<S_EnumTutorialStep, TutoStepData> _tutorials = new Dictionary<S_EnumTutorialStep, TutoStepData>();

    private void OnEnable()
    {
        _onRequestStartTutorialStep.action += StartTutorialStep;
        _onTutorialStepCompleted.action += EndTutorialStep;
    }
    private void OnDisable()
    {
        _onRequestStartTutorialStep.action -= StartTutorialStep;
        _onTutorialStepCompleted.action -= EndTutorialStep;

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

        Debug.Log("Tuto steps count: " + _tutoStepsFinished.Value.Count);
    }

    void StartTutorialStep(S_EnumTutorialStep tutoStep)
    {
        //switch (tutoStep)
        //{
        //    case S_EnumTutorialStep.Movement:
        //        StartMovementTuto();
        //        break;
        //    case S_EnumTutorialStep.Dodge:
        //        break;
        //    case S_EnumTutorialStep.Attack:
        //        break;
        //    case S_EnumTutorialStep.Heal:
        //        break;
        //    case S_EnumTutorialStep.Conviction:
        //        break;
        //    case S_EnumTutorialStep.Parry:
        //        StartParryTuto();
        //        break;
        //    case S_EnumTutorialStep.Targeting:
        //        break;
        //    case S_EnumTutorialStep.AttackSignaling:
        //        break;
        //    case S_EnumTutorialStep.Interact:
        //        break;
        //    case S_EnumTutorialStep.SwapTarget:
        //        break;
        //    default:
        //        break;
        //}

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

    //void StartMovementTuto()
    //{
    //    _tutoPrefabToEnumDictionary[S_EnumTutorialStep.Movement].SetActive(true);
    //}

    //void StartParryTuto()
    //{
    //    if (_tutorials.ContainsKey(S_EnumTutorialStep.Parry))
    //    {
    //        _tutorials.TryGetValue(S_EnumTutorialStep.Parry, out TutoStepData stepData);
    //        if (stepData.IsFinished)
    //        {
    //            // Parry tuto already finished, do not show it again
    //            return;
    //        }
    //        else if (_tutoPrefabToEnumDictionary.ContainsKey(S_EnumTutorialStep.Parry))
    //        {
    //            _tutoPrefabToEnumDictionary[S_EnumTutorialStep.None].SetActive(true);
    //        }
    //    }
    //}

    void StartTuto(S_EnumTutorialStep tutoStep)
    {
        if (_tutorials.ContainsKey(tutoStep))
        {
            _tutorials.TryGetValue(tutoStep, out TutoStepData stepData);
            if (stepData.IsFinished == true)
            {
                // Parry tuto already finished, do not show it again
                return;
            }
            else if (_tutoPrefabToEnumDictionary.ContainsKey(tutoStep))
            {
                _tutoPrefabToEnumDictionary[tutoStep].SetActive(true);
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
        var tuto = _tutoStepsFinished.Value.Find(x => x.Step == S_EnumTutorialStep.ParryProjectile && x.IsFinished == true);

        if (tuto != null && tutoStep == S_EnumTutorialStep.ParryProjectile)
        {
            StartCoroutine(S_Utils.Delay(1.0f, () =>
            {
                _onChangeHighlightTarget.Call(_filterConvictionBar);
                _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Conviction);
                _onChangeActiveStatePanelsFilters.Call();
            }));

            StartCoroutine(S_Utils.Delay(1.5f, () =>
            {
                _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Attack);
            }));
        }

        if (tuto != null && tutoStep == S_EnumTutorialStep.Conviction)
        {
            _onChangeActiveStatePanelsFilters.Call();
        }

        if (tuto != null && tutoStep == S_EnumTutorialStep.Attack)
        {
            //StartCoroutine(S_Utils.Delay(1.0f, () =>
            //{
            //    Debug.Log("Starting Heal Tuto");
            //    _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Heal);
            //}));
        }
    }
}

public class TutoStepData
{
    public string Title;
    public string Description;
    public bool IsFinished;
    public VideoClip TutorialVideo;
}