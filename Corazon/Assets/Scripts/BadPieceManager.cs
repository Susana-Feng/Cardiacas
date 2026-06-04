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
    public float lerpSpeed = 0.8f;
    public float arrivalThreshold = 0.015f;
    public float hoverAmplitude = 0.04f;
    public float hoverFrequency = 1.1f;
    public float staggerDelay = 0.25f;

    [Header("Rotation Settings")]
    public bool applyRotationDuringFloat = true;
    public float rotationLerpSpeed = 45f;
    public float hoverRotationSpeed = 15f;

    [Header("Tutorial Gate")]
    [Tooltip("Assign the tutorial root GameObject (e.g. Tuto1). Music starts when its CoachingCardRoot child is deactivated.")]
    public GameObject tutorialObject;

    [Header("Music")]
    [Tooltip("Plays while the player is removing bad pieces (starts when tutorial disappears).")]
    public AudioClip badPhaseMusic;

    [Tooltip("Plays when all bad pieces are gone and good pieces start floating in.")]
    public AudioClip goodPhaseMusic;

    [Tooltip("How long the crossfade between the two tracks takes (seconds).")]
    public float musicCrossfadeDuration = 1.5f;

    [Tooltip("Volume for both tracks (0–1).")]
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    // -------------------------------------------------------------------------

    private int remaining;
    private bool triggered = false;
    private bool musicStarted = false;   // true once bad-phase music has begun
    private List<GameObject> createdTargets = new List<GameObject>();

    // Two AudioSources let us crossfade cleanly without gaps.
    private AudioSource audioSourceA;
    private AudioSource audioSourceB;

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

        // Create a dedicated child for AudioSources so they are never disabled
        // by anything toggling this GameObject or its components.
        GameObject musicHost = new GameObject("MusicHost");
        musicHost.transform.SetParent(null); // detach — lives at scene root
        DontDestroyOnLoad(musicHost);        // survives scene reloads

        audioSourceA = musicHost.AddComponent<AudioSource>();
        audioSourceB = musicHost.AddComponent<AudioSource>();

        foreach (var src in new[] { audioSourceA, audioSourceB })
        {
            src.loop = true;
            src.playOnAwake = false;
            src.volume = 0f;
            src.spatialBlend = 0f; // 2D / no positional falloff
        }
    }

    private void Start()
    {
        remaining = 0;
        foreach (var bp in badPieces)
            if (bp != null) remaining++;

        Debug.Log($"[BadPieceManager] Ready. {remaining} bad piece(s), {goodPieces.Count} good piece(s).");

        // Music starts only once the tutorial disappears (watched in Update).
        if (tutorialObject == null)
            Debug.LogWarning("[BadPieceManager] No tutorialObject assigned — bad-phase music will never start.");
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
                grabInteractable.enabled = false;
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

        // Crossfade to the good-phase music.
        if (goodPhaseMusic != null)
            StartCoroutine(CrossfadeMusic(audioSourceA, audioSourceB, goodPhaseMusic, musicCrossfadeDuration));
        else
            Debug.LogWarning("[BadPieceManager] No goodPhaseMusic assigned.");

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
    // Music helpers
    // -------------------------------------------------------------------------

    /// Fade in a clip on a source from silence to musicVolume.
    /// <summary>
    /// Fades out and stops all music. Called externally (e.g. HeartBeat) when game music should stop.
    /// </summary>
    public void StopMusic(float fadeDuration = 1f)
    {
        StartCoroutine(FadeOutAndStop(audioSourceA, fadeDuration));
        StartCoroutine(FadeOutAndStop(audioSourceB, fadeDuration));
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        if (source == null || !source.isPlaying) yield break;
        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        source.Stop();
        source.volume = 0f;
    }

    private IEnumerator FadeInMusic(AudioSource source, AudioClip clip, float duration)
    {
        Debug.Log($"[BadPieceManager] FadeInMusic START — clip={clip.name}, duration={duration}, source enabled={source.enabled}, gameObject active={source.gameObject.activeInHierarchy}");
        source.clip = clip;
        source.volume = 0f;
        source.Play();
        Debug.Log($"[BadPieceManager] AudioSource.Play() called — isPlaying={source.isPlaying}");

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, musicVolume, elapsed / duration);
            yield return null;
        }
        source.volume = musicVolume;
        Debug.Log($"[BadPieceManager] FadeInMusic DONE — final volume={source.volume}, isPlaying={source.isPlaying}");
    }

    /// Fade out the current source while fading in a new clip on the other source.
    private IEnumerator CrossfadeMusic(AudioSource fadeOut, AudioSource fadeIn, AudioClip newClip, float duration)
    {
        float startVolume = fadeOut.volume;

        fadeIn.clip = newClip;
        fadeIn.volume = 0f;
        fadeIn.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            fadeOut.volume = Mathf.Lerp(startVolume, 0f, t);
            fadeIn.volume = Mathf.Lerp(0f, musicVolume, t);
            yield return null;
        }

        fadeOut.volume = 0f;
        fadeOut.Stop();
        fadeIn.volume = musicVolume;
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
        if (duration < 1.5f) duration = 1.5f;

        float rotationStartTime = Time.time;
        Quaternion fromRot = state.obj.transform.rotation;
        float rotationDuration = 2.0f;

        while (state.obj != null && !state.isHeld && !state.isSnapped)
        {
            float elapsed = Time.time - startTime;
            float fraction = Mathf.Clamp01(elapsed / duration);
            float smoothFraction = Mathf.SmoothStep(0, 1, fraction);

            state.obj.transform.position = Vector3.Lerp(fromPos, toPos, smoothFraction);

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
                    state.obj.transform.rotation = toRot;
                break;
            }

            yield return null;
        }

        if (state.obj == null || state.isHeld || state.isSnapped) yield break;

        state.obj.transform.position = toPos;
        if (applyRotationDuringFloat)
            state.obj.transform.rotation = toRot;

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
        // Watch for the tutorial disappearing, then kick off bad-phase music.
        // TutoManager never deactivates the root GameObject — it hides CoachingCardRoot instead.
        // So we watch for CoachingCardRoot becoming inactive as the "tutorial dismissed" signal.
        if (!musicStarted && tutorialObject != null)
        {
            Transform cardRoot = tutorialObject.transform.Find("CoachingCardRoot");
            bool tutorialDismissed = cardRoot != null
                ? !cardRoot.gameObject.activeInHierarchy   // CoachingCardRoot was hidden
                : !tutorialObject.activeInHierarchy;       // fallback: root itself hidden

            if (tutorialDismissed)
            {
                musicStarted = true;
                Debug.Log("[BadPieceManager] Tutorial dismissed — starting bad-phase music.");

                if (badPhaseMusic == null)
                    Debug.LogError("[BadPieceManager] badPhaseMusic is NULL — assign it in the Inspector.");
                else
                    StartCoroutine(FadeInMusic(audioSourceA, badPhaseMusic, musicCrossfadeDuration));
            }
        }

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

            s.hoverTimer += Time.deltaTime;
            float yOffset = Mathf.Sin(s.hoverTimer * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
            s.obj.transform.position = s.floatPosition + Vector3.up * yOffset;

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
            if (target != null) Destroy(target);
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

    public void stopHeartbeat()
    {
        StartCoroutine(FadeOutAndStop(audioSourceB, 1f));
    }

}