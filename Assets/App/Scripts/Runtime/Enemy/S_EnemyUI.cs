using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class S_EnemyUI : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Content")]
    [SerializeField] private GameObject content;

    [TabGroup("References")]
    [SerializeField] private Slider sliderHealth;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_Display ssoDisplay;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_UnDisplay ssoUnDisplay;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_AnimationSlider ssoAnimationSlider;

    private Coroutine displayHealthBar = null;
    private Tween healthTween = null;

    private float timeDisplay = 0;

    private void OnDisable()
    {
        healthTween?.Kill();
    }

    private void Update()
    {
        sliderHealth.gameObject.transform.LookAt(Camera.main.transform);
    }

    public void Setup(SSO_EnemyData ssoEnemyData)
    {
        sliderHealth.maxValue = ssoEnemyData.Value.health;
        sliderHealth.value = ssoEnemyData.Value.health;
        timeDisplay = ssoEnemyData.Value.displayUITime;

        if (content.activeInHierarchy)
        {
            CanvasGroup cg = content.GetComponent<CanvasGroup>();
            cg.DOKill();

            cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).OnComplete(() =>
            {
                content.SetActive(false);
            });
        }
    }

    public void UpdateHealthBar(float healthValue)
    {
        CanvasGroup cg = content.GetComponent<CanvasGroup>();
        cg.DOKill();

        if (healthValue <= 0)
        {
            cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).OnComplete(() =>
            {
                content.SetActive(false);
            });
        }
        else
        {
            content.gameObject.SetActive(true);

            cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear);
        }

        healthTween?.Kill();

        healthTween = sliderHealth.DOValue(healthValue, ssoAnimationSlider.Value).SetEase(Ease.OutCubic);

        if (displayHealthBar != null)
        {
            StopCoroutine(displayHealthBar);
            displayHealthBar = null;
        }

        displayHealthBar = StartCoroutine(DisplayHealthBar());
    }

    private IEnumerator DisplayHealthBar()
    {
        yield return new WaitForSeconds(timeDisplay);

        CanvasGroup cg = content.GetComponent<CanvasGroup>();
        cg.DOKill();

        cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).OnComplete(() =>
        {
            content.SetActive(false);
        });
    }
}