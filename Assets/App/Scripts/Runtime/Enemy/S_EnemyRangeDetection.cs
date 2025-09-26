using UnityEngine;
using UnityEngine.Events;

public class S_EnemyRangeDetection : MonoBehaviour
{
    [Header("Settings")]
    [S_TagName] [SerializeField] string playerTag;

    [Header("Input")]
    public UnityEvent<GameObject> onTargetDetected;

    private GameObject targetDetected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetDetected = other.gameObject;
            onTargetDetected.Invoke(targetDetected);
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetDetected = null;
            onTargetDetected.Invoke(targetDetected);
        }
    }
}