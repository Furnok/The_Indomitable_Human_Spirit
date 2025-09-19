using UnityEngine;

public class S_TempCameraFollow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float smoothTime;

    [Header("References")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Camera cameraMain;

    //[Header("Input")]

    //[Header("Output")]

    private Vector3 origin = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        origin = transform.position;
    }

    private void LateUpdate()
    {
        if (!targetTransform) return;

        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(targetTransform.position.x + origin.x, targetTransform.position.y + origin.y, targetTransform.position.z + origin.z), ref velocity, smoothTime);
    }
}