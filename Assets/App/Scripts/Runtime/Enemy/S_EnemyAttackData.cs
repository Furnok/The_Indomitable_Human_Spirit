using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class S_EnemyAttackData : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Colliders")]
    [SerializeField] private Collider weaponCollider;

    [TabGroup("References")]
    [Title("VFX")]
    [SerializeField] private ParticleSystem particleDodgeType;

    [TabGroup("References")]
    [SerializeField] private ParticleSystem particleParryType;

    [TabGroup("References")]
    [SerializeField] private List<ParticleSystem> particlesTrail;

    [TabGroup("References")]
    [Title("Scripts")]
    [SerializeField] private S_Enemy enemy;

    [TabGroup("References")]
    [SerializeField] private S_EnemyWeapon enemyWeapon;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnRequestStartTutorialStep rseOnRequestStartTutorialStep;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCameraFOV rseOnCameraFOV;

    private S_StructEnemyAttackData attackData;

    public void SetAttackMode(S_StructEnemyAttackData enemyAttackData)
    {
        attackData = enemyAttackData;

        if (enemyWeapon != null) enemyWeapon.ChangeAttackData(attackData);
    }

    public void EnableWeaponCollider()
    {
        if (weaponCollider != null) weaponCollider.enabled = true;
    }

    public void DisableWeaponCollider()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void VFXAttackType()
    {
        if (attackData.attackType == S_EnumEnemyAttackType.Parryable || attackData.attackType == S_EnumEnemyAttackType.Projectile)
        {
            if (particleParryType != null) particleParryType.Play();
        }
        else if (attackData.attackType == S_EnumEnemyAttackType.Dodgeable)
        {
            if (particleDodgeType != null) particleDodgeType.Play();
        }
    }

    public void Rotate()
    {
        enemy.RotateEnemyAnim();
    }

    public void StopRotate()
    {
        enemy.StopRotateEnemyAnim();
    }

    public void PlayFmod(string eventName)
    {
        RuntimeManager.PlayOneShot(eventName, transform.position);
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

    public void TriggerTutorialParryStep()
    {
        if (rseOnRequestStartTutorialStep != null) rseOnRequestStartTutorialStep.Call(S_EnumTutorialStep.Parry);
    }
    public void TriggerTutorialDodgeStep()
    {
        if (rseOnRequestStartTutorialStep != null) rseOnRequestStartTutorialStep.Call(S_EnumTutorialStep.Dodge);
    }

    public void TriggerTutorialAttackSignalStep()
    {
        if (rseOnRequestStartTutorialStep != null) rseOnRequestStartTutorialStep.Call(S_EnumTutorialStep.AttackSignaling);
    }

    public void DesactivateEnemy()
    {
        enemy.enabled = false;
    }

    public void SetFOVCam(float value)
    {
        rseOnCameraFOV.Call(value);
    }
}