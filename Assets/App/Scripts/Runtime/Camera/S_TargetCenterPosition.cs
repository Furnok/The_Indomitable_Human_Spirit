using UnityEngine;

public class S_TargetCenterPosition : MonoBehaviour
{
    [Header("Output")]
    [SerializeField] RSE_AddTargetCenter rseAddTargetCenter;

    private void Start()
    {
        rseAddTargetCenter?.Call(gameObject);
    }
}