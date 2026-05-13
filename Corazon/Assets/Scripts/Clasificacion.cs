using UnityEngine;

public class Clasificacion : MonoBehaviour
{
    [Header("Objetos válidos")]
    public GameObject[] validObjects;

    [Header("Particle System")]
    public ParticleSystem particles;

    [Header("Sonidos")]
    public AudioSource audioSource; // fuente de audio
    public AudioClip validSound;    // sonido para objeto correcto
    public AudioClip invalidSound;  // sonido para objeto incorrecto

    [Header("Efecto de luz")]
    public Light pointLight;        // referencia al Point Light
    public float lightDuration = 1f; // tiempo que dura encendido
    public Color validLightColor = Color.yellow; // color para objeto válido
    public Color invalidLightColor = Color.red;  // color para objeto inválido

    [Header("Rebote")]
    public float bounceForce = 6f;

    [Header("Destrucción")]
    public float destroyDelay = 0.5f;

    [Header("Win Condition")]
    public int requiredCount = 3;

    private int validCount = 0;
    private bool isFull = false;

    private void Awake()
    {
        if (particles == null)
            particles = GetComponent<ParticleSystem>();
        if (particles != null)
            particles.Stop();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (pointLight != null)
            pointLight.gameObject.SetActive(false); // luz apagada al inicio
    }

    private void OnTriggerEnter(Collider other)
    {
        bool valid = IsValidObject(other.gameObject);

        if (valid)
        {
            // Partículas solo para objetos válidos
            if (particles != null)
            {
                var main = particles.main;
                main.loop = false;
                particles.Play();
            }

            // Sonido de objeto válido
            if (audioSource != null && validSound != null)
                audioSource.PlayOneShot(validSound);

            // Luz de objeto válido (amarillo configurable)
            if (pointLight != null)
            {
                pointLight.color = validLightColor;
                pointLight.gameObject.SetActive(true);
                CancelInvoke(nameof(DisableLight));
                Invoke(nameof(DisableLight), lightDuration);
            }

            validCount++;

            // Check if this container just became full
            if (!isFull && validCount >= requiredCount)
            {
                isFull = true;
                GameManager.Instance.OnContainerFilled();
            }

            Destroy(other.gameObject, destroyDelay);
        }
        else
        {
            // Sonido de objeto inválido
            if (audioSource != null && invalidSound != null)
                audioSource.PlayOneShot(invalidSound);

            // Luz de objeto inválido (rojo configurable)
            if (pointLight != null)
            {
                pointLight.color = invalidLightColor;
                pointLight.gameObject.SetActive(true);
                CancelInvoke(nameof(DisableLight));
                Invoke(nameof(DisableLight), lightDuration);
            }

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 bounceDirection = (other.transform.position - transform.position).normalized;
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
            }
        }
    }

    private void DisableLight()
    {
        if (pointLight != null)
            pointLight.gameObject.SetActive(false);
    }

    private bool IsValidObject(GameObject obj)
    {
        foreach (GameObject validObj in validObjects)
        {
            if (validObj == obj) return true;
        }
        return false;
    }
}

