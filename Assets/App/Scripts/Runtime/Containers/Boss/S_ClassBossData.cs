using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class S_ClassBossData
{
    [Title("General Settings")]
    public float health = 0;

    public float walkSpeed = 0;

    public float runSpeed = 0;

    public S_EnumBossPhaseState phaseState;

    public float yHeadRemove = 0;

    [Title("UI")]
    [ShowIf("isPhase1")]
    [SuffixLabel("s", Overlay = true)]
    public float timeDisplay;

    [Title("Stun")]
    [SuffixLabel("s", Overlay = true)]
    public float stunDuration = 0;

    [Title("Chase")]
    public float distanceToChase = 0;

    [SuffixLabel("%", Overlay = true)]
    public float distanceToRun = 0;

    [Title("Strafe")]
    [SuffixLabel("°", Overlay = true)]
    public float strafeRotationMin = 0;

    [SuffixLabel("°", Overlay = true)]
    public float strafeRotationMax = 0;

    [Title("Combat")]
    public float initialBossDifficultyLevel;

    public float maxDifficultyLevel;

    [SuffixLabel("s", Overlay = true)]
    public float difficultyGainPerSecond;

    public float difficultyLoseWhenPlayerHit;

    public float difficultyScore;

    public float frequencyScore;

    public float synergieScore;

    //[SuffixLabel("s", Overlay = true)]
    //public float minTimeChooseAttack;

    //[SuffixLabel("s", Overlay = true)]
    //public float maxTimeChooseAttack;

    [SuffixLabel("s", Overlay = true)]
    public float rotationTime = 0;

    [TitleGroup("Jump & Fly")]
    [Title("Gathering")]
    [ShowIf("isPhase2")]
    public float jumpDistanceGathering;

    [ShowIf("isPhase2")]
    public float jumpPowerGathering;

    [ShowIf("isPhase2")]
    public float jumpDurationGathering;

    [Title("PingPong")]
    [ShowIf("isPhase2")]
    public float jumpDistancePingPong;

    [ShowIf("isPhase2")]
    public float jumpPowerPingPong;

    [ShowIf("isPhase2")]
    public float jumpDurationPingPong;

    [ShowIf("isPhase2")]
    public float flyPowerPingPong;

    [ShowIf("isPhase2")]
    public float flyDurationPingPong;

    [Title("Balls")]
    [ShowIf("isPhase2")]
    public float flyPowerBalls;

    [ShowIf("isPhase2")]
    public float flyDurationBalls;

    [Title("Wings Of Hell")]
    [ShowIf("isPhase2")]
    public float flyPowerWings;

    [ShowIf("isPhase2")]
    public float flyDurationWings;



    [Title("Animations")]
    public AnimatorOverrideController controllerOverride = null;

    public List<S_ClassBossAttack> listAttack = new();

    private bool isPhase1 => phaseState == S_EnumBossPhaseState.Phase1;
    private bool isPhase2 => phaseState == S_EnumBossPhaseState.Phase2;
}