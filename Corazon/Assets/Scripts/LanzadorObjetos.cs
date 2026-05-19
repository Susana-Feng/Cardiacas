using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class LanzadorObjetos : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 5f;
    public float intervalo = 1f;

    [Header("Altura de lanzamiento")]
    public float altura = 1f; // configurable desde el inspector

    [Header("Objetos a lanzar")]
    public GameObject[] objetos; // lista de objetos

    [Header("Prefab de explosión")]
    public GameObject explosionPrefab; // asigna aquí tu efecto de partículas

    private Vector3[] posicionesIniciales; // posiciones originales
    private int indiceActual = 0;

    void Start()
    {
        // Guardar la posición inicial de cada objeto
        posicionesIniciales = new Vector3[objetos.Length];
        for (int i = 0; i < objetos.Length; i++)
        {
            posicionesIniciales[i] = objetos[i].transform.position;
            //objetos[i].SetActive(false); // desactivar al inicio si quieres
        }
    }

    // Método público que puedes llamar desde un botón
    public void Relanzar()
    {
        // Inicia la corutina que lanza todos los objetos uno por uno
        StartCoroutine(LanzarTodos());
    }

    private IEnumerator LanzarTodos()
    {
        // Recorre todos los objetos en la lista
        for (int i = 0; i < objetos.Length; i++)
        {
            GameObject objeto = objetos[i];
            if (objeto == null) continue;

            Rigidbody rb = objeto.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Activar el objeto y resetear posición
                objeto.SetActive(true);
                objeto.transform.position = posicionesIniciales[i];
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // Lanzar hacia arriba hasta la altura deseada
                float fuerza = Mathf.Sqrt(2f * Physics.gravity.magnitude * altura);
                rb.AddForce(Vector3.up * fuerza, ForceMode.VelocityChange);
            }

            // Espera el intervalo antes de lanzar el siguiente
            yield return new WaitForSeconds(intervalo);
        }
    }

    // Método para cuando el objeto sea agarrado
    public void OnGrabbed(SelectEnterEventArgs args)
    {
        GameObject grabbedObject = args.interactableObject.transform.gameObject;

        // En vez de destruir, lo desactivamos
        grabbedObject.SetActive(false);
    }

    public void OnGrabbedExplosion(SelectEnterEventArgs args)
    {
        GameObject grabbedObject = args.interactableObject.transform.gameObject;

        // Instanciar la explosión en la posición del objeto
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, grabbedObject.transform.position, Quaternion.identity);
        }

        // Desactivar el objeto
        grabbedObject.SetActive(false);
    }
}