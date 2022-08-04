using UnityEngine;

public class PlayerClap : MonoBehaviour
{
    [SerializeField] private LayerMask _layersToAffect;
    [SerializeField] private float _clapRadius = 10f;
    [SerializeField] private float _dashForce = 5f;

    private Vector3 _dashStartPosition;
    private Vector3 _dashEndPosition;
    private float heightAdditionalForce;
    [SerializeField] private float heightForceMultiplier;
    private bool _dashCalled = false;

    private Collider[] _foundColliders;

    [SerializeField] private float _clapCooldown = 1f;
    private CooldownTimer _clapTimer = new CooldownTimer();
    
    [System.Serializable]
    private enum ClapMode
    {
        Explosion = 0,
        TossUp = 1,
    }

    [Space]
    [SerializeField] private ClapMode currentClapMode;
    [System.Serializable]
    private struct ClapModeParameters
    {
        public float clapForce;
    }
    [SerializeField] private ClapModeParameters _ExplosionParameters;
    [SerializeField] private ClapModeParameters _TossUpParameters;

    private void ApplyForces(Collider[] _colliders)
    {
        switch (currentClapMode)
        {
            case ClapMode.Explosion:
                DoExplosion(_colliders);
                break;
            case ClapMode.TossUp:
                DoTossUp(_colliders);
                break;
        }
    }

    public void Start()
    {
        _clapTimer.cooldownAmount = _clapCooldown;
    }

    public void TryClap()
    {
        if (_clapTimer.CooldownComplete == false) return;

        if (_dashCalled == true)
        {
            _dashEndPosition = transform.position;
            heightAdditionalForce = Vector3.Distance(_dashStartPosition, _dashEndPosition) * heightForceMultiplier;
            Debug.Log("Add force = " + heightAdditionalForce.ToString("F2"));
        }
        else { heightAdditionalForce = 0f; }
       
        _foundColliders = Physics.OverlapSphere(transform.position, _clapRadius, _layersToAffect);

        if (_foundColliders.Length != 0)
        {
            ApplyForces(_foundColliders);
        }
          
        _clapTimer.StartCooldown();

        _dashCalled = false;
    }

    public void Dash(Rigidbody _playerRigidbody)
    {
        _dashCalled = true;
        _dashStartPosition = transform.position;

        _playerRigidbody.velocity = Vector3.zero;
        _playerRigidbody.AddForce(Vector3.down * _dashForce, ForceMode.Impulse);
    }

    private void DoExplosion(Collider[] _collidersToAffect)
    {
        foreach (Collider nearbyObject in _collidersToAffect)
        {
            if (nearbyObject.TryGetComponent(out Rigidbody _rigidbody))
            {
                _rigidbody.AddExplosionForce(_ExplosionParameters.clapForce + heightAdditionalForce, transform.position, _clapRadius);
            }
        }
    }

    private void DoTossUp(Collider[] _collidersToAffect)
    {
        foreach (Collider nearbyObject in _collidersToAffect)
        {
            if (nearbyObject.TryGetComponent(out Rigidbody _rigidbody))
            {
                _rigidbody.AddForce(Vector3.up * (_TossUpParameters.clapForce + heightAdditionalForce), ForceMode.Impulse);
            }
        }
    }
}
