using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public class S_ClassAnimation
{
    [Title("Animation")]
    public AnimationClip animation = null;

    [Title("Animation Root")]
    [Range(0, 100)] public float rootMotionMultiplier = 1;

    [Title("VFX")]
    public bool showVFXAttackType = false;

    [Title("Data")]
    public S_StructAttackData attackData;
}