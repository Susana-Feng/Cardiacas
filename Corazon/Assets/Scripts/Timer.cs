using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textoContador;

    private float tiempoRestante;
    private bool corriendo = false;

    public void IniciarContador(float tiempo)
    {
        tiempoRestante = tiempo;
        ActualizarTexto();
        corriendo = true;
        StartCoroutine(Contar());
    }

    public void DetenerContador()
    {
        corriendo = false;
        StopCoroutine(Contar());
    }

    private IEnumerator Contar()
    {
        while (tiempoRestante > 0 && corriendo)
        {
            tiempoRestante -= Time.deltaTime;
            tiempoRestante = Mathf.Max(tiempoRestante, 0);
            ActualizarTexto();
            yield return null;
        }

        if (tiempoRestante <= 0)
        {
            ActualizarTexto();
            OnTiempoAgotado();
        }
    }

    private void ActualizarTexto()
    {
        int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);
        textoContador.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    private void OnTiempoAgotado()
    {
        Debug.Log("Tiempo agotado!");
    }
}
