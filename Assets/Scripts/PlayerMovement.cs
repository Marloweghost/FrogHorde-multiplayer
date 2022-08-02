using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Jump settings")]
    public float minJumpForce = 15f;
    public float maxJumpForce = 50f;
    public float jumpChargeSpeed = 10f;

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
            HandleJumpInput();
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
    private void HandleJumpInput()
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
    private void AlignRotationWithCamera()
    {
        transform.rotation = _cameraInstance.transform.rotation;
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
