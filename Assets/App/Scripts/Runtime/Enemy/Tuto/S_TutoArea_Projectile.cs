using UnityEngine;

public class S_TutoArea_Projectile : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    //[Header("Inputs")]

    [Header("Outputs")]
    [SerializeField] RSO_HasEnterAreaTuto _rsoHasEnterAreaTuto;
    [SerializeField] RSO_SettingsSaved _rsoSettingsSaved;

    bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _hasTriggered == false && _rsoSettingsSaved.Value.activateTuto == true)
        {
            _rsoHasEnterAreaTuto.Value = true;
            _hasTriggered = true;

        }
    }

}