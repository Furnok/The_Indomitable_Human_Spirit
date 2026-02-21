using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI;

public class S_BossAttack : MonoBehaviour
{
    [TabGroup("Settings")]
    [SerializeField] private SSO_BossData bossData;

    [TabGroup("References")]
    [Title("Colliders")]
    [SerializeField] private Collider bodyCollider;

    [TabGroup("References")]
    [Title("RigidBody")]
    [SerializeField] private Rigidbody rbBody;

    [TabGroup("References")]
    [Title("Projectile")]
    [SerializeField] private S_BossProjectile bossProjectile;

    [TabGroup("References")]
    [SerializeField] private S_EnemyProjectile enemyProjectile;

    [TabGroup("References")]
    [SerializeField] private GameObject projectilePingPongSpawn;

    [TabGroup("References")]
    [SerializeField] private GameObject projectileBallsSpawn;

    [TabGroup("References")]
    [SerializeField] private GameObject fireProjectileSpawn;

    [TabGroup("References")]
    [Title("Scripts")]
    [SerializeField] private S_BossRootMotionModifier rootMotionModifier;

    [TabGroup("References")]
    [SerializeField] private S_BossAttackData bossAttackData;

    [TabGroup("References")]
    [SerializeField] private S_FireProjectile fireProjectile;

    [TabGroup("References")]
    [Title("Center")]
    [SerializeField] private Transform aimPointBoss;

    [TabGroup("References")]
    [Title("Boss")]
    [SerializeField] private GameObject boss;

    [TabGroup("References")]
    [SerializeField] private NavMeshAgent bossNavMeshAgent;

    [TabGroup("References")]
    [Title("Animator")]
    [SerializeField] private Animator animator;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string attackParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string comboParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string jumpParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string flyParam;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnExecuteAttack onExecuteAttack;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnEndFly onEndFly;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnBossDeath rseOnBossDeath;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerDeath rseOnPlayerDeath;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayParticle rseOnPlayParticle;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnStopParticle rseOnStopParticle;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnEndAttack rseOnEndAttack;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnBossStun rseOnBossStun;

    [HideInInspector] public Transform aimPointPlayer = null;

    private S_ClassBossAttack currentAttack = null;
    [HideInInspector] public AnimatorOverrideController overrideController = null;
    private Vector3 pingPongStartPos;
    private Vector3 pingPongPeakPos;
    private int pingPongAnimNumb = 0;
    private bool pingPongInAir = false;

    private Coroutine pingPongJumpCoroutine = null;
    private Coroutine pingPongFlyCoroutine = null;
    private Coroutine pingPongDescendCoroutine = null;
    private Coroutine pingPongCoroutine = null;

    private Coroutine ballsFlyCoroutine = null;
    private Coroutine ballsCoroutine = null;

    private Coroutine gatheringJumpCoroutine = null;
    private Coroutine gatheringCoroutine = null;

    private Coroutine wingsFlyCoroutine = null;
    private Coroutine wingsCoroutine = null;

    private void OnEnable()
    {
        onExecuteAttack.action += DoAttackChoose;
        onEndFly.action += PingPongDescend;
        rseOnBossDeath.action += StopAllAttack;
        rseOnPlayerDeath.action += StopAllAttack;
    }

    private void OnDisable()
    {
        onExecuteAttack.action -= DoAttackChoose;
        onEndFly.action -= PingPongDescend;
        rseOnBossDeath.action -= StopAllAttack;
        rseOnPlayerDeath.action -= StopAllAttack;
    }

    private void DoAttackChoose(S_ClassBossAttack attack)
    {
        currentAttack = attack;

        switch (currentAttack.attackName)
        {
            case "PingPong":
                PingPong();
                break;
            case "Balls":
                Balls();
                break;
            case "Gathering":
                Gathering();
                break;
            case "WingsOfHell":
                WingsOfHell();
                break;
        }
    }

