using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class S_PlayerInputManager : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] PlayerInput _playerInputComponent;

    [Header("Output")]
    [SerializeField] RSE_OnPlayerMove _onPlayerMove;
    [SerializeField] RSE_OnPlayerAttack _onPlayerAttack;
    [SerializeField] RSE_OnPlayerDodge _onPlayerDodge;
    [SerializeField] RSE_OnPlayerInteract _onPlayerInteract;
    [SerializeField] RSE_OnPlayerPause _onPlayerPause;
    [SerializeField] RSE_OnPlayerMeditation _onPlayerMeditation;
    [SerializeField] RSE_OnPlayerParry _onPlayerParry;


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
        game.Attack.performed += OnAttack;
        game.Dodge.performed += OnDodge;
        game.Interact.performed += OnInteract;
        game.Meditation.performed += OnMeditation;
        game.Parry.performed += OnParry;
        game.PauseUnpause.performed += OnPause;

        _playerInputComponent.actions.Enable();

        _playerInputComponent.SwitchCurrentActionMap(_gameMapName);
    }

    private void OnDisable()
    {
        if (!_initialized) return;

        var game = _playerInput.Game;

        game.Move.performed -= OnMoveChanged;
        game.Move.canceled -= OnMoveChanged;
        game.Attack.performed -= OnAttack;
        game.Dodge.performed -= OnDodge;
        game.Interact.performed -= OnInteract;
        game.Meditation.performed -= OnMeditation;
        game.Parry.performed -= OnParry;
        game.PauseUnpause.performed -= OnPause;

        _playerInputComponent.actions.Disable();
    }

    private void OnMoveChanged(InputAction.CallbackContext ctx)
    {
        _onPlayerMove.Call(ctx.ReadValue<Vector2>());
    }

    void OnAttack(InputAction.CallbackContext ctx)
    {
        _onPlayerAttack.Call();
    }

    void OnDodge(InputAction.CallbackContext ctx)
    {
        _onPlayerDodge.Call();
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        _onPlayerInteract.Call();
    }

    void OnPause(InputAction.CallbackContext ctx)
    {
        _onPlayerPause.Call();
    }

    void OnMeditation(InputAction.CallbackContext ctx)
    {
        _onPlayerMeditation.Call();
    }

    void OnParry(InputAction.CallbackContext ctx)
    {
        _onPlayerParry.Call();
    }

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