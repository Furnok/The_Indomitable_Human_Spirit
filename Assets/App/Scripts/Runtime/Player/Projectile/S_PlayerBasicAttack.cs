using UnityEngine;

public class S_PlayerBasicAttack : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float _delayIncantationAttack = 0.5f;

    [Header("Input")]
    [SerializeField] RSE_OnPlayerAttack _onPlayerAttack;

    [Header("Output")]
    [SerializeField] RSO_PlayerIsTargeting _playerIsTargeting;

    bool _canAttack = true;

    private void OnEnable()
    {
        _onPlayerAttack.action += OnPlayerAttackInput;
    }

    private void OnDisable()
    {
        _onPlayerAttack.action -= OnPlayerAttackInput;
    }

    void OnPlayerAttackInput()
    {
        if (!_canAttack) return;


        if (_playerIsTargeting.Value == true)
        {
            
        }
        else
        {

        }

        Debug.Log("Player Basic Attack");

        _canAttack = false;

        S_Utils.Delay(_delayIncantationAttack, () => 
        {
            _canAttack = true;
        });
    }
}