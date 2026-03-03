using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public struct S_StructDataProjectile
{
    [Title("Arc Settings")]
    public float arcHeightMultiplier;

    public float arcDirection;

    public bool randomizeArc;

    [Range(-5, 5)] public float arcRandomDirectionMin;

    [Range(-5, 5)] public float arcRandomDirectionMax;

    [Title("Times")]
    [SuffixLabel("s", Overlay = true)]
    public float travelTime;

    [SuffixLabel("s", Overlay = true)]
    public float lifeTime;

    [SuffixLabel("s", Overlay = true)]
    public float timeReduce;

    [Title("Curve")]
    public AnimationCurve speedAnimationCurve;

    [Title("Reflect")]
    public float reflectDmgMul;

    public int reflectMax;
}