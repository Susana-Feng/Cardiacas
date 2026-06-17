using UnityEngine;
using System.Collections;

public class Teletransportacion : MonoBehaviour
{
    public Transform Target;
    public GameObject ThePlayer;
    public Camera PlayerCamera;

    [Header("Audio")]
    [Tooltip("Plays when the player enters this portal. Leave empty for no audio.")]
    public AudioClip arrivalAudio;

    [Header("Outro")]
    [Tooltip("If this portal leads to the outro scene, assign the outroVO1 clip here so the manager knows its duration.")]
    public AudioClip outroVO1;

    [Header("Heart Room")]
    [Tooltip("Check this on the portal that leads INTO the heart's room.")]
    public bool resumeHeartbeatOnArrival = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ThePlayer)
        {
            Vector3 cameraOffset = PlayerCamera.transform.position - ThePlayer.transform.position;
            cameraOffset.y = 0;
            ThePlayer.transform.position = Target.position - cameraOffset;
            float camaraYaw = PlayerCamera.transform.eulerAngles.y;
            float destinoYaw = Target.eulerAngles.y;
            float deltaYaw = destinoYaw - camaraYaw;
            ThePlayer.transform.Rotate(0, deltaYaw, 0, Space.World);

            if (arrivalAudio != null)
                StartCoroutine(PlayArrivalAudioDelayed());
        }
    }
    public void Teleport()
    {
        Vector3 cameraOffset = PlayerCamera.transform.position - ThePlayer.transform.position;
        cameraOffset.y = 0;

        Vector3 newPosition = Target.position - cameraOffset;
        newPosition.y = ThePlayer.transform.position.y; // ? preserve current height
        ThePlayer.transform.position = newPosition;

        float camaraYaw = PlayerCamera.transform.eulerAngles.y;
        float destinoYaw = Target.eulerAngles.y;
        float deltaYaw = destinoYaw - camaraYaw;
        ThePlayer.transform.Rotate(0, deltaYaw, 0, Space.World);


        GameAudioManager.Instance?.StopAll();
        IntroOutroManager.Instance?.OnTeleportedToGame();

        if (resumeHeartbeatOnArrival)
            HeartBeat.Instance?.Resume();

        if (arrivalAudio != null)
            StartCoroutine(PlayArrivalAudioDelayed());
        if (outroVO1 != null)
            IntroOutroManager.Instance?.StartOutro(outroVO1.length);
    }
    public IEnumerator PlayArrivalAudioDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        GameAudioManager.Instance?.PlayIntroAudio(arrivalAudio);
    }
}