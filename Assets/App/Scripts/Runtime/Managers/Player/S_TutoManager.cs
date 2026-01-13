using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class S_TutoManager : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    [Header("Inputs")]
    [SerializeField] RSE_OnRequestStartTutorialStep _onRequestStartTutorialStep;
    [SerializeField] RSE_OnTutorialStepCompleted _onTutorialStepCompleted;

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

    void StartTutorialStep(S_EnumTutorialStep tutoStep)
    {
        switch (tutoStep)
        {
            case S_EnumTutorialStep.Movement:
                StartMovementTuto();
                break;
            case S_EnumTutorialStep.Dodge:
                break;
            case S_EnumTutorialStep.Attack:
                break;
            case S_EnumTutorialStep.Health:
                break;
            case S_EnumTutorialStep.Conviction:
                break;
            case S_EnumTutorialStep.Parry:
                StartParryTuto();
                break;
            case S_EnumTutorialStep.Targeting:
                break;
            case S_EnumTutorialStep.AttackSignaling:
                break;
            case S_EnumTutorialStep.Interact:
                break;
            default:
                break;
        }
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
    }

    void StartMovementTuto()
    {
    }

    void StartParryTuto()
    {
        _onGamePause.Call(true);
    }
}

public struct TutoStepData
{
    public string Title;
    public string Description;
    public bool IsFinished;
    public VideoClip TutorialVideo;
}