using UnityEngine;

public class Teletransportacion : MonoBehaviour
{
    public Transform Target;
    public GameObject ThePlayer;
    public Camera PlayerCamera; // asigna la XR Camera desde el Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ThePlayer)
        {
            // Calcular el offset entre el XR Origin y la camara real
            Vector3 cameraOffset = PlayerCamera.transform.position - ThePlayer.transform.position;
            cameraOffset.y = 0; // ignorar diferencia vertical

            // Restar el offset para que la camara quede exactamente en el Target
            ThePlayer.transform.position = Target.position - cameraOffset;
        }
    }
}