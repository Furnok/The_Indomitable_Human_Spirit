using UnityEngine;

public class S_PlayerInteract : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    [Header("Input")]
    [SerializeField] RSE_OnPlayerInteract _onPlayerInteract;

    //[Header("Output")]

    private void OnEnable()
    {
        _onPlayerInteract.action += Interact;
    }

    private void OnDisable()
    {
        _onPlayerInteract.action -= Interact;
    }

    void Interact()
    {
        Debug.Log("Player Interact");
    }
}