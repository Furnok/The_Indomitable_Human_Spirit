using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public struct S_StructEnemyAttackData
{
    [HideInInspector] public int attackId;

    [HideInInspector] public int goSourceId;

    [Title("Type")]
    public S_EnumEnemyAttackType attackType;

    [Title("Damage")]
    [ShowIf("isProjectile")]
    [SuffixLabel("s", Overlay = true)]
    public float timeCast;

    [ShowIf("isProjectile")]
    [SuffixLabel("s", Overlay = true)]
    public float timeInterval;

    [ShowIf("isProjectile")]
    public int numberOfProjectiles;

    [Title("Damage")]
    [ShowIf("isTyped")]
    public float damage;

    [Title("KnockBack")]
    [ShowIf("isTyped")]
    [SuffixLabel("s", Overlay = true)]
    public float knockbackHitDuration;

    [ShowIf("isTyped")]
    public float knockbackHitDistance;

    [ShowIf("isTyped")]
    [SuffixLabel("s", Overlay = true)]
    public float knockbackOnParryDuration;

    [ShowIf("isTyped")]
    public float knockbackOnParrryDistance;

    [Title("Parry Tolerance")]
    [ShowIf("isTyped")]
    public float parryToleranceBeforeHit;

    [ShowIf("isTyped")]
    public float parryToleranceAfterHit;

    [Title("Invicibility")]
    [ShowIf("isTyped")]
    [SuffixLabel("s", Overlay = true)]
    public float invicibilityDuration;

    [Title("Conviction")]
    [ShowIf("isTyped")]
    public float convictionReduction;

    [ShowIf("isTyped")]
    public float convictionParryGain;

    [Title("Attack")]
    [ShowIf("isTyped")]
    public bool lastAttack;

    [HideInInspector] public Vector3 attackDirection;

    [HideInInspector] public Vector3 contactPoint;

    private bool isTyped => attackType != S_EnumEnemyAttackType.None;
    private bool isProjectile => attackType == S_EnumEnemyAttackType.Projectile;
}