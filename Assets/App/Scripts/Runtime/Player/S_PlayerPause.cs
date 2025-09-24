using UnityEngine;

public class S_PlayerPause : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    [Header("Input")]
    [SerializeField] RSE_OnPlayerPause _onPlayerPause;

    //[Header("Output")]

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