using UnityEngine;
using System.Collections.Generic;

public class LanzadorObjetos : MonoBehaviour
{
    [Header("Configuración de lanzamiento")]
    public List<GameObject> objetos;       // Lista de objetos a lanzar
    public float intervalo = 2f;           // Tiempo fijo entre lanzamientos
    public float velocidadLanzamiento = 5f; // Velocidad inicial hacia arriba
    public float alturaMaxima = 5f;        // Altura máxima (para referencia visual)

    private int indiceActual = 0;
    private float tiempoUltimoLanzamiento;

    void Update()
    {
        // Lanzar objetos en intervalos fijos
        if (Time.time - tiempoUltimoLanzamiento >= intervalo && objetos.Count > 0)
        {
            LanzarObjeto();
            tiempoUltimoLanzamiento = Time.time;
        }

        // Control de visibilidad según posición Y
        foreach (var obj in objetos)
        {
            if (obj != null)
            {
                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null)
                {
                    if (obj.transform.position.y <= 0.2f)
                        rend.enabled = false; // invisible
                    else
                        rend.enabled = true;  // visible
                }
            }
        }
    }

    void LanzarObjeto()
    {
        GameObject obj = objetos[indiceActual];
        if (obj != null)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Reset posición al suelo
                obj.transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);
                rb.linearVelocity = Vector3.zero;

                // Aplicar fuerza vertical
                rb.AddForce(Vector3.up * velocidadLanzamiento, ForceMode.VelocityChange);
            }
        }

        // Avanzar al siguiente objeto en la lista
        indiceActual = (indiceActual + 1) % objetos.Count;
    }

    // Método para destruir cuando el objeto es agarrado
    public void DestruirObjeto(GameObject obj)
    {
        if (objetos.Contains(obj))
        {
            objetos.Remove(obj);
            Destroy(obj);
        }
    }
}

