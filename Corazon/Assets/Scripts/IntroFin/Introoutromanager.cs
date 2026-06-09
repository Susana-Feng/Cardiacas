using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the intro and outro sequences of the experience.
/// Handles its own background music independently from GameAudioManager.
///
/// INTRO FLOW:
///   1. Scene loads ? introMusic plays + introVO plays automatically, start button visible
///   2. User presses start button ? startButtonVO plays + tutorial begins
///   3. Tutorial dismisses ? begin2Button appears + begin2VO plays
///   4. User presses begin2Button ? delay ? postTutorialMusic crossfades in + postTutorialVO plays ? nextButton appears
///   5. nextButton pressed ? music stops + outroButton appears
///
/// OUTRO FLOW:
///   1. Teletransportacion plays outroVO1 via PlayIntroAudio
///   2. outroVO2 plays after outroVO1 finishes
///   3. outroVO3 plays after outroVO2 finishes
///   4. endObject appears
/// </summary>
public class IntroOutroManager : MonoBehaviour
{
    public static IntroOutroManager Instance { get; private set; }

    [Header("Background Music")]
    [Tooltip("Plays on scene load.")]
    public AudioClip introMusic;

    [Tooltip("Crossfades in when postTutorialVO starts.")]
    public AudioClip postTutorialMusic;

    [Tooltip("How long music crossfades take (seconds).")]
    public float musicCrossfadeDuration = 1.5f;

    [Range(0f, 1f)]
    public float musicVolume = 0.8f;

    [Header("Intro Voiceovers")]
    [Tooltip("Plays automatically when the scene loads.")]
    public AudioClip introVO;

    [Tooltip("Plays when the start button is pressed, alongside the tutorial appearing.")]
    public AudioClip startButtonVO;

    [Tooltip("Plays alongside begin2Button after tutorial dismisses.")]
    public AudioClip begin2VO;

    [Tooltip("Plays after begin2Button is pressed (after delay).")]
    public AudioClip postTutorialVO;

    [Header("Intro UI")]
    [Tooltip("Visible at start, pressing it begins the tutorial.")]
    public GameObject startButton;

    [Tooltip("The tutorial object — hidden until start button is pressed.")]
    public GameObject tutorialObject;

    [Tooltip("Appears after tutorial dismisses alongside begin2VO.")]
    public GameObject begin2Button;

    [Tooltip("Appears after postTutorialVO finishes. Leads to Game 1.")]
    public GameObject nextButton;

    [Tooltip("Button that leads back to the outro scene. Hidden until nextButton is pressed.")]
    public GameObject outroButton;

    [Header("Timing")]
    [Tooltip("Delay in seconds between begin2Button press and postTutorialVO.")]
    public float begin2ToPostTutorialDelay = 1.5f;

    [Header("Outro Voiceovers")]
    [Tooltip("Plays after outroVO1 (fired by Teletransportacion).")]
    public AudioClip outroVO2;

    [Tooltip("Plays after outroVO2.")]
    public AudioClip outroVO3;

    [Tooltip("Delay between outro VOs.")]
    public float outroBetweenDelay = 1f;

    [Header("Outro UI")]
    [Tooltip("Appears after all outro VOs finish (e.g. 'End' text).")]
    public GameObject endObject;

    [Header("Outro Music")]
    public AudioClip outroMusic;

    // -------------------------------------------------------------------------

    private AudioSource musicSourceA;
    private AudioSource musicSourceB;

    private bool startButtonPressed = false;
    private bool tutorialDone = false;
    private bool outroStarted = false;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Set up two music sources for crossfading
        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceB = gameObject.AddComponent<AudioSource>();

