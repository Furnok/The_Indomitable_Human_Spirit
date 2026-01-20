using Sirenix.OdinInspector;
using UnityEngine;

public class S_PlayerDeath : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Collider")]
    [SerializeField] private GameObject _playerHurtBoxCollider;

    [TabGroup("References")]
    [Title("Rigidbody")]
    [SerializeField] Rigidbody _playerRigidbody;

    [TabGroup("References")]
    [Title("Aim Point")]
    [SerializeField] private GameObject _aimPointObject;

    [TabGroup("References")]
    [Title("Others")]
    [SerializeField] GameObject _visuals;

    [TabGroup("References")]
    [SerializeField] GameObject _player;

    [TabGroup("References")]
    [SerializeField] GameObject _colliderMotorGO;

    [TabGroup("References")]
    [SerializeField] Collider _colliderMotor;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerDeath _onPlayerDeathRse;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerRespawn _onPlayerRespawnRse;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationBoolValueChange _rseOnAnimationBoolValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerDead _PlayerDead;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerRespawnPosition rsoPlayerRespawnPosition;

    private void OnEnable()
    {
        _onPlayerDeathRse.action += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        _onPlayerDeathRse.action -= HandlePlayerDeath;
    }

    private void Start()
    {
        StartCoroutine(S_Utils.DelayRealTime(0.1f, () =>
        {
            rsoPlayerRespawnPosition.Value.position = _player.transform.position;
            rsoPlayerRespawnPosition.Value.rotation = _player.transform.rotation;
        }));
    }

    private void HandlePlayerDeath()
    {
        _PlayerDead.Value = true;
        _rseOnAnimationBoolValueChange.Call("isDead", true);

        _playerHurtBoxCollider.SetActive(false);
        _aimPointObject.SetActive(false);

        _playerRigidbody.linearVelocity = Vector3.zero;

        _colliderMotor.providesContacts = false;
        _colliderMotor.enabled = false;

        _colliderMotorGO.SetActive(false);

        _playerRigidbody.useGravity = false;

        Physics.SyncTransforms();
    }
}