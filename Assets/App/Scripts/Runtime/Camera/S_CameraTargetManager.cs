using Unity.Cinemachine;
using UnityEngine;

public class S_CameraTargetManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineTargetGroup targetGroupe;

    [Header("Input")]
    [SerializeField] private RSE_AddTargetCenter rseAddTargetCenter;
    [SerializeField] private RSE_RemoveTargetCenter rseRemoveTargetCenter;

    private void OnEnable()
    {
        rseAddTargetCenter.action += AddTarget;
        rseRemoveTargetCenter.action += RemoveTarget;
    }

    private void OnDisable()
    {
        rseAddTargetCenter.action -= AddTarget;
        rseRemoveTargetCenter.action -= RemoveTarget;
    }

    private void AddTarget(GameObject target)
    {
        if (targetGroupe.FindMember(target.transform) == -1)
        {
            targetGroupe.AddMember(target.transform, 1f, 1f);
        }
    }

    private void RemoveTarget(GameObject target)
    {
        if (targetGroupe.FindMember(target.transform) >= 0)
        {
            targetGroupe.RemoveMember(target.transform);
        }
    }
}