using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class S_TutoManager : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] S_SerializableDictionary<S_EnumTutorialStep, GameObject> _tutoPrefabToEnumDictionary;
    //[SerializeField] List<GameObject> _tutoStepPrefabs;
    [Header("Inputs")]
    [SerializeField] RSE_OnRequestStartTutorialStep _onRequestStartTutorialStep;
    [SerializeField] RSE_OnTutorialStepCompleted _onTutorialStepCompleted;
    [SerializeField] RSE_OnRequestAcceptedTutorialStep _onRequestAcceptedTutorialStep;

    [Header("Outputs")]
    [SerializeField] private RSE_OnGamePause _onGamePause;

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
    }

    private void Awake()
    {
        foreach (var tuto in _tutoPrefabToEnumDictionary)
        {
            _tutorials.Add(tuto.Key, new TutoStepData { IsFinished = false });
        }
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
}

public class TutoStepData
{
    public string Title;
    public string Description;
    public bool IsFinished;
    public VideoClip TutorialVideo;
}