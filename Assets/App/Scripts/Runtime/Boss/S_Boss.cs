using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class S_Boss : MonoBehaviour
{
    #region Variables
    [TabGroup("Settings")]
    [Header("Settings")]
    [SerializeField] private SSO_BossData ssoBossData;

    [TabGroup("References")]
    [Title("Agent")]
    [SerializeField] private NavMeshAgent navMeshAgent;

    [TabGroup("References")]
    [Title("Body")]
    [SerializeField] private GameObject body;

    [TabGroup("References")]
    [Title("Center")]
    [SerializeField] private GameObject center;

    [TabGroup("References")]
    [Title("RigidBody")]
    [SerializeField] private Rigidbody rb;

    [TabGroup("References")]
    [Title("Colliders")]
    [SerializeField] private Collider bodyCollider;

    [TabGroup("References")]
    [SerializeField] private Collider detectionCollider;

    [TabGroup("References")]
    [SerializeField] private Collider hurtCollider;

    [TabGroup("References")]
    [Title("Scripts")]
    [SerializeField] private S_BossDetectionRange bossDetectionRange;

    [TabGroup("References")]
    [Title("Animator")]
    [SerializeField] private Animator animator;

    [TabGroup("References")]
    [Title("Animation Parameters")]
    [SerializeField, S_AnimationName("animator")] private string moveParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string deathParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string idleAttack;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string attackParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string comboParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string stopAttackParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string stunParam;

    [TabGroup("References")]
    [Title("Scripts")]
    [SerializeField] private S_BossHurt bossHurt;

    [TabGroup("References")]
    [SerializeField] private S_BossRootMotionModifier rootMotionModifier;

    [TabGroup("References")]
    [SerializeField] private S_BossAttackData bossAttackData;

    [TabGroup("References")]
    [SerializeField] private S_BossAttack bossAttack;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerGettingHit rseOnPlayerGettingHit;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerDeath rseOnPlayerDeath;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnEndAttack rseOnEndAttack;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnBossStun onBossStun;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnExecuteAttack onExecuteAttack;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnEnemyTargetDied rseOnEnemyTargetDied;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerRespawn rseOnPlayerRespawn;

    private List<S_ClassAttackOwned> listAttackOwneds = new();
    private List<S_ClassAttackOwned> listAttackOwnedPossibilities = new();
    private S_EnumBossState currentState = S_EnumBossState.Idle;
    private S_EnumBossPhaseState currentPhaseState = S_EnumBossPhaseState.Phase1;

    private AnimatorOverrideController overrideController = null;
    private GameObject target = null;
    private Transform aimPoint = null;

    private S_ClassAttackOwned lastAttack = null;
    private S_ClassAttackOwned currentAttack = null;

    private Coroutine comboCoroutine = null;
    private Coroutine stunCoroutine = null;
    private Coroutine resetAttack = null;
    private Coroutine timeChooseAttackCoroutine = null;
    private Coroutine strafeCoroutine = null;

    private float health = 0;
    private float maxHealth = 0;
    private float lastValueHealth = 0;
    private float bossDifficultyLevel = 0;
    private float initDistance = 0;
    private int strafeDirection = 1;

    private bool isPerformingCombo = false;
    private S_EnumBossState? pendingState = null;
    private GameObject pendingTarget = null;

    private bool isDead = false;
    private bool isChasing = false;
    private bool isFighting = false;
    private bool isStunned = false;

    private bool canAttack = false;
    private bool isPlayerDeath = false;
    private bool isAttacking = false;
    private bool isStrafe = false;
    private bool unlockRotate = false;
    
    #endregion

    private void Awake()
    {
        canAttack = true;
        navMeshAgent.avoidancePriority = Random.Range(0, 99);

        Animator anim = animator;
        AnimatorOverrideController instance = new AnimatorOverrideController(ssoBossData.Value.controllerOverride);
        bossAttack.overrideController = instance;

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        ssoBossData.Value.controllerOverride.GetOverrides(overrides);
        instance.ApplyOverrides(overrides);

        anim.runtimeAnimatorController = instance;

        overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
        lastValueHealth = 101f;

        health = ssoBossData.Value.health;
        maxHealth = ssoBossData.Value.health;
        navMeshAgent.speed = ssoBossData.Value.walkSpeed;
        currentPhaseState = ssoBossData.Value.phaseState;

        foreach (var bossAttack in ssoBossData.Value.listAttack)
        {
            var attackData = new S_ClassAttackOwned
            {
                bossAttack = bossAttack,
                frequency = 0,
                score = 0,
            };

            listAttackOwneds.Add(attackData);
        }

        UpdateLastHealthValue();
    }

    private void OnEnable()
    {
        bossDetectionRange.onTargetDetected.AddListener(SetTarget);
        rseOnPlayerGettingHit.action += LoseDifficultyLevel;
        rseOnPlayerDeath.action += PlayerDeath;
        rseOnPlayerRespawn.action += PlayerRespawn;
        rseOnEndAttack.action += SpecialAttackEnd;
        onBossStun.action += UpdateState;
    }
    private void OnDisable()
    {
        bossDetectionRange.onTargetDetected.RemoveListener(SetTarget);
        rseOnPlayerGettingHit.action -= LoseDifficultyLevel;
        rseOnPlayerDeath.action -= PlayerDeath;
        rseOnPlayerRespawn.action -= PlayerRespawn;
        rseOnEndAttack.action -= SpecialAttackEnd;
        onBossStun.action -= UpdateState;
    }
    private void Start()
    {
        UpdateLastHealthValue();
        UpdateState(S_EnumBossState.Idle);
        bossDifficultyLevel = ssoBossData.Value.initialBossDifficultyLevel;
        Debug.Log(listAttackOwnedPossibilities.Count);
    }
    
    private void Update()
    {
        if (target != null && (unlockRotate || !isAttacking) && !isDead) RotateEnemy();

        if (isChasing) Chase();

        if (isFighting) Fight();
    }
    private void FixedUpdate()
    {
        bool isMoving = navMeshAgent.velocity.magnitude > 0.1f;
        animator.SetBool(moveParam, isMoving);
    }
    public void RotateEnemy()
    {
        Vector3 direction = target.transform.position - center.transform.position;
        direction.y = 0;

        Quaternion targetRot = Quaternion.LookRotation(direction);

        transform.DOKill();
        transform.DORotateQuaternion(targetRot, ssoBossData.Value.rotationTime);
    }

    public void RotateEnemyAnim()
    {
        unlockRotate = true;
    }

    public void StopRotateEnemyAnim()
    {
        unlockRotate = false;
    }

    #region States
    private void UpdateState(S_EnumBossState newState)
    {
        ResetBoss();
        currentState = newState;

        switch (currentState)
        {
            case S_EnumBossState.Idle:
                Idle();
                break;
            case S_EnumBossState.Chase:
                Chasing();
                break;
            case S_EnumBossState.Combat:
                Fighting();
                break;
            case S_EnumBossState.Stun:
                Stun();
                break;
            case S_EnumBossState.Death:
                Death();
                break;
        }
    }

    private void ResetBoss()
    {
        Debug.Log("Resetting Boss State...");
        bossAttackData.DisableWeaponCollider();
        bossAttackData.VFXStopTrail();
        navMeshAgent.enabled = true;

        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
            comboCoroutine = null;
        }

        if (timeChooseAttackCoroutine != null)
        {
            StopCoroutine(timeChooseAttackCoroutine);
            timeChooseAttackCoroutine = null;
        }

        if (strafeCoroutine != null)
        {
            StopCoroutine(strafeCoroutine);
            strafeCoroutine = null;
        }

        isPerformingCombo = false;
        isChasing = false;
        isFighting = false;
        isStunned = false;
        isStrafe = false;
        isAttacking = false;
        unlockRotate = false;

        navMeshAgent.ResetPath();
        navMeshAgent.velocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
    }
    #endregion

    #region Target
    private void SetTarget(GameObject newTarget)
    {
        if (newTarget == target || isDead) return;

        target = newTarget;

        if (isPerformingCombo || isStunned)
        {
            pendingTarget = target;
            pendingState = (target == null) ? S_EnumBossState.Idle : S_EnumBossState.Chase;
            return;
        }

        if (target != null)
        {
            newTarget.TryGetComponent<I_AimPointProvider>(out I_AimPointProvider aimPointProvider);
            aimPoint = aimPointProvider != null ? aimPointProvider.GetAimPoint() : newTarget.transform;
            bossAttack.aimPointPlayer = aimPoint;
            UpdateState(S_EnumBossState.Chase);
            StartCoroutine(GainDifficultyLevel());
        }
        else
        {
            if (health != maxHealth)
            {
                health = maxHealth;
            }

            aimPoint = null;
            detectionCollider.enabled = false;

            UpdateState(S_EnumBossState.Idle);
        }
    }
    #endregion

    #region Chase
    private void Chasing()
    {
        Debug.Log("Boss Chasing");
        isChasing = true;

        ChooseAttack();

        navMeshAgent.speed = ssoBossData.Value.walkSpeed;
        initDistance = Vector3.Distance(body.transform.position, target.transform.position);
    }

    private void Chase()
    {
        float distanceToTarget = Vector3.Distance(body.transform.position, target.transform.position);
        bool destinationReached = distanceToTarget <= (ssoBossData.Value.distanceToChase);
        if (!destinationReached)
        {
            navMeshAgent.SetDestination(target.transform.position);

            if (distanceToTarget <= initDistance * (ssoBossData.Value.distanceToRun / 100f))
            {
                if (currentAttack.bossAttack.isAttackDistance)
                {
                    UpdateState(S_EnumBossState.Combat);
                }
                else
                {
                    navMeshAgent.speed = ssoBossData.Value.runSpeed;
                    animator.SetFloat("MoveSpeed", ssoBossData.Value.runSpeed);
                }
            }
        }
        else if (destinationReached)
        {
            UpdateState(S_EnumBossState.Combat);
        }
    }
    #endregion

    #region Idle
    private void Idle()
    {
        Debug.Log("Boss Idle");
    }
    #endregion

    #region Stun
    private void Stun()
    {
        Debug.Log("Boss Stun!");
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        stunCoroutine = StartCoroutine(StunDuration(ssoBossData.Value.stunDuration));
    }

    private IEnumerator StunDuration(float duration)
    {
        isStunned = true;
        animator.SetTrigger(stunParam);
        yield return new WaitForSeconds(duration);
        isStunned = false;
        if (target != null)
        {
            UpdateState(S_EnumBossState.Chase);
        }
        else
        {
            UpdateState(S_EnumBossState.Idle);
        }
    }
    #endregion

    #region Health/Death
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        if (target != null) UpdateHealth(damage);

    }

    private void UpdateHealth(float damage)
    {
        health = Mathf.Max(health - damage, 0);

        UpdateLastHealthValue();

        if (health <= 0) UpdateState(S_EnumBossState.Death);
    }
    private void UpdateLastHealthValue()
    {
        var minValue = (health / maxHealth) * 100;

        SetListAttackPossible(minValue, lastValueHealth);

        lastValueHealth = minValue;
    }
    private void Death()
    {
        isDead = true;
        animator.SetTrigger(deathParam);

        StopAllCoroutines();

        ResetBoss();
        currentAttack = null;
        target = null;

        bodyCollider.enabled = false;
        detectionCollider.enabled = false;
        hurtCollider.enabled = false;
    }
    #endregion

    #region Player
    private void PlayerDeath()
    {
        if (target == null) return;

        if (health != ssoBossData.Value.health)
        {
            health = ssoBossData.Value.health;
            //enemyUI.UpdateHealthBar(health);
        }

        aimPoint = null;
        detectionCollider.enabled = false;
        isPlayerDeath = true;

        if (isPerformingCombo) pendingState = S_EnumBossState.Idle;
        else
        {
            target = null;

            UpdateState(S_EnumBossState.Idle);
        }
    }

    private void PlayerRespawn()
    {
        if (isPlayerDeath)
        {
            isPlayerDeath = false;

            target = null;

            if (currentState != S_EnumBossState.Idle)
            {
                animator.SetTrigger(stopAttackParam);
                animator.SetBool(idleAttack, false);

                UpdateState(S_EnumBossState.Idle);
            }
        }
    } 
    #endregion

    #region Difficulty
    private void LoseDifficultyLevel()
    {
        bossDifficultyLevel -= ssoBossData.Value.difficultyLoseWhenPlayerHit;
        bossDifficultyLevel = Mathf.Clamp(bossDifficultyLevel, 0, ssoBossData.Value.maxDifficultyLevel);
    }

    private IEnumerator GainDifficultyLevel()
    {
        bossDifficultyLevel += ssoBossData.Value.difficultyGainPerSecond;
        bossDifficultyLevel = Mathf.Clamp(bossDifficultyLevel, 0, ssoBossData.Value.maxDifficultyLevel);

        yield return new WaitForSeconds(1);

        StartCoroutine(GainDifficultyLevel());
    }
    #endregion

    #region Attack
    private void AddListAttackPossible(S_ClassAttackOwned bossAttack)
    {
        listAttackOwnedPossibilities.Add(bossAttack);
    }

    private void SetListAttackPossible(float minValue, float maxValue)
    {
        foreach (var attack in listAttackOwneds)
        {
            if (attack.bossAttack.pvBossUnlock >= minValue && attack.bossAttack.pvBossUnlock < maxValue) AddListAttackPossible(attack);
        }
    }

    private void ChooseAttack()
    {
        Debug.Log("Choosing Attack...");

        var minAttackFrequency = listAttackOwnedPossibilities.Min(a => a.frequency);
        int roundDifficulty = Mathf.RoundToInt(bossDifficultyLevel);

        foreach (var attack in listAttackOwnedPossibilities)
        {
            if (attack.bossAttack.difficultyLevel == roundDifficulty) attack.score += ssoBossData.Value.difficultyScore;

            if (attack.frequency == minAttackFrequency) attack.score += ssoBossData.Value.frequencyScore;

            if (lastAttack == null) continue;

            if (attack.bossAttack.listComboData[0].attackData.attackType != lastAttack.bossAttack.listComboData[^1].attackData.attackType) attack.score += ssoBossData.Value.synergieScore;
        }

        var maxScore = listAttackOwnedPossibilities.Max(a => a.score);

        var bestAttacks = listAttackOwnedPossibilities
            .Where(a => a.score == maxScore)
            .ToList();

        var chosenAttack = bestAttacks[Random.Range(0, bestAttacks.Count)];

        currentAttack = chosenAttack;
        Debug.Log("Chosen Attack: " + currentAttack.bossAttack.attackName);

        foreach (var attack in listAttackOwnedPossibilities) attack.score = 0;
    }

    private void Fighting()
    {
        Debug.Log("Boss Fighting");
        isFighting = true;
        animator.SetBool(idleAttack, true);
    }
    private void SpecialAttackEnd()
    {
        Debug.Log("Special Attack End");
        rootMotionModifier.Setup(1);
        animator.SetTrigger(stopAttackParam);
        animator.SetBool(idleAttack, true);

        isPerformingCombo = false;
        isAttacking = false;
        unlockRotate = false;

        ChooseAttack();
        if (resetAttack != null)
        {
            StopCoroutine(resetAttack);
            resetAttack = null;
        }

        resetAttack = StartCoroutine(S_Utils.Delay(lastAttack.bossAttack.timeAfterAttack, () => canAttack = true));
    }
    private void Fight()
    {
        Debug.Log("Fighting...");
        if (canAttack && !isStrafe)
        {
            canAttack = false;

            lastAttack = currentAttack;
            currentAttack.frequency++;

            if(currentAttack.bossAttack.isSpecialAttack)
            {
                onExecuteAttack.Call(currentAttack.bossAttack);
                isPerformingCombo = true;
            }
            else
            {
                if (comboCoroutine != null)
                {
                    StopCoroutine(comboCoroutine);
                    comboCoroutine = null;
                }

                comboCoroutine = StartCoroutine(PlayComboSequence());
            }

            return;
        }
        else if (!isPerformingCombo && !isStrafe)
        {
            isStrafe = true;

            if (strafeCoroutine != null)
            {
                StopCoroutine(strafeCoroutine);
                strafeCoroutine = null;
            }

            StartCoroutine(Strafing());
        }
    }
    private IEnumerator PlayComboSequence()
    {
        Debug.Log("Performing Combo: " + currentAttack.bossAttack.attackName);
        isPerformingCombo = true;

        yield return null;

        animator.SetBool(idleAttack, false);
        isAttacking = false;

        for (int i = 0; i < currentAttack.bossAttack.listComboData.Count; i++)
        {
            Debug.Log("Executing Attack " + (i + 1) + " of " + currentAttack.bossAttack.listComboData.Count);
            isAttacking = true;
            RotateEnemy();

            string overrideKey = (i % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
            overrideController[overrideKey] = currentAttack.bossAttack.listComboData[i].animation;

            rootMotionModifier.Setup(currentAttack.bossAttack.listComboData[i].rootMotionMultiplier);

            bossAttackData.SetAttackMode(currentAttack.bossAttack.listComboData[i].attackData);

            if (currentAttack.bossAttack.listComboData[i].showVFXAttackType) bossAttackData.VFXAttackType();

            animator.SetTrigger(i == 0 ? attackParam : comboParam);

            yield return new WaitForSeconds(currentAttack.bossAttack.listComboData[i].animation.length);

            isAttacking = false;
            RotateEnemy();

            yield return null;
        }

        rootMotionModifier.Setup(1);
        animator.SetTrigger(stopAttackParam);
        animator.SetBool(idleAttack, true);

        isPerformingCombo = false;
        isAttacking = false;
        unlockRotate = false;

        if (pendingState.HasValue)
        {
            animator.SetBool(idleAttack, false);

            UpdateState(pendingState.Value);

            pendingState = null;
        }

        if (pendingTarget != null)
        {
            SetTarget(pendingTarget);
            pendingTarget = null;
        }

        if (isPlayerDeath)
        {
            target = null;
        }

        ChooseAttack();
        if (resetAttack != null)
        {
            StopCoroutine(resetAttack);
            resetAttack = null;
        }

        resetAttack = StartCoroutine(S_Utils.Delay(lastAttack.bossAttack.timeAfterAttack, () => canAttack = true));

    }

    private IEnumerator Strafing()
    {
        Debug.Log("Strafing...");
        strafeDirection = Random.value > 0.5f ? 1 : -1;

        Vector3 offsetPlayer = transform.position - target.transform.position;
        offsetPlayer.y = 0;

        Vector3 offsetAtRadius = offsetPlayer.normalized * (ssoBossData.Value.distanceToChase);

        float angle = Random.Range(ssoBossData.Value.strafeRotationMin, ssoBossData.Value.strafeRotationMax) * strafeDirection;

        Quaternion rot = Quaternion.Euler(0f, angle, 0f);
        Vector3 rotatedOffset = rot * offsetAtRadius;

        Vector3 finalPos = target.transform.position + rotatedOffset;
        navMeshAgent.SetDestination(finalPos);

        animator.SetFloat("MoveSpeed", ssoBossData.Value.walkSpeed);

        yield return new WaitUntil(() => !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= 0.01f);

        yield return new WaitForSeconds(1);

        isStrafe = false;
    }
    #endregion
}