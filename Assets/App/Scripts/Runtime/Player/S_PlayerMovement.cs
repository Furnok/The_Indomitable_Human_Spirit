using UnityEngine;

public class S_PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _turnSpeed = 10f;
    //[S_AnimationName] [SerializeField] string _animParamSpeed;
    //[S_AnimationName] [SerializeField] string _animParamBool;

    [Header("References")]
    [SerializeField] Rigidbody _rigidbody;

    [Header("Input")]
    [SerializeField] RSE_OnPlayerMove _rseOnPlayerMove;

    //[Header("Output")]
    //[SerializeField] RSE_OnAnimationFloatValueChange _rseOnAnimationFloatValueChange;
    [SerializeField] RSE_OnAnimationBoolValueChange _rseOnAnimationBoolValueChange;

    [Header("RSO")]
    [SerializeField] RSO_CameraPosition _rsoCameraPosition;
    [SerializeField] RSO_CameraRotation _rsoCameraRotation;

    Vector2 _moveInput;

    private void Awake()
    {
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }
    private void OnEnable()
    {
        _rseOnPlayerMove.action += Move;
    }

    private void OnDisable()
    {
        _rseOnPlayerMove.action -= Move;
    }

    void Move(Vector2 input)
    {
        _moveInput = input;

        if(_moveInput != Vector2.zero)
        {
            _rseOnAnimationBoolValueChange.Call("isMoving", true);
        }
        else
        {
            _rseOnAnimationBoolValueChange.Call("isMoving", false);
        }
    }

    private void FixedUpdate()
    {
        Quaternion camRot = _rsoCameraRotation ? _rsoCameraRotation.Value : Quaternion.identity; //take the rotation of the camera if exist otherwise take the world

        Vector3 camForward = camRot * Vector3.forward;
        camForward.y = 0f; //ignore vertical camera forward
        camForward.Normalize();

        Vector3 camRight = camRot * Vector3.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 desiredDir = camRight * _moveInput.x + camForward * _moveInput.y; //desired direction in world space from the input and the camera orientation

        if (_moveInput != Vector2.zero) //turn character only if there is some input
        {
            desiredDir.Normalize();
            Quaternion target = Quaternion.LookRotation(desiredDir, Vector3.up);
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, target, _turnSpeed * Time.fixedDeltaTime));
        }
        else
        {
            _rigidbody.angularVelocity = Vector3.zero;
            desiredDir = Vector3.zero;
        }

        float inputMag = Mathf.Clamp01(_moveInput.magnitude);
        Vector3 desiredVel = desiredDir * _moveSpeed * inputMag;


        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.x = desiredVel.x;
        velocity.z = desiredVel.z;
        _rigidbody.linearVelocity = velocity;
    }
}

