using UnityEngine;

public class S_TempCharacterController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;

    //[Header("Input")]

    //[Header("Output")]

    private Vector3 movement = Vector3.zero;

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        movement = new Vector3(moveX, 0f, moveZ);

        rb.linearVelocity = movement * speed;

        float moveSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

        animator.SetFloat("MoveSpeed", moveSpeed);
    }
}