        foreach (var src in new[] { musicSourceA, musicSourceB })
        {
            src.loop = true;
            src.playOnAwake = false;
            src.volume = 0f;
            src.spatialBlend = 0f;
        }
    }

    private void Start()
    {
        if (startButton != null) startButton.SetActive(true);
        if (tutorialObject != null) tutorialObject.SetActive(false);
        if (begin2Button != null) begin2Button.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);
        if (outroButton != null) outroButton.SetActive(false);
        if (endObject != null) endObject.SetActive(false);

        // Play intro music and VO on scene load
        if (introMusic != null)
            StartCoroutine(FadeInMusic(musicSourceA, introMusic));

        if (introVO != null)
            GameAudioManager.Instance?.PlayIntroAudio(introVO);
    }

    private void Update()
    {
        if (!startButtonPressed) return;

        if (!tutorialDone && tutorialObject != null)
        {
            Transform cardRoot = tutorialObject.transform.Find("CoachingCardRoot");
            bool dismissed = cardRoot != null
                ? !cardRoot.gameObject.activeInHierarchy
                : !tutorialObject.activeInHierarchy;

            if (dismissed)
            {
                tutorialDone = true;
                StartCoroutine(AfterTutorialSequence());
            }
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>Called by the start button's OnClick.</summary>
    public void OnStartButtonPressed()
    {
        startButtonPressed = true;
        if (startButton != null) startButton.SetActive(false);
        if (tutorialObject != null) tutorialObject.SetActive(true);

        if (startButtonVO != null)
            GameAudioManager.Instance?.PlayTutorialAudio(startButtonVO);
    }

    /// <summary>Called by the begin2 button's OnClick.</summary>
    public void OnBegin2ButtonPressed()
    {
        if (begin2Button != null) begin2Button.SetActive(false);
        StartCoroutine(PostTutorialSequence());
    }

    /// <summary>Called by the next button's OnClick (leads to Game 1).</summary>
    public void OnTeleportedToGame()
    {
        if (nextButton != null) nextButton.SetActive(false);
        if (outroButton != null) outroButton.SetActive(true);
        StartCoroutine(FadeOutAndStop(musicSourceA));
        StartCoroutine(FadeOutAndStop(musicSourceB));
    }

    // -------------------------------------------------------------------------

    private IEnumerator AfterTutorialSequence()
    {
        if (begin2Button != null) begin2Button.SetActive(true);

        if (begin2VO != null)
            GameAudioManager.Instance?.PlayTutorialAudio(begin2VO);

        yield return null;
    }

    private IEnumerator PostTutorialSequence()
    {
        yield return new WaitForSeconds(begin2ToPostTutorialDelay);

        // Crossfade to post-tutorial music
        if (postTutorialMusic != null)
            StartCoroutine(CrossfadeMusic(musicSourceA, musicSourceB, postTutorialMusic));

        if (postTutorialVO != null)
        {
            GameAudioManager.Instance?.PlayTutorialAudio(postTutorialVO);
            yield return new WaitForSeconds(postTutorialVO.length);
        }

        if (nextButton != null)
            nextButton.SetActive(true);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Call from Teletransportacion when the player arrives for the outro.
    /// outroVO1 is already handled by Teletransportacion.PlayIntroAudio —
    /// pass its duration so this picks up right after.
    /// </summary>
    public void StartOutro(float outroVO1Duration)
    {
        if (outroStarted) return;
        outroStarted = true;
        StartCoroutine(OutroSequence(outroVO1Duration));
    }

    private IEnumerator OutroSequence(float outroVO1Duration)
    {
        if (outroMusic != null)
            StartCoroutine(FadeInMusic(musicSourceA, outroMusic));

        yield return new WaitForSeconds(outroVO1Duration + outroBetweenDelay);

        if (outroVO2 != null)
        {
            GameAudioManager.Instance?.PlayTutorialAudio(outroVO2);
            yield return new WaitForSeconds(outroVO2.length + outroBetweenDelay);
        }

        if (outroVO3 != null)
        {
            GameAudioManager.Instance?.PlayTutorialAudio(outroVO3);
            yield return new WaitForSeconds(outroVO3.length);
        }

        if (endObject != null)
            endObject.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Music helpers
    // -------------------------------------------------------------------------

    private IEnumerator FadeInMusic(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.volume = 0f;
        source.Play();

        float elapsed = 0f;
        while (elapsed < musicCrossfadeDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, musicVolume, elapsed / musicCrossfadeDuration);
            yield return null;
        }
        source.volume = musicVolume;
    }

    private IEnumerator CrossfadeMusic(AudioSource fadeOut, AudioSource fadeIn, AudioClip newClip)
    {
        float startVolume = fadeOut.volume;

        fadeIn.clip = newClip;
        fadeIn.volume = 0f;
        fadeIn.Play();

        float elapsed = 0f;
        while (elapsed < musicCrossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / musicCrossfadeDuration;
            fadeOut.volume = Mathf.Lerp(startVolume, 0f, t);
            fadeIn.volume = Mathf.Lerp(0f, musicVolume, t);
            yield return null;
        }

        fadeOut.volume = 0f;
        fadeOut.Stop();
        fadeIn.volume = musicVolume;
    }

    private IEnumerator FadeOutAndStop(AudioSource source)
    {
        if (source == null || !source.isPlaying) yield break;
        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < musicCrossfadeDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicCrossfadeDuration);
            yield return null;
        }
        source.Stop();
        source.volume = 0f;
    }
}