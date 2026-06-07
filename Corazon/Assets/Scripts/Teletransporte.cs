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

    private IEnumerator PlayArrivalAudioDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        GameAudioManager.Instance?.PlayIntroAudio(arrivalAudio);
    }
}