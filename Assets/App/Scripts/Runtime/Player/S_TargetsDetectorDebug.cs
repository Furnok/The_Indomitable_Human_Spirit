using UnityEngine;

public class S_TargetsDetectorDebug : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Color _gizmoColor = Color.green;
    [SerializeField] bool _drawGizmos = true;

    [Header("References")]
    [SerializeField] SphereCollider _detectionCollider;

    //[Header("Input")]

    //[Header("Output")]
    
    //[Header("RSO")]

    private void OnDrawGizmos()
    {
        if (!enabled || !_drawGizmos) return;
        Gizmos.color = _gizmoColor;
        Gizmos.DrawSphere(gameObject.transform.position/*_detectionCollider.center*/, _detectionCollider.radius);
    }
}