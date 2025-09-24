using UnityEngine;

public class S_EnemyRangeDetection : MonoBehaviour
{
    [Header("Settings")]
    [S_TagName] [SerializeField] string playerTag;

    private GameObject targetDetected;

    //[Header("References")]

    //[Header("Input")]

    //[Header("Output")]
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetDetected = other.gameObject;
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetDetected = null;
        }
    }
}