    #region Attack Phase 2
    private void StopAllAttack()
    {
        StopAllCoroutines();

        if (pingPongJumpCoroutine != null)
        {
            StopCoroutine(pingPongJumpCoroutine);
            pingPongJumpCoroutine = null;
        }
        if (pingPongFlyCoroutine != null)
        {
            StopCoroutine(pingPongFlyCoroutine);
            pingPongFlyCoroutine = null;
        }
        if (pingPongCoroutine != null)
        {
            StopCoroutine(pingPongCoroutine);
            pingPongCoroutine = null;
        }
        if (pingPongDescendCoroutine != null)
        {
            StopCoroutine(pingPongDescendCoroutine);
            pingPongDescendCoroutine = null;
        }

        if (ballsFlyCoroutine != null)
        {
            StopCoroutine(ballsFlyCoroutine);
            ballsCoroutine = null;
        }
        if (ballsCoroutine != null)
        {
            StopCoroutine(ballsFlyCoroutine);
            ballsCoroutine = null;
        }

        if (gatheringJumpCoroutine != null)
        {
            StopCoroutine(gatheringJumpCoroutine);
            gatheringJumpCoroutine = null;
        }
        if (gatheringCoroutine != null)
        {
            StopCoroutine(gatheringCoroutine);
            gatheringCoroutine = null;
        }
        if(wingsCoroutine != null)
        {
            StopCoroutine(wingsCoroutine);
            wingsCoroutine = null;
        }
        if(wingsFlyCoroutine != null)
        {
            StopCoroutine(wingsFlyCoroutine);
            wingsFlyCoroutine = null;
        }
    }

    private IEnumerator DoJump(Rigidbody rb, float distance, float jumpPower, float duration, int animStartIndex, Action<int> onComplete)
    {
        int animNumb = animStartIndex;

        bossNavMeshAgent.enabled = false;

        Vector3 dir = rb.position - aimPointPlayer.position;

        if (dir.sqrMagnitude <= 0.0001f)
            dir = rb.transform.forward;

        dir.Normalize();

        Vector3 desiredTarget = rb.position + dir * distance;

        Vector3 target = GetSafeJumpTarget(desiredTarget, rb.position, distance);

        bool finished = false;

        rb.DOJump(target, jumpPower, 1, duration)
            .SetEase(Ease.OutQuad)
            .OnStart(() =>
            {
                animator.SetTrigger(jumpParam);
            })
            .OnComplete(() =>
            {
                finished = true;
            });

        yield return new WaitUntil(() => finished);

        onComplete?.Invoke(animNumb);
    }

    private IEnumerator DoFly(float jumpPower, float duration, int animStartIndex, Action<int> onComplete)
    {
        int animNumb = animStartIndex;

        bossNavMeshAgent.enabled = false;

        pingPongStartPos = rbBody.position;

        pingPongPeakPos =
            pingPongStartPos + Vector3.up * jumpPower;


        bool finished = false;

        rbBody.DOMove(pingPongPeakPos, duration)
            .SetEase(Ease.OutQuad)
            .OnStart(() =>
            {
                animator.SetTrigger(flyParam);
            })
            .OnComplete(() =>
            {
                rbBody.isKinematic = true;
                pingPongInAir = true;
                finished = true;
            });

        yield return new WaitUntil(() => finished);

        onComplete?.Invoke(animNumb);
    }

    #region PingPong
    private void PingPong()
    {
        pingPongJumpCoroutine = StartCoroutine(DoJump(rbBody, bossData.Value.jumpDistancePingPong, bossData.Value.jumpPowerPingPong, bossData.Value.jumpDurationPingPong, 0, (animIndex) => 
        { pingPongFlyCoroutine = StartCoroutine(DoFly(bossData.Value.flyPowerPingPong, bossData.Value.flyDurationPingPong, animIndex, (nextIndex) => 
        { pingPongCoroutine = StartCoroutine(PingPongCoroutine(nextIndex));
        }));
        }));
    }
    
