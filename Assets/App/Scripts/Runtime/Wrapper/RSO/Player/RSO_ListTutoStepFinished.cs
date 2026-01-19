using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RSO_ListTutoStepFinished", menuName = "Data/RSO/Tutorial/RSO_ListTutoStepFinished")]
public class RSO_ListTutoStepFinished : BT.ScriptablesObject.RuntimeScriptableObject<List<TutoStepFinish>> {}

public class TutoStepFinish
{
    public S_EnumTutorialStep Step;
    public bool IsFinished;
}