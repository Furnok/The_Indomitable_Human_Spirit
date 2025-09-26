using UnityEngine;

public class S_PlayerHeal : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] RSE_OnPlayerHeal _onPlayerHeal;

    private void OnEnable()
    {
        _onPlayerHeal.action += Heal;
    }

    private void OnDisable()
    {
        _onPlayerHeal.action -= Heal;
    }

    void Heal()
    {
        Debug.Log("Player Heal");
    }
}