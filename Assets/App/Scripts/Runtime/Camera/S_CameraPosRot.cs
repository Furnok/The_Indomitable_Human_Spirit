using UnityEngine;

public class S_CameraPosRot : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    [Header("RSO")]

    [SerializeField] RSO_CameraPosition _rsoCameraPosition;
    [SerializeField] RSO_CameraRotation _rsoCameraRotation;


    //[Header("Output")]


    private void LateUpdate()
    {
        if (_rsoCameraPosition != null)
        {
            _rsoCameraPosition.Value = transform.position;
        }

        if (_rsoCameraRotation != null)
        {
            _rsoCameraRotation.Value = transform.rotation;
        }
    }
}