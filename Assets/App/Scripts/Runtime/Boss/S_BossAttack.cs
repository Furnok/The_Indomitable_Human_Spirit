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
    [SerializeField] private float backDistance;

    [TabGroup("Settings")]
    [SerializeField] private float jumpPower;

    [TabGroup("Settings")]
    [SerializeField] private float duration;

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
    [Title("Scripts")]
    [SerializeField] private S_BossRootMotionModifier rootMotionModifier;

    [TabGroup("References")]
    [SerializeField] private S_BossAttackData bossAttackData;

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
    private Coroutine pingPongJumpCoroutine = null;
    private Vector3 pingPongStartPos;
    private Vector3 pingPongPeakPos;
    private int pingPongAnimNumb = 0;
    private bool pingPongInAir = false;


    private void OnEnable()
    {
        onExecuteAttack.action += DoAttackChoose;
        onEndFly.action += PingPongDescend;
    }

    private void OnDisable()
    {
        onExecuteAttack.action -= DoAttackChoose;
        onEndFly.action -= PingPongDescend;
    }

    private void DoAttackChoose(S_ClassBossAttack attack)
    {
        currentAttack = attack;

        switch (currentAttack.attackName)
        {
            case "Simon":
                Simon();
                break;
            case "Dualliste":
                Dualliste();
                break;
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
    private void Simon()
    {
    }

    private void Dualliste()
    {
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
    private IEnumerator PingPongJump(Rigidbody rb, Vector3 playerPos, float distance, float jumpPower, float duration, int numJumps, bool keepY = true)
    {
        int animNumb = 0;
        bossNavMeshAgent.enabled = false;

        Vector3 dir = rb.position - playerPos;
        if (dir.sqrMagnitude <= 1e-6f) dir = rb.transform.forward;
        dir.Normalize();

        Vector3 target = rb.position + dir * distance;
        if (keepY) target.y = rb.position.y;

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
                StartCoroutine(PingPongFly(animNumb));
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
                StartCoroutine(PingPongCoroutine(animNumb));
            });

    }
    private void PingPongDescend()
    {
        if (pingPongInAir)
        {
            StartCoroutine(PingPongDescendCoroutine());
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
    private void Balls()
    {
        StartCoroutine(BallsFly(true));
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
                StartCoroutine(BallsCoroutine(animNumb,stun));
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
                for(int j = 0; j < currentAttack.listComboData[i].attackData.numberOfProjectiles; j++)
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

    #region Gathering
    private void Gathering()
    {

        Debug.Log("Gathering Jump");
        StartCoroutine(GatheringCoroutine(rbBody, aimPointPlayer.position, backDistance, jumpPower, duration, 1));

    }

    private IEnumerator GatheringCoroutine(Rigidbody rb, Vector3 playerPos, float distance, float jumpPower, float duration, int numJumps, bool keepY = true)
    {
        int animNumb = 0;
        bossNavMeshAgent.enabled = false;

        Vector3 dir = rb.position - playerPos;
        if (dir.sqrMagnitude <= 1e-6f) dir = rb.transform.forward;
        dir.Normalize();

        Vector3 target = rb.position + dir * distance;
        if (keepY) target.y = rb.position.y;

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
                StartCoroutine(GatheringAttack(animNumb));
            });
        yield return null;

    }
    private IEnumerator GatheringAttack(int value)
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

        bossAttackData.SetAttackMode(currentAttack.listComboData[animNumb].attackData);

        if (currentAttack.listComboData[animNumb].showVFXAttackType) bossAttackData.VFXAttackType();

        Debug.Log("Play Particle + Animation + Set AttackMode");
        animator.SetTrigger(animNumb == 0 ? attackParam : comboParam);
        rseOnPlayParticle.Call();
        bossNavMeshAgent.enabled = true;
        rseOnEndAttack.Call();

    } 
    #endregion
    private void WingsOfHell()
    {
    }
    #endregion
}