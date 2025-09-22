using Unity.Cinemachine;
using UnityEngine;

public class S_CameraTargetManager : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] private CinemachineTargetGroup targetGroupe;

    [Header("Input")]
    [SerializeField] private RSE_OnPlayerAwake rseOnPlayerAwake;

    //[Header("Output")]

    private void OnEnable()
    {
        rseOnPlayerAwake.action += AddTarget;
    }

    private void OnDisable()
    {
        rseOnPlayerAwake.action -= AddTarget;
    }

    private void AddTarget(GameObject target)
    {
        targetGroupe.AddMember(target.transform, 1f, 1f);
    }
}