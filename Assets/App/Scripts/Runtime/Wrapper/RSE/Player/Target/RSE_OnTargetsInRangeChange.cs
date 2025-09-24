using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RSE_OnTargetsInRangeChange", menuName = "Data/RSE/Player/Target/OnTargetsInRangeChange")]
public class RSE_OnTargetsInRangeChange : BT.ScriptablesObject.RuntimeScriptableEvent<HashSet<GameObject>>{}