    private void PingPongDescend()
    {
        if (pingPongInAir)
        {
            if (pingPongDescendCoroutine != null)
            {
                StopCoroutine(pingPongDescendCoroutine);
                pingPongDescendCoroutine = null;
            }
            pingPongDescendCoroutine = StartCoroutine(PingPongDescendCoroutine());
        }
    }
    private IEnumerator PingPongDescendCoroutine()
    {
        if (!pingPongInAir)
            yield break;

        bossNavMeshAgent.enabled = false;
        rbBody.isKinematic = false;
        int animNumb = pingPongAnimNumb;

        rbBody.DOMove(pingPongStartPos, bossData.Value.flyDurationPingPong)
            .SetEase(Ease.InQuad)
            .OnStart(() =>
            {
                animator.SetTrigger(flyParam);
            })
            .OnComplete(() =>
            {
                rseOnEndAttack.Call();
                pingPongAnimNumb = 0;
                bossNavMeshAgent.enabled = true;
                pingPongInAir = false;
            });
    }
    private IEnumerator PingPongCoroutine(int value)
    {
        yield return null;
        bossNavMeshAgent.enabled = false;
        for (int i = value; i < currentAttack.listComboData.Count; i++)
        {
            string overrideKey = (i % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
            overrideController[overrideKey] = currentAttack.listComboData[i].animation;

            rootMotionModifier.Setup(currentAttack.listComboData[i].rootMotionMultiplier);

            bossAttackData.SetAttackMode(currentAttack.listComboData[i].attackData);

            if (currentAttack.listComboData[i].showVFXAttackType) bossAttackData.VFXAttackType();

            animator.SetTrigger(i == 0 ? attackParam : comboParam);

            yield return new WaitForSeconds(currentAttack.listComboData[i].animation.length);

            if (currentAttack.listComboData[i].attackData.attackType == S_EnumAttackType.Projectile)
            {
                yield return new WaitForSeconds(currentAttack.listComboData[i].attackData.timeCast);

                S_BossProjectile projectileInstance = Instantiate(bossProjectile, projectilePingPongSpawn.transform.position, Quaternion.identity);
                projectileInstance.Initialize(aimPointBoss, aimPointPlayer, currentAttack.listComboData[i].attackData);
            }
            yield return null;
            pingPongAnimNumb = i;
        }
    }
    #endregion

    #region Balls
    private void Balls()
    {
        ballsFlyCoroutine = StartCoroutine(DoFly(bossData.Value.flyPowerBalls, bossData.Value.flyDurationBalls, 0, (animIndex) => { ballsCoroutine = StartCoroutine(BallsCoroutine(animIndex));}));
    }

    private IEnumerator BallsCoroutine(int value)
    {
        yield return null;
        bossNavMeshAgent.enabled = false;
        for (int i = value; i < currentAttack.listComboData.Count; i++)
        {
            string overrideKey = (i % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
            overrideController[overrideKey] = currentAttack.listComboData[i].animation;

            rootMotionModifier.Setup(currentAttack.listComboData[i].rootMotionMultiplier);

            bossAttackData.SetAttackMode(currentAttack.listComboData[i].attackData);

            if (currentAttack.listComboData[i].showVFXAttackType) bossAttackData.VFXAttackType();

            animator.SetTrigger(i == 0 ? attackParam : comboParam);

            yield return new WaitForSeconds(currentAttack.listComboData[i].animation.length);

            if (currentAttack.listComboData[i].attackData.attackType == S_EnumAttackType.Projectile)
            {
                for (int j = 0; j < currentAttack.listComboData[i].attackData.numberOfProjectiles; j++)
                {
                    yield return new WaitForSeconds(currentAttack.listComboData[i].attackData.timeCast);

                    S_EnemyProjectile projectileInstance = Instantiate(enemyProjectile, projectileBallsSpawn.transform.position, Quaternion.identity);
                    projectileInstance.Initialize(aimPointBoss, aimPointPlayer, currentAttack.listComboData[i].attackData);
                }
            }
            yield return new WaitForSeconds(currentAttack.listComboData[i].attackData.timeInterval);

        }
        yield return null;
        rbBody.isKinematic = false;
        bossNavMeshAgent.enabled = true;
        rseOnBossStun.Call(S_EnumBossState.Stun);
    } 
    #endregion

    #region Gathering
    private void Gathering()
    {
        gatheringJumpCoroutine = StartCoroutine(DoJump(rbBody, bossData.Value.jumpDistanceGathering, bossData.Value.jumpPowerGathering, bossData.Value.jumpDurationGathering, 0, (animIndex) => { gatheringCoroutine = StartCoroutine(GatheringCoroutine(animIndex));}));
    }

    private IEnumerator GatheringCoroutine(int value)
    {
        int animNumb = value;

        string overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        rseOnStopParticle.Call();

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;

        overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);

        if (currentAttack.listComboData[animNumb].showVFXAttackType) bossAttackData.VFXAttackType();

        S_FireProjectile s_FireProjectile = Instantiate(fireProjectile, fireProjectileSpawn.transform.position, Quaternion.identity);
        s_FireProjectile.Initialize(transform.rotation, currentAttack.listComboData[animNumb].attackData);

        rseOnPlayParticle.Call();
        rbBody.isKinematic = false;
        bossNavMeshAgent.enabled = true;
        rseOnEndAttack.Call();
    }
    #endregion

    #region WingsOfHell
    private void WingsOfHell()
    {
        wingsFlyCoroutine = StartCoroutine(DoFly( bossData.Value.flyPowerWings, bossData.Value.flyDurationWings, 0, (animIndex) => { wingsCoroutine = StartCoroutine(WingsOfHellCoroutine(animIndex)); }));
    }
    private IEnumerator DescendCoroutine()
    {
        if (!pingPongInAir)
            yield break;

        bossNavMeshAgent.enabled = false;
        rbBody.isKinematic = false;


        rbBody.DOMove(pingPongStartPos, bossData.Value.flyDurationWings)
            .SetEase(Ease.InQuad)
            .OnStart(() =>
            {
                animator.SetTrigger(flyParam);
            })
            .OnComplete(() =>
            {
                bossNavMeshAgent.enabled = true;
                rbBody.isKinematic = false;
                pingPongInAir = false;
            });
    }
    private IEnumerator WingsOfHellCoroutine(int index)
    {
        yield return null;
        bossNavMeshAgent.enabled = false;
        
        for (int i = index; i < currentAttack.listComboData.Count; i++)
        {
            string overrideKey = (i % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
            overrideController[overrideKey] = currentAttack.listComboData[i].animation;

            rootMotionModifier.Setup(currentAttack.listComboData[i].rootMotionMultiplier);

            bossAttackData.SetAttackMode(currentAttack.listComboData[i].attackData);

            if (currentAttack.listComboData[i].showVFXAttackType) bossAttackData.VFXAttackType();

            animator.SetTrigger(i == 0 ? attackParam : comboParam);

            yield return new WaitForSeconds(currentAttack.listComboData[i].animation.length);

            if (currentAttack.listComboData[i].attackData.attackType == S_EnumAttackType.Projectile)
            {
                for (int j = 0; j < currentAttack.listComboData[i].attackData.numberOfProjectiles; j++)
                {
                    yield return new WaitForSeconds(currentAttack.listComboData[i].attackData.timeCast);

                    S_EnemyProjectile projectileInstance = Instantiate(enemyProjectile, projectileBallsSpawn.transform.position, Quaternion.identity);
                    projectileInstance.Initialize(aimPointBoss, aimPointPlayer, currentAttack.listComboData[i].attackData);
                }
                StartCoroutine(DescendCoroutine());
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(currentAttack.listComboData[i].attackData.timeInterval);  
        }

        yield return null;
        rbBody.isKinematic = false;
        bossNavMeshAgent.enabled = true;
        rseOnEndAttack.Call();
    }
    #endregion

    private Vector3 GetSafeJumpTarget(Vector3 desired, Vector3 fallbackPosition, float maxSampleDistance)
    {
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(desired, out navHit, maxSampleDistance, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        RaycastHit hit;
        Vector3 rayStart = desired + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f))
        {
            return hit.point;
        }

        return fallbackPosition;
    }
    #endregion
}