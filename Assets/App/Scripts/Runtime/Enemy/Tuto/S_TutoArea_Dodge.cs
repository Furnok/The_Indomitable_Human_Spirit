using UnityEngine;

public class S_TutoArea_Dodge : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] S_Enemy _enemyTuto;

    [Header("Inputs")]
    [SerializeField] RSO_SettingsSaved _rsoSettingsSaved;

    [Header("Outputs")]
    [SerializeField] RSE_OnRequestStartTutorialStep _onRequestStartTutorialStep;


    Collider _other;
    bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _hasTriggered == false && _rsoSettingsSaved.Value.activateTuto == true && other != null)
        {
            this._other = other;
            _hasTriggered = true;
            _enemyTuto.SetTargetInMaxTravelZone(_other.gameObject);
            _enemyTuto.SetTarget(_other.gameObject);

            _onRequestStartTutorialStep.Call(S_EnumTutorialStep.None);
        }
    }
}