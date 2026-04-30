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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (particles == null) return;

        bool valid = IsValidObject(other.gameObject);

        Color particleColor = valid ? Color.green : Color.red;
        var main = particles.main;
        main.startColor = particleColor;
        main.loop = false;
        particles.Play();

        if (valid)
        {
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

    private bool IsValidObject(GameObject obj)
    {
        foreach (GameObject validObj in validObjects)
        {
            if (validObj == obj) return true;
        }
        return false;
    }
}