using UnityEngine;

public class S_TutoArea_Heal : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    //[Header("Inputs")]

    [Header("Outputs")]
    [SerializeField] RSE_OnRequestStartTutorialStep _onRequestStartTutorialStep;
    [SerializeField] private RSE_OnPlayerGainConviction _onPlayerGainConviction;

    [SerializeField] RSO_SettingsSaved _rsoSettingsSaved;
    [SerializeField] RSE_OnPlayerHealthReduced _onPlayerHealthReduced;
    [SerializeField] RSO_PlayerCurrentHealth _rsoPlayerCurrentHealth;
    [SerializeField] SSO_PlayerStats _ssoPlayerStats;
    [SerializeField] SSO_PlayerConvictionData _ssoPlayerConvictionData;

    bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _hasTriggered == false && _rsoSettingsSaved.Value.activateTuto == true)
        {
            _hasTriggered = true;

            if(_rsoPlayerCurrentHealth.Value >= _ssoPlayerStats.Value.maxHealth)
            {
                _onPlayerHealthReduced.Call(20);
            }
            _onPlayerGainConviction.Call(_ssoPlayerConvictionData.Value.healCost);

            StartCoroutine(S_Utils.Delay(0.1f, () =>
            {
                Debug.Log("Start Heal Tuto");
                _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Heal);
            }));
        }
    }
}