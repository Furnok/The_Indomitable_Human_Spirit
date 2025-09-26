using UnityEngine;

public class S_Reminiscence : MonoBehaviour
{
    [Header("Settings")]
    [S_TagName] [SerializeField] private string tagPlayer;

    //[Header("References")]

    [Header("Input")]
    [SerializeField] private RSE_OnPlayerInteract rseOnPlayerInteract;

    //[Header("Output")]

    private void OnDisable()
    {
        rseOnPlayerInteract.action -= Interract;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagPlayer))
        {
            rseOnPlayerInteract.action += Interract;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagPlayer))
        {
            rseOnPlayerInteract.action -= Interract;
        }
    }

    private void Interract()
    {
        Debug.Log("Interract");
    }
}