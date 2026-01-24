using DG.Tweening;
using TMPro;
using UnityEngine;

public class S_EnemyUIDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshPro text;

    public void Initialize(float damage)
    {
        float currentValue = 0f;

        DOTween.To(() => currentValue, x =>
        {
            currentValue = x;
            text.text = Mathf.RoundToInt(x).ToString();
        },
        damage,
        0.5f
        ).SetEase(Ease.Linear);

        text.transform.DOMoveY(transform.position.y + 1, 1).SetEase(Ease.Linear);

        text.DOFade(0f, 1).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}