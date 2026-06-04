using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class S_UISelectable : MonoBehaviour
{
    [TabGroup("Settings")]
    [Title("Transitions")]
    [SuffixLabel("s", Overlay = true)]
    [SerializeField] private float transition = 0.2f;

    [TabGroup("Settings")]
    [Title("Colors")]
    [SerializeField] private Color32 colorMouseEnter = new(200, 200, 200, 255);

    [TabGroup("Settings")]
    [SerializeField] private Color32 colorMouseDown = new(150, 150, 150, 255);

    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference uiClick;

    [TabGroup("References")]
    [SerializeField] private EventReference uiHover;

    [TabGroup("References")]
    [Title("Images")]
    [SerializeField] private Image image;

    [TabGroup("References")]
    [SerializeField] private Image image2;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Navigation rsoNavigation;

    private Color32 colorBase = new();
    private Color32 colorBase2 = new();

    private bool mouseOver = false;
    private bool isPressed = false;
    private bool isSelected = false;

    private Tween colorTween = null;
    private Tween colorTween2 = null;

    private void OnEnable()
    {
        colorBase = image.color;

        if (image2 != null) colorBase2 = image2.color;
    }

    private void OnDisable()
    {
        colorTween?.Kill();
        colorTween2?.Kill();
        image.color = colorBase;

        if (image2 != null) image2.color = colorBase2;

        mouseOver = false;
        isPressed = false;
        isSelected = false;
    }

    public void MouseEnter(Selectable uiElement)
    {
        if (uiElement.interactable)
        {
            RuntimeManager.PlayOneShot(uiHover);

            if (!isPressed) PlayColorTransition(colorMouseEnter, colorMouseEnter);

            mouseOver = true;
        }
    }

    public void MouseExit(Selectable uiElement)
    {
        if (uiElement.interactable)
        {
            RuntimeManager.PlayOneShot(uiHover);

            if (!isPressed) PlayColorTransition(colorBase, colorBase2);

            mouseOver = false;
        }
    }


    public void MouseDown(Selectable uiElement)
    {
        if (uiElement.interactable)
        {
            PlayColorTransition(colorMouseDown, colorMouseDown);

            isPressed = true;
        }
    }

    public void MouseUp(Selectable uiElement)
    {
        if (uiElement.interactable)
        {
            if (mouseOver) PlayColorTransition(colorMouseEnter, colorMouseEnter);
            else PlayColorTransition(colorBase, colorBase2);

            isPressed = false;
        }
    }

    public void Selected(Selectable uiElement)
    {
        if (uiElement.interactable && Gamepad.current != null)
        {
            RuntimeManager.PlayOneShot(uiHover);

            PlayColorTransition(colorMouseEnter, colorMouseEnter);
            rsoNavigation.Value.selectableFocus = uiElement;

            isSelected = true;
        }
    }

    public void Unselected(Selectable uiElement)
    {
        if (uiElement.interactable && Gamepad.current != null) PlayColorTransition(colorBase, colorBase2);
        else if (isSelected)
        {
            PlayColorTransition(colorBase, colorBase2);

            isSelected = false;
        }
    }

    public void Clicked(Selectable uiElement)
    {
        if (uiElement.interactable)
        {
            RuntimeManager.PlayOneShot(uiClick);

            if (Gamepad.current != null)
            {
                rsoNavigation.Value.selectablePressOld = rsoNavigation.Value.selectablePress;
                rsoNavigation.Value.selectablePress = uiElement;
            }
        }
    }

    public void ClickedNotAudio(Selectable uiElement)
    {
        if (uiElement.interactable)
        {
            if (Gamepad.current != null)
            {
                rsoNavigation.Value.selectablePressOld = rsoNavigation.Value.selectablePress;
                rsoNavigation.Value.selectablePress = uiElement;
            }
        }
    }

    public void PlayAudio(Selectable uiElement)
    {
        if (uiElement.interactable) RuntimeManager.PlayOneShot(uiClick);
    }

    public void ClickedWindow(Selectable uiElement)
    {
        rsoNavigation.Value.selectablePressOldWindow = uiElement;
    }

    private void PlayColorTransition(Color32 targetColor, Color32 targetColor2)
    {
        colorTween?.Kill();
        colorTween2?.Kill();

        colorTween = image.DOColor(targetColor, transition).SetEase(Ease.Linear).SetUpdate(true);

        if (image2 != null) colorTween2 = image2.DOColor(targetColor2, transition).SetEase(Ease.Linear).SetUpdate(true);
    }
}