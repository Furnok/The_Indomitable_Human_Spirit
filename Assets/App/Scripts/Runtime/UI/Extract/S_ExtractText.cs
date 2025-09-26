using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_ExtractText : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float duration;
    [SerializeField] float scrollStart;

    [Header("References")]
    [SerializeField] TextMeshProUGUI textContent;
    [SerializeField] ScrollRect scrollRect;

    [Header("Output")]
    [SerializeField] RSE_ExtractTextInfo RSE_ExtractTextInfo;

    string textExtract;
    private bool userIsScrolling = false;
    private float userScrollThreshold = 0.05f;
    private Tweener textDisplay;

    private void OnEnable()
    {
        RSE_ExtractTextInfo.action += GetExtractText;
        StartCoroutine(S_Utils.Delay(0.2f, () => DisplayTextContent(textExtract, duration)));
    }
    private void OnDisable()
    {
        RSE_ExtractTextInfo.action -= GetExtractText;
        textContent.text = "";
        textDisplay.Kill();
    }
    
    void GetExtractText(string text)
    {
        textExtract = text;
    }
    void DisplayTextContent(string fullText, float duration)
    {
        textContent.maxVisibleCharacters = 0;
        textContent.text = fullText;

        float initialScrollPos = scrollRect.verticalNormalizedPosition;
        textDisplay = DOTween.To(() => 0, x => {
            int length = Mathf.Clamp(x, 0, fullText.Length);
            textContent.maxVisibleCharacters = length;
            float progress = (float)length / fullText.Length;
            if (!userIsScrolling && progress >= scrollStart)
            {
                float adjustedProgress = Mathf.InverseLerp(scrollStart, 1f, progress);
                float targetPos = Mathf.Lerp(1f, 0f, adjustedProgress);
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetPos, 0.1f);
            }
        }, fullText.Length, duration).SetEase(Ease.Linear);

    }
    private void Update()
    {
        float scrollPos = scrollRect.verticalNormalizedPosition;

        if (!_hasLastScrollPos)
        {
            _lastScrollPos = scrollPos;
            _hasLastScrollPos = true;
            userIsScrolling = false;
            return;
        }

        if (Mathf.Abs(scrollPos - _lastScrollPos) > userScrollThreshold)
        {
            userIsScrolling = true;
        }
        else
        {
            userIsScrolling = false;
        }

        _lastScrollPos = scrollPos;
    }

    private float _lastScrollPos;
    private bool _hasLastScrollPos = false;
}