using Sirenix.OdinInspector;
using System;

[Serializable]
public class S_ClassCameraData
{
    [Title("Camera")]
    public float cameraDistanceMinPlayer = 0;

    [Title("Camera Targeting")]
    public float shoulderOffsetAmountPositive = 0;

    public float shoulderOffsetAmountNegative = 0;

    public float maxShoulderOffset = 0;

    public float maxFrontOffset = 0;

    public float maxBackOffset = 0;

    public float maxShoulderDistance = 0;

    [SuffixLabel("°", Overlay = true)]
    public float minVerticalCameraPlayer = 0;

    [SuffixLabel("°", Overlay = true)]
    public float maxVerticalCameraPlayer = 0;

    [SuffixLabel("s", Overlay = true)]
    public float offsetTransitionUpdateTime = 0;

    [SuffixLabel("s", Overlay = true)]
    public float offsetTransitionTime = 0;

    [Title("Camera FOV")]
    public float fovDodge = 0;

    [SuffixLabel("s", Overlay = true)]
    public float fovDodgeSwitchTime = 0;

    public float fovParry = 0;

    [SuffixLabel("s", Overlay = true)]
    public float fovParrySwitchTime = 0;

    public float fovFight = 0;

    [SuffixLabel("s", Overlay = true)]
    public float fovFightSwitchTime = 0;

    [Title("Cinematic")]
    [SuffixLabel("s", Overlay = true)]
    public float holdSkipTime = 0;

    [SuffixLabel("s", Overlay = true)]
    public float startDisplaySkipTime = 0;

    [Title("Player")]
    public float fadeSpeedPlayer = 0;
}