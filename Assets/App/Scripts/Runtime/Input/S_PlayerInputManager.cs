using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class S_PlayerInputManager : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] PlayerInput _playerInputComponent;


    [Header("RSE")]
    [SerializeField] RSE_OnPlayerMove _onPlayerMove;

    private IA_PlayerInput _playerInput;
    private bool _initialized;
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

        _playerInputComponent.actions.Enable();

        _playerInputComponent.SwitchCurrentActionMap(_gameMapName);
    }

    private void OnDisable()
    {
        if (!_initialized) return;

        var game = _playerInput.Game;

        game.Move.performed -= OnMoveChanged;
        game.Move.canceled -= OnMoveChanged;

        _playerInputComponent.actions.Disable();

    }

    private void OnMoveChanged(InputAction.CallbackContext ctx)
    {
        _onPlayerMove.Call(ctx.ReadValue<Vector2>());
    }

   

    public void DeactivateInput()
    {
        if (!_initialized) return;
        _playerInputComponent.actions.Disable();
    }

    public void ActivateGameAction()
    {
        if (!_initialized) return;
        _playerInputComponent.actions.Enable();
        _playerInputComponent.SwitchCurrentActionMap(_gameMapName);
    }

    public void OnGameOver()
    {
        if (!_initialized) return;
        _playerInputComponent.actions.Enable();
        _playerInputComponent.SwitchCurrentActionMap(_uiMapName);
    }

    
}