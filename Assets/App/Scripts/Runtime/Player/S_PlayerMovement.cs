using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

public class S_PlayerMovement : MonoBehaviour
{
    #region Inspector
    [TabGroup("Settings")]
    [SerializeField] private float groundCheckDist = 0.5f;

    [TabGroup("Settings")]
    [SerializeField] private float stopFromWall = 0.03f;

    /*
    [TabGroup("Settings")]
    [SerializeField] private float skin = 0.02f;

    [TabGroup("Settings")]
    [Title("Stick To Ground")]
    [SerializeField] private float stickToGroundForce = 12f;
    */

    [TabGroup("Settings")]
    [Title("Edge Guard")]
    [SerializeField] private bool preventFallFromEdges = true;

    [TabGroup("Settings")]
    [SerializeField] private float edgeProbeDistance = 0.6f;

    [TabGroup("Settings")]
    [SerializeField] private float edgeProbeHeight = 0.5f;

    [TabGroup("Settings")]
    [Title("Slope Slowdown")]
    [SerializeField, Range(0f, 60f)] private float slopeSlowStart = 5f;

    [TabGroup("Settings")]
    [SerializeField, Range(0f, 60f)] private float slopeSlowMax = 45f;

    [TabGroup("Settings")]
    [SerializeField, Range(0f, 0.9f)] private float slopeSlowAtMax = 0.10f;

