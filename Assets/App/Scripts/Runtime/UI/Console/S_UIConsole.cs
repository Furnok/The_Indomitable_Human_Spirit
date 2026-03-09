using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class S_UIConsole : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference uiSound;

    [TabGroup("References")]
    [Title("Scroll View")]
    [SerializeField] private ScrollRect scrollRect;

    [TabGroup("References")]
    [Title("Slider")]
    [SerializeField] private Scrollbar sliderScroll;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnShowMouseCursor rseOnShowMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnNeedCursor rseOnNeedCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_ConsoleDisplay rsoConsoleDisplay;

    private int lastSoundFrame = -1;
    private float lastValue = -1;
    private bool startMove = false;
    private bool isStick = false;

    private void OnEnable()
    {
        rsoConsoleDisplay.Value = true;

        if (Gamepad.current == null) rseOnShowMouseCursor.Call();
        rseOnNeedCursor.Call(true);

        scrollRect.verticalNormalizedPosition = 0;
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
                RuntimeManager.PlayOneShot(uiSound);
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
                        lastValue = roundedValue;

                        if (!startMove)
                        {
                            startMove = true;
                            RuntimeManager.PlayOneShot(uiSound);
                        }
                    }
                    break;
                case MoveDirection.Down:
                    lastSoundFrame = Time.frameCount;

                    roundedValue = Mathf.Round(sliderScroll.value * 1000f) / 1000f;
                    if (roundedValue != lastValue && sliderScroll.size < 1)
                    {
                        lastValue = roundedValue;
                        if (!startMove)
                        {
                            startMove = true;
                            RuntimeManager.PlayOneShot(uiSound);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}