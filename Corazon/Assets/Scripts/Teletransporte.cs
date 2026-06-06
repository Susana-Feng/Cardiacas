using UnityEngine;

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
            // Calcular el offset entre el XR Origin y la camara real
            Vector3 cameraOffset = PlayerCamera.transform.position - ThePlayer.transform.position;
            cameraOffset.y = 0;

            ThePlayer.transform.position = Target.position - cameraOffset;

            float camaraYaw = PlayerCamera.transform.eulerAngles.y;
            float destinoYaw = Target.eulerAngles.y;
            float deltaYaw = destinoYaw - camaraYaw;

            ThePlayer.transform.Rotate(0, deltaYaw, 0, Space.World);

            // Play this portal's arrival audio
            if (arrivalAudio != null)
                GameAudioManager.Instance?.PlayIntroAudio(arrivalAudio);
        }
    }
}