using Sirenix.OdinInspector;
using UnityEngine;

public class S_PlayerRespawn : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Animation")]
    [SerializeField, S_AnimationName] private string _deadParam;

    [TabGroup("References")]
    [Title("Collider")]
    [SerializeField] private GameObject _playerHurtBoxCollider;

    [TabGroup("References")]
    [Title("Rigidbody")]
    [SerializeField] private Rigidbody _playerRigidbody;

    [TabGroup("References")]
    [Title("Aim Point")]
    [SerializeField] private GameObject _aimPointObject;

    [TabGroup("References")]
    [Title("Others")]
    [SerializeField] private GameObject _colliderMotorGO;

    [TabGroup("References")]
    [Title("Others")]
    [SerializeField] private Collider _colliderMotor;

    [TabGroup("References")]
    [SerializeField] private GameObject _visuals;

    [TabGroup("References")]
    [SerializeField] private GameObject _player;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerRespawn _onPlayerRespawnRse;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerAddState _onPlayerAddStateRse;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationBoolValueChange _onAnimationBoolValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerHealthUpdate _onPlayerHealthUpdate;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerConvictionUpdate _onPlayerConvictionUpdate;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnTPCam _onTPCam;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerRespawnPosition _playerRespawnPosition;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentHealth _playerCurrentHealth;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentConviction _playerCurrentConviction;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerDead _PlayerDead;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats _playerStats;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerConvictionData _playerConvictionData;
   
    private void OnEnable()
    {
        _onPlayerRespawnRse.action += HandlePlayerRespawn;
    }

    private void OnDisable()
    {
        _onPlayerRespawnRse.action -= HandlePlayerRespawn;

        // Reset respawn position for now change it afterwards
        _playerRespawnPosition.Value.rotation = Quaternion.identity;
    }

    private void HandlePlayerRespawn()
    {
        _player.transform.position = _playerRespawnPosition.Value.position;
        _player.transform.rotation = _playerRespawnPosition.Value.rotation;

        Physics.SyncTransforms();

        _onTPCam.Call();

        _playerRigidbody.useGravity = true;

        _aimPointObject.SetActive(true);
        _playerHurtBoxCollider.SetActive(true);

        _colliderMotorGO.SetActive(true);
        _colliderMotor.providesContacts = true;
        _colliderMotor.enabled = true;

        _playerRigidbody.linearVelocity = Vector3.zero;

        _onAnimationBoolValueChange.Call(_deadParam, false);

        _onPlayerAddStateRse.Call(S_EnumPlayerState.None);

        _playerCurrentHealth.Value = _playerStats.Value.maxHealth;
        _onPlayerHealthUpdate.Call(_playerCurrentHealth.Value);

        _playerCurrentConviction.Value = _playerConvictionData.Value.startConviction;
        _onPlayerConvictionUpdate.Call(_playerCurrentConviction.Value);

        _PlayerDead.Value = false;
    }
}