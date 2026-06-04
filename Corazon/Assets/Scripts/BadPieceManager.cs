using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BadPieceManager : MonoBehaviour
{
    public static BadPieceManager Instance { get; private set; }

    [Header("Pieces")]
    public List<GameObject> badPieces = new List<GameObject>();
    public List<GameObject> goodPieces = new List<GameObject>();

    [Header("Float Targets")]
    public List<Transform> floatTargets = new List<Transform>();

    [Header("Float Settings")]
    public float lerpSpeed = 0.8f;  // Slow movement speed
    public float arrivalThreshold = 0.015f;
    public float hoverAmplitude = 0.04f;
    public float hoverFrequency = 1.1f;
    public float staggerDelay = 0.25f;  // Longer delay between pieces

    [Header("Rotation Settings")]
    public bool applyRotationDuringFloat = true;
    public float rotationLerpSpeed = 45f;  // Slow rotation speed (degrees per second)
    public float hoverRotationSpeed = 15f;  // Very slow rotation while hovering

    // -------------------------------------------------------------------------

    private int remaining;
    private bool triggered = false;
    private List<GameObject> createdTargets = new List<GameObject>();

    private class PieceState
    {
        public GameObject obj;
        public XRGrabInteractable grab;
        public Rigidbody rb;
        public Vector3 floatPosition;
        public bool isHeld;
        public bool isSnapped;
        public float hoverTimer;
        public Quaternion targetRotation;
        public bool hasTargetRotation;
    }

    private List<PieceState> states = new List<PieceState>();

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        remaining = 0;
        foreach (var bp in badPieces)
            if (bp != null) remaining++;

        Debug.Log($"[BadPieceManager] Ready. {remaining} bad piece(s), {goodPieces.Count} good piece(s).");
    }

    // -------------------------------------------------------------------------

    public void OnBadPieceRemoved(GameObject piece)
    {
        var puzzleComponent = piece.GetComponent<CorrectRotationPuzzle>();
        if (puzzleComponent != null)
        {
            puzzleComponent.enabled = false;
            var grabInteractable = piece.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = false;
            }
        }

        remaining--;
        Debug.Log($"[BadPieceManager] '{piece.name}' removed. {remaining} left.");

        if (remaining <= 0 && !triggered)
        {
            triggered = true;
            StartCoroutine(FloatGoodPiecesIn());
        }

        Destroy(piece);
    }

    // -------------------------------------------------------------------------

    private IEnumerator FloatGoodPiecesIn()
    {
        Debug.Log("[BadPieceManager] Floating good pieces in!");

        for (int i = 0; i < goodPieces.Count; i++)
        {
            GameObject obj = goodPieces[i];
            if (obj == null) continue;

            var rotationPuzzle = obj.GetComponent<CorrectRotationPuzzle>();
            if (rotationPuzzle == null)
            {
                Debug.LogError($"[BadPieceManager] Good piece '{obj.name}' is missing CorrectRotationPuzzle component!");
                continue;
            }

            if (rotationPuzzle.targetSlot == null)
            {
                GameObject rotationTarget = new GameObject($"{obj.name}_RotationTarget");
                rotationTarget.transform.SetParent(this.transform);
                rotationTarget.transform.position = obj.transform.position;
                rotationTarget.transform.rotation = Quaternion.Euler(0, 270, 0);

                rotationPuzzle.targetSlot = rotationTarget.transform;
                createdTargets.Add(rotationTarget);
                Debug.Log($"[BadPieceManager] Created rotation target for '{obj.name}'");
            }

            Vector3 targetPos = floatTargets.Count > 0
                ? floatTargets[i % floatTargets.Count].position
                : obj.transform.position + Vector3.up * 1.2f;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            XRGrabInteractable grab = obj.GetComponent<XRGrabInteractable>();

            if (grab != null)
            {
                grab.trackRotation = false;
                grab.trackPosition = true;
                grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            }

            PieceState state = new PieceState
            {
                obj = obj,
                grab = grab,
                rb = rb,
                floatPosition = targetPos,
                isHeld = false,
                isSnapped = false,
                hoverTimer = 0f,
                targetRotation = rotationPuzzle.targetSlot.rotation,
                hasTargetRotation = true
            };
            states.Add(state);

            if (grab != null)
            {
                grab.selectEntered.RemoveAllListeners();
                grab.selectExited.RemoveAllListeners();
                grab.selectEntered.AddListener((args) => OnPieceGrabbed(state));
                grab.selectExited.AddListener((args) => OnPieceReleased(state));
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.None;
            }

            StartCoroutine(LerpToFloatPositionAndRotation(state, obj.transform.position, targetPos, rotationPuzzle.targetSlot.rotation));

            yield return new WaitForSeconds(staggerDelay);
        }
    }

    // -------------------------------------------------------------------------

    private IEnumerator LerpToFloatPositionAndRotation(PieceState state, Vector3 fromPos, Vector3 toPos, Quaternion toRot)
    {
        if (state.rb != null)
        {
            state.rb.isKinematic = true;
            state.rb.useGravity = false;
            state.rb.constraints = RigidbodyConstraints.None;
        }

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(fromPos, toPos);
        float duration = journeyLength / lerpSpeed;

        // Ensure minimum duration for smooth slow movement
        if (duration < 1.5f) duration = 1.5f;

        float rotationStartTime = Time.time;
        Quaternion fromRot = state.obj.transform.rotation;
        float rotationDuration = 2.0f; // Take 2 seconds to fully rotate

        while (state.obj != null && !state.isHeld && !state.isSnapped)
        {
            float elapsed = Time.time - startTime;
            float fraction = Mathf.Clamp01(elapsed / duration);
            float smoothFraction = Mathf.SmoothStep(0, 1, fraction);

            // Smooth position lerp
            state.obj.transform.position = Vector3.Lerp(fromPos, toPos, smoothFraction);

            // Smooth rotation lerp
            if (applyRotationDuringFloat)
            {
                float rotationElapsed = Time.time - rotationStartTime;
                float rotationFraction = Mathf.Clamp01(rotationElapsed / rotationDuration);
                float smoothRotationFraction = Mathf.SmoothStep(0, 1, rotationFraction);
                state.obj.transform.rotation = Quaternion.Slerp(fromRot, toRot, smoothRotationFraction);
            }

            if (fraction >= 1.0f)
            {
                state.obj.transform.position = toPos;
                if (applyRotationDuringFloat)
                {
                    state.obj.transform.rotation = toRot;
                }
                break;
            }

            yield return null;
        }

        if (state.obj == null || state.isHeld || state.isSnapped) yield break;

        state.obj.transform.position = toPos;
        if (applyRotationDuringFloat)
        {
            state.obj.transform.rotation = toRot;
        }

        state.hoverTimer = 0f;

        Debug.Log($"[BadPieceManager] '{state.obj.name}' ready to grab");
    }

    // -------------------------------------------------------------------------

    private IEnumerator LerpToFloat(PieceState state, Vector3 from, Vector3 to)
    {
        if (state.rb != null)
        {
            state.rb.isKinematic = true;
            state.rb.useGravity = false;
            state.rb.constraints = RigidbodyConstraints.None;
        }

        if (state.obj != null)
            state.obj.transform.position = from;

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(from, to);
        float duration = journeyLength / lerpSpeed;
        if (duration < 0.5f) duration = 0.5f;

        while (state.obj != null && !state.isHeld && !state.isSnapped)
        {
            float elapsed = Time.time - startTime;
            float fraction = Mathf.Clamp01(elapsed / duration);

            if (fraction >= 1.0f)
            {
                state.obj.transform.position = to;
                break;
            }

            if (state.obj != null)
                state.obj.transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, fraction));

            yield return null;
        }

        if (state.obj == null || state.isHeld || state.isSnapped) yield break;

        if (state.obj != null)
            state.obj.transform.position = to;

        state.hoverTimer = 0f;
    }

    // -------------------------------------------------------------------------

    private void OnPieceGrabbed(PieceState state)
    {
        state.isHeld = true;

        if (state.rb != null)
        {
            state.rb.isKinematic = false;
            state.rb.useGravity = false;
            var rotationPuzzle = state.obj.GetComponent<CorrectRotationPuzzle>();
            if (rotationPuzzle != null && rotationPuzzle.enabled)
            {
                state.rb.constraints = RigidbodyConstraints.FreezeRotationX |
                                      RigidbodyConstraints.FreezeRotationZ;
            }
        }
    }

    private void OnPieceReleased(PieceState state)
    {
        if (state.isSnapped) return;

        state.isHeld = false;

        if (state.obj != null)
            StartCoroutine(LerpToFloat(state, state.obj.transform.position, state.floatPosition));
    }

    // -------------------------------------------------------------------------

    private void Update()
    {
        foreach (var s in states)
        {
            if (s.obj == null) continue;
            if (s.isHeld) continue;
            if (s.isSnapped) continue;

            if (s.grab != null && !s.grab.enabled)
            {
                s.isSnapped = true;
                continue;
            }

            if (s.rb != null && !s.rb.isKinematic) continue;
            if (Vector3.Distance(s.obj.transform.position, s.floatPosition) > arrivalThreshold * 4f) continue;

            // Hover bob
            s.hoverTimer += Time.deltaTime;
            float yOffset = Mathf.Sin(s.hoverTimer * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
            s.obj.transform.position = s.floatPosition + Vector3.up * yOffset;

            // Slow rotation while hovering
            var rotationPuzzle = s.obj.GetComponent<CorrectRotationPuzzle>();
            if (rotationPuzzle != null && rotationPuzzle.targetSlot != null && !s.isHeld)
            {
                s.obj.transform.rotation = Quaternion.RotateTowards(
                    s.obj.transform.rotation,
                    rotationPuzzle.targetSlot.rotation,
                    hoverRotationSpeed * Time.deltaTime
                );
            }
        }
    }

    // -------------------------------------------------------------------------

    private void OnDestroy()
    {
        foreach (var target in createdTargets)
        {
            if (target != null)
                Destroy(target);
        }
        createdTargets.Clear();
    }

    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        if (floatTargets == null) return;
        Gizmos.color = Color.green;
        foreach (var t in floatTargets)
        {
            if (t == null) continue;
            Gizmos.DrawWireSphere(t.position, 0.12f);
            Gizmos.DrawLine(t.position, t.position + Vector3.up * 0.25f);
        }
    }
}