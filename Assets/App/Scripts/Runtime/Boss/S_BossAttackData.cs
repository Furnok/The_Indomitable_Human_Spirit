using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class S_BossAttackData : MonoBehaviour
{
    [TabGroup("Settings")]
    [Title("Times")]
    [SuffixLabel("s", Overlay = true)]
    [SerializeField] private float timeDisplay;

    [TabGroup("References")]
    [Title("GameObject")]
    [SerializeField] private GameObject afterImageWeaponPrefabs;

    [TabGroup("References")]
    [Title("Colliders")]
    [SerializeField] private Collider weaponCollider;

    [TabGroup("References")]
    [SerializeField] private Collider afterImageWeaponCollider;

    [TabGroup("References")]
    [Title("VFX")]
    [SerializeField] private ParticleSystem particleDodgeType;

    [TabGroup("References")]
    [SerializeField] private ParticleSystem particleParryType;

    [TabGroup("References")]
    [SerializeField] private List<ParticleSystem> particlesTrail;

    [TabGroup("References")]
    [Title("Scripts")]
    [SerializeField] private S_Boss boss;

    [TabGroup("References")]
    [SerializeField] private S_BossWeapon bossWeapon;

    [TabGroup("References")]
    [SerializeField] private S_BossWeapon bossAfterImageWeapon;

    private S_StructAttackData attackData;

    public void SetAttackMode(S_StructAttackData bossAttackData)
    {
        attackData = bossAttackData;

        if (bossWeapon != null) bossWeapon.ChangeAttackData(attackData);

        if (bossAfterImageWeapon != null) bossAfterImageWeapon.ChangeAttackData(attackData);
    }

    public void EnableWeaponCollider()
    {
        if (weaponCollider != null) weaponCollider.enabled = true;
    }

    public void DisableWeaponCollider()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void EnableAfterImageWeaponCollider()
    {
        if (afterImageWeaponCollider != null) afterImageWeaponCollider.enabled = true;
    }
    public void DisableAfterImageWeaponCollider()
    {
        if (afterImageWeaponCollider != null) afterImageWeaponCollider.enabled = false;
    }

    public void EnableAfterImageWeapon()
    {
        if (afterImageWeaponPrefabs != null) afterImageWeaponPrefabs.SetActive(true);
    }
    public void DisableAfterImageWeapon()
    {
        if (afterImageWeaponPrefabs != null) afterImageWeaponPrefabs.SetActive(false);
    }

    public void Rotate()
    {
        boss.RotateEnemyAnim();
    }

    public void StopRotate()
    {
        boss.StopRotateEnemyAnim();
    }

    public void PlayFmod(string eventName)
    {
        RuntimeManager.PlayOneShot(eventName, transform.position);
    }

    public void VFXAttackType()
    {
        if (attackData.attackType == S_EnumAttackType.Parryable || attackData.attackType == S_EnumAttackType.Projectile)
        {
            if (particleParryType != null) particleParryType.Play();
        }
        else if (attackData.attackType == S_EnumAttackType.Dodgeable)
        {
            if (particleDodgeType != null) particleDodgeType.Play();
        }
    }

    public void VFXStartTrail()
    {
        if (particlesTrail == null || particlesTrail.Count == 0) return;

        foreach (ParticleSystem particle in particlesTrail) particle.Play();
    }

    public void VFXStopTrail()
    {
        if (particlesTrail == null || particlesTrail.Count == 0) return;

        foreach (ParticleSystem particle in particlesTrail) particle.Stop();
    }
}