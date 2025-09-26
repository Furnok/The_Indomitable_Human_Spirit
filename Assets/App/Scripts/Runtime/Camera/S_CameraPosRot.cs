using UnityEngine;

public class S_CameraPosRot : MonoBehaviour
{
    [Header("Output")]
    [SerializeField] private RSO_CameraPosition rsoCameraPosition;
    [SerializeField] private RSO_CameraRotation rsoCameraRotation;

    private void LateUpdate()
    {
        CameraPosition();

        CameraRotation();
    }

    private void CameraPosition()
    {
        if (rsoCameraPosition != null)
        {
            rsoCameraPosition.Value = transform.position;
        }
    }

    private void CameraRotation()
    {
        if (rsoCameraRotation != null)
        {
            rsoCameraRotation.Value = transform.rotation;
        }
    }
}