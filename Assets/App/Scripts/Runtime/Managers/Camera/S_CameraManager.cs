using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class S_CameraManager : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference uiSound;

    [TabGroup("References")]
    [Title("Camera Main")]
    [SerializeField] private Camera cameraMain;

    [TabGroup("References")]
    [Title("Cinemachine")]
    [SerializeField] private CinemachineCamera cinemachineCameraIntro;

    [TabGroup("References")]
    [SerializeField] private CinemachineCamera cinemachineCameraRail;

    [TabGroup("References")]
    [SerializeField] private CinemachineCamera cinemachineCameraBridge;

    [TabGroup("References")]
    [SerializeField] private CinemachineCamera cinemachineCameraPlayer;

    [TabGroup("References")]
    [SerializeField] private CinemachineThirdPersonFollow cinemachineThirdPersonFollow;

    [TabGroup("References")]
    [SerializeField] private List<CinemachineCamera> cinemachineCameraCinematic;

    [TabGroup("References")]
    [Title("Target")]
    [SerializeField] private Transform playerPoint;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerMove rseOnPlayerMove;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnStartTargeting rseOnStartTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnNewTargeting rseOnNewTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerCancelTargeting rseOnPlayerCancelTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnStopTargeting rseOnStopTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnCameraIntro rseOnCameraIntro;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnCameraCinematic rseOnCameraCinematic;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnCinematicFinish rseOnCinematicFinish;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnCameraShake rseOnCameraShake;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerCenter rseOnPlayerCenter;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnSkipInput rseOnSkipInput;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnSkipCancelInput rseOnSkipCancelInput;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnSkipIntro rseOnSkipIntro;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCinematicInputEnabled rseOnCinematicInputEnabled;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGameInputEnabled rseOnGameInputEnabled;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnDisplayUIGame rseOnDisplayUIGame;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnDisplaySkip rseOnDisplaySkip;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnSkipHold rseOnSkipHold;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCancelTargeting rseOnCancelTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnSendConsoleMessage rseOnSendConsoleMessage;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnUpdateVisibility rseUpdateVisibility;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerIsDodging rsoPlayerIsDodging;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_CameraData ssoCameraData;

    private ModeCamera currentMode = ModeCamera.None;
    private ModeCamera oldMode = ModeCamera.None;
    private ModeCamera newMode = ModeCamera.None;
    private CinemachineCamera currentCam = null;
    private CinemachineCamera oldCam = null;
    private CinemachineCamera newCam = null;
    private CinemachineCamera targetCam = null;
    private Transform playerPos = null;
    private Transform currentTarget = null;

    private Sequence transitionSequence = null;
    Tweener moveTween = null;
    Tweener rotateTween = null;
    private Tween shoulderTween = null;
    private Tween rotationTween = null;
    private float currentAlpha = 1f;
    private float lastDirection = 0f;

    private Coroutine shakeRoutine = null;
    private Coroutine skipRoutine = null;

    private bool isSkipping = false;
    private float skipHold = 0;

    private const int Focus = 2;
    private const int FocusCinematic = 100;
    private const int Unfocus = 1;

    private void Awake()
    {
        currentCam = cinemachineCameraRail;
        currentMode = ModeCamera.Rail;
        cinemachineCameraBridge.Target.TrackingTarget = playerPoint;

        if (cinemachineThirdPersonFollow != null) cinemachineThirdPersonFollow.ShoulderOffset = ssoCameraData.Value.targetShoulderOffsetPositive;
    }

    private void OnEnable()
    {
        rseOnPlayerCenter.action += PlayerPos;
        rseOnStartTargeting.action += SwitchCameraMode;
        rseOnNewTargeting.action += SetTarget;
        rseOnPlayerCancelTargeting.action += SetTarget;
        rseOnStopTargeting.action += SwitchCameraMode;
        rseOnCameraIntro.action += CameraIntro;
        rseOnCameraCinematic.action += SwitchCinematicCamera;
        rseOnCameraShake.action += CameraShake;
        rseOnPlayerMove.action += InputsMove;
        rseOnCinematicFinish.action += FinishCinematic;
        rseOnSkipInput.action += StartSkip;
        rseOnSkipCancelInput.action += StopSkip;
        rseOnSkipIntro.action += SkipIntro;
    }

    private void OnDisable()
    {
        rseOnPlayerCenter.action -= PlayerPos;
        rseOnStartTargeting.action -= SwitchCameraMode;
        rseOnNewTargeting.action -= SetTarget;
        rseOnPlayerCancelTargeting.action -= SetTarget;
        rseOnStopTargeting.action -= SwitchCameraMode;
        rseOnCameraIntro.action -= CameraIntro;
        rseOnCameraCinematic.action -= SwitchCinematicCamera;
        rseOnCameraShake.action -= CameraShake;
        rseOnPlayerMove.action -= InputsMove;
        rseOnCinematicFinish.action -= FinishCinematic;
        rseOnSkipInput.action -= StartSkip;
        rseOnSkipCancelInput.action -= StopSkip;
        rseOnSkipIntro.action -= SkipIntro;
    }

    private void Update()
    {
        if (playerPos == null) return;

        playerPoint.position = playerPos.position;
        HandleCameraRotation();
        HandlePlayerFade();
        HandleSkipHold();
    }

    private void SetTarget(GameObject target)
    {
        if (target != null)
        {
            if (currentTarget == target.transform) currentTarget = null;
            else currentTarget = target.transform;
        }
    }

    private void PlayerPos(Transform player)
    {
        playerPos = player;
        cinemachineCameraRail.Target.TrackingTarget = player;
    }

    private void InputsMove(Vector2 move)
    {
        if (currentMode == ModeCamera.Player)
        {
            if (move.x > 0 && lastDirection <= 0)
            {
                ChangeShoulderOffset(ssoCameraData.Value.targetShoulderOffsetNegative);
                lastDirection = move.x;
            }
            else if (move.x < 0 && lastDirection >= 0)
            {
                ChangeShoulderOffset(ssoCameraData.Value.targetShoulderOffsetPositive);
                lastDirection = move.x;
            }
        }
        else shoulderTween?.Kill();
    }

    #region Camera System
    private void SwitchCameraMode()
    {
        shoulderTween?.Kill();
        transitionSequence?.Kill();
        moveTween?.Kill();
        rotateTween?.Kill();

        switch (currentMode)
        {
            case ModeCamera.Player:
                rseOnSendConsoleMessage.Call("Player Stop Targeting!");
                cinemachineCameraBridge.Target.TrackingTarget = currentTarget;
                cinemachineThirdPersonFollow.ShoulderOffset = ssoCameraData.Value.targetShoulderOffsetPositive;
                Transition(cinemachineCameraPlayer, cinemachineCameraRail, ModeCamera.Rail, playerPoint);
                break;

            case ModeCamera.Rail:
                rseOnSendConsoleMessage.Call("Player is Targeting!");
                cinemachineCameraBridge.Target.TrackingTarget = currentTarget;
                cinemachineThirdPersonFollow.ShoulderOffset = ssoCameraData.Value.targetShoulderOffsetPositive;
                lastDirection = 0f;

                Vector3 dir = (currentTarget.position - playerPos.position).normalized;
                dir.y = 0f;
                playerPoint.rotation = Quaternion.LookRotation(dir, Vector3.up);

                Transition(cinemachineCameraRail, cinemachineCameraPlayer, ModeCamera.Player, currentTarget);
                break;

            case ModeCamera.Bridge:
                if (oldMode == ModeCamera.Player)
                {
                    rseOnSendConsoleMessage.Call("Player is Targeting!");
                    oldMode = ModeCamera.Rail;
                    newMode = ModeCamera.Player;
                }
                else
                {
                    rseOnSendConsoleMessage.Call("Player Stop Targeting!");
                    oldMode = ModeCamera.Player;
                    newMode = ModeCamera.Rail;
                }

                if (targetCam == oldCam) targetCam = newCam;
                else targetCam = oldCam;

                if (targetCam == null) return;

                moveTween?.Kill();
                rotateTween?.Kill();
                rotationTween?.Kill();
                transitionSequence?.Kill();

                moveTween = cinemachineCameraBridge.transform.DOMove(targetCam.transform.position, 0.4f).SetEase(Ease.Linear);
                rotateTween = cinemachineCameraBridge.transform.DORotateQuaternion(targetCam.transform.rotation, 0.4f).SetEase(Ease.Linear);

                transitionSequence = DOTween.Sequence().Join(moveTween).Join(rotateTween).OnComplete(() =>
                {
                    cinemachineCameraBridge.Priority = Unfocus;
                    targetCam.Priority = Focus;

                    currentCam = targetCam;
                    currentMode = newMode;
                    targetCam = null;
                });
                break;
        }
    }

    private void Transition(CinemachineCamera from, CinemachineCamera to, ModeCamera nextMode, Transform newTarget)
    {
        transitionSequence?.Kill();
        moveTween?.Kill();
        rotateTween?.Kill();

        cinemachineCameraBridge.transform.SetPositionAndRotation(from.transform.position, from.transform.rotation);

        from.Priority = Unfocus;
        cinemachineCameraBridge.Priority = Focus;

        oldCam = from;
        newCam = to;
        currentCam = cinemachineCameraBridge;
        oldMode = currentMode;
        newMode = nextMode;
        currentMode = ModeCamera.Bridge;

        bool moveDone = false;
        bool rotateDone = false;

        moveTween = DOVirtual.Float(0f, 1f, 0.4f, t =>
        {
            Vector3 start = from.transform.position;
            Vector3 current = to.transform.position;
            cinemachineCameraBridge.transform.position = Vector3.Lerp(start, current, t);

            float distance = Vector3.Distance(cinemachineCameraBridge.transform.position, current);
            if (!moveDone && distance <= 0.1f) 
            { 
                moveDone = true;
                cinemachineCameraBridge.transform.position = current;

                if (rotateDone) OnTransitionComplete(to); 
            }
        }).SetEase(Ease.Linear);

        rotateTween = DOVirtual.Float(0f, 1f, 0.4f, t =>
        {
            Quaternion start = from.transform.rotation;
            Quaternion current = to.transform.rotation;
            cinemachineCameraBridge.transform.rotation = Quaternion.Slerp(start, current, t);

            float angle = Quaternion.Angle(cinemachineCameraBridge.transform.rotation, current);
            if (!rotateDone && angle <= 0.1f)
            {
                rotateDone = true;
                cinemachineCameraBridge.transform.rotation = current;

                if (moveDone) OnTransitionComplete(to);
            }
        }).SetEase(Ease.Linear);
    }

    private void OnTransitionComplete(CinemachineCamera to)
    {
        cinemachineCameraBridge.Priority = Unfocus;
        to.Priority = Focus;

        currentCam = to;
        currentMode = newMode;
    }
    #endregion

    #region Cinematic System
    private void CameraIntro()
    {
        if (cinemachineCameraIntro.transform.parent.gameObject.activeInHierarchy)
        {
            StartSkipTimer();

            rseOnDisplayUIGame.Call(false);
            rseOnCinematicInputEnabled.Call();

            currentCam = cinemachineCameraIntro;
            currentMode = ModeCamera.Cinematic;

            var anim = currentCam.GetComponent<Animator>();
            if (anim)
            {
                anim.Rebind();
                anim.Update(0f);
                anim.enabled = true;
                anim.SetTrigger("Play");
            }
        }
        else
        {
            rseOnDisplayUIGame.Call(true);
            rseOnGameInputEnabled.Call();
        }
    }

    private void SkipIntro()
    {
        var anim = cinemachineCameraIntro.GetComponent<Animator>();
        if (anim) anim.enabled = false;

        cinemachineCameraIntro.Priority = Unfocus;
        cinemachineCameraRail.Priority = Focus;
        currentCam = cinemachineCameraRail;
        currentMode = ModeCamera.Rail;
    }

    private void SwitchCinematicCamera(int index)
    {
        if (index < 0 || index >= cinemachineCameraCinematic.Count) return;

        shoulderTween?.Kill();
        transitionSequence?.Kill();
        moveTween?.Kill();
        rotateTween?.Kill();

        StartSkipTimer();

        rseOnDisplayUIGame.Call(false);
        rseOnCinematicInputEnabled.Call();

        currentTarget = null;

        cinemachineCameraRail.Priority = Unfocus;
        cinemachineCameraPlayer.Priority = Unfocus;
        cinemachineCameraCinematic[index].Priority = FocusCinematic;
        currentCam = cinemachineCameraCinematic[index];
        currentMode = ModeCamera.Cinematic;

        rseOnCancelTargeting.Call();

        var anim = currentCam.GetComponent<Animator>();
        if (anim)
        {
            anim.Rebind();
            anim.Update(0f);
            anim.enabled = true;
            anim.SetTrigger("Play");
        }
    }

    private void FinishCinematic()
    {
        StopSkip();

        rseOnDisplaySkip.Call(false);

        var anim = currentCam.GetComponent<Animator>();
        if (anim) anim.enabled = false;

        currentCam.Priority = Unfocus;
        cinemachineCameraRail.Priority = Focus;
        currentCam = cinemachineCameraRail;
        currentMode = ModeCamera.Rail;

        isSkipping = false;

        rseOnDisplayUIGame.Call(true);
        rseOnGameInputEnabled.Call();
    }
    #endregion

    #region Camera Shake
    private void CameraShake(S_ClassCameraShake data)
    {
        var perlin = currentCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (perlin == null) return;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            perlin.AmplitudeGain = perlin.FrequencyGain = 0;
        }

        perlin.AmplitudeGain = data.amplitude;
        perlin.FrequencyGain = data.frequency;

        shakeRoutine = StartCoroutine(S_Utils.Delay(data.duration, () =>
        {
            perlin.AmplitudeGain = perlin.FrequencyGain = 0;
        }));
    }
    #endregion

    #region Handle Systems
    private void HandleCameraRotation()
    {
        if (currentMode == ModeCamera.Rail) cinemachineCameraBridge.transform.position = cinemachineCameraRail.transform.position;

        if (currentMode == ModeCamera.Rail)
        {
            playerPoint.rotation = playerPos.rotation;
            return;
        }

        if (currentMode == ModeCamera.Player)
        {
            Vector3 dir = (currentTarget.position - playerPos.position).normalized;

            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

            Vector3 euler = targetRot.eulerAngles;

            if (euler.x > 180f) euler.x -= 360f;

            euler.x = Mathf.Clamp(euler.x, ssoCameraData.Value.minVerticalCameraPlayer, ssoCameraData.Value.maxVerticalCameraPlayer);

            targetRot = Quaternion.Euler(euler);

            float angle = Quaternion.Angle(playerPoint.transform.rotation, targetRot);

            if (angle > 0.1f)
            {
                float duration = rsoPlayerIsDodging.Value || currentMode == ModeCamera.Bridge ? ssoCameraData.Value.rotationCameraPlayerDodgeDuration : ssoCameraData.Value.rotationCameraPlayerDuration;
                rotationTween?.Kill();
                rotationTween = playerPoint.DORotateQuaternion(targetRot, duration).SetEase(Ease.Linear);
            }
        }
    }

    private void HandlePlayerFade()
    {
        float distance = Vector3.Distance(cameraMain.transform.position, playerPos.position);
        bool hide = distance <= ssoCameraData.Value.cameraDistanceMinPlayer;

        float targetAlpha = hide ? 0f : 1f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, ssoCameraData.Value.fadeSpeedPlayer * Time.deltaTime);

        rseUpdateVisibility.Call(currentAlpha);
    }

    private void HandleSkipHold()
    {
        if (!isSkipping) return;

        skipHold += Time.deltaTime;

        rseOnSkipHold.Call(skipHold);

        if (skipHold >= ssoCameraData.Value.holdSkipTime + 0.35f) SkipCinematic();
    }

    private void StartSkipTimer()
    {
        skipRoutine = StartCoroutine(S_Utils.Delay(ssoCameraData.Value.StartDisplaySkipTime, () =>
        {
            skipHold = 0f;

            rseOnDisplaySkip.Call(true);
            rseOnSkipHold.Call(skipHold);
        }));
    }

    private void StartSkip()
    {
        if (skipRoutine != null)
        {
            StopCoroutine(skipRoutine);
            skipRoutine = null;
        }

        RuntimeManager.PlayOneShot(uiSound);

        skipHold = 0f;
        isSkipping = true;

        rseOnDisplaySkip.Call(true);
        rseOnSkipHold.Call(skipHold);
    }

    private void StopSkip()
    {
        if (skipRoutine != null)
        {
            StopCoroutine(skipRoutine);
            skipRoutine = null;

            RuntimeManager.PlayOneShot(uiSound);
        }

        skipHold = 0f;
        isSkipping = false;

        rseOnSkipHold.Call(skipHold);
    }

    private void SkipCinematic()
    {
        if (skipRoutine != null)
        {
            StopCoroutine(skipRoutine);
            skipRoutine = null;
        }

        StartCoroutine(InstantBlendlessSwitch());
        FinishCinematic();
    }

    private IEnumerator InstantBlendlessSwitch()
    {
        var brain = cameraMain.GetComponent<CinemachineBrain>();

        if (brain == null) yield break;

        brain.enabled = false;

        yield return null;

        brain.enabled = true;
    }

    private void ChangeShoulderOffset(Vector3 target)
    {
        if (cinemachineThirdPersonFollow == null) return;

        shoulderTween?.Kill();
        shoulderTween = DOTween.To(
            () => cinemachineThirdPersonFollow.ShoulderOffset,
            x => cinemachineThirdPersonFollow.ShoulderOffset = x,
            target,
            ssoCameraData.Value.switchDurationCamera
        ).SetEase(Ease.Linear);
    }
    #endregion
}