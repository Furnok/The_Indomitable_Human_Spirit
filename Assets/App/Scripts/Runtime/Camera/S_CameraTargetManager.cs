using Unity.Cinemachine;
using UnityEngine;

public class S_CameraTargetManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineTargetGroup targetGroupe;

    [Header("Input")]
    [SerializeField] private RSE_OnNewTargeting rseOnNewTargeting;
    [SerializeField] private RSE_OnPlayerCancelTargeting rseOnPlayerCancelTargeting;

    private void OnEnable()
    {
        rseOnNewTargeting.action += AddTarget;
        rseOnPlayerCancelTargeting.action += RemoveTarget;
    }

    private void OnDisable()
    {
        rseOnNewTargeting.action -= AddTarget;
        rseOnPlayerCancelTargeting.action -= RemoveTarget;
    }

    private void AddTarget(GameObject target)
    {
        if (target != null && targetGroupe.FindMember(target.transform) == -1)
        {
            targetGroupe.AddMember(target.transform, 1f, 1f);
        }
    }

    private void RemoveTarget(GameObject target)
    {
        if (target != null && targetGroupe.FindMember(target.transform) >= 0)
        {
            targetGroupe.RemoveMember(target.transform);
        }
    }
}