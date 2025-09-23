using UnityEngine;

public class S_TargetDetector : MonoBehaviour
{
    //[Header("Settings")]
    

    //[Header("References")]

    //[Header("Input")]

    [Header("Output")]
    [SerializeField] RSE_OnEnemyEnterTargetingRange _onEnemyEnterTargetingRange;
    [SerializeField] RSE_OnEnemyExitTargetingRange _onEnemyExitTargetingRange;

    //[Header("RSO")]

    private void Awake()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (_onEnemyEnterTargetingRange != null)
            {
                _onEnemyEnterTargetingRange.Call(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (_onEnemyExitTargetingRange != null)
            {
                _onEnemyExitTargetingRange.Call(other.gameObject);
            }
        }

    }

}