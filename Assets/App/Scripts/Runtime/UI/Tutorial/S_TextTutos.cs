using TMPro;
using UnityEngine;

public class S_TextTutos : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] S_SerializableDictionary<S_EnumTutorialStep, S_SerializableDictionary<S_EnumDevice, Sprite>> _spritesByDevice;


    [Header("References")]
    [SerializeField] TextMeshProUGUI _textTutoMovement;
    [SerializeField] TextMeshProUGUI _textTutoDodge;
    [SerializeField] TextMeshProUGUI _textTutoAttack;
    [SerializeField] TextMeshProUGUI _textTutoHeal;
    [SerializeField] TextMeshProUGUI _textTutoParry;
    [SerializeField] TextMeshProUGUI _textTutoTargeting;
    [SerializeField] TextMeshProUGUI _textTutoSwapTarget;
    [SerializeField] TextMeshProUGUI _textTutoParryProjectile;
    [SerializeField] TextMeshProUGUI _textTutoInteract;

    [Header("Inputs")]
    [SerializeField] RSO_Device _rsoDevice;

    //[Header("Outputs")]

    private void Update()
    {
        //UpdateText(S_EnumDevice.KeyboardMouse); // For testing

    }

    void UpdateText(S_EnumDevice newDevice)
    {
         switch (newDevice)
        {
            case S_EnumDevice.None:
            case S_EnumDevice.KeyboardMouse:
                Sprite sprite = _spritesByDevice[S_EnumTutorialStep.Targeting]
                                      [S_EnumDevice.KeyboardMouse];

                _textTutoTargeting.text = _textTutoTargeting.text.Replace(
                    "{PARRY}",
                    $"<sprite name=\"{sprite.name}\">"
                );
                
                break;
            case S_EnumDevice.PlaystationController:
                
                break;
            case S_EnumDevice.XboxController:
                break;
        }
    }
}