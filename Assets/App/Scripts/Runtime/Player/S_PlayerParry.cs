using UnityEngine;

public class S_PlayerParry : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] RSE_OnPlayerParry _onPlayerParry;

    private void OnEnable()
    {
        _onPlayerParry.action += Parry;
    }

    private void OnDisable()
    {
        _onPlayerParry.action -= Parry;
    }

    void Parry()
    {
        Debug.Log("Player Parry");
    }
}