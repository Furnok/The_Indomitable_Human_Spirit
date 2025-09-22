using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

public class S_CameraFOV : MonoBehaviour
{
    public CinemachineCamera virtualCamera;
    public CinemachineSplineDolly splineDolly;  // this is the body component controlling position along the spline

    [Tooltip("FOV values for each knot. Must match or exceed number of knots in the Spline Container.")]
    public float[] fovPerKnot;

    void Update()
    {
        if (virtualCamera == null || splineDolly == null)
            return;

        var spline = splineDolly.Spline;
        if (spline == null)
            return;

        // How many knots (waypoints)
        int knotCount = fovPerKnot.Length;  // or use whatever property gives the number of knots
        if (knotCount <= 0)
            return;

        if (fovPerKnot == null || fovPerKnot.Length < knotCount)
        {
            Debug.LogWarning("fovPerKnot array length is less than number of knots in spline");
            return;
        }

        // Get current position in “Knot” units
        float posKnotUnits = splineDolly.CameraPosition;
        // When PositionUnits is Knot or normalized or distance, this means a float value: e.g. 2.4 = between knot 2 and 3
        // If it's Distance or Normalized, you might need to convert to knot units

        // If using PositionUnits = Knot, great. If not, convert:
        if (splineDolly.PositionUnits != PathIndexUnit.Knot)
        {
            // convert the CameraPosition into Knot units
            // One way: map normalized or distance into knot units
            float normalized = 0f;
            if (splineDolly.PositionUnits == PathIndexUnit.Normalized)
            {
                normalized = posKnotUnits;  // already 0..1
            }
            else if (splineDolly.PositionUnits == PathIndexUnit.Distance)
            {
                // get total spline length
                float totalLength = spline.CalculateLength();  // approximation
                normalized = Mathf.Clamp01(posKnotUnits / totalLength);
            }
            posKnotUnits = normalized * (knotCount - 1);
        }

        // Determine indices
        int indexA = Mathf.FloorToInt(posKnotUnits);
        int indexB = Mathf.Min(indexA + 1, knotCount - 1);

        float t = posKnotUnits - indexA;

        float fovA = fovPerKnot[indexA];
        float fovB = fovPerKnot[indexB];

        float fov = Mathf.Lerp(fovA, fovB, t);

        virtualCamera.Lens.FieldOfView = fov;
    }
}