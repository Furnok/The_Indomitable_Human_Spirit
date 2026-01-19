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
    [SerializeField] private CinemachineCamera cinemachineCameraPlayer;

    [TabGroup("References")]
    [SerializeField] private CinemachineOrbitalFollow cinemachineCameraOrbitalFollow;

    [TabGroup("References")]
    [SerializeField] private List<CinemachineCamera> cinemachineCameraCinematic;

    [TabGroup("References")]
    [Title("Target")]
    [SerializeField] private Transform playerPoint;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnNewTargeting rseOnNewTargeting;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerCancelTargeting rseOnPlayerCancelTargeting;

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

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnResetCam rseOnResetCam;

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
    [SerializeField] private SSO_CameraData ssoCameraData;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerMove rseOnPlayerMove;

    private CinemachineCamera currentCam = null;
    private Transform playerPos = null;
    private Transform currentTarget = null;

    private float currentAlpha = 1f;

    private Coroutine shakeRoutine = null;
    private Coroutine skipRoutine = null;

    private bool isSkipping = false;
    private float skipHold = 0;

    private const int Focus = 2;
    private const int FocusCinematic = 100;
    private const int Unfocus = 1;

    private Tween shoulderTween = null;
    private float lastDirection = 0f;
    private bool offsetMove = false;

    private void Awake()
    {
        cinemachineCameraPlayer.Target.TrackingTarget = playerPoint;

        currentCam = cinemachineCameraPlayer;

        LookActivated(true);
    }

    private void OnEnable()
    {
        rseOnPlayerCenter.action += PlayerPos;
        rseOnNewTargeting.action += SetTarget;
        rseOnPlayerCancelTargeting.action += SetTarget;
        rseOnCameraIntro.action += CameraIntro;
        rseOnCameraCinematic.action += SwitchCinematicCamera;
        rseOnCameraShake.action += CameraShake;
        rseOnCinematicFinish.action += FinishCinematic;
        rseOnSkipInput.action += StartSkip;
        rseOnSkipCancelInput.action += StopSkip;
        rseOnSkipIntro.action += SkipIntro;
        rseOnResetCam.action += ResetCam;

        rseOnPlayerMove.action += InputsMove;
    }

    private void OnDisable()
    {
        rseOnPlayerCenter.action -= PlayerPos;
        rseOnNewTargeting.action -= SetTarget;
        rseOnPlayerCancelTargeting.action -= SetTarget;
        rseOnCameraIntro.action -= CameraIntro;
        rseOnCameraCinematic.action -= SwitchCinematicCamera;
        rseOnCameraShake.action -= CameraShake;
        rseOnCinematicFinish.action -= FinishCinematic;
        rseOnSkipInput.action -= StartSkip;
        rseOnSkipCancelInput.action -= StopSkip;
        rseOnSkipIntro.action -= SkipIntro;
        rseOnResetCam.action -= ResetCam;

        rseOnPlayerMove.action -= InputsMove;
    }

    private void Update()
    {
        if (playerPos == null) return;

        playerPoint.position = playerPos.position;

        HandlePlayerFade();
        HandleSkipHold();
        HandleTargeting();

        if (lastDirection > 0)
        {
            ChangeShoulderOffsetWorld(ssoCameraData.Value.shoulderOffsetAmountNegative, 0);
        }
        else if (lastDirection < 0)
        {
            ChangeShoulderOffsetWorld(ssoCameraData.Value.shoulderOffsetAmountPositive, 0);
        }
    }

    private void SetTarget(GameObject target)
    {
        if (target != null)
        {
            if (currentTarget == target.transform)
            {
                currentTarget = null;

                LookActivated(true);

                ChangeShoulderOffset(new Vector3(0, 1, 0));
                lastDirection = 0;
            }
            else
            {
                currentTarget = target.transform;

                LookActivated(false);
            }
        }
    }

    private void PlayerPos(Transform player)
    {
        playerPos = player;
        cinemachineCameraPlayer.Target.TrackingTarget = player;
    }

    #region Camera System
    private void SwitchPlayerCamera()
    {
        cinemachineCameraPlayer.Priority = Focus;
        currentCam.Priority = Unfocus;
        currentCam = cinemachineCameraPlayer;

        LookActivated(true);
    }

    private void ResetCam()
    {
        cinemachineCameraPlayer.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value = cinemachineCameraPlayer.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Center;
        cinemachineCameraPlayer.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value = cinemachineCameraPlayer.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Center;
    }
    #endregion

    #region Cinematic System
    private void CameraIntro()
    {
        if (cinemachineCameraIntro.transform.parent.gameObject.activeInHierarchy)
        {
            StartSkipTimer();

            LookActivated(false);

            rseOnDisplayUIGame.Call(false);
            rseOnCinematicInputEnabled.Call();

            currentCam = cinemachineCameraIntro;

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
            LookActivated(true);

            currentCam = cinemachineCameraPlayer;

            rseOnDisplayUIGame.Call(true);
            rseOnGameInputEnabled.Call();
        }
    }

    private void SkipIntro()
    {
        var anim = cinemachineCameraIntro.GetComponent<Animator>();
        if (anim) anim.enabled = false;

        SwitchPlayerCamera();
    }

    private void SwitchCinematicCamera(int index)
    {
        if (index < 0 || index >= cinemachineCameraCinematic.Count) return;

        LookActivated(false);

        StartSkipTimer();

        rseOnDisplayUIGame.Call(false);
        rseOnCinematicInputEnabled.Call();

        cinemachineCameraPlayer.Priority = Unfocus;
        cinemachineCameraCinematic[index].Priority = FocusCinematic;
        currentCam = cinemachineCameraCinematic[index];

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

        SwitchPlayerCamera();

        isSkipping = false;

        rseOnDisplayUIGame.Call(true);
        rseOnGameInputEnabled.Call();

    }

    private void LookActivated(bool value)
    {
        cinemachineCameraPlayer.GetComponent<CinemachineInputAxisController>().enabled = value;
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

    private void HandleTargeting()
    {
        if (currentTarget)
        {
            Vector3 dir = currentTarget.position - playerPos.position;

            if (dir.sqrMagnitude < 0.001f) return;

            float targetYaw = Quaternion.LookRotation(dir).eulerAngles.y;

            cinemachineCameraOrbitalFollow.HorizontalAxis.Value = Mathf.LerpAngle(cinemachineCameraOrbitalFollow.HorizontalAxis.Value, targetYaw, Time.deltaTime * 5f);

            float targetPitch = Quaternion.LookRotation(dir).eulerAngles.x;
            targetPitch = Mathf.Clamp(targetPitch, ssoCameraData.Value.minVerticalCameraPlayer, ssoCameraData.Value.maxVerticalCameraPlayer);

            cinemachineCameraOrbitalFollow.VerticalAxis.Value = Mathf.LerpAngle(cinemachineCameraOrbitalFollow.VerticalAxis.Value, targetPitch, Time.deltaTime * 5f);
        }
    }

    private void InputsMove(Vector2 move)
    {
        if (currentTarget != null)
        {
            if (move.x > 0 && lastDirection <= 0)
            {
                lastDirection = move.x;
            }
            else if (move.x < 0 && lastDirection >= 0)
            {
                lastDirection = move.x;
            }
        }
        else shoulderTween?.Kill();
    }

    private void ChangeShoulderOffsetWorld(float sideAmount, float forwardAmount)
    {
        Transform t = playerPos;

        Vector3 toTarget = (currentTarget.position - playerPos.position).normalized;
        Vector3 cameraRight = Vector3.Cross(Vector3.up, toTarget);
        Vector3 cameraForward = Vector3.Cross(toTarget, cameraRight);

        Vector3 worldOffset = cameraRight * sideAmount + cameraForward * forwardAmount;
        Vector3 targetOffset = new Vector3(worldOffset.x, cinemachineCameraOrbitalFollow.TargetOffset.y, worldOffset.z);

        ChangeShoulderOffset(targetOffset);
    }

    private void ChangeShoulderOffset(Vector3 target)
    {
        shoulderTween?.Kill();
        shoulderTween = DOTween.To(() => cinemachineCameraOrbitalFollow.TargetOffset, x => cinemachineCameraOrbitalFollow.TargetOffset = x, target, ssoCameraData.Value.offsetTransitionTime).SetEase(Ease.Linear);
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
    #endregion
}