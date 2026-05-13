using UnityEngine;

public class DesactivarAlCaer : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Si el objeto toca el suelo (tag "Suelo")
        if (collision.gameObject.CompareTag("Suelo"))
        {
            gameObject.SetActive(false);
        }
    }
}

