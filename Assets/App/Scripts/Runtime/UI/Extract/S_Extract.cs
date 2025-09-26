using UnityEngine;

public class S_Extract : MonoBehaviour
{
    [Header("Settings")]
    [S_TagName][SerializeField] string tagPlayer;

    [Header("Input")]
    [SerializeField] SSO_ExtractText SSO_ExtractText;
    [SerializeField] RSE_DisplayExtract RSE_DisplayExtract;
    [SerializeField] RSE_ExtractTextInfo RSE_ExtractTextInfo;

    [Header("Output")]
    [SerializeField] RSE_OnPlayerInteract RSE_OnPlayerInteract;

    private void OnDisable()
    {
        RSE_OnPlayerInteract.action -= ExtractInteract;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagPlayer))
        {
            RSE_OnPlayerInteract.action += ExtractInteract;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagPlayer))
        {
            RSE_OnPlayerInteract.action -= ExtractInteract;
        }
    }

    void ExtractInteract()
    {
        RSE_DisplayExtract.Call(true);
        RSE_ExtractTextInfo.Call(SSO_ExtractText.Value);
    }
}