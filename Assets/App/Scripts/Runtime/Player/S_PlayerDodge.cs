using UnityEngine;

public class S_PlayerDodge : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] RSE_OnPlayerDodge _onPlayerDodge;

    void OnEnable()
    {
        _onPlayerDodge.action += Dodge;
    }

    private void OnDisable()
    {
        _onPlayerDodge.action -= Dodge;
    }

    void Dodge()
    {
        Debug.Log("Player Dodge");
    }
}