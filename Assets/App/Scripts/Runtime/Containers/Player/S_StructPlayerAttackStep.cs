using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public struct S_StructPlayerAttackStep
{
    [Title("Data")]
    public int step;

    public int multipliers;

    public float ammountConvitionNeeded;

    public float radius;

    [SuffixLabel("s", Overlay = true)]
    public float timeHoldingInput;

    public float speed;

    public Color color;

    public S_StructDataProjectile projectileData;
}