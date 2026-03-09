using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class S_CursorManager : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Cursors")]
    [SerializeField] private Texture2D defaultCursor;

    [TabGroup("References")]
    [SerializeField] private Texture2D SelectableCursor;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnShowMouseCursor rseOnShowMouseCursor;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnHideMouseCursor rseOnHideMouseCursor;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnNeedCursor rseOnNeedCursor;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnSetFocus rseOnSetFocus;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnResetCursor rseOnResetCursor;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnResetFocus rseOnResetFocus;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnMouseEnterUI rseOnMouseEnterUI;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnMouseLeaveUI rseOnMouseLeaveUI;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Navigation rsoNavigation;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Device rsoDevice;

    private bool needCursor = false;

    private void Awake()
    {
        if (Gamepad.current != null) Device(Gamepad.current);
        else rsoDevice.Value = S_EnumDevice.KeyboardMouse;
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        rseOnShowMouseCursor.action += ShowMouseCursor;
        rseOnHideMouseCursor.action += HideMouseCursor;
        rseOnNeedCursor.action += NeedCursor;
        rseOnSetFocus.action += SetFocus;
        rseOnResetCursor.action += ResetCursor;
        rseOnResetFocus.action += ResetFocus;
        rseOnMouseEnterUI.action += MouseEnter;
        rseOnMouseLeaveUI.action += MouseLeave;
    }

    private void OnDisable() 
    {
        rsoNavigation.Value = new();

        InputSystem.onDeviceChange -= OnDeviceChange;
        rseOnShowMouseCursor.action -= ShowMouseCursor;
        rseOnHideMouseCursor.action -= HideMouseCursor;
        rseOnNeedCursor.action -= NeedCursor;
        rseOnSetFocus.action -= SetFocus;
        rseOnResetCursor.action -= ResetCursor;
        rseOnResetFocus.action -= ResetFocus;
        rseOnMouseEnterUI.action -= MouseEnter;
        rseOnMouseLeaveUI.action -= MouseLeave;
    }

    private void Device(InputDevice device)
    {
        string layout = device.layout.ToLower();
        string displayName = device.displayName?.ToLower() ?? "";
        string description = device.description.product?.ToLower() ?? "";

        if (layout.Contains("dualshock") || layout.Contains("dualSense") || displayName.Contains("playstation") || description.Contains("dualshock") || description.Contains("dualsense"))
        {
            rsoDevice.Value = S_EnumDevice.PlaystationController;
        }
        else if (layout.Contains("xinput") || displayName.Contains("xbox") || description.Contains("xbox"))
        {
            rsoDevice.Value = S_EnumDevice.XboxController;
        }
        else rsoDevice.Value = S_EnumDevice.None;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            if (change == InputDeviceChange.Added)
            {
                HideMouseCursor();

                Device(device);

                if (rsoNavigation.Value.selectableDefault != null) SetFocus(rsoNavigation.Value.selectableDefault);
            }
            else if (change == InputDeviceChange.Removed)
            {
                ResetFocus();

                rsoDevice.Value = S_EnumDevice.KeyboardMouse;
                ShowMouseCursor();
            }
        }
    }

    private void ShowMouseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    private void HideMouseCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    private void NeedCursor(bool value)
    {
        needCursor = value;
    }

    private void MouseEnter(Selectable uiElement)
    {
        if (uiElement.interactable) Cursor.SetCursor(SelectableCursor, Vector2.zero, CursorMode.Auto);
    }

    private void MouseLeave(Selectable uiElement)
    {
        if (uiElement.interactable) Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    private void SetFocus(Selectable uiElement)
    {
        if (uiElement != null && uiElement.interactable && Gamepad.current != null)
        {
            uiElement.Select();
            rsoNavigation.Value.selectableFocus = uiElement;
        }
    }

    private void ResetCursor()
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    private void ResetFocus()
    {
        EventSystem.current.SetSelectedGameObject(null);
        rsoNavigation.Value.selectableFocus = null;
    }
}