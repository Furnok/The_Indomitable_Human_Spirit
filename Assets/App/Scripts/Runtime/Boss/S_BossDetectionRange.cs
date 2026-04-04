using Sirenix.OdinInspector;
using UnityEngine;

public class S_BossDetectionRange : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Filters")]
    [SerializeField][S_TagName] private string playerTag;

    [TabGroup("References")]
    [Title("Scripts")]
    [SerializeField] private S_Boss boss;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) boss.DetectTarget(other.gameObject);
    }
}