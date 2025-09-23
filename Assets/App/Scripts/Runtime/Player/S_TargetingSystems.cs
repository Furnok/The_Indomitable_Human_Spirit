using UnityEngine;

public class S_TargetingSystems : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    //[Header("Input")]

    //[Header("Output")]

    [Header("RSO")]
    [SerializeField] RSO_EnemyInTargetingArea _enemyInTargetingArea;

    private void Awake()
    {
        if (_enemyInTargetingArea != null)
        {
            _enemyInTargetingArea.Value.Clear();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy in range: " + other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy out of range: " + other.name);
        }

    }
}