using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

public class S_TextTutos : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] S_SerializableDictionary<S_EnumTutorialStep, S_SerializableDictionary<S_EnumDevice, Sprite>> _spritesByDevice;

    [Header("References")]
    [SerializeField] List<GameObject> _tutorialsTextObjects = new();

    [Header("References - TMP")]
    [SerializeField] TextMeshProUGUI _textTutoMovement;
    [SerializeField] TextMeshProUGUI _textTutoDodge;
    [SerializeField] TextMeshProUGUI _textTutoDodgeDup;
    [SerializeField] TextMeshProUGUI _textTutoAttack;
    [SerializeField] TextMeshProUGUI _textTutoHeal;
    [SerializeField] TextMeshProUGUI _textTutoParry;
    [SerializeField] TextMeshProUGUI _textTutoParryDup;
    [SerializeField] TextMeshProUGUI _textTutoTargeting;
    [SerializeField] TextMeshProUGUI _textTutoSwapTarget;
    [SerializeField] TextMeshProUGUI _textTutoParryProjectile;
    [SerializeField] TextMeshProUGUI _textTutoInteract;

    [SerializeField] TextMeshProUGUI _textTutoConviction;
    [SerializeField] TextMeshProUGUI _textTutoAttackSignaling;

    [Header("Inputs")]
    [SerializeField] RSO_Device _rsoDevice;

    //[Header("Outputs")]

    private string _tplTargeting;
    private string _tplParry;
    private string _tplParryDuplicate;
    private string _tplInteract;
    private string _tplParryProjectile;
    private string _tplDodge;
    private string _tplDodgeDuplicate;
    private string _tplHeal;
    private string _tplAttack;
    private string _tplMovement;
    private string _tplSwapTarget;

    private string _tplConviction;
    private string _tplAttackSignaling;

    private Coroutine _refreshRoutine = null;

    private void Awake()
    {
        
    }

    private void Update()
    {
        //UpdateText(_rsoDevice.Value); // For testing
        //Debug.Log(_tplTargeting);
        //Debug.Log(_textTutoTargeting.text);
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        _rsoDevice.onValueChanged -= UpdateText;

        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void Start()
    {
        _rsoDevice.onValueChanged += UpdateText;

        RequestRefreshTemplatesThenApply();
    }

    private void RequestRefreshTemplatesThenApply()
    {
        if (_refreshRoutine != null)
            StopCoroutine(_refreshRoutine);

        _refreshRoutine = StartCoroutine(Co_RefreshTemplatesThenApply());
    }

    private IEnumerator Co_RefreshTemplatesThenApply()
    {
        foreach (var txtObj in _tutorialsTextObjects)
        {
            txtObj.SetActive(true);
        }

        yield return null;
        yield return null;

        SnapshotTemplatesFromLocalizedTexts();
        UpdateText(_rsoDevice.Value);

        foreach (var txtObj in _tutorialsTextObjects)
        {
            txtObj.SetActive(false);
        }
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

    private void OnLocaleChanged(UnityEngine.Localization.Locale _)
    {
        RequestRefreshTemplatesThenApply();
    }

    private void SnapshotTemplatesFromLocalizedTexts()
    {
        _tplTargeting = _textTutoTargeting.text;
        _tplParry = _textTutoParry.text;
        _tplInteract = _textTutoInteract.text;
        _tplParryProjectile = _textTutoParryProjectile.text;
        _tplDodge = _textTutoDodge.text;
        _tplHeal = _textTutoHeal.text;
        _tplAttack = _textTutoAttack.text;
        _tplMovement = _textTutoMovement.text;
        _tplSwapTarget = _textTutoSwapTarget.text;

        _tplConviction = _textTutoConviction.text;
        _tplAttackSignaling = _textTutoAttackSignaling.text;

        _tplParryDuplicate = _textTutoParryDup.text;
        _tplDodgeDuplicate = _textTutoDodgeDup.text;
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
            _textTutoTargeting.text = _tplTargeting.Replace("{TARGETING}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Parry, device);
        if (sprite != null)
            _textTutoParry.text = _tplParry.Replace("{PARRY}", $"<sprite name=\"{sprite.name}\">");

        string result = _tplAttack;

        var attack = GetSpriteOrNull(S_EnumTutorialStep.Attack, device);
        if (attack != null)
            result = result.Replace("{ATTACK}", $"<sprite name=\"{attack.name}\">");

        var attackUp = GetSpriteOrNull(S_EnumTutorialStep.AttackUpgrade, device);
        if (attackUp != null)
            result = result.Replace("{ATTACK_UPGRADE}", $"<sprite name=\"{attackUp.name}\">");

        _textTutoAttack.text = result;

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Dodge, device);
        if (sprite != null)
            _textTutoDodge.text = _tplDodge.Replace("{DODGE}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Movement, device);
        if (sprite != null)
            _textTutoMovement.text = _tplMovement.Replace("{MOVE}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.SwapTarget, device);
        if (sprite != null)
            _textTutoSwapTarget.text = _tplSwapTarget.Replace("{SWAP_TARGET}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.ParryProjectile, device);
        if (sprite != null)
            _textTutoParryProjectile.text = _tplParryProjectile.Replace("{PARRY}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Interact, device);
        if (sprite != null)
            _textTutoInteract.text = _tplInteract.Replace("{INTERACT}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Heal, device);
        if (sprite != null)
            _textTutoHeal.text = _tplHeal.Replace("{HEAL}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.Conviction, device);
        if (sprite != null)
            _textTutoConviction.text = _tplConviction.Replace("{NEXT}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.AttackSignaling, device);
        if (sprite != null)
            _textTutoAttackSignaling.text = _tplAttackSignaling.Replace("{NEXT}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.ParryDuplicate, device);
        if (sprite != null)
            _textTutoParryDup.text = _tplParryDuplicate.Replace("{Parry}", $"<sprite name=\"{sprite.name}\">");

        sprite = GetSpriteOrNull(S_EnumTutorialStep.DodgeDuplicate, device);
        if (sprite != null)
            _textTutoDodgeDup.text = _tplDodgeDuplicate.Replace("{Dodge}", $"<sprite name=\"{sprite.name}\">");
    }


}