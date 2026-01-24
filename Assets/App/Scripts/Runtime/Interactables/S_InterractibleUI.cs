using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_InterractibleUI : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Transform")]
    [SerializeField] private Transform content;

    [TabGroup("References")]
    [Title("Images")]
    [SerializeField] private Image image;

    [TabGroup("References")]
    [Title("Text")]
    [SerializeField] private TextMeshProUGUI text;

    [TabGroup("References")]
    [Title("Keyboard & Mouse")]
    [SerializeField] private Sprite imageKeyboardMouse;

    [TabGroup("References")]
    [Title("PlayStation")]
    [SerializeField] private Sprite imagePlayStation;

    [TabGroup("References")]
    [Title("Xbox")]
    [SerializeField] private Sprite imageXbox;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Device rsoDevice;

    private void LateUpdate()
    {
        if (content.gameObject.activeInHierarchy)
        {
            if (rsoDevice.Value == S_EnumDevice.KeyboardMouse)
            {
                image.sprite = imageKeyboardMouse;
                text.text = "E";
            }
            else if (rsoDevice.Value == S_EnumDevice.PlaystationController)
            {
                image.sprite = imagePlayStation;
                text.text = "";
            }
            else if (rsoDevice.Value == S_EnumDevice.XboxController)
            {
                image.sprite = imageXbox;
                text.text = "";
            }

            content.LookAt(content.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }
    }
}