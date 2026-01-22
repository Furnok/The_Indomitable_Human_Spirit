using UnityEngine;

public class S_WallTriggerTuto : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] RSO_SettingsSaved _rsoSettingsSaved;

    //[Header("Inputs")]

    [Header("Outputs")]
    [SerializeField] RSE_OnPlayerLooseConviction _onPlayerLooseConviction;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")  && _rsoSettingsSaved.Value.activateTuto == true)
        {
            _onPlayerLooseConviction.Call(10000f);
        }
    }
}