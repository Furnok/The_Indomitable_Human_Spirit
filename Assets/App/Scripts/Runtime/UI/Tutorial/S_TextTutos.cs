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
        UpdateText(_rsoDevice.Value); // For testing
    }

    private void OnEnable()
    {
        _rsoDevice.onValueChanged += UpdateText;
    }

    private void OnDisable()
    {
        _rsoDevice.onValueChanged -= UpdateText;
    }

    private void Start()
    {
        UpdateText(_rsoDevice.Value);
    }

    void UpdateText(S_EnumDevice newDevice)
    {
         switch (newDevice)
        {
            case S_EnumDevice.None:
            case S_EnumDevice.KeyboardMouse:
                UpdateDeviceText(S_EnumDevice.KeyboardMouse);
                break;
            case S_EnumDevice.PlaystationController:
                UpdateDeviceText(S_EnumDevice.PlaystationController);
                break;
            case S_EnumDevice.XboxController:
                UpdateDeviceText(S_EnumDevice.XboxController);
                break;
        }
    }

    private Sprite GetSpriteOrNull(S_EnumTutorialStep step, S_EnumDevice device)
    {
        if (_spritesByDevice == null) return null;
        if (!_spritesByDevice.TryGetValue(step, out var inner) || inner == null) return null;
        if (!inner.TryGetValue(device, out var sprite)) return null;
        return sprite;
    }

    void UpdateDeviceText(S_EnumDevice device)
    {
        Sprite sprite;

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Targeting, device);
        if (sprite != null)
            _textTutoTargeting.text = _textTutoTargeting.text.Replace("{TARGETING}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Parry, device);
        if (sprite != null)
            _textTutoParry.text = _textTutoParry.text.Replace("{PARRY}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Attack, device);
        if (sprite != null)
            _textTutoAttack.text = _textTutoAttack.text.Replace("{ATTACK}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Dodge, device);
        if (sprite != null)
            _textTutoDodge.text = _textTutoDodge.text.Replace("{DODGE}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Movement, device);
        if (sprite != null)
            _textTutoMovement.text = _textTutoMovement.text.Replace("{MOVE}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.SwapTarget, device);
        if (sprite != null)
            _textTutoSwapTarget.text = _textTutoSwapTarget.text.Replace("{SWAP_TARGET}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.ParryProjectile, device);
        if (sprite != null)
            _textTutoParryProjectile.text = _textTutoParryProjectile.text.Replace("{PARRY_PROJECTILE}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Interact, device);
        if (sprite != null)
            _textTutoInteract.text = _textTutoInteract.text.Replace("{INTERACT}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Heal, device);
        if (sprite != null)
            _textTutoHeal.text = _textTutoHeal.text.Replace("{HEAL}", $"<sprite name=\"{sprite.name}\">");
    }


}