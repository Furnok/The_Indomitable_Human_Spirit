using UnityEngine;

public class S_DebugDirection : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    //[Header("Input")]

    //[Header("Output")]

    [SerializeField] float _length = 2.0f;
    [SerializeField] float _yOffset = 1.3f;
    [SerializeField] Color _color = Color.green;

    void OnDrawGizmos()
    {
        if (!enabled) return;
        Gizmos.color = _color;

        Vector3 start = new Vector3(transform.position.x, transform.position.y + _yOffset, transform.position.z);
        Vector3 end = start + transform.forward * _length;

        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, 0.1f);
    }
}