using UnityEngine;

public class LanzadorObjetos : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 5f;
    public float intervalo = 1f;

    private Rigidbody rb;
    private Vector3 posicionInicial;
    private float timer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        posicionInicial = transform.position;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= intervalo)
        {
            timer = 0f;
            Relanzar();
        }
    }

    void Relanzar()
    {
        // Resetear posición y velocidad
        transform.position = posicionInicial;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Lanzar hacia arriba
        rb.AddForce(Vector3.up * velocidad, ForceMode.VelocityChange);
    }
}
