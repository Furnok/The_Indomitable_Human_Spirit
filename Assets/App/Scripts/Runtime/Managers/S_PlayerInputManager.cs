using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class S_PlayerInputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInput _playerInputComponent;

    [Header("Output")]
    [SerializeField] RSE_OnPlayerMove _onPlayerMove;
    [SerializeField] RSE_OnPlayerAttack _onPlayerAttack;
    [SerializeField] RSE_OnPlayerDodge _onPlayerDodge;
    [SerializeField] RSE_OnPlayerInteract _onPlayerInteract;
    [SerializeField] RSE_OnPlayerPause _onPlayerPause;
    [SerializeField] RSE_OnPlayerMeditation _onPlayerMeditation;
    [SerializeField] RSE_OnPlayerMeditationCancel _onPlayerMeditationCancel;
    [SerializeField] RSE_OnPlayerParry _onPlayerParry;
    [SerializeField] RSE_OnPlayerTargeting _onPlayerTargeting;
    [SerializeField] RSE_OnPlayerTargetingCancel _onPlayerTargetingCancel;
    [SerializeField] RSE_OnPlayerSwapTarget _onPlayerSwapTarget;
    [SerializeField] RSE_OnPlayerHeal _OnPlayerHeal;

    IA_PlayerInput _playerInput;
    bool _initialized;
    string _gameMapName;
    string _uiMapName;

    private void Awake()
    {
        if (_playerInputComponent == null)
        {
            Debug.LogError("PlayerInput component missing");
            enabled = false;
            return;
        }

        _playerInput = new IA_PlayerInput();
        _playerInputComponent.actions = _playerInput.asset;
        _initialized = true;

        _gameMapName = _playerInput.Game.Get().name;
        _uiMapName = _playerInput.UI.Get().name;

    }

    private void OnEnable()
    {
        if (!_initialized) return;

        var game = _playerInput.Game;
        game.Move.performed += OnMoveChanged;
        game.Move.canceled += OnMoveChanged;
        game.Attack.performed += OnAttackInput;
        game.Dodge.performed += OnDodgeInput;
        game.Interact.performed += OnInteractInput;
        game.Meditation.performed += OnMeditationInput;
        game.Meditation.canceled += OnMeditationCancelInput;
        game.Parry.performed += OnParryInput;
        game.Pause.performed += OnPauseInput;
        game.Targeting.performed += OnTargetingInput;
        game.Targeting.canceled += OnTargetingCancelInput;
        game.SwapTarget.performed += OnSwapTargetInput;
        game.Heal.performed += OnHealInput;

        _playerInputComponent.actions.Enable();

        _playerInputComponent.SwitchCurrentActionMap(_gameMapName);
    }

    private void OnDisable()
    {
        if (!_initialized) return;

        var game = _playerInput.Game;

        game.Move.performed -= OnMoveChanged;
        game.Move.canceled -= OnMoveChanged;
        game.Attack.performed -= OnAttackInput;
        game.Dodge.performed -= OnDodgeInput;
        game.Interact.performed -= OnInteractInput;
        game.Meditation.performed -= OnMeditationInput;
        game.Meditation.canceled -= OnMeditationCancelInput;
        game.Parry.performed -= OnParryInput;
        game.Pause.performed -= OnPauseInput;
        game.Targeting.performed -= OnTargetingInput;
        game.Targeting.canceled -= OnTargetingCancelInput;
        game.SwapTarget.performed -= OnSwapTargetInput;
        game.Heal.performed -= OnHealInput;

        _playerInputComponent.actions.Disable();
    }

    #region Input Callback Methods
    void OnMoveChanged(InputAction.CallbackContext ctx)
    {
        _onPlayerMove.Call(ctx.ReadValue<Vector2>());
    }

    void OnTargetingInput(InputAction.CallbackContext ctx)
    {
        _onPlayerTargeting.Call();
    }

    void OnTargetingCancelInput(InputAction.CallbackContext ctx)
    {
        _onPlayerTargetingCancel.Call();
    }

    void OnSwapTargetInput(InputAction.CallbackContext ctx)
    {
        _onPlayerSwapTarget.Call();
    }


    void OnAttackInput(InputAction.CallbackContext ctx)
    {
        _onPlayerAttack.Call();
    }

    void OnDodgeInput(InputAction.CallbackContext ctx)
    {
        _onPlayerDodge.Call();
    }

    void OnInteractInput(InputAction.CallbackContext ctx)
    {
        _onPlayerInteract.Call();
    }

    void OnPauseInput(InputAction.CallbackContext ctx)
    {
        _onPlayerPause.Call();
    }

    void OnMeditationInput(InputAction.CallbackContext ctx)
    {
        _onPlayerMeditation.Call();
    }

    void OnMeditationCancelInput(InputAction.CallbackContext ctx)
    {
        _onPlayerMeditationCancel.Call();
    }

    void OnParryInput(InputAction.CallbackContext ctx)
    {
        _onPlayerParry.Call();
    }

    void OnHealInput(InputAction.CallbackContext ctx)
    {
        _OnPlayerHeal.Call();
    }


    #endregion

    private void DeactivateInput()
    {
        if (!_initialized) return;
        _playerInputComponent.actions.Disable();
    }

    private void ActivateGameAction()
    {
        if (!_initialized) return;
        _playerInputComponent.actions.Enable();
        _playerInputComponent.SwitchCurrentActionMap(_gameMapName);
    }

    private void OnGameOver()
    {
        if (!_initialized) return;
        _playerInputComponent.actions.Enable();
        _playerInputComponent.SwitchCurrentActionMap(_uiMapName);
    }
}