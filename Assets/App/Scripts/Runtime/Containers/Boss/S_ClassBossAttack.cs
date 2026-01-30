using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

[Serializable]
public class S_ClassBossAttack
{
    [Title("Attack Settings")]
    public string attackName = "";

    public bool isSpecialAttack = false;

    [SuffixLabel("s", Overlay = true)]
    public float timeAfterAttack = 0;

    public bool isAttackDistance = false;

    [SuffixLabel("%", Overlay = true)]
    public float pvBossUnlock = 0;

    public float difficultyLevel = 0;

    public float distanceMin = 0;

    public float distanceToChase = 0;

    public float distanceToLoseAttack = 0;

    [Title("Combo Settings")]
    public List<S_ClassAnimation> listComboData = new();
}