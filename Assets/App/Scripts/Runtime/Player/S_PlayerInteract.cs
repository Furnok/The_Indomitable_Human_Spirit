using UnityEngine;

public class S_PlayerInteract : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] RSE_OnPlayerInteract _onPlayerInteract;

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