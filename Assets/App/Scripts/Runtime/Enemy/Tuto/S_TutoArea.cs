using DG.Tweening;
using UnityEngine;

public class S_TutoArea : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float _ennemyFinalPosY = 0f;
    [SerializeField] float _looseHealthStartTuto = 30f;


    [Header("References")]
    [SerializeField] S_Enemy _enemyTuto;
    [SerializeField] S_Enemy _enemyDemonTuto;
    [SerializeField] GameObject _enemyVisuals;
    [SerializeField] GameObject _enemyDemonVisuals;
    [SerializeField] Collider _enemyDetectionRange;
    [SerializeField] Collider _enemyDetectionMaxRange;

    [Header("Inputs")]
    [SerializeField] RSO_SettingsSaved _rsoSettingsSaved;
    [SerializeField] RSO_HasEnterAreaTuto _rsoHasEnterAreaTuto;

    [Header("Outputs")]
    [SerializeField] RSE_OnRequestStartTutorialStep _onRequestStartTutorialStep;
    [SerializeField] RSE_OnTutorialStepCompleted _onTutorialStepCompleted;
    [SerializeField] RSE_OnPlayerHealthReduced _onPlayerHealthReduced;


    bool _hasTriggered = false;
    Collider _other;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _hasTriggered == false && _rsoSettingsSaved.Value.activateTuto == true)
        {
            this._other = other;
            _hasTriggered = true;
            _enemyTuto.gameObject.SetActive(true);
            _rsoHasEnterAreaTuto.Value = true;

            if (_rsoSettingsSaved.Value.activateTuto == true)
            {
                _onPlayerHealthReduced.Call(_looseHealthStartTuto);
            }

            _onRequestStartTutorialStep.Call(S_EnumTutorialStep.None);

            _enemyVisuals.transform.DOLocalMoveY(_ennemyFinalPosY, 2).OnComplete(() => SpawnEnemyTuto(other));
        }
    }

    private void Awake()
    {
        if (_enemyTuto != null)
        {
            //_enemyTuto.enabled = false;
            //_enemyTuto.SetTarget(null);
            //_enemyTuto.SetTargetInMaxTravelZone(null);

            _enemyTuto.gameObject.SetActive(false);
        }
        _rsoHasEnterAreaTuto.Value = false;
        _enemyDemonTuto.gameObject.SetActive(false);
    }

    void SpawnEnemyTuto(Collider player)
    {
        if (_enemyTuto != null)
        {
            //_enemyTuto.enabled = true;

            //_enemyTuto.SetTarget(null);
            //_enemyTuto.SetTargetInMaxTravelZone(null);



            //_enemyDetectionRange.enabled = true;
            //_enemyDetectionMaxRange.enabled = true;
            _enemyTuto.SetTargetInMaxTravelZone(player.gameObject);
            _enemyTuto.SetTarget(player.gameObject);

            _onRequestStartTutorialStep.Call(S_EnumTutorialStep.Targeting);
        }
    }

    private void OnEnable()
    {
        _onTutorialStepCompleted.action += OnTutorialStepCompleted;
    }

    private void OnDisable()
    {
        _onTutorialStepCompleted.action -= OnTutorialStepCompleted;
    }

    void OnTutorialStepCompleted(S_EnumTutorialStep tutoStep)
    {
        if (tutoStep == S_EnumTutorialStep.Dodge)
        {
            _enemyDemonTuto.gameObject.SetActive(true);

            _enemyDemonVisuals.transform.DOLocalMoveY(_ennemyFinalPosY, 2).OnComplete(() => SpawnEnemyDemonTuto());
        }
    }

    void SpawnEnemyDemonTuto()
    {
        if (_enemyDemonTuto != null)
        {
            _enemyDemonTuto.SetTargetInMaxTravelZone(_other.gameObject);
            _enemyDemonTuto.SetTarget(_other.gameObject);

            _onRequestStartTutorialStep.Call(S_EnumTutorialStep.SwapTarget);
        }
    }
}