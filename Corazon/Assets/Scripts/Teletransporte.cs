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

            // Calcular la diferencia de rotacion entre la camara actual y el eje X del destino
            float camaraYaw = PlayerCamera.transform.eulerAngles.y;
            float destinoYaw = Target.eulerAngles.y; // el eje X local apunta segun esta rotacion
            float deltaYaw = destinoYaw - camaraYaw;

            // Aplicar la diferencia al XR Origin para que la camara quede mirando hacia el eje X del destino
            ThePlayer.transform.Rotate(0, deltaYaw, 0, Space.World);
        }
    }
}