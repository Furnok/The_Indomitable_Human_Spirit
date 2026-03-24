using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class S_BossDetectionRange : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Filters")]
    [SerializeField][S_TagName] private string playerTag;

    [HideInInspector] public UnityEvent<GameObject> onTargetDetected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            onTargetDetected.Invoke(other.gameObject);
        }
    }
}