    [TabGroup("References")]
    [Title("Animations")]
    [SerializeField, S_AnimationName] private string moveParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName] private string speedParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName] private string _strafXParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName] private string _strafYParam;

    [TabGroup("References")]
    [Title("Layers")]
    [SerializeField] private LayerMask groundMask;

    [TabGroup("References")]
    [SerializeField] private LayerMask obstacleMask;

    [TabGroup("References")]
    [Title("Grounding")]
    [SerializeField] private CapsuleCollider capsule;

    [TabGroup("References")]
    [Title("Rigidbody")]
    [SerializeField] private Rigidbody rigidbodyPlayer;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerMove rseOnPlayerMove;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnNewTargeting rseOnNewTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerCancelTargeting rseOnPlayerCancelTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnParrySuccess _rseOnParrySuccess;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerHit _rseOnPlayerHit;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDataLoad rseOnDataLoad;

    [TabGroup("Inputs")]
    [SerializeField] private RSO_GameInPause _rsoGameInPause;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationBoolValueChange rseOnAnimationBoolValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationFloatValueChange rseOnAnimationFloatValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerAddState _onPlayerAddState;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CameraRotation rsoCameraRotation;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerIsTargeting rsoPlayerIsTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CurrentInputActionMap rsoCurrentInputActionMap;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerIsDodging _playerIsDodging;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentState _playerCurrentState;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerPosition rsoPlayerPosition;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerRotation rsoPlayerRotation;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_DataSaved rsoDataSaved;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStateTransitions _playerStateTransitions;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats _playerStats;
    #endregion

    private float maxSlopeAngle => _playerStats.Value.maxSlopeAngle;
    private float maxDownStepAngle => _playerStats.Value.maxSlopeAngle;

    private Vector2 moveInput = Vector2.zero;
    //private bool inputCanceledOrNoInput = true;
    private Quaternion camRotInInputPerformed = Quaternion.identity;
    private Transform target = null;

    private bool isGrounded = false;
    private Vector3 groundNormal = Vector3.up;
    private float groundAngle = 0;

    private Coroutine knockbackCoroutine = null;

    private void Awake()
    {
        rsoPlayerPosition.Value = transform.position;
        rsoPlayerRotation.Value = transform.rotation;

        rigidbodyPlayer.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnDestroy()
    {
        rsoPlayerRotation.Value = Quaternion.identity;
    }

    private void OnEnable()
    {
        rsoPlayerPosition.Value = transform.position;
        rsoPlayerRotation.Value = transform.rotation;

        rseOnPlayerMove.action += Move;
        rseOnNewTargeting.action += ChangeNewTarget;
        rseOnPlayerCancelTargeting.action += CancelTarget;

        _rseOnParrySuccess.action += DoKnockbackOnParry;
        _rseOnPlayerHit.action += DoKnockbackOnHit;

        _rsoGameInPause.onValueChanged += OnPauseChange;

        rseOnDataLoad.action += SetValueFromData;
    }

    private void OnDisable()
    {
        rseOnPlayerMove.action -= Move;
        rseOnNewTargeting.action -= ChangeNewTarget;
        rseOnPlayerCancelTargeting.action -= CancelTarget;

        _rseOnParrySuccess.action -= DoKnockbackOnParry;
        _rseOnPlayerHit.action -= DoKnockbackOnHit;

        _rsoGameInPause.onValueChanged -= OnPauseChange;

        rseOnDataLoad.action -= SetValueFromData;
    }

    void SetValueFromData()
    {
        transform.position = rsoDataSaved.Value.position;
        transform.rotation = rsoDataSaved.Value.rotation;
    }

    private void FixedUpdate()
    {
        //return;
        UpdateGround();

        if (_playerStateTransitions.Value.CanTransition(_playerCurrentState.Value, S_EnumPlayerState.Moving) == false)
        {
            rseOnAnimationFloatValueChange.Call(speedParam, 0);
            rseOnAnimationFloatValueChange.Call(_strafXParam, 0f);
            rseOnAnimationFloatValueChange.Call(_strafYParam, 0f);
            rseOnAnimationBoolValueChange.Call(moveParam, false);

            if (_playerCurrentState.Value != S_EnumPlayerState.Dodging) // Allow movement after dodging
            {
                rigidbodyPlayer.linearVelocity = Vector3.zero;
            }
        }

        if(_playerCurrentState.Value == S_EnumPlayerState.Dodging)
        {
            if (rsoPlayerIsTargeting.Value && target != null)
            {
                Vector3 toTarget = target.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 1e-4f)
                {
                    Quaternion face = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                    rigidbodyPlayer.MoveRotation(
                        Quaternion.Slerp(rigidbodyPlayer.rotation, face, _playerStats.Value.turnSpeedTargeting * Time.fixedDeltaTime)
                    );
                }
            }
        }

        if (rsoCurrentInputActionMap.Value == S_EnumPlayerInputActionMap.Game)
        {
            if (_playerStateTransitions.Value.CanTransition(_playerCurrentState.Value, S_EnumPlayerState.Moving) == true ||
                _playerStateTransitions.Value.CanTransition(_playerCurrentState.Value, S_EnumPlayerState.Running) == true)
            {
                BuildDesiredDirection(out Vector3 desiredDir, out float baseSpeed);

                if (rsoPlayerIsTargeting.Value && target != null)
                {
                    Vector3 toTarget = target.position - transform.position;
                    toTarget.y = 0f;
                    if (toTarget.sqrMagnitude > 1e-4f)
                    {
                        Quaternion face = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                        rigidbodyPlayer.MoveRotation(
                            Quaternion.Slerp(rigidbodyPlayer.rotation, face, _playerStats.Value.turnSpeedTargeting * Time.fixedDeltaTime)
                        );
                    }
                }
                else if (desiredDir.sqrMagnitude > 1e-6f)
                {
                    Quaternion face = Quaternion.LookRotation(desiredDir, Vector3.up);
                    rigidbodyPlayer.MoveRotation(
                        Quaternion.Slerp(rigidbodyPlayer.rotation, face, _playerStats.Value.turnSpeed * Time.fixedDeltaTime)
                    );
                }
                else
                {
                    rigidbodyPlayer.angularVelocity = Vector3.zero;
                }

                Vector3 groundDir = desiredDir;
                if (isGrounded && groundDir.sqrMagnitude > 1e-6f)
                {
                    groundDir = Vector3.ProjectOnPlane(groundDir, groundNormal).normalized;
                }

                float speedMul = 1f;
                if (isGrounded && groundDir.sqrMagnitude > 1e-6f)
                {
                    Vector3 upslope = Vector3.ProjectOnPlane(Vector3.up, groundNormal);
                    float upslopeMag = upslope.magnitude;

                    if (upslopeMag > 1e-6f)
                    {
                        upslope /= upslopeMag;
                        float uphillDot = Vector3.Dot(groundDir, upslope);
                        if (uphillDot > 0f)
                        {
                            float t = Mathf.InverseLerp(slopeSlowStart, slopeSlowMax, groundAngle);
                            float penalty = Mathf.Clamp01(t) * slopeSlowAtMax;
                            speedMul = 1f - penalty;

                            speedMul = 1f - (1f - speedMul) * uphillDot;
                            speedMul = Mathf.Clamp(speedMul, 0.1f, 1f);
                        }
                    }
                }
                baseSpeed *= speedMul;

                groundDir = LimitSteepDescend(groundDir);

                Vector3 desiredVel = groundDir * baseSpeed;
                if (preventFallFromEdges && desiredVel.sqrMagnitude > 1e-6f)
                {
                    Vector3 stepDir = desiredVel.normalized;
                    if (!HasGroundAhead(rigidbodyPlayer.position, stepDir * edgeProbeDistance, out _))
                        desiredVel = Vector3.zero;
                }

                Vector3 vel = rigidbodyPlayer.linearVelocity;
                vel.x = desiredVel.x;
                vel.z = desiredVel.z;

                if (isGrounded)
                {
                    float vn = Vector3.Dot(vel, groundNormal);
                    if (vn > 0f) vel -= groundNormal * vn;
                    const float targetVn = -0.6f;
                    if (vn > targetVn) vel += -groundNormal * (vn - targetVn);
                }
                rigidbodyPlayer.linearVelocity = vel;

                // Animations
                float horizSpeed = new Vector2(vel.x, vel.z).magnitude;
                rseOnAnimationFloatValueChange.Call(speedParam, horizSpeed);

                bool targetMode = rsoPlayerIsTargeting.Value && target != null;
                PushMovementAnims(targetMode, horizSpeed, moveInput);

                if (moveInput.sqrMagnitude > 0.0001f && _playerCurrentState.Value != S_EnumPlayerState.Running)
                {
                    _onPlayerAddState.Call(S_EnumPlayerState.Moving);
                }
                else if (_playerCurrentState.Value == S_EnumPlayerState.Running && moveInput.sqrMagnitude > 0.0001f)
                {
                    //_isInputCanceled = false;
                }
                else
                {
                    _onPlayerAddState.Call(S_EnumPlayerState.None);
                }

                rsoPlayerPosition.Value = transform.position;
                rsoPlayerRotation.Value = transform.rotation;
            }
        }
        else
        {
            rigidbodyPlayer.linearVelocity = Vector3.zero;
            rseOnAnimationFloatValueChange.Call(speedParam, 0);
        }
    }

    private void OnPauseChange(bool pauseValue)
    {
        if (pauseValue == true)
            Move(Vector2.zero);
    }

    private void ChangeNewTarget(GameObject newTarget)
    {
        target = newTarget.transform;
    }

    private void CancelTarget(GameObject oldTarget)
    {
        target = null;
    }

    private void Move(Vector2 input)
    {
        moveInput = input;

        //if (moveInput != Vector2.zero)
        //{
        //    inputCanceledOrNoInput = false;
        //}
        //else
        //{
        //    inputCanceledOrNoInput = true;
        //}
    }

    private void DoKnockbackOnHit(S_StructAttackContact attackContact)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackCoroutine(attackContact, false));
    }

    private void DoKnockbackOnParry(S_StructAttackContact attackContact)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackCoroutine(attackContact, true));
    }

    private IEnumerator KnockbackCoroutine(S_StructAttackContact contact, bool fromParry)
    {
        var data = contact.data;

        contact.source.TryGetComponent(out I_EnemyTransformProvider transformProfider);

        Transform enemyTransform = transformProfider != null ? transformProfider.GetEnemyTransform() : null;
        //enemyTransform = null;

        bool isNull = enemyTransform == null;

        float duration = fromParry ? data.knockbackOnParryDuration : data.knockbackHitDuration;
        float distance = fromParry ? data.knockbackOnParrryDistance : data.knockbackHitDistance;

        if (duration <= 0f || distance <= 0f)
            yield break;

        Vector3 dir;

        if(enemyTransform != null)
        {
            dir = (transform.position - enemyTransform.position).normalized;
        }
        else
        {
            if (contact.data.attackDirection != Vector3.zero)
            {
                dir = contact.data.attackDirection.normalized;
            }
            else
            {
                dir = (transform.position - contact.source.transform.position).normalized;
            }
        }

        dir = Vector3.ProjectOnPlane(dir, groundNormal).normalized;
        if (dir.sqrMagnitude < 0.0001f) yield break;

        float allowedDist = ProbeObstacle(dir, distance);

        allowedDist = ProbeGroundAhead(dir, allowedDist);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + dir * allowedDist;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;

            Vector3 next = Vector3.Lerp(startPos, endPos, t);

            rigidbodyPlayer.MovePosition(next);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rigidbodyPlayer.MovePosition(endPos);
    }

    private void GetCapsuleWorldEnds(out Vector3 top, out Vector3 bottom, out float radius)
    {
        float height = Mathf.Max(capsule.height * Mathf.Abs(transform.lossyScale.y), capsule.radius * 2f);
        radius = capsule.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
        Vector3 center = transform.TransformPoint(capsule.center);
        Vector3 up = transform.up;
        top = center + up * (height * 0.5f - radius);
        bottom = center - up * (height * 0.5f - radius);
    }

    private bool CheckGround(out RaycastHit hit)
    {
        GetCapsuleWorldEnds(out var top, out var bottom, out var radius);

        Vector3 probeStart = bottom + Vector3.up * 0.02f;
        float dist = groundCheckDist + 0.05f;

        bool ok = Physics.Raycast(
            probeStart, Vector3.down, out hit, dist,
            groundMask, QueryTriggerInteraction.Ignore
        );

        if (ok)
        {
            float ang = Vector3.Angle(hit.normal, Vector3.up);
            if (ang > maxSlopeAngle) ok = false;
        }
        return ok;
    }

    private void UpdateGround()
    {
        if (CheckGround(out var hit))
        {
            isGrounded = true;
            groundNormal = hit.normal;
            groundAngle = Vector3.Angle(groundNormal, Vector3.up);
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            groundAngle = 90f;
        }
    }

    /*
    private void ApplyGroundStick(ref Vector3 vel)
    {
        if (!isGrounded) return;

        if (vel.y > 0f) vel.y = 0f;
        rigidbodyPlayer.AddForce(Vector3.down * stickToGroundForce, ForceMode.Acceleration);

        if (CheckGround(out var hit) && rigidbodyPlayer.linearVelocity.magnitude <= 3f)
        {
            float corr = hit.distance - skin;
            if (Mathf.Abs(corr) < 0.03f)
            {
                rigidbodyPlayer.MovePosition(rigidbodyPlayer.position + Vector3.down * corr);
            }
        }
    }
    */

    private Vector3 LimitSteepDescend(Vector3 along)
    {
        if (!isGrounded) return along;
        if (groundAngle <= maxSlopeAngle) return along;

        Vector3 downhill = Vector3.Cross(Vector3.Cross(groundNormal, Vector3.up), groundNormal).normalized;
        float dotDown = Vector3.Dot(along, downhill);
        if (dotDown > 0f) along -= downhill * dotDown;
        return along.normalized;
    }

    private bool HasGroundAhead(Vector3 currentPos, Vector3 horizontalStep, out RaycastHit hit)
    {
        Vector3 probeOrigin = currentPos + horizontalStep + Vector3.up * edgeProbeHeight;
        if (Physics.Raycast(probeOrigin, Vector3.down, out hit, edgeProbeHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            float ang = Vector3.Angle(hit.normal, Vector3.up);
            return ang <= maxDownStepAngle;
        }
        return false;
    }

    private void BuildDesiredDirection(out Vector3 desiredDir, out float baseSpeed)
    {
        float inputMag = Mathf.Clamp01(moveInput.magnitude);
        bool running = (_playerCurrentState.Value == S_EnumPlayerState.Running);

        if (rsoPlayerIsTargeting.Value && target != null)// If targeting mode
        {
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 1e-6f)
            {
                Vector3 forward = toTarget.normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized; //Strafe basis

                desiredDir = (right * moveInput.x + forward * moveInput.y);
            }
            else
            {
                desiredDir = Vector3.zero;
            }

            baseSpeed = (running ? _playerStats.Value.runSpeed : _playerStats.Value.strafeSpeed) * inputMag;
        }
        else
        {
            Quaternion camRot = (/*inputCanceledOrNoInput ? (*/rsoCameraRotation ? rsoCameraRotation.Value : Quaternion.identity/*) : camRotInInputPerformed*/);

            Vector3 camF = camRot * Vector3.forward; camF.y = 0f; camF.Normalize();
            Vector3 camR = camRot * Vector3.right; camR.y = 0f; camR.Normalize();

            desiredDir = camR * moveInput.x + camF * moveInput.y;
            baseSpeed = (running ? _playerStats.Value.runSpeed : _playerStats.Value.moveSpeed) * inputMag;

            //if (inputCanceledOrNoInput) camRotInInputPerformed = camRot;
        }

        if (desiredDir.sqrMagnitude > 1e-6f) desiredDir.Normalize();
    }

    private void PushMovementAnims(bool isTargetMode, float horizSpeed, Vector2 rawInput)
    {
        rseOnAnimationFloatValueChange.Call(speedParam, horizSpeed);

        if (rawInput.sqrMagnitude > 0.0001f)
        {
            rseOnAnimationBoolValueChange.Call(moveParam, true);

            if (isTargetMode)
            {
                rseOnAnimationFloatValueChange.Call(_strafXParam, rawInput.x);
                rseOnAnimationFloatValueChange.Call(_strafYParam, rawInput.y);
            }
            else
            {
                rseOnAnimationFloatValueChange.Call(_strafXParam, 0f);
                rseOnAnimationFloatValueChange.Call(_strafYParam, 0f);
            }
        }
        else
        {
            rseOnAnimationBoolValueChange.Call(moveParam, false);
            rseOnAnimationFloatValueChange.Call(_strafXParam, 0f);
            rseOnAnimationFloatValueChange.Call(_strafYParam, 0f);
            rseOnAnimationFloatValueChange.Call(speedParam, 0f);
        }
    }

    private float ProbeObstacle(Vector3 dir, float maxDist)
    {
        GetCapsuleWorldEnds(out var top, out var bottom, out var radius);

        bool overlapping = Physics.CheckCapsule(
        bottom, top, radius,
        obstacleMask, QueryTriggerInteraction.Ignore
        );

        if (overlapping)
        {
            return 0f;
        }

        float castDist = maxDist + stopFromWall;

        if (Physics.CapsuleCast(bottom, top, radius, dir, out var hit, castDist, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            float allowed = hit.distance - stopFromWall;
            if (allowed < 0f) allowed = 0f;

            return Mathf.Min(maxDist, allowed);
        }

        return maxDist;
    }

    private float ProbeGroundAhead(Vector3 dir, float maxDist)
    {
        if (!preventFallFromEdges) return maxDist;

        Vector3 probePos = transform.position + dir * maxDist + Vector3.up * edgeProbeHeight;
        if (Physics.Raycast(probePos, Vector3.down, out var hit, edgeProbeHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            float ang = Vector3.Angle(hit.normal, Vector3.up);
            if (ang > maxDownStepAngle)
            {
                return 0f;
            }
            return maxDist;
        }
        return 0f;
    }
}