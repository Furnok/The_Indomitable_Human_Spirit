using UnityEngine;

public class S_TutoArea_FirstEncounter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float _distanceToTriggerTuto = 5f;

    [Header("References")]
    [SerializeField] S_Enemy _enemyTuto;

    [Header("Inputs")]
    [SerializeField] RSO_SettingsSaved _rsoSettingsSaved;
    [SerializeField] RSE_OnTutorialStepCompleted _onTutorialStepCompleted;
    [SerializeField] RSE_OnRequestAcceptedTutorialStep _onRequestAcceptedTutorialStep;

    [Header("Outputs")]
    [SerializeField] RSE_OnRequestStartTutorialStep _onRequestStartTutorialStep;
    [SerializeField] private RSE_OnPlayerTargetingCancel rseOnPlayerTargetingCancel;


    Collider _other;
    bool _hasTriggered = false;
    bool _playerInRange = false;

    private void OnEnable()
    {
        _onTutorialStepCompleted.action += OnFinishTutoAttack;
        _onRequestAcceptedTutorialStep.action += OnStartTutoAttack;
    }

    private void OnDisable()
    {
        _onTutorialStepCompleted.action -= OnFinishTutoAttack;
        _onRequestAcceptedTutorialStep.action -= OnStartTutoAttack;
    }


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

    private void FixedUpdate()
    {
        if (_hasTriggered == true && _playerInRange == false)
        {
            if (_other == null)
            {
                Debug.LogError("Player is null");
                return;
            }
          

            if (Vector3.Distance(_other.transform.position, this.transform.position) < _distanceToTriggerTuto)
            {
                _playerInRange = true;
                rseOnPlayerTargetingCancel.Call();

                StartCoroutine(S_Utils.Delay(0.1f, () =>
                {
                    _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Targeting);
                }));
            }
        }

    }

    void OnStartTutoAttack(S_EnumTutorialStep Step)
    {
        //if (Step == S_EnumTutorialStep.Attack)
        //    _enemyTuto.enabled = false;
    }

    void OnFinishTutoAttack(S_EnumTutorialStep Step)
    {
        if (Step == S_EnumTutorialStep.Parry)
            _enemyTuto.enabled = false;

        if (Step == S_EnumTutorialStep.Attack)
            _enemyTuto.enabled = true;
    }
}