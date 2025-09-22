using UnityEngine;

public class S_TempCharacterController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed;
    [SerializeField] float runSpeed;

    [Header("References")]
    [SerializeField] private Rigidbody rb;

    //[Header("Input")]

    [Header("Output")]
    [SerializeField] RSE_OnAnimationBoolValueChange RSE_OnAnimationBoolValueChange;
    [SerializeField] RSE_OnAnimationFloatValueChange RSE_OnAnimationFloatValueChange;

    private Vector3 movement = Vector3.zero;

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        movement = new Vector3(moveX, 0, moveZ);

        if (movement != Vector3.zero)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                movement.Normalize();
                Quaternion target = Quaternion.LookRotation(movement, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, speed * Time.fixedDeltaTime));

                Vector3 velocity = movement * runSpeed;
                velocity.y = rb.linearVelocity.y;
                rb.linearVelocity = velocity;

                float moveSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

                RSE_OnAnimationFloatValueChange.Call("MoveSpeed", moveSpeed);
            }
            else
            {
                movement.Normalize();
                Quaternion target = Quaternion.LookRotation(movement, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, speed * Time.fixedDeltaTime));

                Vector3 velocity = movement * speed;
                velocity.y = rb.linearVelocity.y;
                rb.linearVelocity = velocity;

                float moveSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
                RSE_OnAnimationFloatValueChange.Call("MoveSpeed", moveSpeed);
            }

            RSE_OnAnimationBoolValueChange.Call("isMoving", true);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.angularVelocity = Vector3.zero;

            RSE_OnAnimationBoolValueChange.Call("isMoving", false);
            RSE_OnAnimationFloatValueChange.Call("MoveSpeed", 0);
        }
    }
}