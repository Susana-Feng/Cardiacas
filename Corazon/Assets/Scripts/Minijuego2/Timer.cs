using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textoContador;

    [Header("SFX Fin de Partida")]
    public AudioSource SFX_Fin_Partida; // <-- Asignar en el Inspector

    [Header("LOC Fin de Partida")]
    public AudioSource LOC_Fin_Partida;

    [Header("Ir al Minijuego 3")]
    [SerializeField]
    GameObject irMinijuego3;

    [SerializeField] private QuadImageSlideshow slideshow;
    

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
        GameManager2.Instance.gameWon = true;
        slideshow.StartSlideshow();
        StartCoroutine(FinPartida());
    }

    private IEnumerator FinPartida()
    {
        if (SFX_Fin_Partida != null)
        {
            SFX_Fin_Partida.Play();

            yield return new WaitForSeconds(SFX_Fin_Partida.clip.length);
        }

        if (LOC_Fin_Partida != null)
        {
            LOC_Fin_Partida.Play();

            yield return new WaitForSeconds(LOC_Fin_Partida.clip.length);
            
            if (irMinijuego3 != null)
            {
                irMinijuego3.SetActive(true);
            }
        }
    }
}
