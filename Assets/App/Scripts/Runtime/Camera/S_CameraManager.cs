using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

public class S_CameraManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float[] fovPerKnot;
    [SerializeField] private float transitionSpeed;

    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCameraRail;
    [SerializeField] private CinemachineCamera cinemachineCameraPlayer;
    [SerializeField] private CinemachineSplineDolly splineDolly;
    [SerializeField] private CinemachineBasicMultiChannelPerlin perlin;
    [SerializeField] private CinemachineTargetGroup targetGroup;

    [Header("Input")]
    [SerializeField] private RSE_CameraShake rseCameraShake;
    [SerializeField] private RSO_PlayerIsTargeting rsoplayerIsTargeting;

    private Coroutine shake = null;
    private CinemachineCamera[] allVCams = null;

    private CinemachineCamera currentCamera = null;

    private void OnEnable()
    {
        rsoplayerIsTargeting.onValueChanged += SwitchCameraTargeting;
        currentCamera = cinemachineCameraRail;
    }

    private void OnDisable()
    {
        rsoplayerIsTargeting.onValueChanged -= SwitchCameraTargeting;
    }

    private void SwitchCameraTargeting(bool value)
    {
        if (value)
        {
            cinemachineCameraRail.Priority = 2;
            cinemachineCameraPlayer.Priority = 3;
            currentCamera = cinemachineCameraPlayer;
        }
        else
        {
            cinemachineCameraRail.Priority = 3;
            cinemachineCameraPlayer.Priority = 2;
            currentCamera = cinemachineCameraRail;
        }
    }

    /*

    private void OnEnable()
    {
        rseCameraShake.action += CameraShake;
    }

    private void OnDisable()
    {
        rseCameraShake.action -= CameraShake;
    }

    private void Start()
    {
        allVCams = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
    }

    private void LateUpdate()
    {
        FOV();
    }

    private void FOV()
    {
        if (cinemachineCameraRail == null || splineDolly == null)
            return;

        var spline = splineDolly.Spline;
        if (spline == null)
            return;

        int knotCount = fovPerKnot.Length;

        if (fovPerKnot == null || fovPerKnot.Length < knotCount)
        {
            return;
        }

        float posKnotUnits = splineDolly.CameraPosition;

        if (splineDolly.PositionUnits != PathIndexUnit.Knot)
        {
            float normalized = 0f;

            if (splineDolly.PositionUnits == PathIndexUnit.Normalized)
            {
                normalized = posKnotUnits;
            }
            else if (splineDolly.PositionUnits == PathIndexUnit.Distance)
            {
                float totalLength = spline.CalculateLength();
                normalized = Mathf.Clamp01(posKnotUnits / totalLength);
            }

            posKnotUnits = normalized * (knotCount - 1);
        }

        int indexA = Mathf.FloorToInt(posKnotUnits);
        int indexB = Mathf.Min(indexA + 1, knotCount - 1);

        float t = posKnotUnits - indexA;

        float fovA = fovPerKnot[indexA];
        float fovB = fovPerKnot[indexB];

        float fov = Mathf.Lerp(fovA, fovB, t);

        cinemachineCameraRail.Lens.FieldOfView = fov;
    }

    private void CameraShake(S_ClassCameraShake classCameraShake)
    {
        if (shake != null)
        {
            StopCoroutine(shake);
            perlin.AmplitudeGain = 0;
            perlin.FrequencyGain = 0;
            shake = null;
        }

        perlin.AmplitudeGain = classCameraShake.amplitude;
        perlin.FrequencyGain = classCameraShake.frequency;

        shake = StartCoroutine(S_Utils.Delay(classCameraShake.duration, () =>
        {
            perlin.AmplitudeGain = 0;
            perlin.FrequencyGain = 0;
        }));
    }
    */
}