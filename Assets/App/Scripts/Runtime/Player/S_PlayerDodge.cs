using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class S_PlayerDodge : MonoBehaviour
{
    #region Variables
    [TabGroup("Settings")]
    [SerializeField] private float edgeProbeHeight = 0.5f;

    [TabGroup("Settings")]
    [SerializeField] private float stopFromWall = 0.03f;

    [TabGroup("References")]
    [Title("Layers")]
    [SerializeField] private LayerMask groundMask;

    [TabGroup("References")]
    [SerializeField] private LayerMask obstacleMask;

    [TabGroup("References")]
    [SerializeField] private LayerMask obstacle2Mask;

    [TabGroup("References")]
    [Title("Animations")]
    [SerializeField, S_AnimationName] private string _dodgeParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName] private string _dodgeDirXParam;

    [TabGroup("References")]
    [SerializeField, S_AnimationName] private string _dodgeDirYParam;

    [TabGroup("References")]
    [Title("World / Collision")]
    [SerializeField] private CapsuleCollider _capsule;

    [TabGroup("References")]
    [Title("Rigidbody")]
    [SerializeField] private Rigidbody _rb;

    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference _dodgeSound;

    [TabGroup("References")]
    [SerializeField] private EventReference _dodgeSimpleSound;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerMove _rseOnPlayerMove;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnNewTargeting _rseOnNewTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerCancelTargeting _rseOnPlayerCancelTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerGettingHit _rseOnPlayerGettingHit;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerDodgeInputCancel _onPlayerDodgeInputCancel;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerDodgeInput rseOnPlayerDodge;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerRespawn rseOnPlayerRespawn;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerAddState _onPlayerAddState;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationBoolValueChange rseOnAnimationBoolValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationFloatValueChange rseOnAnimationFloatValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerGainConviction _onPlayerGainConviction;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerDodgePerfect _rseOnDodgePerfect;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnSendConsoleMessage rseOnSendConsoleMessage;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCameraFOV rseOnCameraFOV;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerIsDodging _playerIsDodging;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerIsTargeting _playerIsTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerCurrentState _playerCurrentState;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_AttackDataInDodgeableArea _attackDataInDodgeableArea;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_AttackCanHitPlayer _attackCanHitPlayer;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerConvictionData _playerConvictionData;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStateTransitions _playerStateTransitions;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_AnimationTransitionDelays _animationTransitionDelays;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStateTransitions _ssoPlayerStateTransitions;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats _playerStats;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_CameraData ssoCameraData;
    #endregion

    private float maxSlopeAngle => _playerStats.Value.maxSlopeAngle;
    private float maxDownStepAngle => _playerStats.Value.maxSlopeAngle;

    private Vector2 _moveInput = Vector2.zero;
    private Vector2 _moveInputWhenPressingDodge = Vector2.zero;

    private Transform _target = null;

    private Coroutine _dodgeCoroutine = null;
    private Coroutine _prepareRunCoroutine = null;

    private float _linearDamping = 0;

    private bool _canRunAfterDodge = false;
    private bool _dodgeUp = true;

    private bool haveDodgePerfect = false;

    private void Awake()
    {
        _linearDamping = _rb.linearDamping;
    }

    private void OnEnable()
    {
        rseOnPlayerDodge.action += TryDodge;
        _rseOnPlayerMove.action += Move;
        _rseOnNewTargeting.action += ChangeNewTarget;
        _rseOnPlayerCancelTargeting.action += CancelTarget;
        _rseOnPlayerGettingHit.action += CancelDodge;
        _onPlayerDodgeInputCancel.action += CancelInputdodge;
        rseOnPlayerRespawn.action += ResetDodgeableArea;

        _canRunAfterDodge = false;
    }

    private void OnDisable()
    {
        rseOnPlayerDodge.action -= TryDodge;
        _rseOnPlayerMove.action -= Move;
        _rseOnNewTargeting.action -= ChangeNewTarget;
        _rseOnPlayerCancelTargeting.action -= CancelTarget;
        _rseOnPlayerGettingHit.action -= CancelDodge;
        _onPlayerDodgeInputCancel.action -= CancelInputdodge;
        rseOnPlayerRespawn.action -= ResetDodgeableArea;
    }

    private void ResetDodgeableArea()
    {
        _attackDataInDodgeableArea.Value.Clear();
    }

    private void ChangeNewTarget(GameObject newTarget)
    {
        _target = newTarget.transform;
    }

    private void CancelTarget(GameObject oldTarget)
    {
        _target = null;
    }

    private void TryDodge()
    {
        if (_playerStateTransitions.Value.CanTransition(_playerCurrentState.Value, S_EnumPlayerState.Dodging) == false || _dodgeUp == false) return;

        _onPlayerAddState.Call(S_EnumPlayerState.Dodging);

        rseOnSendConsoleMessage.Call("Player Dodge!");

        _dodgeUp = false;
        StartCoroutine(S_Utils.Delay(_playerStats.Value.dodgeCooldown, () =>
        {
            _dodgeUp = true;
        }));

        RuntimeManager.PlayOneShot(_dodgeSimpleSound);

        // Test TriggerDodgePerfect
        var isDodgePrefect = _attackDataInDodgeableArea.Value.Count > 0;
        if (isDodgePrefect)
        {
            haveDodgePerfect = true;

            Debug.Log("Dodge perfect");
            RuntimeManager.PlayOneShot(_dodgeSound);

            S_ClassCameraFOV fov = new S_ClassCameraFOV();
            fov.value = ssoCameraData.Value.fovDodge;
            fov.time = ssoCameraData.Value.fovDodgeSwitchTime;

            rseOnCameraFOV.Call(fov);

            //_onPlayerGainConviction.Call(_playerConvictionData.Value.dodgeSuccessGain);
            _rseOnDodgePerfect.Call();

            foreach (var attackData in _attackDataInDodgeableArea.Value)
            {
                if (_attackCanHitPlayer.Value.ContainsKey(attackData.Key) == true)
                {
                    _attackCanHitPlayer.Value.Remove(attackData.Key);
                }
            }
        }

        Vector3 dodgeDirection = Vector3.zero;

        if (_playerIsTargeting.Value == false) dodgeDirection = transform.forward;
        else dodgeDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        if (dodgeDirection == Vector3.zero) dodgeDirection = -transform.forward;

        dodgeDirection.Normalize();

        rseOnAnimationFloatValueChange.Call(_dodgeDirXParam, dodgeDirection.x);
        rseOnAnimationFloatValueChange.Call(_dodgeDirYParam, dodgeDirection.z);

        rseOnAnimationBoolValueChange.Call(_dodgeParam, true);

        _playerIsDodging.Value = true;

        if (_dodgeCoroutine != null) StopCoroutine(_dodgeCoroutine);

        _dodgeCoroutine = StartCoroutine(StartupAndDash(dodgeDirection));
    }

    private IEnumerator StartupAndDash(Vector3 dodgeDir)
    {
        float startup = _animationTransitionDelays.Value.dodgeStartupDelay;
        if (startup > 0f) yield return new WaitForSeconds(startup);

        _rb.linearDamping = 0f;
        _rb.angularVelocity = Vector3.zero;
        _playerIsDodging.Value = true;
        _canRunAfterDodge = true;
        _moveInputWhenPressingDodge = _moveInput;

        float dur = _playerStats.Value.dodgeDuration;
        float wantedDist = _playerStats.Value.dodgeDistance;
        AnimationCurve curve = _playerStats.Value._speedDodgeCurve;

        Vector3 groundNormal;
        bool onGround = CheckGround(out groundNormal);
        if (onGround) dodgeDir = Vector3.ProjectOnPlane(dodgeDir, groundNormal).normalized;
        else dodgeDir = dodgeDir.normalized;

        float elapsed = 0f;
        float travelled = 0f;

        Vector3 startPos = transform.position;

        while (elapsed < dur)
        {
            float t = elapsed / dur;
            float speedMul = curve != null ? curve.Evaluate(t) : 1f;

            float frameDist = (wantedDist / dur) * speedMul * Time.deltaTime;

            float remaining = wantedDist - travelled;
            if (remaining <= 0f) break;
            if (frameDist > remaining) frameDist = remaining;

            Vector3 desiredDir;
            if (_target != null)
            {
                Vector3 toTarget = _target.position - transform.position;
                toTarget.y = 0f;
                Vector3 forward = toTarget.normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

                if (_moveInputWhenPressingDodge == Vector2.zero)
                {
                    desiredDir = -forward;
                }
                else
                {
                    desiredDir = (right * _moveInputWhenPressingDodge.x + forward * _moveInputWhenPressingDodge.y);
                }
            }
            else
            {
                desiredDir = dodgeDir;
            }

            onGround = CheckGround(out groundNormal);
            Vector3 stepDir = dodgeDir;  //put in there desiredDir to make dodge direction depend on the target
            if (onGround) stepDir = Vector3.ProjectOnPlane(stepDir, groundNormal).normalized;

            float allowed = ProbeObstacle(stepDir, frameDist);
            if (allowed <= 0f) break;

            allowed = ProbeGroundAhead(stepDir, allowed, groundNormal);
            if (allowed <= 0f) break;

            Vector3 nextPos = transform.position + stepDir * allowed;
            _rb.MovePosition(nextPos);

            travelled += allowed;
            elapsed += Time.deltaTime;

            yield return null;
        }

        _rb.linearVelocity = Vector3.zero;
        _rb.linearDamping = _linearDamping;
        _playerIsDodging.Value = false;
        rseOnAnimationBoolValueChange.Call(_dodgeParam, false);

        if (haveDodgePerfect)
        {
            haveDodgePerfect = false;

            S_ClassCameraFOV fov = new S_ClassCameraFOV();
            fov.value = -ssoCameraData.Value.fovDodge;
            fov.time = -ssoCameraData.Value.fovDodgeSwitchTime;

            rseOnCameraFOV.Call(fov);
        }

        float rec = _animationTransitionDelays.Value.dodgeRecoveryDelay;
        if (rec > 0f) yield return new WaitForSeconds(rec);

        _onPlayerAddState.Call(S_EnumPlayerState.None);

        yield return new WaitForSeconds(_playerStats.Value.delayBeforeRunningAfterDodge);

        if (_canRunAfterDodge && _playerStateTransitions.Value.CanTransition(_playerCurrentState.Value, S_EnumPlayerState.Running))
        {
            _onPlayerAddState.Call(S_EnumPlayerState.Running);
        }

        _dodgeCoroutine = null;
    }

    private void Move(Vector2 input)
    {
        _moveInput = input;
    }

    private void CancelDodge()
    {
        _canRunAfterDodge = false;
        _playerIsDodging.Value = false;

        if (_dodgeCoroutine != null) StopCoroutine(_dodgeCoroutine);

        _rb.linearDamping = _linearDamping;
        rseOnAnimationBoolValueChange.Call(_dodgeParam, false);

        if (_prepareRunCoroutine != null) StopCoroutine(_prepareRunCoroutine);

        ResetValue();
    }

    private void CancelInputdodge()
    {
        _canRunAfterDodge = false;

        /*
        if (_playerCurrentState.Value != PlayerState.Running && _prepareRunCoroutine != null && _dodgeCoroutine == null)
        {
            StopCoroutine(_prepareRunCoroutine);
            _onPlayerAddState.Call(PlayerState.None);
        }
        */
        
        if(_playerCurrentState.Value == S_EnumPlayerState.Running /*&& _dodgeCoroutine != null*/) _onPlayerAddState.Call(S_EnumPlayerState.None);
    }

    private void ResetValue()
    {
        _playerIsDodging.Value = false;
        _canRunAfterDodge = false;
        rseOnAnimationBoolValueChange.Call(_dodgeParam, false);
        _attackDataInDodgeableArea.Value.Clear();
    }

    private void GetCapsuleWorldEnds(out Vector3 top, out Vector3 bottom, out float radius)
    {
        float height = Mathf.Max(_capsule.height * Mathf.Abs(transform.lossyScale.y), _capsule.radius * 2f);
        radius = _capsule.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
        Vector3 center = transform.TransformPoint(_capsule.center);
        Vector3 up = transform.up;
        top = center + up * (height * 0.5f - radius);
        bottom = center - up * (height * 0.5f - radius);
    }

    private bool CheckGround(out Vector3 normal)
    {
        GetCapsuleWorldEnds(out var top, out var bottom, out var radius);
        Vector3 start = bottom + Vector3.up * 0.02f;
        float dist = 0.6f;

        if (Physics.Raycast(start, Vector3.down, out var hit, dist, groundMask, QueryTriggerInteraction.Ignore))
        {
            float ang = Vector3.Angle(hit.normal, Vector3.up);
            if (ang <= maxSlopeAngle)
            {
                normal = hit.normal;
                return true;
            }
        }
        normal = Vector3.up;

        return false;
    }

    private float ProbeObstacle(Vector3 dir, float maxDist)
    {
        GetCapsuleWorldEnds(out var top, out var bottom, out var radius);

        bool overlapping = Physics.CheckCapsule(
        bottom, top, radius,
        obstacleMask, QueryTriggerInteraction.Ignore);

        if (overlapping) return 0f;

        bool overlapping2 = Physics.CheckCapsule(
        bottom, top, radius,
        obstacle2Mask, QueryTriggerInteraction.Ignore);

        if (overlapping2) return 0f;

        float castDist = maxDist + stopFromWall;

        if (Physics.CapsuleCast(bottom, top, radius, dir, out var hit, castDist, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            float allowed = hit.distance - stopFromWall;
            if (allowed < 0f) allowed = 0f;

            return Mathf.Min(maxDist, allowed);
        }

        return maxDist;
    }

    private float ProbeGroundAhead(Vector3 dir, float maxDist, Vector3 currentGroundNormal)
    {
        Vector3 probePos = transform.position + dir * maxDist + Vector3.up * edgeProbeHeight;
        if (Physics.Raycast(probePos, Vector3.down, out var hit, edgeProbeHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            float ang = Vector3.Angle(hit.normal, Vector3.up);
            if (ang > maxDownStepAngle) return 0f;

            return maxDist;
        }

        return 0f;
    }
}