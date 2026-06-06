using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class CorrectRotationPuzzle : MonoBehaviour
{
    [Header("Reference Object")]
    public Transform targetSlot;

    [Header("Rotation Speed")]
    public float rotationSpeed = 90f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private XRGrabInteractable _grab;
    private bool _isHeld;
    private bool _isDestroyed = false;
    private Rigidbody _rb;
    private Quaternion _initialTargetRotation;

    private void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();

        if (_grab == null)
        {
            Debug.LogError($"[CorrectRotation] No XRGrabInteractable on {name}");
            enabled = false;
            return;
        }

        if (targetSlot == null)
        {
            Debug.LogError($"[CorrectRotation] No targetSlot assigned on {name} - disabling script");
            enabled = false;
            return;
        }

        _initialTargetRotation = targetSlot.rotation;

        // Configure the grab for rotation control
        _grab.trackRotation = false;  // XRI won't control rotation
        _grab.trackPosition = true;    // XRI controls position

        // Set movement type
        _grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;

        // Add listeners
        _grab.selectEntered.AddListener(OnGrab);
        _grab.selectExited.AddListener(OnRelease);

        if (enableDebugLogs)
            Debug.Log($"[CorrectRotation] '{name}' initialized, target: {targetSlot.name}");
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (_isDestroyed || this == null || gameObject == null) return;

        _isHeld = true;

        // Configure rigidbody for grabbed state
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = false;
            // Freeze all rotation axes - we'll control rotation manually
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            // Keep position movement free
            _rb.constraints &= ~RigidbodyConstraints.FreezePositionX;
            _rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
            _rb.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        }

        if (enableDebugLogs)
            Debug.Log($"[CorrectRotation] '{name}' grabbed - rotation control active");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (_isDestroyed || this == null || gameObject == null) return;

        _isHeld = false;

        // Restore rigidbody
        if (_rb != null)
        {
            _rb.constraints = RigidbodyConstraints.None;
        }

        if (enableDebugLogs)
            Debug.Log($"[CorrectRotation] '{name}' released");
    }

    private void FixedUpdate()
    {
        if (_isDestroyed || this == null || gameObject == null) return;
        if (!_isHeld || targetSlot == null) return;

        // Apply rotation
        Quaternion currentRot = transform.rotation;
        Quaternion targetRot = targetSlot.rotation;

        // Only rotate if not already aligned
        float angleToTarget = Quaternion.Angle(currentRot, targetRot);
        if (angleToTarget > 0.01f)
        {
            Quaternion newRotation = Quaternion.RotateTowards(
                currentRot,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );

            transform.rotation = newRotation;

            // Sync with rigidbody if it exists
            if (_rb != null && !_rb.isKinematic)
            {
                _rb.MoveRotation(newRotation);
            }

            if (enableDebugLogs && angleToTarget > 1f)
                Debug.Log($"[CorrectRotation] '{name}' rotating: {angleToTarget:F1}° to target");
        }
    }

    private void OnDestroy()
    {
        _isDestroyed = true;

        if (_grab != null)
        {
            _grab.selectEntered.RemoveListener(OnGrab);
            _grab.selectExited.RemoveListener(OnRelease);
        }
    }
}