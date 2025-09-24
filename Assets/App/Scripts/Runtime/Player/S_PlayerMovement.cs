using UnityEngine;

public class S_PlayerMovement : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] Rigidbody _rigidbody;

    [Header("Input")]
    [SerializeField] RSE_OnPlayerMove _rseOnPlayerMove;

    [Header("Output")]
    [SerializeField] RSE_OnAnimationBoolValueChange _rseOnAnimationBoolValueChange;
    [SerializeField] RSE_OnAnimationFloatValueChange _rseOnAnimationFloatValueChange;

    [Header("RSO")]
    [SerializeField] RSO_CameraPosition _rsoCameraPosition;
    [SerializeField] RSO_CameraRotation _rsoCameraRotation;
    [SerializeField] RSO_PlayerPosition _rsoPlayerPosition;
    [SerializeField] RSO_PlayerIsTargeting _rsoPlayerIsTargeting;
    [SerializeField] RSO_TargetPosition _rsoTargetPosition;

    [Header("SSO")]
    [SerializeField] SSO_PlayerMovementSpeed _ssoPlayerMovementSpeed;
    [SerializeField] SSO_PlayerTurnSpeed _ssoPlayerTurnSpeed;
    [SerializeField] SSO_PlayerStrafeSpeed _ssoPlayerStrafeSpeed;

    Vector2 _moveInput;

    private void Awake()
    {
        _rsoPlayerPosition.Value = transform.position;

        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnDestroy()
    {
        _rsoPlayerPosition.Value = Vector3.zero;
    }
    private void OnEnable()
    {
        _rseOnPlayerMove.action += Move;
    }

    private void OnDisable()
    {
        _rseOnPlayerMove.action -= Move;
        _rsoPlayerPosition.Value = Vector3.zero;
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
        
        if (_rsoPlayerIsTargeting.Value && _rsoTargetPosition.Value != null)
        {
            Vector3 directionToTarget = _rsoTargetPosition.Value - transform.position;
            directionToTarget.y = 0f; // Ignore the heigth

            if (directionToTarget.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, targetRotation, _ssoPlayerTurnSpeed.Value * Time.fixedDeltaTime));
            }

            Vector3 right = Vector3.Cross(Vector3.up, directionToTarget.normalized);
            Vector3 forward = directionToTarget.normalized;

            Vector3 desiredDirection = right * _moveInput.x + forward * _moveInput.y;
            desiredDirection.Normalize();

            float inputMagnitude = Mathf.Clamp01(_moveInput.magnitude);
            Vector3 desiredVelocity = desiredDirection * _ssoPlayerStrafeSpeed.Value * inputMagnitude;

            Vector3 velocityTargeting = _rigidbody.linearVelocity;
            velocityTargeting.x = desiredVelocity.x;
            velocityTargeting.z = desiredVelocity.z;
            _rigidbody.linearVelocity = velocityTargeting;

            _rseOnAnimationFloatValueChange.Call("MoveSpeed", velocityTargeting.magnitude);


            _rsoPlayerPosition.Value = transform.position;
            return;
        }



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
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, target, _ssoPlayerTurnSpeed.Value * Time.fixedDeltaTime));
        }
        else
        {
            _rigidbody.angularVelocity = Vector3.zero;
            desiredDir = Vector3.zero;
        }

        float inputMag = Mathf.Clamp01(_moveInput.magnitude);
        Vector3 desiredVel = desiredDir * _ssoPlayerMovementSpeed.Value * inputMag;


        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.x = desiredVel.x;
        velocity.z = desiredVel.z;
        _rigidbody.linearVelocity = velocity;

        _rseOnAnimationFloatValueChange.Call("MoveSpeed", velocity.magnitude);

        _rsoPlayerPosition.Value = transform.position;
    }
}

