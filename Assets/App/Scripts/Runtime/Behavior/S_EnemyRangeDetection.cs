using UnityEngine;

public class S_EnemyRangeDetection : MonoBehaviour
{
    [Header("Settings")]
    [S_TagName] [SerializeField] string playerTag;

    private GameObject targetDetected;

    //[Header("References")]

    [Header("Input")]
    [SerializeField] RSO_TargetValue RSO_TargetValue;

    //[Header("Output")]
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetDetected = other.gameObject;
            RSO_TargetValue.Value = targetDetected;
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetDetected = null;
            RSO_TargetValue.Value = null;
        }
    }
}