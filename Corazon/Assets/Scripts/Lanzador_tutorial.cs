using UnityEngine;

public class LanzadorTutorial : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 5f;
    public float intervalo = 1f;

    [Header("Objetos a lanzar")]
    public GameObject[] objetos; // lista de objetos

    private Vector3[] posicionesIniciales; // posiciones originales
    private int indiceActual = 0;
    private float timer = 0f;

    void Start()
    {
        // Guardar la posición inicial de cada objeto
        posicionesIniciales = new Vector3[objetos.Length];
        for (int i = 0; i < objetos.Length; i++)
        {
            posicionesIniciales[i] = objetos[i].transform.position;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= intervalo && objetos.Length > 0)
        {
            timer = 0f;
            Relanzar();
        }
    }

    void Relanzar()
    {
        GameObject objeto = objetos[indiceActual];
        Rigidbody rb = objeto.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Resetear posición a la original
            objeto.transform.position = posicionesIniciales[indiceActual];
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Lanzar hacia arriba
            rb.AddForce(Vector3.up * velocidad, ForceMode.VelocityChange);
        }

        // Avanzar al siguiente objeto en la lista
        indiceActual = (indiceActual + 1) % objetos.Length;
    }
}
