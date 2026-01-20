using DG.Tweening;
using TMPro;
using UnityEngine;

public class S_EnemyUIDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshPro text;

    public void Initialize(float damage)
    {
        text.text = damage.ToString();

        text.transform.DOMoveY(transform.position.y + 1, 1).SetEase(Ease.Linear);

        text.DOFade(0f, 1).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}