using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class S_UIExtract : MonoBehaviour
{
    [TabGroup("Settings")]
    [Title("Scroll")]
    [SuffixLabel("s", Overlay = true)]
    [SerializeField] private float scrollStart;

    [TabGroup("Settings")]
    [Title("Display")]
    [SuffixLabel("s", Overlay = true)]
    [SerializeField] private float startDisplay;

    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference uiSound;

    [TabGroup("References")]
    [SerializeField] private EventReference uiClick;

    [TabGroup("References")]
    [Title("Text")]
    [SerializeField] private TextMeshProUGUI textContent;

    [TabGroup("References")]
    [Title("Scroll View")]
    [SerializeField] private ScrollRect scrollRect;

    [TabGroup("References")]
    [Title("Slider")]
    [SerializeField] private Scrollbar sliderScroll;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDisplayExtract rseOnDisplayExtract;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerPause rseOnPlayerPause;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCloseWindow rseOnCloseWindow;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGameInputEnabled rseOnGameInputEnabled;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnResetFocus rseOnResetFocus;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnShowMouseCursor rseOnShowMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnHideMouseCursor rseOnHideMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnNeedCursor rseOnNeedCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Navigation rsoNavigation;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CurrentWindows rsoCurrentWindows;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_InConsole rsoInConsole;

    private bool displayText = false;
    private bool userIsScrolling = false;

    private Tween textDisplay = null;
    private Tween scrollTween = null;

    private bool isClosing = false;

    private int lastSoundFrame = -1;
    private float lastValue = -1;
    private bool startMove = false;
    private bool isStick = false;

    private void OnEnable()
    {
        rseOnDisplayExtract.action += DisplayTextContent;
        rseOnPlayerPause.action += CloseEscape;

        if (Gamepad.current == null) rseOnShowMouseCursor.Call();
        rseOnNeedCursor.Call(true);

        scrollRect.verticalNormalizedPosition = 1;
    }

    private void OnDisable()
    {
        rseOnDisplayExtract.action -= DisplayTextContent;
        rseOnPlayerPause.action -= CloseEscape;

        displayText = false;
        textContent.text = "";
        userIsScrolling = false;
        textDisplay?.Kill();
        scrollTween?.Kill();
        textDisplay = null;
        scrollTween = null;
        isClosing = false;
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == sliderScroll.gameObject && Gamepad.current != null && startMove)
        {
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();

            bool stickActive = Mathf.Abs(leftStick.y) > 0.5f;
            bool stick2Active = Mathf.Abs(rightStick.y) > 0.5f;

            if (stickActive || stick2Active) isStick = true;
            else if (isStick)
            {
                startMove = false;
                RuntimeManager.PlayOneShot(uiClick);
            }
            else
            {
                bool dpadDown = dpad.y < 0;
                bool dpadUp = dpad.y > 0;
                if (!dpadDown && !dpadUp && startMove) startMove = false;
            }
        }
        else isStick = false;
    }

    public void OnScrollChanged()
    {
        if (displayText && !userIsScrolling)
        {
            userIsScrolling = true;
            scrollTween?.Kill();
            scrollTween = null;
        }
    }

    private void CloseEscape()
    {
        if (!isClosing)
        {
            if (rsoCurrentWindows.Value[^1] == gameObject && !rsoInConsole.Value) Close();
        }
    }

    public void Close()
    {
        if (!isClosing)
        {
            rseOnHideMouseCursor.Call();
            rseOnNeedCursor.Call(false);

            RuntimeManager.PlayOneShot(uiSound);

            isClosing = true;
            rseOnGameInputEnabled.Call();
            rseOnCloseWindow.Call(gameObject);
            rsoNavigation.Value.selectableFocus = null;
            rseOnResetFocus.Call();
        }
    }

    public void SliderAudio(BaseEventData eventData)
    {
        if (Gamepad.current != null)
        {
            if (lastSoundFrame == Time.frameCount) return;

            AxisEventData axisData = eventData as AxisEventData;
            MoveDirection direction = axisData.moveDir;
            float roundedValue = 0;

            switch (direction)
            {
                case MoveDirection.Up:
                    lastSoundFrame = Time.frameCount;

                    roundedValue = Mathf.Round(sliderScroll.value * 1000f) / 1000f;
                    if (roundedValue != lastValue && sliderScroll.size < 1)
                    {
                        OnScrollChanged();
                        lastValue = roundedValue;

                        if (!startMove)
                        {
                            startMove = true;
                            RuntimeManager.PlayOneShot(uiClick);
                        }
                    }
                    break;
                case MoveDirection.Down:
                    lastSoundFrame = Time.frameCount;

                    roundedValue = Mathf.Round(sliderScroll.value * 1000f) / 1000f;
                    if (roundedValue != lastValue && sliderScroll.size < 1)
                    {
                        OnScrollChanged();
                        lastValue = roundedValue;

                        if (!startMove)
                        {
                            startMove = true;
                            RuntimeManager.PlayOneShot(uiClick);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void DisplayTextContent(S_ClassExtract classExtract)
    {
        string fullText = classExtract.text.GetLocalizedString();

        textContent.maxVisibleCharacters = 0;
        textContent.text = fullText;

        StartCoroutine(S_Utils.Delay(startDisplay, () =>
        {
            displayText = true;

            textDisplay = DOTween.To(() => 0, x =>
            {
                int length = Mathf.Clamp(x, 0, fullText.Length);
                textContent.maxVisibleCharacters = length;

                float progress = (float)length / fullText.Length;

                if (!userIsScrolling && progress >= scrollStart)
                {
                    float adjustedProgress = Mathf.InverseLerp(scrollStart, 1f, progress);
                    float targetPos = Mathf.Lerp(1f, 0f, adjustedProgress);

                    scrollTween = scrollRect.DOVerticalNormalizedPos(targetPos, 0.1f).SetEase(Ease.Linear);
                }
            }, fullText.Length, classExtract.duration).SetEase(Ease.Linear);
        }));
    }
}