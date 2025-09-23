using Unity.Cinemachine;
using UnityEngine;

public class S_CameraTargetManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineTargetGroup targetGroupe;

    [Header("Input")]
    [SerializeField] RSE_AddTargetCenter rseTargetCenter;
    [SerializeField] RSE_RemoveTargetCenter rseRemoveTargetCenter;

    private void OnEnable()
    {
        rseTargetCenter.action += AddTarget;
        rseRemoveTargetCenter.action += RemoveTarget;
    }

    private void OnDisable()
    {
        rseTargetCenter.action -= AddTarget;
        rseRemoveTargetCenter.action -= RemoveTarget;
    }

    private void AddTarget(GameObject target)
    {
        targetGroupe.AddMember(target.transform, 1f, 1f);
    }

    private void RemoveTarget(GameObject target)
    {
        targetGroupe.RemoveMember(target.transform);
    }
}