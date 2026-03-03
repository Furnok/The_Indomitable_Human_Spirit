using Sirenix.OdinInspector;
using UnityEngine;

public class S_PlayerParticleEffectManager : MonoBehaviour
{
    [TabGroup("Settings")]
    [Title("Offsets")]
    [SerializeField] private float _forwardOffsetParry = 0.5f;

    [TabGroup("Settings")]
    [SerializeField] private float _upwardOffsetParry = 0.2f;

    [TabGroup("References")]
    [Title("Parents")]
    [SerializeField] private Transform _particleEffectParent;

    [TabGroup("References")]
    [SerializeField] private Transform _chargingEffectParent;

    [TabGroup("References")]
    [SerializeField] private Transform _targetAttract;

    [TabGroup("References")]
    [SerializeField] private Transform _convictionDodgeEffectParent;

    [TabGroup("References")]
    [Title("Prefab")]
    [SerializeField] private GameObject _prefabParryEffect;

    [TabGroup("References")]
    [SerializeField] private S_ParticlesAttract _prefabParticlesAttractParryGain;

    [TabGroup("References")]
    [SerializeField] private S_ParticlesAttract _prefabParticlesAttractDodgeGain;

    [TabGroup("References")]
    [SerializeField] private ParticleSystem _healPaticleSystem;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnParrySuccess _onParrySuccess;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnAttackStartPerformed _onAttackStartPerformed;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerGettingHit _rseOnPlayerGettingHit;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnSpawnProjectile rseOnSpawnProjectile;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerDodgePerfect _rseOnDodgePerfect;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerHealPerformed _rseOnPlayerHealPerfomed;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerConvictionData _playerConvictionData;


    S_ParticlesAttract attract;
    private void Awake()
    {
        attract = Instantiate(_prefabParticlesAttractDodgeGain, _convictionDodgeEffectParent.position, _convictionDodgeEffectParent.rotation, _convictionDodgeEffectParent);

    }

    private void OnEnable()
    {
        _onParrySuccess.action += ActiveParryEffect;

        _onAttackStartPerformed.action += ActiveChargeEffect;
        _rseOnPlayerGettingHit.action += DeactiveChargeEffect;
        rseOnSpawnProjectile.action += DeactiveChargeEffect;
        _rseOnDodgePerfect.action += ActiveDodgeEffect;

        _rseOnPlayerHealPerfomed.action += PlayHealEffect;
    }

    private void OnDisable()
    {
        _onParrySuccess.action -= ActiveParryEffect;

        _onAttackStartPerformed.action -= ActiveChargeEffect;
        _rseOnPlayerGettingHit.action -= DeactiveChargeEffect;
        rseOnSpawnProjectile.action -= DeactiveChargeEffect;
        _rseOnDodgePerfect.action -= ActiveDodgeEffect;

        _rseOnPlayerHealPerfomed.action -= PlayHealEffect;
    }

    private void ActiveParryEffect(S_StructAttackContact contact)
    {
        Vector3 offset = transform.forward * _forwardOffsetParry + transform.up * _upwardOffsetParry;

        var spawnPoint = contact.data.contactPoint + offset;

        var parryeffect = Instantiate(_prefabParryEffect, spawnPoint, Quaternion.identity, _particleEffectParent);
        var attract = Instantiate(_prefabParticlesAttractParryGain, spawnPoint, _targetAttract.rotation, _targetAttract);

        attract.InitializeTransform(_targetAttract, contact.data.convictionParryGain);

        Destroy(parryeffect, 2f);
    }

    private void ActiveDodgeEffect()
    {
        StartCoroutine(S_Utils.Delay(0.1f, () =>
        {
            attract.InitializeTransform(_targetAttract, _playerConvictionData.Value.dodgeSuccessGain);

        }));
    }

    private void ActiveChargeEffect()
    {
        _chargingEffectParent.gameObject.SetActive(true);
    }

    private void DeactiveChargeEffect()
    {
        _chargingEffectParent.gameObject.SetActive(false);
    }

    private void DeactiveChargeEffect(float value)
    {
        _chargingEffectParent.gameObject.SetActive(false);
    }

    private void PlayHealEffect()
    {
        _healPaticleSystem.Play();
    }
}