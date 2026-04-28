using UnityEngine;

public class Clasificacion : MonoBehaviour
{
    [Header("Objetos válidos")]
    public GameObject[] validObjects;

    [Header("Particle System")]
    public ParticleSystem particles;

    [Header("Rebote")]
    public float bounceForce = 5f;

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
        main.duration = 2f;
        main.loop = false;

        particles.Play();

        if (!IsValidObject(other.gameObject))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Dirección de rebote: desde el centro del área hacia el objeto
                Vector3 bounceDirection = (other.transform.position - transform.position).normalized;

                // Asegurarse que el objeto no esté kinematic momentáneamente
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