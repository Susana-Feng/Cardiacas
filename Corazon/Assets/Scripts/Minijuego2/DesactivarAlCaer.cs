using UnityEngine;

public class DesactivarAlCaer : MonoBehaviour
{
    [Header("SFX Caida")]
    public AudioSource SFX_Caida_Source;
    [SerializeField]
    public AudioClip SFX_Caida;

    private void OnCollisionEnter(Collision collision)
    {
        // Si el objeto toca el suelo (tag "Suelo")
        if (collision.gameObject.CompareTag("Suelo"))
        {
            if (gameObject.activeSelf == true) {
                gameObject.SetActive(false);
                SFX_Caida_Source.Play();
            }
        }
    }
}

