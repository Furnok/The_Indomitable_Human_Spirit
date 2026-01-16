using DG.Tweening;
using UnityEngine;

public class S_TutoArea : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float _ennemyFinalPosY = 0f;

    [Header("References")]
    [SerializeField] S_Enemy _enemyTuto;
    [SerializeField] GameObject _enemyVisuals;
    [SerializeField] Collider _enemyDetectionRange;
    [SerializeField] Collider _enemyDetectionMaxRange;

    //[Header("Inputs")]

    [Header("Outputs")]
    [SerializeField] RSE_OnRequestStartTutorialStep _onRequestStartTutorialStep;

    bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _hasTriggered == false)
        {
            _hasTriggered = true;
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
        }

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
}