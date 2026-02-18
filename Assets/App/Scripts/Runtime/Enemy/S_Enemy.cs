using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class S_Enemy : MonoBehaviour
{
    [TabGroup("Settings")]
    [Title("Enemy Data")]
    [SerializeField] private SSO_EnemyData ssoEnemyData;

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
    [SerializeField, S_AnimationName("animator")] private string combatParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string attackParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string comboParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string stunParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName("animator")] private string deadParam;

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
    private AnimatorOverrideController overrideController = null;
    private S_ClassAnimationsCombos combo = null;

    private int currentPatrolIndex = 0;
    private List<GameObject> patrolPointsList = new();
    private Vector3 posBeforeChase = Vector3.zero;

    private GameObject targetInZone = null;
    private GameObject currentTarget = null;
    private Transform aimPoint = null;

    private S_EnumEnemyState currentState = S_EnumEnemyState.None;

    private Tween rotateTween = null;

    private Coroutine idleCoroutine = null;
    private Coroutine patrolingCoroutine = null;
    private Coroutine returnPatrolingCoroutine = null;
    private Coroutine attackCoroutine = null;
    private Coroutine resetAttack = null;
    private Coroutine stunCoroutine = null;

    private bool isIdle = false;
    private bool isPatroling = false;
    private bool isReturnPatroling = false;
    private bool isChasing = false;
    private bool isCombat = false;
    private bool isAttack = false;
    private bool isStun = false;
    private bool isDead = false;

    private bool isPlayerDeath = false;
    private bool canAttack = true;
    private bool unlockRotate = false;

    private void Awake()
    {
        patrolPointsList.Clear();

        foreach (Transform child in patrolPoints) patrolPointsList.Add(child.gameObject);

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

        canAttack = true;
        SetCombo();

        UpdateState(S_EnumEnemyState.Idle);
    }

    private void Update()
    {
        if ((currentState == S_EnumEnemyState.Chasing || currentState == S_EnumEnemyState.Combat || currentState == S_EnumEnemyState.Attack) && !isDead) enemyHeadLookAtIK.SetTarget(currentTarget);

        if (currentTarget != null && (unlockRotate || !isAttack) && !isDead) RotateEnemy();

        if (isChasing && !isDead) Chasing();

        if (isCombat && !isDead) Combat();
    }

    public void RotateEnemy()
    {
        if (currentTarget != null)
        {
            Vector3 direction = currentTarget.transform.position - center.transform.position;
            direction.y = 0;

            Quaternion targetRot = Quaternion.LookRotation(direction);

            rotateTween?.Kill();
            rotateTween = transform.DORotateQuaternion(targetRot, ssoEnemyData.Value.rotationTime);
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

    private void SetDestinationToPatrolPoint(Vector3 newPos)
    {
        if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas)) navMeshAgent.SetDestination(hit.position);
        else navMeshAgent.SetDestination(newPos);
    }

    #region States
    private void UpdateState(S_EnumEnemyState newState)
    {
        if (currentState == newState) return;

        ResetEnemy();

        currentState = newState;

        switch (currentState)
        {
            case S_EnumEnemyState.Idle:
                StartIdle();
                break;
            case S_EnumEnemyState.Patroling:
                StartPatroling();
                break;
            case S_EnumEnemyState.ReturnPatroling:
                StartReturnPatroling();
                break;
            case S_EnumEnemyState.Chasing:
                StartChasing();
                break;
            case S_EnumEnemyState.Combat:
                StartCombat();
                break;
            case S_EnumEnemyState.Attack:
                StartAttack();
                break;
            case S_EnumEnemyState.Stun:
                StartStun();
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

        isIdle = false;
        isPatroling = false;
        isReturnPatroling = false;
        isChasing = false;
        isCombat = false;
        isAttack = false;
        isStun = false;
        isDead = false;

        unlockRotate = false;

        rotateTween?.Kill();

        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }

        if (patrolingCoroutine != null)
        {
            StopCoroutine(patrolingCoroutine);
            patrolingCoroutine = null;
        }

        if (returnPatrolingCoroutine != null)
        {
            StopCoroutine(returnPatrolingCoroutine);
            returnPatrolingCoroutine = null;
        }

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
        animator.SetBool(combatParam, false);
    }
    #endregion

    #region Target
    public void SetTargetInMaxTravelZone(GameObject newTarget)
    {
        if (isDead) return;

        if (currentTarget == newTarget) targetInZone = null;
        else targetInZone = newTarget;
    }

    public void DetectTarget(GameObject newTarget)
    {
        if (isDead || currentTarget == newTarget) return;

        SetTarget(newTarget);
    }

    public void SetTarget(GameObject newTarget)
    {
        if (isDead || newTarget != targetInZone) return;

        currentTarget = newTarget;

        if (currentTarget != null)
        {
            newTarget.TryGetComponent<I_AimPointProvider>(out I_AimPointProvider aimPointProvider);
            aimPoint = aimPointProvider != null ? aimPointProvider.GetAimPoint() : newTarget.transform;

            if (posBeforeChase == Vector3.zero) posBeforeChase = center.transform.position;

            UpdateState(S_EnumEnemyState.Chasing);
        }
        else
        {
            aimPoint = null;

            animator.SetBool(attackParam, false);

            if (resetAttack != null)
            {
                StopCoroutine(resetAttack);
                resetAttack = null;
            }

            resetAttack = StartCoroutine(S_Utils.Delay(ssoEnemyData.Value.attackCooldown, () => canAttack = true));

            if (!ssoEnemyData.Value.isIdle) UpdateState(S_EnumEnemyState.ReturnPatroling);
            else UpdateState(S_EnumEnemyState.Idle);
        }
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

        if (currentTarget != null) UpdateHealth(damage);
        else
        {
            if (targetInZone != null)
            {
                UpdateHealth(damage);

                if (!isDead) SetTarget(targetInZone);
            }
        }
    }

    private void UpdateHealth(float damage)
    {
        TextDamageDisplay(damage);

        health = Mathf.Max(health - damage, 0);

        enemyUI.UpdateHealthBar(health);

        if (health <= 0) UpdateState(S_EnumEnemyState.Death);
        else if (damage >= ssoEnemyData.Value.health / 2) UpdateState(S_EnumEnemyState.Stun);
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
        if (currentTarget == null) return;

        currentTarget = null;
        targetInZone = null;
        aimPoint = null;

        isPlayerDeath = true;
    }

    private void PlayerRespawn()
    {
        if (isPlayerDeath)
        {
            isPlayerDeath = false;

            UpdateState(S_EnumEnemyState.ReturnPatroling);
        }
    }
    #endregion

    #region Idle
    private void StartIdle()
    {
        if (isIdle || ssoEnemyData.Value.isIdle) return;

        isIdle = true;

        navMeshAgent.speed = ssoEnemyData.Value.speedPatrol;
        navMeshAgent.stoppingDistance = 0.2f;

        float waitTime = Random.Range(ssoEnemyData.Value.startPatrolWaitMin, ssoEnemyData.Value.startPatrolWaitMax );

        animator.SetBool(moveParam, false);

        idleCoroutine = StartCoroutine(S_Utils.Delay(waitTime, () => UpdateState(S_EnumEnemyState.Patroling)));
    }
    #endregion

    #region Patroling
    private void StartPatroling()
    {
        if (patrolPointsList == null || patrolPointsList.Count == 0 || isPatroling) return;

        isPatroling = true;

        detectionCollider.enabled = true;
        navMeshAgent.speed = ssoEnemyData.Value.speedPatrol;
        navMeshAgent.stoppingDistance = 0.2f;

        animator.SetBool(moveParam, true);

        patrolingCoroutine = StartCoroutine(PatrolingRoutine());
    }

    private IEnumerator PatrolingRoutine()
    {
        while (isPatroling)
        {
            GameObject targetPoint = patrolPointsList[currentPatrolIndex];

            animator.SetBool(moveParam, true);

            SetDestinationToPatrolPoint(targetPoint.transform.position);

            yield return new WaitUntil(() => !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance);

            animator.SetBool(moveParam, false);

            yield return new WaitForSeconds(Random.Range(ssoEnemyData.Value.patrolPointWaitMin, ssoEnemyData.Value.patrolPointWaitMax));

            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPointsList.Count;
        }
    }

    private void StartReturnPatroling()
    {
        if (isReturnPatroling) return;

        isReturnPatroling = true;

        detectionCollider.enabled = false;
        enemyHeadLookAtIK.SetTarget(null);

        navMeshAgent.speed = ssoEnemyData.Value.speedReturnPatrol;
        navMeshAgent.stoppingDistance = 0.2f;

        returnPatrolingCoroutine = StartCoroutine(ReturnBack());
    }

    private IEnumerator ReturnBack()
    {
        yield return new WaitForSeconds(ssoEnemyData.Value.returnPatrolWait);

        animator.SetBool(moveParam, true);

        SetDestinationToPatrolPoint(posBeforeChase);

        yield return new WaitUntil(() => !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance);

        posBeforeChase = center.transform.position;

        UpdateState(S_EnumEnemyState.Patroling);
    }
    #endregion

    #region Chasing
    private void StartChasing()
    {
        if (isChasing) return;

        isChasing = true;
        navMeshAgent.stoppingDistance = combo.distanceToChase - 0.2f;

        navMeshAgent.speed = ssoEnemyData.Value.speedChase;

        animator.SetBool(moveParam, true);

        SetCombo();
    }

    private void Chasing()
    {
        float distance = Vector3.Distance(center.transform.position, currentTarget.transform.position);

        bool destinationReached = distance <= combo.distanceToChase;

        if (!destinationReached) SetDestinationToPatrolPoint(currentTarget.transform.position);
        else if (destinationReached) UpdateState(S_EnumEnemyState.Combat);
    }
    #endregion

    #region Combat
    private void SetCombo()
    {
        int rnd = Random.Range(0, ssoEnemyData.Value.listCombos.Count);

        combo = ssoEnemyData.Value.listCombos[rnd];
    }

    private void StartCombat()
    {
        if (isCombat) return;

        isCombat = true;
        navMeshAgent.stoppingDistance = combo.distanceToChase - 0.2f;

        navMeshAgent.speed = ssoEnemyData.Value.speedChase;

        animator.SetBool(moveParam, false);
        animator.SetBool(combatParam, true);
    }

    private void Combat()
    {
        if (canAttack)
        {
            float distance = Vector3.Distance(center.transform.position, currentTarget.transform.position);

            if (distance > combo.distanceToLoseAttack) UpdateState(S_EnumEnemyState.Chasing);
            else
            {
                canAttack = false;

                UpdateState(S_EnumEnemyState.Attack);
            }
        }
        else if (!isAttack)
        {
            float distance = Vector3.Distance(center.transform.position, currentTarget.transform.position);

            if (distance > combo.distanceToLoseAttack) UpdateState(S_EnumEnemyState.Chasing);
        }
    }
    #endregion

    #region Attack
    private void StartAttack()
    {
        if (isAttack) return;

        isAttack = true;

        navMeshAgent.stoppingDistance = combo.distanceToChase - 0.2f;

        navMeshAgent.speed = ssoEnemyData.Value.speedChase;

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        attackCoroutine = StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        if (rsoCurrentTarget.Value == body)
        {
            S_ClassCameraFOV fov = new();
            fov.value = ssoCameraData.Value.fovFight;
            fov.time = ssoCameraData.Value.fovFightSwitchTime;
            fov.reset = true;

            rseOnCameraFOV.Call(fov);
        }

        for (int i = 0; i < combo.listAnimationsCombos.Count; i++)
        {
            if (isPlayerDeath && currentTarget == null) break;
            else
            {
                float distance = Vector3.Distance(center.transform.position, currentTarget.transform.position);

                if (distance > combo.distanceToLoseAttack) break;
            }

            RotateEnemy();

            string overrideKey = (i % 2 == 0) ? "AttackAnimation" : "AttackAnimation2";
            overrideController[overrideKey] = combo.listAnimationsCombos[i].animation;

            rootMotionModifier.Setup(combo.listAnimationsCombos[i].rootMotionMultiplier, combo.distanceMin);

            enemyAttackData.SetAttackMode(combo.listAnimationsCombos[i].attackData);

            if (combo.listAnimationsCombos[i].showVFXAttackType) enemyAttackData.VFXAttackType();

            if (i == 0) animator.SetBool(attackParam, true);
            else animator.SetTrigger(comboParam);

            yield return new WaitForSeconds(combo.listAnimationsCombos[i].animation.length);

            if (combo.listAnimationsCombos[i].attackData.attackType == S_EnumAttackType.Projectile)
            {
                yield return new WaitForSeconds(combo.listAnimationsCombos[i].attackData.timeCast);

                if (!isPlayerDeath && currentTarget != null)
                {
                    S_EnemyProjectile projectileInstance = Instantiate(enemyProjectile, spawnProjectilePoint.transform.position, Quaternion.identity);
                    projectileInstance.Initialize(bodyCollider.transform, aimPoint, combo.listAnimationsCombos[i].attackData);
                }
            }

            RotateEnemy();

            enemyAttackData.DisableWeaponCollider();
            enemyAttackData.VFXStopTrail();
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

        resetAttack = StartCoroutine(S_Utils.Delay(ssoEnemyData.Value.attackCooldown, () => canAttack = true));

        if (!isPlayerDeath || currentTarget != null) UpdateState(S_EnumEnemyState.Chasing);
        else
        {
            animator.SetBool(moveParam, false);

            targetInZone = null;
            currentTarget = null;
        }
    }
    #endregion

    #region HeavyHit
    private void StartStun()
    {
        if (isStun) return;

        isStun = true;
        enemyAttackData.Interuption(true);

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

        resetAttack = StartCoroutine(S_Utils.Delay(ssoEnemyData.Value.attackCooldown, () => canAttack = true));

        stunCoroutine = StartCoroutine(Stun());
    }

    private IEnumerator Stun()
    {
        yield return new WaitForSeconds(ssoEnemyData.Value.stunTime);

        enemyAttackData.Interuption(false);

        if (currentTarget != null) UpdateState(S_EnumEnemyState.Chasing);
        else UpdateState(S_EnumEnemyState.ReturnPatroling);
    }
    #endregion

    #region Death
    private void Death()
    {
        if (isDead) return;

        isDead = true;
        enemyAttackData.Interuption(true);

        navMeshAgent.stoppingDistance = 0.2f;
        navMeshAgent.speed = 0;

        rootMotionModifier.Setup(1, 0);
        enemyHeadLookAtIK.IsDead(true);
        animator.SetTrigger(deadParam);

        if (rsoCurrentTarget.Value == body)
        {
            S_ClassCameraFOV fov = new();
            fov.value = 60;
            fov.time = ssoCameraData.Value.fovFightSwitchTime;
            fov.reset = true;

            rseOnCameraFOV.Call(fov);
        }

        rseOnEnemyTargetDied.Call(body);

        if (resetAttack != null)
        {
            StopCoroutine(resetAttack);
            resetAttack = null;
        }

        canAttack = false;
        currentTarget = null;
        targetInZone = null;

        bodyCollider.enabled = false;
        detectionCollider.enabled = false;
        hurtCollider.enabled = false;

        var list = rsoDataSaved.Value.enemy;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].entity == gameObject)
            {
                list[i].isDead = isDead;
                break;
            }
        }
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