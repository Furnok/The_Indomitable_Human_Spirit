using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class S_Enemy : MonoBehaviour
{
    [TabGroup("Settings")]
    [Title("Enemy Data")]
    [SerializeField] private SSO_EnemyData ssoEnemyData;

    [TabGroup("Settings")]
    [Title("Patrol Points")]
    [SerializeField] private List<GameObject> patrolPointsList;

    [TabGroup("References")]
    [Title("Agent")]
    [SerializeField] private NavMeshAgent navMeshAgent;

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
    [SerializeField, S_AnimationName("animator")] private string hitHeavyParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string strafXParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string strafYParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string moveSpeedParam;

    [TabGroup("References")]
    [Title("Body")]
    [SerializeField] private GameObject body;

    [TabGroup("References")]
    [Title("Center")]
    [SerializeField] private GameObject center;

    [TabGroup("References")]
    [Title("Projectile")]
    [SerializeField] private GameObject spawnProjectilePoint;

    [TabGroup("References")]
    [Title("Scripts")]
    [SerializeField] private S_EnemyAttackData enemyAttackData;

    [TabGroup("References")]
    [SerializeField] private S_EnemyDetectionRange enemyDetectionRange;

    [TabGroup("References")]
    [SerializeField] private S_EnemyHurt enemyHurt;

    [TabGroup("References")]
    [SerializeField] private S_EnemyUI enemyUI;

    [TabGroup("References")]
    [SerializeField] private S_EnemyMaxTravelZone enemyMaxTravelZone;

    [TabGroup("References")]
    [SerializeField] private S_EnemyHeadLookAtIK enemyHeadLookAtIK;

    [TabGroup("References")]
    [SerializeField] private S_EnemyRootMotionModifier rootMotionModifier;

    [TabGroup("References")]
    [SerializeField] private S_EnemyProjectile enemyProjectile;

    [TabGroup("References")]
    [Title("Patrol Points Parent")]
    [SerializeField] private Transform patrolPoints;

    [TabGroup("References")]
    [Title("UI Damage")]
    [SerializeField] private S_EnemyUIDamage enemyUIDamage;

    [TabGroup("References")]
    [SerializeField] private GameObject textParent;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnEnemyTargetDied rseOnEnemyTargetDied;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDataLoad rseOnDataLoad;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerDeath rseOnPlayerDeath;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnSendConsoleMessage rseOnSendConsoleMessage;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerRespawn rseOnPlayerRespawn;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCameraFOV rseOnCameraFOV;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_DataSaved rsoDataSaved;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CurrentTarget rsoCurrentTarget;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_CameraData ssoCameraData;

    private float health = 0;

    private int currentPatrolIndex = 0;

    private Transform aimPoint = null;

    private AnimatorOverrideController overrideController = null;

    private GameObject targetInZone = null;
    private GameObject target = null;

    private S_EnumEnemyState currentState = S_EnumEnemyState.Idle;
    private S_ClassAnimationsCombos combo = null;

    private Coroutine idleCoroutine = null;
    private Coroutine comboCoroutine = null;
    private Coroutine stunCoroutine = null;
    private Coroutine resetAttack = null;
    private Coroutine patrolingCoroutine = null;
    private Coroutine returnBackCoroutine = null;

    private bool isPerformingCombo = false;

    private bool isPatroling = false;
    private bool isChasing = false;
    private bool isFighting = false;
    private bool isHeavyHit = false;
    private bool isDead = false;

    private bool canAttack = false;
    private bool isPlayerDeath = false;
    private bool isAttacking = false;
    private bool unlockRotate = false;

    private Vector3 posBeforeChase = Vector3.zero;

    private void Awake()
    {
        Refresh();

        canAttack = true;
        navMeshAgent.avoidancePriority = Random.Range(0, 99);

        Animator anim = animator;
        AnimatorOverrideController instance = new AnimatorOverrideController(ssoEnemyData.Value.controllerOverride);

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        ssoEnemyData.Value.controllerOverride.GetOverrides(overrides);
        instance.ApplyOverrides(overrides);

        anim.runtimeAnimatorController = instance;

        overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;

        health = ssoEnemyData.Value.health;
        navMeshAgent.speed = ssoEnemyData.Value.speedPatrol;

        enemyDetectionRange.Setup(ssoEnemyData);
        enemyUI.Setup(ssoEnemyData);
        enemyMaxTravelZone.Setup(ssoEnemyData);
    }

    private void OnEnable()
    {
        rseOnPlayerDeath.action += PlayerDeath;
        rseOnPlayerRespawn.action += PlayerRespawn;
        rseOnDataLoad.action += LoadEnemy;
    }

    private void OnDisable()
    {
        rseOnPlayerDeath.action -= PlayerDeath;
        rseOnPlayerRespawn.action -= PlayerRespawn;
        rseOnDataLoad.action -= LoadEnemy;
    }

    private void Start()
    {
        S_ClassEnemySaved enemy = new S_ClassEnemySaved
        {
            entity = gameObject,
            isDead = false,
        };

        rsoDataSaved.Value.enemy.Add(enemy);

        UpdateState(S_EnumEnemyState.Idle);
    }

    private void Update()
    {
        enemyHeadLookAtIK.SetTarget(target);

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
        if (target != null)
        {
            Vector3 direction = target.transform.position - center.transform.position;
            direction.y = 0;

            Quaternion targetRot = Quaternion.LookRotation(direction);

            transform.DOKill();
            transform.DORotateQuaternion(targetRot, ssoEnemyData.Value.rotationTime);
        }
    }

    public void RotateEnemyAnim()
    {
        unlockRotate = true;
    }

    public void StopRotateEnemyAnim()
    {
        unlockRotate = false;
    }

    #region Patrol Points
    [TabGroup("Settings")]
    [Button(ButtonSizes.Medium)]
    private void RefreshPatrolPoints()
    {
        Refresh();
    }

    private void Refresh()
    {
        patrolPointsList.Clear();

        foreach (Transform child in patrolPoints) patrolPointsList.Add(child.gameObject);
    }
    #endregion

    #region States
    private void UpdateState(S_EnumEnemyState newState)
    {
        ResetEnemy();

        currentState = newState;

        switch (currentState)
        {
            case S_EnumEnemyState.Idle:
                Idle();
                break;
            case S_EnumEnemyState.Patroling:
                Patroling();
                break;
            case S_EnumEnemyState.Chasing:
                Chasing();
                break;
            case S_EnumEnemyState.Fighting:
                Fighting();
                break;
            case S_EnumEnemyState.HeavyHit:
                HeavyHit();
                break;
            case S_EnumEnemyState.Death:
                Death();
                break;
        }
    }

    private void ResetEnemy()
    {
        enemyAttackData.DisableWeaponCollider();
        enemyAttackData.VFXStopTrail();

        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }

        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
            comboCoroutine = null;
        }

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        if (patrolingCoroutine != null)
        {
            StopCoroutine(patrolingCoroutine);
            patrolingCoroutine = null;
        }

        isPerformingCombo = false;
        isPatroling = false;
        isChasing = false;
        isFighting = false;
        isHeavyHit = false;
        isAttacking = false;
        unlockRotate = false;

        animator.SetTrigger(stopAttackParam);
        animator.SetFloat(strafXParam, 0);
        animator.SetFloat(strafYParam, 0);
        animator.SetFloat(moveSpeedParam, 0);

        navMeshAgent.ResetPath();
        navMeshAgent.velocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
    }
    #endregion

    #region Target
    public void SetTargetInMaxTravelZone(GameObject newTarget)
    {
        if (isDead) return;

        targetInZone = newTarget;
    }

    public void SetTarget(GameObject newTarget)
    {
        if (newTarget == target || isDead || newTarget != targetInZone) return;

        target = newTarget;

        if (isPerformingCombo || isHeavyHit) return;

        if (target != null)
        {
            newTarget.TryGetComponent<I_AimPointProvider>(out I_AimPointProvider aimPointProvider);
            aimPoint = aimPointProvider != null ? aimPointProvider.GetAimPoint() : newTarget.transform;

            if (posBeforeChase == Vector3.zero) posBeforeChase = transform.position;

            UpdateState(S_EnumEnemyState.Chasing);
        }
        else
        {
            if (health != ssoEnemyData.Value.health)
            {
                health = ssoEnemyData.Value.health;
                enemyUI.UpdateHealthBar(health);
            }

            aimPoint = null;
            detectionCollider.enabled = false;

            animator.SetBool(idleAttack, false);

            if (returnBackCoroutine != null)
            {
                StopCoroutine(returnBackCoroutine);
                returnBackCoroutine = null;
            }

            returnBackCoroutine = StartCoroutine(ReturnBack());
        }
    }

    private IEnumerator ReturnBack()
    {
        UpdateState(S_EnumEnemyState.ReturnBack);

        navMeshAgent.speed = ssoEnemyData.Value.speedChase;

        navMeshAgent.SetDestination(posBeforeChase);

        yield return new WaitUntil(() => !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= 0.01f);

        posBeforeChase = Vector3.zero;

        UpdateState(S_EnumEnemyState.Patroling);
    }
    #endregion

    #region Load
    private void LoadEnemy()
    {
        int index = 0;

        for (int i = 0; i < rsoDataSaved.Value.enemy.Count; i++)
        {
            if (rsoDataSaved.Value.enemy[i].entity == gameObject)
            {
                index = i;
                break;
            }
        }

        if (rsoDataSaved.Value.enemy[index].isDead) UpdateState(S_EnumEnemyState.Death);
    }
    #endregion

    #region Damage & Health
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        if (target != null) UpdateHealth(damage);
        else
        {
            if (targetInZone != null)
            {
                UpdateHealth(damage);

                if (!isDead)
                {
                    SetTarget(targetInZone);

                    UpdateState(S_EnumEnemyState.Chasing);
                }
            }
        }
    }

    private void UpdateHealth(float damage)
    {
        rseOnSendConsoleMessage.Call(gameObject.transform.parent.name + " Take " + damage + " Damage!");

        TextDamageDisplay(damage);

        health = Mathf.Max(health - damage, 0);

        enemyUI.UpdateHealthBar(health);

        if (health <= 0) UpdateState(S_EnumEnemyState.Death);
        else if (damage >= 1) UpdateState(S_EnumEnemyState.HeavyHit);
    }

    private void TextDamageDisplay(float damage)
    {
        S_EnemyUIDamage textDamage = Instantiate(enemyUIDamage, textParent.transform.position, Quaternion.identity);
        textDamage.Initialize(damage);
    }
    #endregion

    #region Player
    private void PlayerDeath()
    {
        if (target == null) return;

        if (health != ssoEnemyData.Value.health)
        {
            health = ssoEnemyData.Value.health;
            enemyUI.UpdateHealthBar(health);
        }

        aimPoint = null;
        detectionCollider.enabled = false;
        isPlayerDeath = true;

        targetInZone = null;
    }

    private void PlayerRespawn()
    {
        if (isPlayerDeath)
        {
            isPlayerDeath = false;
            detectionCollider.enabled = false;

            target = null;
            targetInZone = null;

            if (currentState != S_EnumEnemyState.ReturnBack)
            {
                animator.SetBool(idleAttack, false);
                animator.SetTrigger(stopAttackParam);

                if (resetAttack != null)
                {
                    StopCoroutine(resetAttack);
                    resetAttack = null;
                }

                resetAttack = StartCoroutine(S_Utils.Delay(ssoEnemyData.Value.attackCooldown, () => canAttack = true));

                rootMotionModifier.Setup(1, 0);

                ResetEnemy();

                navMeshAgent.speed = ssoEnemyData.Value.speedChase;

                if (returnBackCoroutine != null)
                {
                    StopCoroutine(returnBackCoroutine);
                    returnBackCoroutine = null;
                }

                returnBackCoroutine = StartCoroutine(ReturnBack());
            }
        }
    }
    #endregion

    #region Idle
    private void Idle()
    {
        float waitTime = Random.Range(ssoEnemyData.Value.startPatrolWaitMin, ssoEnemyData.Value.startPatrolWaitMax );

        idleCoroutine = StartCoroutine(S_Utils.Delay(waitTime, () => UpdateState(S_EnumEnemyState.Patroling)));
    }
    #endregion

    #region Patroling
    private void Patroling()
    {
        isPatroling = true;
        detectionCollider.enabled = true;

        if (patrolPointsList == null || patrolPointsList.Count == 0) return;

        navMeshAgent.speed = ssoEnemyData.Value.speedPatrol;

        patrolingCoroutine = StartCoroutine(PatrolingRoutine());

        rseOnSendConsoleMessage.Call(gameObject.transform.parent.name + " Start Patroling!");
    }

    private IEnumerator PatrolingRoutine()
    {
        while (isPatroling)
        {
            GameObject targetPoint = patrolPointsList[currentPatrolIndex];

            navMeshAgent.SetDestination(targetPoint.transform.position);

            while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance) yield return null;

            float waitTime = Random.Range(ssoEnemyData.Value.patrolPointWaitMin, ssoEnemyData.Value.patrolPointWaitMax);

            yield return new WaitForSeconds(waitTime);

            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPointsList.Count;
        }
    }
    #endregion

    #region Chasing
    private void Chasing()
    {
        isChasing = true;

        SetCombo();

        navMeshAgent.speed = ssoEnemyData.Value.speedChase;

        rseOnSendConsoleMessage.Call(gameObject.transform.parent.name + " is Chasing the Player!");
    }

    private void Chase()
    {
        float distance = Vector3.Distance(center.transform.position, target.transform.position);

        bool destinationReached = distance <= (combo.distanceToChase);

        if (!destinationReached) navMeshAgent.SetDestination(target.transform.position);
        else if (destinationReached) UpdateState(S_EnumEnemyState.Fighting);
    }
    #endregion

    #region Fighting
    private void SetCombo()
    {
        int rnd = Random.Range(0, ssoEnemyData.Value.listCombos.Count);

        combo = ssoEnemyData.Value.listCombos[rnd];
    }

    private void Fighting()
    {
        isFighting = true;

        animator.SetBool(idleAttack, true);
    }

    private void Fight()
    {
        if (canAttack)
        {
            float distance = Vector3.Distance(center.transform.position, target.transform.position);

            if (distance > combo.distanceToLoseAttack)
            {
                navMeshAgent.speed = ssoEnemyData.Value.speedChase;

                animator.SetBool(idleAttack, false);

                UpdateState(S_EnumEnemyState.Chasing);

                return;
            }
            else
            {
                canAttack = false;

                if (comboCoroutine != null)
                {
                    StopCoroutine(comboCoroutine);
                    comboCoroutine = null;
                }

                comboCoroutine = StartCoroutine(PlayComboSequence());

                return;
            }
        }
        else if (!isPerformingCombo)
        {
            float distance = Vector3.Distance(center.transform.position, target.transform.position);

            if (distance > combo.distanceToLoseAttack)
            {
                navMeshAgent.speed = ssoEnemyData.Value.speedChase;

                animator.SetBool(idleAttack, false);

                UpdateState(S_EnumEnemyState.Chasing);

                return;
            }
        }
    }
    private IEnumerator PlayComboSequence()
    {
        isPerformingCombo = true;

        yield return null;

        rseOnSendConsoleMessage.Call(gameObject.transform.parent.name + " is Attacking with a Combo!");

        animator.SetBool(idleAttack, false);
        isAttacking = false;

        if (rsoCurrentTarget.Value == body)
        {
            S_ClassCameraFOV fov = new S_ClassCameraFOV();
            fov.value = ssoCameraData.Value.fovFight;
            fov.time = ssoCameraData.Value.fovFightSwitchTime;
            fov.reset = true;

            rseOnCameraFOV.Call(fov);
        }

        for (int i = 0; i < combo.listAnimationsCombos.Count; i++)
        {
            isAttacking = true;
            RotateEnemy();

            string overrideKey = (i % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
            overrideController[overrideKey] = combo.listAnimationsCombos[i].animation;

            rootMotionModifier.Setup(combo.listAnimationsCombos[i].rootMotionMultiplier, combo.distanceMin);

            enemyAttackData.SetAttackMode(combo.listAnimationsCombos[i].attackData);

            if (combo.listAnimationsCombos[i].showVFXAttackType) enemyAttackData.VFXAttackType();
            
            animator.SetTrigger(i == 0 ? attackParam : comboParam);

            yield return new WaitForSeconds(combo.listAnimationsCombos[i].animation.length);

            if (combo.listAnimationsCombos[i].attackData.attackType == S_EnumEnemyAttackType.Projectile)
            {
                yield return new WaitForSeconds(combo.listAnimationsCombos[i].attackData.timeCast);

                if (target != null)
                {
                    S_EnemyProjectile projectileInstance = Instantiate(enemyProjectile, spawnProjectilePoint.transform.position, Quaternion.identity);
                    projectileInstance.Initialize(bodyCollider.transform, aimPoint, combo.listAnimationsCombos[i].attackData);
                }
            }

            isAttacking = false;
            RotateEnemy();

            if (target != null)
            {
                float distance = Vector3.Distance(center.transform.position, target.transform.position);
                if (distance > combo.distanceToLoseAttack)
                {
                    enemyAttackData.DisableWeaponCollider();
                    enemyAttackData.VFXStopTrail();

                    break;
                }
            }
            else
            {
                enemyAttackData.DisableWeaponCollider();
                enemyAttackData.VFXStopTrail();

                break;
            }

            yield return null;
        }

        rootMotionModifier.Setup(1, 0);
        animator.SetTrigger(stopAttackParam);
        animator.SetBool(idleAttack, true);

        isPerformingCombo = false;
        isAttacking = false;
        unlockRotate = false;

        if (rsoCurrentTarget.Value == body)
        {
            S_ClassCameraFOV fov2 = new S_ClassCameraFOV();
            fov2.value = 60;
            fov2.time = ssoCameraData.Value.fovFightSwitchTime;
            fov2.reset = true;

            rseOnCameraFOV.Call(fov2);
        }

        if (isPlayerDeath)
        {
            SetTarget(null);

            if (resetAttack != null)
            {
                StopCoroutine(resetAttack);
                resetAttack = null;
            }

            resetAttack = StartCoroutine(S_Utils.Delay(ssoEnemyData.Value.attackCooldown, () => canAttack = true));

            yield break;
        }

        if (target != null)
        {
            SetCombo();
        }
        else
        {
            animator.SetBool(idleAttack, false);
            detectionCollider.enabled = false;

            ResetEnemy();

            navMeshAgent.speed = ssoEnemyData.Value.speedChase;

            if (returnBackCoroutine != null)
            {
                StopCoroutine(returnBackCoroutine);
                returnBackCoroutine = null;
            }

            returnBackCoroutine = StartCoroutine(ReturnBack());
        }

        if (resetAttack != null)
        {
            StopCoroutine(resetAttack);
            resetAttack = null;
        }

        resetAttack = StartCoroutine(S_Utils.Delay(ssoEnemyData.Value.attackCooldown, () => canAttack = true));
    }
    #endregion

    #region HeavyHit
    private void HeavyHit()
    {
        isHeavyHit = true;

        animator.SetBool(idleAttack, false);
        animator.SetTrigger(hitHeavyParam);

        if (resetAttack != null)
        {
            StopCoroutine(resetAttack);
            resetAttack = null;
        }

        resetAttack = StartCoroutine(S_Utils.Delay(ssoEnemyData.Value.attackCooldown, () => canAttack = true));

        rootMotionModifier.Setup(1, 0);

        if (rsoCurrentTarget.Value == body)
        {
            S_ClassCameraFOV fov = new S_ClassCameraFOV();
            fov.value = 60;
            fov.time = ssoCameraData.Value.fovFightSwitchTime;
            fov.reset = true;

            rseOnCameraFOV.Call(fov);
        }

        rseOnSendConsoleMessage.Call(gameObject.transform.parent.name + " is Stun!");

        stunCoroutine = StartCoroutine(S_Utils.Delay(ssoEnemyData.Value.stunTime, () =>
        {
            if (target != null)
            {
                SetCombo();

                navMeshAgent.speed = ssoEnemyData.Value.speedChase;

                UpdateState(S_EnumEnemyState.Chasing);
            }
            else
            {
                detectionCollider.enabled = false;

                ResetEnemy();

                navMeshAgent.speed = ssoEnemyData.Value.speedChase;

                if (returnBackCoroutine != null)
                {
                    StopCoroutine(returnBackCoroutine);
                    returnBackCoroutine = null;
                }

                returnBackCoroutine = StartCoroutine(ReturnBack());
            }
        }));
    }
    #endregion

    #region Death
    private void Death()
    {
        isDead = true;
        animator.SetBool(idleAttack, false);

        animator.SetTrigger(deathParam);

        var list = rsoDataSaved.Value.enemy;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].entity == gameObject)
            {
                list[i].isDead = isDead;
                break;
            }
        }

        if (resetAttack != null)
        {
            StopCoroutine(resetAttack);
            resetAttack = null;
        }

        canAttack = false;
        target = null;
        targetInZone = null;

        if (rsoCurrentTarget.Value == body)
        {
            S_ClassCameraFOV fov = new S_ClassCameraFOV();
            fov.value = 60;
            fov.time = ssoCameraData.Value.fovFightSwitchTime;
            fov.reset = true;

            rseOnCameraFOV.Call(fov);
        }

        bodyCollider.enabled = false;
        detectionCollider.enabled = false;
        hurtCollider.enabled = false;

        enemyHeadLookAtIK.IsDead(true);

        rseOnEnemyTargetDied.Call(body);

        rseOnSendConsoleMessage.Call(gameObject.transform.parent.name + " is Dead!");
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (patrolPointsList == null || patrolPointsList.Count < 2) return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < patrolPointsList.Count; i++)
        {
            GameObject current = patrolPointsList[i];
            GameObject next = patrolPointsList[(i + 1) % patrolPointsList.Count];

            if (current != null && next != null)
            {
                Gizmos.DrawLine(current.transform.position, next.transform.position);
                Gizmos.DrawSphere(current.transform.position, 0.2f);
            }
        }

        if (Application.isPlaying && patrolPointsList.Count > 0)
        {
            if (isPatroling && currentPatrolIndex >= 0 && currentPatrolIndex < patrolPointsList.Count)
            {
                GameObject target = patrolPointsList[currentPatrolIndex];
                if (target != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(target.transform.position, 0.3f);
                }
            }
        }
    }
}