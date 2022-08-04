using UnityEngine;
using UnityEngine.Events;
using Mirror;

[RequireComponent(typeof(PlayerClap))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Jump settings")]
    public float minJumpForce = 15f;
    public float maxJumpForce = 50f;
    public float jumpChargeSpeed = 10f;
    public float sidewaysJumpForce = 5f;

    private float _currentJumpForce;

    [Header("Ground Detection")]
    [SerializeField]
    private LayerMask _groundDetectionMask;
    [SerializeField]
    private float _groundCheckSphereRadius = 0.5f;

    [Header("References")]
    [SerializeField] private Camera _cameraPrefab;

    private Camera _cameraInstance;
    private Rigidbody _rigidbody;

    [Header("Called Events")]
    public UnityEvent ClapCalledEvent;
    public UnityEvent<Rigidbody> DashCalledEvent;
    private bool _isDashing = false;

    // В методе Awake() свойство isLocalPlayer неактуально
    private void Start()
    {
        if (!isLocalPlayer) return;

        _rigidbody = GetComponent<Rigidbody>();
        _cameraInstance = Instantiate(_cameraPrefab, transform.position, transform.rotation);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (CanJump())
        {
            HandleJumpForwardInput();
            HandleJumpSidewaysInput();
            HandleOnGroundClapInput();
        }
        else
        {
            HandleMidAirClapInput();
        }

        if (_isDashing == true)
        {
            if (IsLandedAfterDash())
            {
                ClapCalledEvent.Invoke();
            }
        }

        AlignRotationWithCamera();
    }

    private void OnDestroy()
    {
        if (_cameraInstance == null) return;

        Destroy(_cameraInstance.gameObject);
    }

    private bool CanJump()
    {
        bool isHit = Physics.CheckSphere(transform.position + Vector3.down * 0.05f, _groundCheckSphereRadius, _groundDetectionMask);

        return isHit;
    }
    private void HandleJumpForwardInput()
    {
        if (Input.GetButton("Jump"))
        {
            if (_currentJumpForce < maxJumpForce)
            {
                _currentJumpForce += Time.deltaTime * jumpChargeSpeed;
            }
            else
            {
                _currentJumpForce = maxJumpForce;
            }
        }
        else
        {
            if (_currentJumpForce > 0f)
            {
                _currentJumpForce += minJumpForce;
                _rigidbody.AddForce((transform.up / 3 + transform.forward).normalized * _currentJumpForce, ForceMode.Impulse);
                
                _currentJumpForce = 0f;
            }
        }
    }
    private void HandleJumpSidewaysInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyUp(KeyCode.A))
        {
            _rigidbody.AddForce((-transform.right + Vector3.up).normalized * sidewaysJumpForce, ForceMode.Impulse);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyUp(KeyCode.D))
        {
            _rigidbody.AddForce((transform.right + Vector3.up).normalized * sidewaysJumpForce, ForceMode.Impulse);
        }
    }
    private void AlignRotationWithCamera()
    {
        transform.rotation = _cameraInstance.transform.rotation;
    }

    private void HandleOnGroundClapInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClapCalledEvent.Invoke();
        }
    }
    private void HandleMidAirClapInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DashCalledEvent.Invoke(_rigidbody);
            _isDashing = true;
        }
    }
    private bool IsLandedAfterDash()
    {
        if (CanJump() == true)
        {
            _isDashing = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        if (CanJump())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.05f, _groundCheckSphereRadius);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.05f, _groundCheckSphereRadius);
        }
    }
}
