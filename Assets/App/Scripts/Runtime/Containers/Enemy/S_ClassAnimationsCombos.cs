using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

[Serializable]
public class S_ClassAnimationsCombos
{
    [Title("Distances")]
    public float distanceMin = 0;

    public float distanceToChase = 0;

    public float distanceToLoseAttack = 0;

    [Title("Tuto")]
    public bool isTutoCombo = false;

    public S_EnumTutorialStep tutoStepToUnlock;

    [Title("Combos")]
    public List<S_ClassAnimation> listAnimationsCombos = new();
}