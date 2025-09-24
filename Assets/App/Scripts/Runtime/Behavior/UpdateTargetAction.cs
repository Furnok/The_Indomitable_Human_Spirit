using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update Target", story: "Update [Target] Value [RSO_TargetValue]", category: "Action", id: "932e8c774f7cf98c9d51a836d321d15c")]
public partial class UpdateTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<RSO_TargetValue> RSO_TargetValue;
    protected override Status OnUpdate()
    {
        Target.Value = RSO_TargetValue.Value.Value;
        return Target.Value == null ? Status.Failure : Status.Success;
    }
}

