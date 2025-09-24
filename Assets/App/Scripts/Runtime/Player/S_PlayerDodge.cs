using UnityEngine;

public class S_PlayerDodge : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    [Header("Input")]
    [SerializeField] RSE_OnPlayerDodge _onPlayerDodge;

    //[Header("Output")]

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