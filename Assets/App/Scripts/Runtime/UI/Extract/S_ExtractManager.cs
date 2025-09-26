using UnityEngine;

public class S_ExtractManager : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] GameObject extractCanvas;

    //[Header("Input")]

    //[Header("Output")]
    [SerializeField] RSE_DisplayExtract RSE_DisplayExtract;

    private void OnEnable()
    {
        RSE_DisplayExtract.action += DiplayExtract;
    }
    private void OnDisable()
    {
        RSE_DisplayExtract.action -= DiplayExtract;
    }

    void DiplayExtract(bool display)
    {
        if (display)
        {
            extractCanvas.SetActive(true);
        }
        else
        {
            extractCanvas.SetActive(false);
        }
    }
}