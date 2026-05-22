using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class LanzadorTutorial : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 5f;
    public float intervalo = 1f;

    [Header("Altura de lanzamiento")]
    public float alturaMaxima = 3f; // hasta donde sube el objeto

    [Header("Objeto 1 - Se congela al bajar")]
    public float alturaCongeladoObjeto1 = 1f; // donde se congela al caer

    [Header("Objeto 2 - Se congela al bajar y luego cae")]
    public float offsetEsperaObjeto2 = 1f;
    public float alturaCongeladoObjeto2 = 1f; // donde se congela al caer
    public float tiempoCongeladoObjeto2 = 3f;

    [Header("Objetos a lanzar")]
    public GameObject[] objetos;

    [Header("Componentes a activar cuando el objeto 2 se desactive")]
    public GameObject[] componentesAActivar;

    [Header("Efecto de confetti")]
    public GameObject confettiEffect;

    private Vector3[] posicionesIniciales;
    private bool objeto1Desactivado = false;

    void Start()
    {
        posicionesIniciales = new Vector3[objetos.Length];
        for (int i = 0; i < objetos.Length; i++)
        {
            posicionesIniciales[i] = objetos[i].transform.position;
        }

        foreach (GameObject componente in componentesAActivar)
        {
            if (componente != null)
                componente.SetActive(false);
        }
    }

    public void Relanzar()
    {
        objeto1Desactivado = false;
        StartCoroutine(LanzarTodos());
    }

    private IEnumerator LanzarTodos()
    {
        // --- OBJETO 1 ---
        if (objetos.Length > 0 && objetos[0] != null)
        {
            GameObject obj1 = objetos[0];
            Rigidbody rb1 = obj1.GetComponent<Rigidbody>();

            if (rb1 != null)
            {
                obj1.SetActive(true);
                obj1.transform.position = posicionesIniciales[0];
                rb1.isKinematic = false;
                rb1.useGravity = true;
                rb1.linearVelocity = Vector3.zero;
                rb1.angularVelocity = Vector3.zero;

                float fuerza1 = Mathf.Sqrt(2f * Physics.gravity.magnitude * alturaMaxima);
                rb1.AddForce(Vector3.up * fuerza1, ForceMode.VelocityChange);

                yield return StartCoroutine(SubirYCongelarAlBajar(obj1, rb1, alturaCongeladoObjeto1));
            }
        }

        // --- ESPERAR A QUE EL OBJETO 1 SEA DESACTIVADO ---
        yield return new WaitUntil(() => objeto1Desactivado);
        Debug.Log("Objeto 1 desactivado, esperando offset...");

        // --- OFFSET ANTES DE LANZAR OBJETO 2 ---
        yield return new WaitForSeconds(offsetEsperaObjeto2);

        // --- OBJETO 2 ---
        if (objetos.Length > 1 && objetos[1] != null)
        {
            GameObject obj2 = objetos[1];
            Rigidbody rb2 = obj2.GetComponent<Rigidbody>();

            if (rb2 != null)
            {
                obj2.SetActive(true);
                obj2.transform.position = posicionesIniciales[1];
                rb2.isKinematic = false;
                rb2.useGravity = true;
                rb2.linearVelocity = Vector3.zero;
                rb2.angularVelocity = Vector3.zero;

                float fuerza2 = Mathf.Sqrt(2f * Physics.gravity.magnitude * alturaMaxima);
                rb2.AddForce(Vector3.up * fuerza2, ForceMode.VelocityChange);

                yield return StartCoroutine(SubirYCongelarAlBajar(obj2, rb2, alturaCongeladoObjeto2));

                Debug.Log($"Objeto 2 congelado, esperando {tiempoCongeladoObjeto2} segundos...");
                yield return new WaitForSeconds(tiempoCongeladoObjeto2);

                // Descongelar y dejar caer
                rb2.isKinematic = false;
                rb2.useGravity = true;
                Debug.Log("Objeto 2 cayendo...");

                yield return new WaitUntil(() => !obj2.activeSelf);
                Debug.Log("Objeto 2 desactivado, activando componentes...");

                ActivarComponentes();
            }
        }
    }

    private IEnumerator SubirYCongelarAlBajar(GameObject obj, Rigidbody rb, float alturaCongelado)
    {
        float alturaInicial = obj.transform.position.y;
        float alturaCongeladoMundo = alturaInicial + alturaCongelado;

        // --- PASO 1: Esperar a que empiece a caer (velocidad vertical negativa) ---
        yield return new WaitUntil(() => rb.linearVelocity.y < 0 || !obj.activeSelf);

        if (!obj.activeSelf) yield break;

        Debug.Log($"{obj.name} en pico, comenzando a bajar...");

        // --- PASO 2: Esperar a que baje hasta la altura de congelado ---
        yield return new WaitUntil(() => obj.transform.position.y <= alturaCongeladoMundo || !obj.activeSelf);

        if (!obj.activeSelf) yield break;

        // --- PASO 3: Congelar ---
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        obj.transform.position = new Vector3(obj.transform.position.x, alturaCongeladoMundo, obj.transform.position.z);
        Debug.Log($"{obj.name} congelado a altura: {obj.transform.position.y}");
    }

    private void ActivarComponentes()
    {
        foreach (GameObject componente in componentesAActivar)
        {
            if (componente != null)
            {
                componente.SetActive(true);
                Debug.Log($"Componente activado: {componente.name}");
            }
        }
    }

    public void OnGrabbed(SelectEnterEventArgs args)
    {
        GameObject grabbedObject = args.interactableObject.transform.gameObject;

        if (objetos.Length > 0 && grabbedObject == objetos[0])
        {
            objeto1Desactivado = true;
        }

        grabbedObject.SetActive(false);
    }

    public void OnGrabbedCorrecto(SelectEnterEventArgs args)
    {
        GameObject grabbedObject = args.interactableObject.transform.gameObject;

        if (confettiEffect != null)
        {
            // Mover el efecto a la posición del objeto
            confettiEffect.transform.position = grabbedObject.transform.position;

            // Activar el efecto
            confettiEffect.SetActive(true);

            // Desactivarlo cuando termine la partícula
            ParticleSystem ps = confettiEffect.GetComponent<ParticleSystem>();
            if (ps != null)
                StartCoroutine(DesactivarEfectoConfetti(ps.main.duration));
        }

        grabbedObject.SetActive(false);
    }

    private IEnumerator DesactivarEfectoConfetti(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        confettiEffect.SetActive(false);
    }
}
