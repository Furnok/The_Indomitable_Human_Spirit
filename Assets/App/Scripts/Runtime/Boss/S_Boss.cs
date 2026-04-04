using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private Collider reflectCollider;

    [TabGroup("References")]
    [SerializeField] private Collider detectionCollider;

    [TabGroup("References")]
    [SerializeField] private Collider hurtCollider;

    [TabGroup("References")]
    [Title("Scripts")]
    [SerializeField] private S_BossDetectionRange bossDetectionRange;

    [TabGroup("References")]
    [SerializeField] private S_EnemyHeadLookAtIK enemyHeadLookAtIK;

    [TabGroup("References")]
    [Title("Animator")]
    [SerializeField] private Animator animator;

    [TabGroup("References")]
    [Title("Animation Parameters")]
    [SerializeField, S_AnimationName("animator")] private string moveParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string moveSpeedParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string combatParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string attackParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string comboParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string stunParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string deathParam;

    [TabGroup("References")]
    [Title("Scripts")]
    [SerializeField] private S_BossHurt bossHurt;

    [TabGroup("References")]
    [SerializeField] private S_BossRootMotionModifier rootMotionModifier;

    [TabGroup("References")]
    [SerializeField] private S_BossAttackData bossAttackData;

    [TabGroup("References")]
    [SerializeField] private S_BossAttack bossAttack;

    [TabGroup("References")]
    [Title("UI Damage")]
    [SerializeField] private S_EnemyUIDamage bossUIDamage;

    [TabGroup("References")]
    [SerializeField] private GameObject textParent;

    [TabGroup("References")]
    [ShowIf("@ssoBossData != null && ssoBossData.Value.phaseState == S_EnumBossPhaseState.Phase1")]
    [SerializeField] private S_BossPhase1UI bossPhase1UI;

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

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnDisplayBossHealth rseOnDisplayBossHealth;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnBossHealthSetup rseOnBossHealthSetup;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnUpdateBossHealth rseOnUpdateBossHealth;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnBossDeath rseOnBossDeath;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnStartBossP1 rseOnStartBossP1;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnStartBossP2 rseOnStartBossP2;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnEndBossP1 rseOnEndBossP1;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnEndBossP2 rseOnEndBossP2;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnFinishBossP1 rseOnFinishBossP1;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnFinishBossP2 rseOnFinishBossP2;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCameraFOV rseOnCameraFOV;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CurrentTarget rsoCurrentTarget;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_CameraData ssoCameraData;

    private float health = 0;
    private float maxHealth = 0;
    private float lastValueHealth = 0;
    private float bossDifficultyLevel = 0;
    private float initDistance = 0;

    private AnimatorOverrideController overrideController = null;
    private S_ClassAttackOwned lastAttack = null;
    private S_ClassAttackOwned currentAttack = null;
    private S_ClassAttackOwned ultimateAttack = null;
    private List<S_ClassAttackOwned> listAttackOwneds = new();
    private List<S_ClassAttackOwned> listAttackOwnedPossibilities = new();

    private Vector3 posSpawn = Vector3.zero;
    private Quaternion rotSpawn = Quaternion.identity;

    private GameObject target = null;
    private Transform aimPoint = null;

    private S_EnumBossState currentState = S_EnumBossState.Idle;
    private S_EnumBossPhaseState currentPhaseState = S_EnumBossPhaseState.Phase1;

    private Tween rotateTween = null;

    private Coroutine difficultyCoroutine = null;
    private Coroutine attackCoroutine = null;
    private Coroutine resetAttack = null;
    private Coroutine stunCoroutine = null;

    private bool isIdle = false;
    private bool isReturnIdle = false;
    private bool isChasing = false;
    private bool isCombat = false;
    private bool isAttack = false;
    private bool isStun = false;
    private bool isDead = false;

    private bool isPlayerDeath = false;
    private bool canAttack = false;
    private bool unlockRotate = false;
    #endregion

    private void Awake()
    {
        navMeshAgent.avoidancePriority = Random.Range(0, 99);

        Animator anim = animator;
        AnimatorOverrideController instance = new AnimatorOverrideController(ssoBossData.Value.controllerOverride);

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        ssoBossData.Value.controllerOverride.GetOverrides(overrides);
        instance.ApplyOverrides(overrides);

        anim.runtimeAnimatorController = instance;
        bossAttack.overrideController = instance;

        overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;

        health = ssoBossData.Value.health;
        maxHealth = ssoBossData.Value.health;
        lastValueHealth = 101f;
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

        if (currentPhaseState == S_EnumBossPhaseState.Phase1)
        {
            bossPhase1UI.Setup(ssoBossData);
        }
        else if (currentPhaseState == S_EnumBossPhaseState.Phase2)
        {
            rseOnUpdateBossHealth.Call(health);
        }

        UpdateLastHealthValue();
    }

    private void OnEnable()
    {
        rseOnPlayerDeath.action += PlayerDeath;
        rseOnPlayerRespawn.action += PlayerRespawn;

        rseOnPlayerGettingHit.action += LoseDifficultyLevel;
        rseOnEndAttack.action += SpecialAttackEnd;
        onBossStun.action += UpdateState;
    }

    private void OnDisable()
    {
        rseOnPlayerDeath.action -= PlayerDeath;
        rseOnPlayerRespawn.action -= PlayerRespawn;

        rseOnPlayerGettingHit.action -= LoseDifficultyLevel;
        rseOnEndAttack.action -= SpecialAttackEnd;
        onBossStun.action -= UpdateState;
    }

    private void Start()
    {
        posSpawn = transform.position;
        rotSpawn = transform.rotation;

        canAttack = true;
        bossDifficultyLevel = ssoBossData.Value.initialBossDifficultyLevel;

        if (currentPhaseState == S_EnumBossPhaseState.Phase2) StartCoroutine(S_Utils.DelayRealTime(3f, () => bossDetectionRange.gameObject.SetActive(true)));

        UpdateState(S_EnumBossState.Idle);
    }
    
    private void Update()
    {
        if ((currentState == S_EnumBossState.Chasing || currentState == S_EnumBossState.Combat || currentState == S_EnumBossState.Attack) && !isDead) enemyHeadLookAtIK.SetTarget(target, ssoBossData.Value.yHeadRemove);

        if (target != null && (unlockRotate || !isAttack) && !isDead) RotateBoss();

        if (isChasing && !isDead) Chase();

        if (isCombat && !isDead) Combat();
    }

    public void RotateBoss()
    {
        if (target != null)
        {
            Vector3 direction = target.transform.position - center.transform.position;
            direction.y = 0;

            Quaternion targetRot = Quaternion.LookRotation(direction);

            rotateTween?.Kill();
            rotateTween = transform.DORotateQuaternion(targetRot, ssoBossData.Value.rotationTime);
        }
    }

    public void RotateBossAnim()
    {
        unlockRotate = true;
    }

    public void StopRotateBossAnim()
    {
        unlockRotate = false;
    }

    #region States
    private void UpdateState(S_EnumBossState newState)
    {
        if (currentState == newState) return;

        ResetBoss();

        currentState = newState;

        switch (currentState)
        {
            case S_EnumBossState.Idle:
                StartIdle();
                break;
            case S_EnumBossState.ReturnIdle:
                StartReturnIdle();
                break;
            case S_EnumBossState.Chasing:
                StartChasing();
                break;
            case S_EnumBossState.Combat:
                StartCombat();
                break;
            case S_EnumBossState.Attack:
                StartAttack();
                break;
            case S_EnumBossState.Stun:
                StartStun();
                break;
            case S_EnumBossState.Death:
                Death();
                break;
        }
    }

    private void ResetBoss()
    {
        bossAttackData.DisableWeaponCollider();
        bossAttackData.VFXStopTrail();

        isIdle = false;
        isReturnIdle = false;
        isChasing = false;
        isCombat = false;
        isAttack = false;
        isStun = false;
        isDead = false;

        unlockRotate = false;

        rotateTween?.Kill();

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        navMeshAgent.stoppingDistance = 0.2f;
        navMeshAgent.ResetPath();
        navMeshAgent.velocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;

        animator.SetBool(moveParam, false);
        animator.SetFloat(moveSpeedParam, navMeshAgent.speed);

        overrideController["AttackAnimation"] = overrideController["IdleAnimation"];
        overrideController["AttackAnimation2"] = overrideController["IdleAnimation"];
    }
    #endregion

    #region Target
    public void DetectTarget(GameObject newTarget)
    {
        if (isDead || target == newTarget) return;

        SetTarget(newTarget);
    }

    private void SetTarget(GameObject newTarget)
    {
        if (newTarget == target || isDead) return;

        target = newTarget;

        if (target != null)
        {
            newTarget.TryGetComponent<I_AimPointProvider>(out I_AimPointProvider aimPointProvider);
            aimPoint = aimPointProvider != null ? aimPointProvider.GetAimPoint() : newTarget.transform;
            bossAttack.aimPointPlayer = aimPoint;

            UpdateState(S_EnumBossState.Chasing);

            if (difficultyCoroutine != null)
            {
                StopCoroutine(difficultyCoroutine);
                difficultyCoroutine = null;
            }

            difficultyCoroutine = StartCoroutine(GainDifficultyLevel());

            if (currentPhaseState == S_EnumBossPhaseState.Phase1) rseOnStartBossP1.Call();
            else if (currentPhaseState == S_EnumBossPhaseState.Phase2)
            {
                rseOnDisplayBossHealth.Call(true);
                rseOnBossHealthSetup.Call(maxHealth);

                rseOnStartBossP2.Call();
            }
        }
        else
        {
            aimPoint = null;

            animator.SetBool(attackParam, false);

            enemyHeadLookAtIK.SetTarget(null, 0);

            if (resetAttack != null)
            {
                StopCoroutine(resetAttack);
                resetAttack = null;
            }

            resetAttack = StartCoroutine(S_Utils.Delay(lastAttack.bossAttack.timeAfterAttack, () => canAttack = true));

            UpdateState(S_EnumBossState.ReturnIdle);
        }
    }
    #endregion

    #region Damage & Health
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        if (target != null) UpdateHealth(damage);
    }

    private void UpdateHealth(float damage)
    {
        TextDamageDisplay(damage);

        health = Mathf.Max(health - damage, 0);

        if (currentPhaseState == S_EnumBossPhaseState.Phase1)
        {
            bossPhase1UI.UpdateHealthBar(health);
        }
        else if (currentPhaseState == S_EnumBossPhaseState.Phase2)
        {
            rseOnUpdateBossHealth.Call(health);
        }

        UpdateLastHealthValue();

        if (health <= 0) UpdateState(S_EnumBossState.Death);
    }

    private void UpdateLastHealthValue()
    {
        var minValue = (health / maxHealth) * 100;

        SetListAttackPossible(minValue, lastValueHealth);

        lastValueHealth = minValue;
    }

    private void TextDamageDisplay(float damage)
    {
        S_EnemyUIDamage textDamage = Instantiate(bossUIDamage, textParent.transform.position, Quaternion.identity);
        textDamage.Initialize(damage);
    }
    #endregion

    #region Player
    private void PlayerDeath()
    {
        if (target == null) return;

        target = null;
        aimPoint = null;

        if (difficultyCoroutine != null)
        {
            StopCoroutine(difficultyCoroutine);
            difficultyCoroutine = null;
        }

        bossDetectionRange.gameObject.SetActive(false);
        listAttackOwnedPossibilities.Clear();
        lastValueHealth = 101f;
        bossDifficultyLevel = ssoBossData.Value.initialBossDifficultyLevel;

        isPlayerDeath = true;
    }

    private void PlayerRespawn()
    {
        if (isPlayerDeath)
        {
            isPlayerDeath = false;

            UpdateState(S_EnumBossState.ReturnIdle);
        }
    }
    #endregion

    #region Idle
    private void StartIdle()
    {
        if (isIdle) return;

        isIdle = true;

        navMeshAgent.speed = ssoBossData.Value.walkSpeed;
        navMeshAgent.stoppingDistance = 0.2f;

        animator.SetBool(moveParam, false);
        animator.SetFloat(moveSpeedParam, navMeshAgent.speed);
    }

    private void StartReturnIdle()
    {
        if (isReturnIdle) return;

        isReturnIdle = true;

        if (health != maxHealth)
        {
            health = maxHealth;

            if (currentPhaseState == S_EnumBossPhaseState.Phase1)
            {
                bossPhase1UI.UpdateHealthBar(health);
            }
            else if (currentPhaseState == S_EnumBossPhaseState.Phase2)
            {
                rseOnUpdateBossHealth.Call(health);
            }
        }

        rseOnDisplayBossHealth.Call(false);
        UpdateLastHealthValue();

        transform.position = posSpawn;
        transform.rotation = rotSpawn;

        if (currentPhaseState == S_EnumBossPhaseState.Phase1) rseOnEndBossP1.Call();
        else if (currentPhaseState == S_EnumBossPhaseState.Phase2) rseOnEndBossP2.Call();

        if (currentPhaseState == S_EnumBossPhaseState.Phase1) bossDetectionRange.gameObject.SetActive(true);

        if (currentPhaseState == S_EnumBossPhaseState.Phase2) StartCoroutine(S_Utils.DelayRealTime(3f, () => bossDetectionRange.gameObject.SetActive(true)));

        UpdateState(S_EnumBossState.Idle);
    }
    #endregion

    #region Chasing
    private void StartChasing()
    {
        if (isChasing) return;

        isChasing = true;
        navMeshAgent.stoppingDistance = ssoBossData.Value.distanceToChase - 0.5f;
        navMeshAgent.speed = ssoBossData.Value.walkSpeed;

        animator.SetBool(moveParam, true);
        animator.SetFloat(moveSpeedParam, navMeshAgent.speed);
        animator.SetBool(combatParam, false);

        ChooseAttack();

        initDistance = Vector3.Distance(center.transform.position, target.transform.position);
    }

    private void Chase()
    {
        float distanceToTarget = Vector3.Distance(center.transform.position, target.transform.position);
        bool destinationReached = distanceToTarget <= ssoBossData.Value.distanceToChase;

        if (!destinationReached)
        {
            navMeshAgent.SetDestination(target.transform.position);

            if (distanceToTarget <= initDistance * (ssoBossData.Value.distanceToRun / 100f))
            {
                if (currentAttack.bossAttack.isAttackDistance) UpdateState(S_EnumBossState.Combat);
                else
                {
                    navMeshAgent.speed = ssoBossData.Value.runSpeed;
                    animator.SetFloat(moveSpeedParam, ssoBossData.Value.runSpeed);
                }
            }
        }
        else if (destinationReached) UpdateState(S_EnumBossState.Combat);
    }
    #endregion

    #region Combat
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

        difficultyCoroutine = StartCoroutine(GainDifficultyLevel());
    }

    private void AddListAttackPossible(S_ClassAttackOwned bossAttack)
    {
        listAttackOwnedPossibilities.Add(bossAttack);
    }

    private void SetListAttackPossible(float minValue, float maxValue)
    {
        foreach (var attack in listAttackOwneds)
        {
            if (attack.bossAttack.pvBossUnlock >= minValue && attack.bossAttack.pvBossUnlock < maxValue)
            {
                if (attack.bossAttack.attackName == "Gathering" || attack.bossAttack.attackName == "Wings Of Hell") ultimateAttack = attack;
                else AddListAttackPossible(attack);
            }
        }
    }

    private void ChooseAttack()
    {
        var minAttackFrequency = listAttackOwnedPossibilities.Min(a => a.frequency);
        int roundDifficulty = Mathf.RoundToInt(bossDifficultyLevel);

        if (ultimateAttack == null)
        {
            foreach (var attack in listAttackOwnedPossibilities)
            {
                if (attack.bossAttack.difficultyLevel == roundDifficulty) attack.score += ssoBossData.Value.difficultyScore;

                if (attack.frequency == minAttackFrequency) attack.score += ssoBossData.Value.frequencyScore;

                if (lastAttack == null) continue;

                if (attack.bossAttack.listComboData[0].attackData.attackType != lastAttack.bossAttack.listComboData[^1].attackData.attackType) attack.score += ssoBossData.Value.synergieScore;
            }

            var maxScore = listAttackOwnedPossibilities.Max(a => a.score);

            var bestAttacks = listAttackOwnedPossibilities.Where(a => a.score == maxScore).ToList();

            var chosenAttack = bestAttacks[Random.Range(0, bestAttacks.Count)];
            currentAttack = chosenAttack;

            foreach (var attack in listAttackOwnedPossibilities) attack.score = 0;
        }
        else
        {
            currentAttack = ultimateAttack;
            ultimateAttack = null;
            foreach (var attack in listAttackOwnedPossibilities) attack.score = 0;
        }
    }

    private void StartCombat()
    {
        if (isCombat) return;

        isCombat = true;
        navMeshAgent.stoppingDistance = ssoBossData.Value.distanceToChase - 0.5f;
        navMeshAgent.speed = ssoBossData.Value.walkSpeed;

        animator.SetBool(moveParam, false);
        animator.SetFloat(moveSpeedParam, ssoBossData.Value.walkSpeed);
        animator.SetBool(combatParam, true);
    }

    private void Combat()
    {
        if (canAttack && target != null)
        {
            float distanceToTarget = Vector3.Distance(body.transform.position, target.transform.position);
            bool destinationReached = distanceToTarget <= (ssoBossData.Value.distanceToChase);

            if (!destinationReached) UpdateState(S_EnumBossState.Chasing);
            else
            {
                canAttack = false;

                UpdateState(S_EnumBossState.Attack);
            }
        }
        else if (!isAttack && target != null)
        {
            float distanceToTarget = Vector3.Distance(center.transform.position, target.transform.position);
            bool destinationReached = distanceToTarget <= (ssoBossData.Value.distanceToChase);

            if (!destinationReached) UpdateState(S_EnumBossState.Chasing);
        }
    }
    #endregion

    #region Attack
    private void StartAttack()
    {
        if (isAttack) return;

        isAttack = true;
        navMeshAgent.stoppingDistance = ssoBossData.Value.distanceToChase - 0.5f;
        navMeshAgent.speed = ssoBossData.Value.walkSpeed;

        if (currentAttack.bossAttack.attackName == "Gathering" || currentAttack.bossAttack.attackName == "Wings Of Hell")
        {
            listAttackOwnedPossibilities.RemoveAt(listAttackOwnedPossibilities.IndexOf(currentAttack));
        }
        else
        {
            lastAttack = currentAttack;
            currentAttack.frequency++;
        }

        if (currentAttack.bossAttack.isSpecialAttack)
        {
            onExecuteAttack.Call(currentAttack.bossAttack);
        }
        else
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            attackCoroutine = StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.3f);

        if (rsoCurrentTarget.Value == body)
        {
            S_ClassCameraFOV fov = new();
            fov.value = ssoCameraData.Value.fovFight;
            fov.time = ssoCameraData.Value.fovFightSwitchTime;
            fov.reset = true;

            rseOnCameraFOV.Call(fov);
        }

        for (int i = 0; i < currentAttack.bossAttack.listComboData.Count; i++)
        {
            if (isPlayerDeath && target == null) break;

            RotateBoss();

            string overrideKey = (i % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
            overrideController[overrideKey] = currentAttack.bossAttack.listComboData[i].animation;

            rootMotionModifier.Setup(currentAttack.bossAttack.listComboData[i].rootMotionMultiplier, 1.5f);

            bossAttackData.SetAttackMode(currentAttack.bossAttack.listComboData[i].attackData);

            if (currentAttack.bossAttack.listComboData[i].showVFXAttackType) bossAttackData.VFXAttackType();

            if (i == 0) animator.SetBool(attackParam, true);
            else animator.SetTrigger(comboParam);

            yield return new WaitForSeconds(currentAttack.bossAttack.listComboData[i].animation.length);

            RotateBoss();

            bossAttackData.DisableWeaponCollider();
            bossAttackData.VFXStopTrail();
            unlockRotate = false;
        }

        isAttack = false;

        rootMotionModifier.Setup(1, 0);

        animator.SetBool(attackParam, false);

        if (rsoCurrentTarget.Value == body)
        {
            S_ClassCameraFOV fov2 = new();
            fov2.value = 60;
            fov2.time = ssoCameraData.Value.fovFightSwitchTime;
            fov2.reset = true;

            rseOnCameraFOV.Call(fov2);
        }

        if (resetAttack != null)
        {
            StopCoroutine(resetAttack);
            resetAttack = null;
        }

        resetAttack = StartCoroutine(S_Utils.Delay(lastAttack.bossAttack.timeAfterAttack, () => canAttack = true));

        yield return new WaitForSeconds(0.3f);

        if (!isPlayerDeath || target != null)
        {
            UpdateState(S_EnumBossState.Chasing);
        }
        else
        {
            animator.SetBool(moveParam, false);
            animator.SetFloat(moveSpeedParam, navMeshAgent.speed);

            target = null;
        }
    }

    private void SpecialAttackEnd()
    {
        isAttack = false;
        unlockRotate = false;

        rootMotionModifier.Setup(1, 0);

        animator.SetBool(attackParam, false);

        reflectCollider.enabled = false;
        rb.isKinematic = false;

        if (resetAttack != null)
        {
            StopCoroutine(resetAttack);
            resetAttack = null;
        }

        resetAttack = StartCoroutine(S_Utils.Delay(lastAttack.bossAttack.timeAfterAttack, () => canAttack = true));

        if (!isPlayerDeath || target != null)
        {
            UpdateState(S_EnumBossState.Chasing);
        }
        else
        {
            animator.SetBool(moveParam, false);
            animator.SetFloat(moveSpeedParam, navMeshAgent.speed);

            target = null;
        }
    }
    #endregion

    #region Stun
    private void StartStun()
    {
        if (isStun) return;

        isStun = true;
        reflectCollider.enabled = false;
        bossAttackData.Interuption(true);

        navMeshAgent.stoppingDistance = 0.2f;
        navMeshAgent.speed = 0;

        rootMotionModifier.Setup(1, 0);
        animator.SetTrigger(stunParam);

        if (rsoCurrentTarget.Value == body)
        {
            S_ClassCameraFOV fov = new();
            fov.value = 60;
            fov.time = ssoCameraData.Value.fovFightSwitchTime;
            fov.reset = true;

            rseOnCameraFOV.Call(fov);
        }

        if (resetAttack != null)
        {
            StopCoroutine(resetAttack);
            resetAttack = null;
        }

        resetAttack = StartCoroutine(S_Utils.Delay(lastAttack.bossAttack.timeAfterAttack, () => canAttack = true));

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        stunCoroutine = StartCoroutine(Stun());
    }

    private IEnumerator Stun()
    {
        yield return new WaitForSeconds(ssoBossData.Value.stunDuration);

        bossAttackData.Interuption(false);

        if (target != null) UpdateState(S_EnumBossState.Chasing);
        else UpdateState(S_EnumBossState.ReturnIdle);
    }
    #endregion

    private void Death()
    {
        if (isDead) return;

        isDead = true;
        bossAttackData.Interuption(true);

        navMeshAgent.stoppingDistance = 0.2f;
        navMeshAgent.speed = 0;

        rootMotionModifier.Setup(1, 0);
        enemyHeadLookAtIK.IsDead(true);
        animator.SetTrigger(deathParam);

        if (rsoCurrentTarget.Value == body)
        {
            S_ClassCameraFOV fov = new();
            fov.value = 60;
            fov.time = ssoCameraData.Value.fovFightSwitchTime;
            fov.reset = true;

            rseOnCameraFOV.Call(fov);
        }

        rseOnBossDeath.Call();
        rseOnEnemyTargetDied.Call(body);

        if (currentPhaseState == S_EnumBossPhaseState.Phase2) rseOnDisplayBossHealth.Call(false);

        if (resetAttack != null)
        {
            StopCoroutine(resetAttack);
            resetAttack = null;
        }

        canAttack = false;
        target = null;

        bodyCollider.enabled = false;
        detectionCollider.enabled = false;
        hurtCollider.enabled = false;

        if (currentPhaseState == S_EnumBossPhaseState.Phase1)
        {
            rseOnEndBossP1.Call();
            StartCoroutine(S_Utils.Delay(ssoBossData.Value.timeAfterDeathToFinishBoss, () => rseOnFinishBossP1.Call()));
        }
        else if (currentPhaseState == S_EnumBossPhaseState.Phase2)
        {
            rseOnEndBossP2.Call();
            StartCoroutine(S_Utils.Delay(ssoBossData.Value.timeAfterDeathToFinishBoss, () => rseOnFinishBossP2.Call()));
        }
    }
}