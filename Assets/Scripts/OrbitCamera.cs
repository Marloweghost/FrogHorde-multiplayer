using UnityEngine;
using Mirror;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : NetworkBehaviour
{
    [SerializeField]
    private Transform _focus = default;

    [SerializeField, Range(1f, 20f)]
    private float distance = 5f;

    [SerializeField, Min(0f)]
    private float focusRadius = 1f;

    [SerializeField, Range(0f, 1f)]
    private float focusCentering = 0.5f;

    [SerializeField, Range(1f, 360f)]
    private float rotationSpeed = 90f;

    [SerializeField, Range(-89f, 89f)]
    private float minVerticalAngle = -30f, maxVerticalAngle = 60f;

    [SerializeField, Min(0f)]
    private float alignDelay = 5f;
    private float lastManualRotationTime;

    [SerializeField, Range(0f, 90f)]
    private float alignSmoothRange = 45f;

    [SerializeField]
    private LayerMask obstructionMask = -1;

    private Vector3 _focusPoint, _previousFocusPoint;

    private Vector2 orbitAngles = new Vector2(45f, 0f);

    private Camera regularCamera;
    private Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y = regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    private void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }


    private void Awake()
    {
        _focus = NetworkClient.localPlayer.transform;
        regularCamera = GetComponent<Camera>();
        _focusPoint = _focus.position;
        transform.localRotation = Quaternion.Euler(orbitAngles);
    }

    private void LateUpdate()
    {
        UpdateFocusPoint();
        Quaternion lookRotation;
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(orbitAngles);
        }
        else
        {
            lookRotation = transform.localRotation;
        }
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = _focusPoint - lookDirection * distance;

        Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = _focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, obstructionMask))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    private void UpdateFocusPoint()
    {
        _previousFocusPoint = _focusPoint;
        Vector3 targetPoint = _focus.position;
        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, _focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }
            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }
            _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
        }
    }

    private bool ManualRotation()
    {
        Vector2 input = new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }

    private void ConstrainAngles()
    {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        } 
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }

    private bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false;
        }

        Vector2 movement = new Vector2(
            _focusPoint.x - _previousFocusPoint.x,
            _focusPoint.z - _previousFocusPoint.z);

        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.0001f)
        {
            return false;
        }

        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        //orbitAngles.y = headingAngle;
        return true;
    }

    private float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }
}
