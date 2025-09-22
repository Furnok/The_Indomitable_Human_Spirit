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

        movement = new Vector3(moveX, 0, moveZ);

        if (movement != Vector3.zero)
        {
            movement.Normalize();
            Quaternion target = Quaternion.LookRotation(movement, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, speed * Time.fixedDeltaTime));

            Vector3 velocity = movement * speed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;

            float moveSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
            animator.SetFloat("MoveSpeed", moveSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.angularVelocity = Vector3.zero;

            animator.SetFloat("MoveSpeed", 0);
        }
    }
}