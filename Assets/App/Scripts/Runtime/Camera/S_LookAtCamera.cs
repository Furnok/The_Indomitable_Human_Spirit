using UnityEngine;

public class S_LookAtCamera : MonoBehaviour
{
    [SerializeField] private bool followCam;

    [SerializeField] private float maxDistance;

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, gameObject.transform.position);
        bool show = distance <= maxDistance;

        if (meshRenderer != null) meshRenderer.enabled = show;
    }

    private void LateUpdate()
    {
        if (!followCam) return;

        var lookPos = Camera.main.transform;

        transform.LookAt(lookPos);
    }
}