using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class S_TargetingManager : MonoBehaviour
{
    [TabGroup("References")]
    [Title("General")]
    [SerializeField] private LayerMask obstacleMask;

    [TabGroup("References")]
    [Title("Indicators")]
    [SerializeField] private GameObject _previewIndicatorPrefab;

    [TabGroup("References")]
    [SerializeField] private GameObject _previewSwapIndicatorPrefab;

    [TabGroup("References")]
    [SerializeField] private GameObject _lockedIndicatorPrefab;

    [TabGroup("References")]
    [Title("Sounds")]
    [SerializeField] private EventReference _targetLockOnSound;

    [TabGroup("References")]
    [SerializeField] private EventReference _targetLockOffSound;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnTargetsInRangeChange rseOnTargetsInRangeChange;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerTargeting rseOnPlayerTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerTargetingCancel rseOnPlayerTargetingCancel;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnCancelTargeting rseOnCancelTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerSwapTarget rseOnPlayerSwapTarget;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnEnemyTargetDied rseOnEnemyTargetDied;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerCenter _rseOnPlayerCenter;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerDeath _onPlayerDeathRse;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnStartTargeting rseOnStartTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnNewTargeting rseOnNewTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerCancelTargeting rseOnPlayerCancelTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnStopTargeting rseOnStopTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnAnimationBoolValueChange rseOnAnimationBoolValueChange;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnResetCam rseOnResetCam;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCameraFOV rseOnCameraFOV;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerIsTargeting rsoPlayerIsTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerPosition rsoPlayerPosition;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_SettingsSaved rsoSettingsSaved;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_TargetPosition rsoTargetPosition;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerMaxDistanceTargeting ssoPlayerMaxDistanceTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerTargetRangeRadius ssoPlayerTargetRangeRadius;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_TargetObstacleBreakDelay ssoPargetObstacleBreakDelay;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_FrontConeAngle ssoFrontConeAngle;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_Display ssoDisplay;

    private GameObject currentTarget = null;
    private HashSet<GameObject> targetsPossible = new();
    private float obstacleTimer = 0f;
    private Transform _playerCenterTransform = null;

    private GameObject _previewGO = null;
    private GameObject _lockedGO = null;
    private GameObject _swapLeftGO = null;
    private GameObject _swapRightGO = null;

    private Transform _previewTargetTransform = null;
    private Transform _lockedTargetTransfrom = null;
    private Transform _swapLeftTargetTransform = null;
    private Transform _swapRightTargetTransform = null;

    private bool _previewGOActive = false;
    private bool _lockedGOActive = false;
    private bool _swapLeftGOActive = false;
    private bool _swapRightGOActive = false;

    private void Awake()
    {
        rsoPlayerIsTargeting.Value = false;

        _previewGO = Instantiate(_previewIndicatorPrefab, gameObject.transform);
        _lockedGO = Instantiate(_lockedIndicatorPrefab, gameObject.transform);

        _swapLeftGO = Instantiate(_previewSwapIndicatorPrefab, transform);
        _swapRightGO = Instantiate(_previewSwapIndicatorPrefab, transform);

        _previewGO.SetActive(false);
        _lockedGO.SetActive(false);
        _swapLeftGO.SetActive(false);
        _swapRightGO.SetActive(false);
    }

    private void OnEnable()
    {
        rsoPlayerIsTargeting.Value = false;

        rseOnTargetsInRangeChange.action += OnChangeTargetsPosible;
        rseOnPlayerTargeting.action += OnPlayerTargetingInput;
        rseOnPlayerTargetingCancel.action += OnPlayerCancelTargetingInput;
        rseOnCancelTargeting.action += CancelTargeting;
        rseOnPlayerSwapTarget.action += OnSwapTargetInput;

        rseOnEnemyTargetDied.action += OnEnemyTargetDied;

        _rseOnPlayerCenter.action += GetPlayerCenterTransform;

        _onPlayerDeathRse.action += CancelTargeting;
    }

    private void OnDisable()
    {
        rseOnTargetsInRangeChange.action -= OnChangeTargetsPosible;
        rseOnPlayerTargeting.action -= OnPlayerTargetingInput;
        rseOnPlayerTargetingCancel.action -= OnPlayerCancelTargetingInput;
        rseOnCancelTargeting.action -= CancelTargeting;
        rseOnPlayerSwapTarget.action -= OnSwapTargetInput;

        rseOnEnemyTargetDied.action -= OnEnemyTargetDied;
        _rseOnPlayerCenter.action -= GetPlayerCenterTransform;

        _onPlayerDeathRse.action -= CancelTargeting;
    }

    private void Update()
    {
        var selection = TargetSelectionExist();

        //Debug.Log("Targets: " + (targetsPossible.Count));

        if (targetsPossible.Count > 0 && selection != null || currentTarget != null)
        {
            if (currentTarget == null)
            {
                if (selection.TryGetComponent(out I_Targetable targetable)) _previewTargetTransform = targetable.GetTargetLockOnAnchorTransform();
                else _previewTargetTransform = selection.transform;

                DisplayPreviewArrow(_previewGO);
                UnDisplayLockedArrow(_lockedGO);

                _previewGO.transform.position = _previewTargetTransform.position;

                UnDisplayLeftArrow(_swapLeftGO);
                UnDisplayRightArrow(_swapRightGO);
            }
            else if (currentTarget != null)
            {
                if (currentTarget.TryGetComponent(out I_Targetable targetable)) _lockedTargetTransfrom = targetable.GetTargetLockOnAnchorTransform();
                else _lockedTargetTransfrom = currentTarget.transform;

                UnDisplayPreviewArrow(_previewGO);
                DisplayLockedArrow(_lockedGO);

                _lockedGO.transform.position = _lockedTargetTransfrom.position;

                // Swap indicators  
                var candidates = BuildSwapCandidates();

                GameObject leftTarget = GetSwapTarget(-1f, candidates);
                GameObject rightTarget = GetSwapTarget(+1f, candidates);

                if (leftTarget != null && rightTarget != null && leftTarget == rightTarget)
                {
                    UnDisplayLeftArrow(_swapLeftGO);

                    if (rightTarget.TryGetComponent(out I_Targetable t)) _swapRightTargetTransform = t.GetTargetLockOnAnchorTransform();
                    else _swapRightTargetTransform = rightTarget.transform;

                    DisplayRightArrow(_swapRightGO);
                    _swapRightGO.transform.position = _swapRightTargetTransform.position;

                    return;
                }
                // Left
                if (leftTarget != null && leftTarget != currentTarget)
                {
                    if (leftTarget.TryGetComponent(out I_Targetable leftTargetable)) _swapLeftTargetTransform = leftTargetable.GetTargetLockOnAnchorTransform();
                    else _swapLeftTargetTransform = leftTarget.transform;

                    DisplayLeftArrow(_swapLeftGO);
                    _swapLeftGO.transform.position = _swapLeftTargetTransform.position;
                }
                else
                {
                    UnDisplayLeftArrow(_swapLeftGO);
                }

                // Right
                if (rightTarget != null && rightTarget != currentTarget)
                {
                    if (rightTarget.TryGetComponent(out I_Targetable rightTargetable)) _swapRightTargetTransform = rightTargetable.GetTargetLockOnAnchorTransform();
                    else _swapRightTargetTransform = rightTarget.transform;

                    DisplayRightArrow(_swapRightGO);
                    _swapRightGO.transform.position = _swapRightTargetTransform.position;
                }
                else UnDisplayRightArrow(_swapRightGO);
            }
        }
        else
        {
            UnDisplayPreviewArrow(_previewGO);
            UnDisplayLockedArrow(_lockedGO);
            UnDisplayLeftArrow(_swapLeftGO);
            UnDisplayRightArrow(_swapRightGO);
        }
    }

    private void FixedUpdate()
    {
        if (currentTarget != null && rsoPlayerIsTargeting.Value == true)
        {
            S_LookAt lookAt = currentTarget.GetComponent<S_LookAt>();

            if (lookAt != null && lookAt.GetAimPoint() != null) rsoTargetPosition.Value = lookAt.GetAimPoint();
            else rsoTargetPosition.Value = currentTarget.transform.position;

            float distance = Vector3.Distance(rsoPlayerPosition.Value, currentTarget.transform.position);

            if (distance > ssoPlayerMaxDistanceTargeting.Value && currentTarget != null) CancelTargeting();

            var playerPos = new Vector3(rsoPlayerPosition.Value.x, rsoPlayerPosition.Value.y + 1.0f, rsoPlayerPosition.Value.z);

            if (currentTarget == null) return;

            Vector3 dir = (currentTarget.transform.position - playerPos).normalized;
            if (Physics.Raycast(playerPos, dir, out RaycastHit hit, distance, obstacleMask))
            {

                if (hit.collider.gameObject != currentTarget)
                {
                    obstacleTimer += Time.fixedDeltaTime;
                    if (obstacleTimer >= ssoPargetObstacleBreakDelay.Value) CancelTargeting();
                }
                else obstacleTimer = 0f;
            }
            else obstacleTimer = 0f;
        }
    }

    private void DisplayPreviewArrow(GameObject arrow)
    {
        if (!_previewGOActive)
        {
            _previewGOActive = true;
            DisplayTargeting(arrow);
        }
    }

    private void UnDisplayPreviewArrow(GameObject arrow)
    {
        if (_previewGOActive)
        {
            _previewGOActive = false;
            UnDisplayTargeting(arrow);
        }
    }

    private void DisplayLockedArrow(GameObject arrow)
    {
        if (!_lockedGOActive)
        {
            _lockedGOActive = true;
            DisplayTargeting(arrow);
        }
    }

    private void UnDisplayLockedArrow(GameObject arrow)
    {
        if (_lockedGOActive)
        {
            _lockedGOActive = false;
            UnDisplayTargeting(arrow);
        }
    }

    private void DisplayLeftArrow(GameObject arrow)
    {
        if (!_swapLeftGOActive)
        {
            _swapLeftGOActive = true;
            DisplayTargeting(arrow);
        }
    }

    private void UnDisplayLeftArrow(GameObject arrow)
    {
        if (_swapLeftGOActive)
        {
            _swapLeftGOActive = false;
            UnDisplayTargeting(arrow);
        }
    }

    private void DisplayRightArrow(GameObject arrow)
    {
        if (!_swapRightGOActive)
        {
            _swapRightGOActive = true;
            DisplayTargeting(arrow);
        }
    }

    private void UnDisplayRightArrow(GameObject arrow)
    {
        if (_swapRightGOActive)
        {
            _swapRightGOActive = false;
            UnDisplayTargeting(arrow);
        }
    }

    private void DisplayTargeting(GameObject arrow)
    {
        CanvasGroup cg = arrow.GetComponent<CanvasGroup>();
        cg.DOKill();

        arrow.SetActive(true);

        cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear);
    }

    private void UnDisplayTargeting(GameObject arrow)
    {
        CanvasGroup cg = arrow.GetComponent<CanvasGroup>();
        cg.DOKill();

        cg.DOFade(0f, 0).SetEase(Ease.Linear).OnComplete(() =>
        {
            arrow.SetActive(false);
        });
    }

    private void GetPlayerCenterTransform(Transform playerCenter)
    {
        _playerCenterTransform = playerCenter;
    }

    private void OnChangeTargetsPosible(HashSet<GameObject> targetsList)
    {
        targetsPossible = targetsList;
    }

    private void OnPlayerTargetingInput()
    {
        if (rsoSettingsSaved.Value.holdLockTarget == false && rsoPlayerIsTargeting.Value == true)
        {
            CancelTargeting();
            return;
        }

        if (targetsPossible.Count == 0 || rsoPlayerIsTargeting.Value == true)
        {
            rseOnResetCam.Call();
            return;
        }

        currentTarget = TargetSelection();

        if (currentTarget != null)
        {
            rseOnNewTargeting.Call(currentTarget);
            rsoPlayerIsTargeting.Value = true;

            S_LookAt lookAt = currentTarget.GetComponent<S_LookAt>();

            if (lookAt != null && lookAt.GetAimPoint() != null) rsoTargetPosition.Value = lookAt.GetAimPoint();
            else rsoTargetPosition.Value = currentTarget.transform.position;

            rseOnStartTargeting.Call();
            RuntimeManager.PlayOneShot(_targetLockOnSound);
        }
    }

    private void OnPlayerCancelTargetingInput()
    {
        if (rsoSettingsSaved.Value.holdLockTarget == false) return;
        
        CancelTargeting();
    }

    private void CancelTargeting()
    {
        rsoPlayerIsTargeting.Value = false;
        rsoTargetPosition.Value = Vector3.zero;

        if (currentTarget != null)
        {
            rseOnPlayerCancelTargeting.Call(currentTarget);
            rseOnAnimationBoolValueChange.Call("TargetLock", false);
            RuntimeManager.PlayOneShot(_targetLockOffSound);

            rseOnStopTargeting.Call();
        }

        RuntimeManager.PlayOneShot(_targetLockOffSound);

        currentTarget = null;
        obstacleTimer = 0f;
    }

    private void OnEnemyTargetDied(GameObject enemy)
    {
        if (currentTarget == enemy)
        {
            rsoPlayerIsTargeting.Value = false;
            rsoTargetPosition.Value = Vector3.zero;

            if (currentTarget != null)
            {
                rseOnPlayerCancelTargeting.Call(currentTarget);

                currentTarget = TargetSelection();

                if (currentTarget != null)
                {
                    rseOnNewTargeting.Call(currentTarget);
                    rsoPlayerIsTargeting.Value = true;

                    S_LookAt lookAt = currentTarget.GetComponent<S_LookAt>();

                    if (lookAt != null && lookAt.GetAimPoint() != null) rsoTargetPosition.Value = lookAt.GetAimPoint();
                    else rsoTargetPosition.Value = currentTarget.transform.position;

                    RuntimeManager.PlayOneShot(_targetLockOnSound);
                }
                else
                {
                    rseOnAnimationBoolValueChange.Call("TargetLock", false);

                    rseOnStopTargeting.Call();

                    RuntimeManager.PlayOneShot(_targetLockOffSound);
                }
            }
        }
    }

    private void OnSwapTargetInput(float axis) // axis = -1 gauche, +1 droite
    {
        if (targetsPossible.Count == 0 || rsoPlayerIsTargeting.Value == false) return;

        List<(GameObject go, float angle)> candidates = new List<(GameObject, float)>();

        foreach (var target in targetsPossible)
        {
            if (target == null) continue;

            Vector3 toTarget = (target.transform.position - rsoPlayerPosition.Value).normalized;

            if (_playerCenterTransform == null) return;

            float signedAngle = Vector3.SignedAngle(_playerCenterTransform.forward, toTarget, Vector3.up);

            float distanceMax = Vector3.Distance(_playerCenterTransform.position, target.transform.position);

            Vector3 dir = (target.transform.position - _playerCenterTransform.position).normalized;
            if (Physics.Raycast(_playerCenterTransform.position, dir, out RaycastHit hit, distanceMax, obstacleMask)) continue;

            candidates.Add((target, signedAngle));
        }

        if (candidates.Count == 0) return;

        float currentAngle = candidates.Find(c => c.go == currentTarget).angle; // Angle from the current target

        GameObject bestTarget = null;
        float bestDelta = float.MaxValue;

        foreach (var (go, angle) in candidates)
        {
            if (go == currentTarget) continue;

            float delta = angle - currentAngle;

            // Normalise in [-180, 180]
            if (delta > 180) delta -= 360;
            if (delta < -180) delta += 360;

            // If axis > 0 ? right ? look for the smallest positive delta
            // If axis < 0 ? left ? look for the largest negative delta (the closest to 0)
            if (axis > 0 && delta > 0 && delta < bestDelta)
            {
                bestDelta = delta;
                bestTarget = go;
            }
            else if (axis < 0 && delta < 0 && Mathf.Abs(delta) < bestDelta)
            {
                bestDelta = Mathf.Abs(delta);
                bestTarget = go;
            }
        }

        // If nothing found
        if (bestTarget == null)
        {
            if (axis > 0)  bestTarget = candidates.OrderBy(c => c.angle).First().go; // further left
            else bestTarget = candidates.OrderByDescending(c => c.angle).First().go; // further right
        }

        if (bestTarget != null && bestTarget != currentTarget)
        {
            rseOnPlayerCancelTargeting.Call(currentTarget);
            currentTarget = bestTarget;
            rseOnNewTargeting.Call(bestTarget);

            S_ClassCameraFOV fov = new S_ClassCameraFOV();
            fov.value = 60;
            fov.time = 0.5f;
            fov.reset = true;

            rseOnCameraFOV.Call(fov);

            RuntimeManager.PlayOneShot(_targetLockOnSound);
        }
    }

    private GameObject TargetSelection()
    {
        GameObject selectedTarget = null;

        float bestScore = float.MaxValue;

        foreach (var target in targetsPossible)
        {
            if (target == null) continue;

            Vector3 toTarget = (target.transform.position - rsoPlayerPosition.Value);
            float distance = toTarget.magnitude;

            if (_playerCenterTransform == null) return null;

            float angle = Vector3.Angle(_playerCenterTransform.forward, toTarget);

            bool inFrontCone = angle <= ssoFrontConeAngle.Value * 0.5f;

            // Priority for the taget in the front cone
            float score = inFrontCone ? distance : distance + 1000f;

            if (score < bestScore && target != currentTarget)
            {
                float distanceMax = Vector3.Distance(_playerCenterTransform.position, target.transform.position);

                Vector3 dir = (target.transform.position - _playerCenterTransform.position).normalized;
                if (Physics.Raycast(_playerCenterTransform.position, dir, out RaycastHit hit, distanceMax, obstacleMask)) continue;
                else
                {
                    bestScore = score;
                    selectedTarget = target;
                }
            }
        }

        if (selectedTarget != null)  rseOnAnimationBoolValueChange.Call("TargetLock", true);

        return selectedTarget;
    }

    private GameObject TargetSelectionExist()
    {
        GameObject selectedTarget = null;

        float bestScore = float.MaxValue;

        foreach (var target in targetsPossible)
        {
            if (target == null) continue;

            Vector3 toTarget = (target.transform.position - rsoPlayerPosition.Value);
            float distance = toTarget.magnitude;

            if (_playerCenterTransform == null) return null;

            float angle = Vector3.Angle(_playerCenterTransform.forward, toTarget);

            bool inFrontCone = angle <= ssoFrontConeAngle.Value * 0.5f;

            // Priority for the taget in the front cone
            float score = inFrontCone ? distance : distance + 1000f;

            if (score < bestScore && target != currentTarget)
            {
                float distanceMax = Vector3.Distance(_playerCenterTransform.position, target.transform.position);

                Vector3 dir = (target.transform.position - _playerCenterTransform.position).normalized;
                if (Physics.Raycast(_playerCenterTransform.position, dir, out RaycastHit hit, distanceMax, obstacleMask)) continue;
                else
                {
                    bestScore = score;
                    selectedTarget = target;
                }
            }
        }

        return selectedTarget;
    }

    private List<(GameObject go, float angle)> BuildSwapCandidates()
    {
        List<(GameObject go, float angle)> candidates = new();

        foreach (var target in targetsPossible)
        {
            if (target == null) continue;

            if (_playerCenterTransform == null) return candidates;

            Vector3 toTarget = (target.transform.position - rsoPlayerPosition.Value).normalized;
            float signedAngle = Vector3.SignedAngle(_playerCenterTransform.forward, toTarget, Vector3.up);

            float distanceMax = Vector3.Distance(_playerCenterTransform.position, target.transform.position);
            Vector3 dir = (target.transform.position - _playerCenterTransform.position).normalized;

            if (Physics.Raycast(_playerCenterTransform.position, dir, out RaycastHit hit, distanceMax, obstacleMask)) continue;

            candidates.Add((target, signedAngle));
        }

        return candidates;
    }

    private GameObject GetSwapTarget(float axis, List<(GameObject go, float angle)> candidates)
    {
        if (candidates == null || candidates.Count == 0 || currentTarget == null) return null;

        int index = candidates.FindIndex(c => c.go == currentTarget);

        float currentAngle;

        if (index >= 0) currentAngle = candidates[index].angle;
        else currentAngle = 0f;

        GameObject bestTarget = null;
        float bestDelta = float.MaxValue;

        foreach (var (go, angle) in candidates)
        {
            if (go == currentTarget) continue;

            float delta = angle - currentAngle;

            if (delta > 180f) delta -= 360f;
            if (delta < -180f) delta += 360f;

            if (axis > 0f)
            {
                if (delta > 0f && delta < bestDelta)
                {
                    bestDelta = delta;
                    bestTarget = go;
                }
            }
            else if (axis < 0f)
            {
                if (delta < 0f && Mathf.Abs(delta) < bestDelta)
                {
                    bestDelta = Mathf.Abs(delta);
                    bestTarget = go;
                }
            }
        }

        if (bestTarget == null && candidates.Count > 1)
        {
            if (axis > 0f) bestTarget = candidates.OrderBy(c => c.angle).First().go;
            else if (axis < 0f) bestTarget = candidates.OrderByDescending(c => c.angle).First().go;
        }

        return bestTarget;
    }

    private void OnDrawGizmos()
    {
        var playerPos = Vector3.zero;

        if (currentTarget != null && rsoPlayerIsTargeting != null && rsoPlayerIsTargeting.Value)
        {
            playerPos = new Vector3(rsoPlayerPosition.Value.x, rsoPlayerPosition.Value.y + 1.0f, rsoPlayerPosition.Value.z);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(playerPos, currentTarget.transform.position);

            Vector3 dir = (currentTarget.transform.position - playerPos).normalized;
            float distance = Vector3.Distance(playerPos, currentTarget.transform.position);
            if (Physics.Raycast(playerPos, dir, out RaycastHit hit, distance, obstacleMask))
            {
                if (hit.collider.gameObject != currentTarget)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(playerPos, hit.point);
                }
            }
        }

        playerPos = new Vector3(rsoPlayerPosition.Value.x, rsoPlayerPosition.Value.y + 1.0f, rsoPlayerPosition.Value.z);

        if (rsoPlayerPosition == null || ssoPlayerTargetRangeRadius == null) return;

        Vector3 origin = playerPos;
        float radius = ssoPlayerTargetRangeRadius.Value;

        float halfAngle = ssoFrontConeAngle.Value * 0.5f;

        Quaternion leftRot = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(halfAngle, Vector3.up);

        if (_playerCenterTransform == null) return;

        Vector3 leftDir = leftRot * _playerCenterTransform.forward;
        Vector3 rightDir = rightRot * _playerCenterTransform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + leftDir * radius);
        Gizmos.DrawLine(origin, origin + rightDir * radius);
    }
}