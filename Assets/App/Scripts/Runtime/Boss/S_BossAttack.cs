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
    [Title("Gethering Settings")]
    [SerializeField] private float backDistanceGathering;

    [TabGroup("Settings")]
    [SerializeField] private float jumpPowerGathering;

    [TabGroup("Settings")]
    [SerializeField] private float durationGathering;

    [TabGroup("Settings")]
    [Title("PingPong Settings")]
    [SerializeField] private float backDistancePingPong;

    [TabGroup("Settings")]
    [SerializeField] private float jumpPowerPingPong;

    [TabGroup("Settings")]
    [SerializeField] private float durationPingPong;

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
    }
    
    #region PingPong
    private void PingPong()
    {
        if (pingPongJumpCoroutine != null)
        {
            StopCoroutine(pingPongJumpCoroutine);
            pingPongJumpCoroutine = null;
        }
        pingPongJumpCoroutine = StartCoroutine(PingPongJump(rbBody, aimPointPlayer.position, backDistancePingPong, jumpPowerPingPong, durationPingPong, 1));
    }
    private IEnumerator PingPongJump(Rigidbody rb, Vector3 playerPos, float distance, float jumpPower, float duration, int numJumps)
    {
        int animNumb = 0;
        bossNavMeshAgent.enabled = false;

        Vector3 dir = rb.position - playerPos;
        if (dir.sqrMagnitude <= 1e-6f) dir = rb.transform.forward;
        dir.Normalize();

        Vector3 desiredTarget = rb.position + dir * distance;

        Vector3 target = GetSafeJumpTarget(desiredTarget, rb.position, distance);

        string overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play Jump Prepa Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;

        Sequence seq = rb.DOJump(target, jumpPower, numJumps, duration)
            .SetEase(Ease.OutQuad).OnStart(() =>
            {
                overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
                overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

                Debug.Log("Play Jump Animation");
                animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);
                animNumb++;
            })
            .OnComplete(() =>
            {
                if (pingPongFlyCoroutine != null)
                {
                    StopCoroutine(pingPongFlyCoroutine);
                    pingPongFlyCoroutine = null;
                }
                pingPongFlyCoroutine = StartCoroutine(PingPongFly(animNumb));
            });
        yield return null;
    }
    private IEnumerator PingPongFly(int value)
    {
        if (currentAttack == null || currentAttack.listComboData == null || currentAttack.listComboData.Count == 0)
            yield break;

        int animNumb = value;
        bossNavMeshAgent.enabled = false;

        string overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play JumpFall Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);


        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;
        pingPongStartPos = rbBody.position;
        pingPongPeakPos = pingPongStartPos + Vector3.up * jumpPowerPingPong;

        overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play Fly Prepa Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;

        rbBody.DOMove(pingPongPeakPos, durationPingPong)
            .SetEase(Ease.OutQuad)
            .OnStart(() =>
            {
                overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
                if (animNumb < currentAttack.listComboData.Count)
                    overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

                Debug.Log("Play Fly Up Animation");
                animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);
                animNumb++;
            })
            .OnComplete(() =>
            {
                rbBody.isKinematic = true;
                pingPongAnimNumb = animNumb;
                pingPongInAir = true;

                if (pingPongCoroutine != null)
                {
                    StopCoroutine(pingPongCoroutine);
                    pingPongCoroutine = null;
                }
                pingPongCoroutine = StartCoroutine(PingPongCoroutine(animNumb));
            });

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
        // Sécurité
        if (!pingPongInAir)
            yield break;

        // Prépare override/animation pour la redescente
        bossNavMeshAgent.enabled = false;
        rbBody.isKinematic = false;
        int animNumb = pingPongAnimNumb;
        Debug.Log("PingPong Descend Anim Numb: " + animNumb);
        string overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play Fly Fall Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;

        // Tween de descente vers la position de départ
        rbBody.DOMove(pingPongStartPos, durationPingPong)
            .SetEase(Ease.InQuad)
            .OnStart(() =>
            {
                string ovKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
                if (animNumb < currentAttack.listComboData.Count)
                    overrideController[ovKey] = currentAttack.listComboData[animNumb].animation;

                Debug.Log("Play Fly Down Animation");
                animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);
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

            if (currentAttack.listComboData[i].attackData.attackType == S_EnumEnemyAttackType.Projectile)
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
        if (ballsFlyCoroutine != null)
        {
            StopCoroutine(ballsFlyCoroutine);
            ballsFlyCoroutine = null;
        }
        ballsFlyCoroutine = StartCoroutine(BallsFly(true));
    }

    private IEnumerator BallsFly(bool stun)
    {
        if (currentAttack == null || currentAttack.listComboData == null || currentAttack.listComboData.Count == 0)
            yield break;

        int animNumb = 0;
        bossNavMeshAgent.enabled = false;

        string overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play JumpFall Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);


        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;
        pingPongStartPos = rbBody.position;
        pingPongPeakPos = pingPongStartPos + Vector3.up * jumpPowerPingPong;

        overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play Fly Prepa Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;

        rbBody.DOMove(pingPongPeakPos, durationPingPong)
            .SetEase(Ease.OutQuad)
            .OnStart(() =>
            {
                overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
                if (animNumb < currentAttack.listComboData.Count)
                    overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

                Debug.Log("Play Fly Up Animation");
                animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);
                animNumb++;
            })
            .OnComplete(() =>
            {
                rbBody.isKinematic = true;
                pingPongAnimNumb = animNumb;
                pingPongInAir = true;

                if (ballsCoroutine != null)
                {
                    StopCoroutine(ballsCoroutine);
                    ballsCoroutine = null;
                }
                ballsCoroutine = StartCoroutine(BallsCoroutine(animNumb, stun));
            });
    }

    private IEnumerator BallsCoroutine(int value, bool stun)
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

            if (currentAttack.listComboData[i].attackData.attackType == S_EnumEnemyAttackType.Projectile)
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
        bossNavMeshAgent.enabled = true;
        rbBody.isKinematic = false;
        if (stun)
        {
            rseOnBossStun.Call(S_EnumBossState.Stun);
        }
    } 
    #endregion

    #region Gathering
    private void Gathering()
    {

        Debug.Log("Gathering Jump");
        if (gatheringJumpCoroutine != null)
        {
            StopCoroutine(gatheringJumpCoroutine);
            gatheringJumpCoroutine = null;
        }
        gatheringJumpCoroutine = StartCoroutine(GatheringJumpCoroutine(rbBody, aimPointPlayer.position, backDistanceGathering, jumpPowerGathering, durationGathering, 1));

    }

    private IEnumerator GatheringJumpCoroutine(Rigidbody rb, Vector3 playerPos, float distance, float jumpPower, float duration, int numJumps, bool keepY = true)
    {
        int animNumb = 0;
        bossNavMeshAgent.enabled = false;

        Vector3 dir = rb.position - playerPos;
        if (dir.sqrMagnitude <= 1e-6f) dir = rb.transform.forward;
        dir.Normalize();

        Vector3 desiredTarget = rb.position + dir * distance;

        Vector3 target = GetSafeJumpTarget(desiredTarget, rb.position, distance);

        string overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play Jump Prepa Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;

        Sequence seq = rb.DOJump(target, jumpPower, numJumps, duration)
            .SetEase(Ease.OutQuad).OnStart(() =>
            {
                overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
                overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

                Debug.Log("Play Jump Animation");
                animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);
                animNumb++;
            })
            .OnComplete(() =>
            {
                if (gatheringCoroutine != null)
                {
                    StopCoroutine(gatheringCoroutine);
                    gatheringCoroutine = null;
                }
                gatheringCoroutine = StartCoroutine(GatheringCoroutine(animNumb));
            });
        yield return null;

    }
    private IEnumerator GatheringCoroutine(int value)
    {
        int animNumb = value;

        string overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play JumpFall Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;

        overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Stop Particle + Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        rseOnStopParticle.Call();

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;

        overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play Particle + Animation + Set AttackMode");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);

        if (currentAttack.listComboData[animNumb].showVFXAttackType) bossAttackData.VFXAttackType();

        S_FireProjectile s_FireProjectile = Instantiate(fireProjectile, fireProjectileSpawn.transform.position, Quaternion.identity);
        s_FireProjectile.Initialize(transform.rotation, currentAttack.listComboData[animNumb].attackData);

        rseOnPlayParticle.Call();
        bossNavMeshAgent.enabled = true;
        rseOnEndAttack.Call();
    }
    #endregion

    #region WingsOfHell
    private void WingsOfHell()
    {
        StartCoroutine(WingsOfHellFly());
    }

    private IEnumerator WingsOfHellFly()
    {
        if (currentAttack == null || currentAttack.listComboData == null || currentAttack.listComboData.Count == 0)
            yield break;

        int animNumb = 0;
        bossNavMeshAgent.enabled = false;

        string overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play JumpFall Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);


        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;
        pingPongStartPos = rbBody.position;
        pingPongPeakPos = pingPongStartPos + Vector3.up * jumpPowerPingPong;

        overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
        overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

        Debug.Log("Play Fly Prepa Animation");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);

        yield return new WaitForSeconds(currentAttack.listComboData[animNumb].animation.length);
        animNumb++;

        rbBody.DOMove(pingPongPeakPos, durationPingPong)
            .SetEase(Ease.OutQuad)
            .OnStart(() =>
            {
                overrideKey = (animNumb % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
                if (animNumb < currentAttack.listComboData.Count)
                    overrideController[overrideKey] = currentAttack.listComboData[animNumb].animation;

                Debug.Log("Play Fly Up Animation");
                animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);
                animNumb++;
            })
            .OnComplete(() =>
            {
                rbBody.isKinematic = true;
                pingPongAnimNumb = animNumb;
                pingPongInAir = true;
                StartCoroutine(WingsOfHellCoroutine(animNumb));
            });
    }

    private IEnumerator WingsOfHellCoroutine(int value)
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
            if (currentAttack.listComboData[i].attackData.attackType == S_EnumEnemyAttackType.Projectile)
            {
                yield return new WaitForSeconds(currentAttack.listComboData[i].attackData.timeCast);
                S_BossProjectile projectileInstance = Instantiate(bossProjectile, projectilePingPongSpawn.transform.position, Quaternion.identity);
                projectileInstance.Initialize(aimPointBoss, aimPointPlayer, currentAttack.listComboData[i].attackData);
            }
            yield return null;
        }
    }
    #endregion

    private Vector3 GetSafeJumpTarget(Vector3 desired, Vector3 fallbackPosition, float maxSampleDistance)
    {
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(desired, out navHit, maxSampleDistance, NavMesh.AllAreas))
        {
            Debug.Log($"[S_BossAttack] GetSafeJumpTarget -> NavMesh hit at {navHit.position}");
            return navHit.position;
        }

        // try raycast down from above the desired position to find ground
        RaycastHit hit;
        Vector3 rayStart = desired + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f))
        {
            Debug.Log($"[S_BossAttack] GetSafeJumpTarget -> Raycast ground at {hit.point}");
            return hit.point;
        }

        Debug.LogWarning("[S_BossAttack] GetSafeJumpTarget -> No navmesh or ground found near desired target, using fallback");
        return fallbackPosition;
    }
    #endregion
}