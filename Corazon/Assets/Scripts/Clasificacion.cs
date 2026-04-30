using UnityEngine;

public class Clasificacion : MonoBehaviour
{
    [Header("Objetos válidos")]
    public GameObject[] validObjects;

    [Header("Particle System")]
    public ParticleSystem particles;

    [Header("Rebote")]
    public float bounceForce = 6f;

    [Header("Destrucción")]
    public float destroyDelay = 0.5f;

    private void Awake()
    {
        if (particles == null)
            particles = GetComponent<ParticleSystem>();

        if (particles != null)
            particles.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (particles == null) return;

        Color particleColor = IsValidObject(other.gameObject) ? Color.green : Color.red;

        var main = particles.main;
        main.startColor = particleColor;
        //main.duration = 2f;
        main.loop = false;

        particles.Play();

        if (IsValidObject(other.gameObject))
        {
            Destroy(other.gameObject, destroyDelay);
        }
        else
        {
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

    private void OnTriggerExit(Collider other)
    {
        // Las partículas duran 2 segundos y se apagan solas
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
