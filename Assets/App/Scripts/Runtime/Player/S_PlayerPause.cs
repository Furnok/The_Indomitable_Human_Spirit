using UnityEngine;

public class S_PlayerPause : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] RSE_OnPlayerPause _onPlayerPause;

    private void OnEnable()
    {
        _onPlayerPause.action += Pause;
    }

    private void OnDisable()
    {
        _onPlayerPause.action -= Pause;
    }

    void Pause()
    {
        Debug.Log("Player Pause");
    }
}