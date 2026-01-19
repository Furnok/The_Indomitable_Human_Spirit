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

    public float shoulderOffsetDistanceMulti = 0;

    [SuffixLabel("°", Overlay = true)]
    public float minVerticalCameraPlayer = 0;

    [SuffixLabel("°", Overlay = true)]
    public float maxVerticalCameraPlayer = 0;

    [SuffixLabel("s", Overlay = true)]
    public float offsetTransitionTime = 0f;

    [Title("Cinematic")]
    [SuffixLabel("s", Overlay = true)]
    public float holdSkipTime = 0;

    [SuffixLabel("s", Overlay = true)]
    public float StartDisplaySkipTime = 0;

    [Title("Player")]
    public float fadeSpeedPlayer = 0;
}