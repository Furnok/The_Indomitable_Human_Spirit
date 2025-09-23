using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

public class S_CameraManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float[] fovPerKnot;
    [SerializeField] private float transitionSpeed;

    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineSplineDolly splineDolly;
    [SerializeField] private CinemachineBasicMultiChannelPerlin perlin;
    [SerializeField] private CinemachineTargetGroup targetGroup;

    [Header("Input")]
    [SerializeField] private RSE_CameraShake rseCameraShake;

    private Coroutine shake;
    private float currentZOffset = -4;

    private void OnEnable()
    {
        rseCameraShake.action += CameraShake;
    }

    private void OnDisable()
    {
        rseCameraShake.action -= CameraShake;
    }

    void Update()
    {
        CameraOffsetZ();
    }

    private void LateUpdate()
    {
        FOV();
    }
    private void CameraOffsetZ()
    {
        if (targetGroup.Targets.Count > 1)
        {
            Vector3 enemyLocalPos = targetGroup.Targets[1].Object.transform.InverseTransformPoint(targetGroup.Targets[0].Object.transform.position);

            float targetZ = (enemyLocalPos.z < 0) ? Mathf.Abs(-4) : -Mathf.Abs(-4);

            currentZOffset = Mathf.Lerp(currentZOffset, targetZ, Time.deltaTime * transitionSpeed);

            splineDolly.SplineOffset = new Vector3(splineDolly.SplineOffset.x, splineDolly.SplineOffset.y, currentZOffset);
        }
    }

    private void FOV()
    {
        if (cinemachineCamera == null || splineDolly == null)
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

        cinemachineCamera.Lens.FieldOfView = fov;
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
}