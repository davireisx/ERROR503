using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniIntro : MonoBehaviour
{
    [System.Serializable]
    public class TextoComTempo
    {
        public GameObject texto;
        public float tempoAtivo = 2f;
    }

    [Header("Elementos Visuais")]
    public GameObject imagemFundo;
    public GameObject blur;

    [Header("Lista de Textos (com tempos individuais)")]
    public List<TextoComTempo> textosSequencia = new List<TextoComTempo>();

    [Header("Fade")]
    public CanvasGroup fadeCanvas; // CanvasGroup do fade
    public float tempoFade = 1.5f;
    public float atrasoFade = 0.5f; // tempo antes de começar o fade
    public float esperaEntreFades = 0.5f; // tempo que o fade fica totalmente visível antes de sumir

    [Header("Tempos Gerais")]
    public float tempoInicial = 1.5f;

    void Start()
    {
        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;

        StartCoroutine(SequenciaIntro());
    }

    IEnumerator SequenciaIntro()
    {
        imagemFundo.SetActive(true);
        blur.SetActive(true);

        yield return new WaitForSeconds(tempoInicial);

        // Mostra os textos um por um
        for (int i = 0; i < textosSequencia.Count; i++)
        {
            textosSequencia[i].texto.SetActive(true);

            if (i > 0)
                textosSequencia[i - 1].texto.SetActive(false);

            yield return new WaitForSeconds(textosSequencia[i].tempoAtivo);
        }

        // Desativa o último texto e faz o fade in/out
        if (textosSequencia.Count > 0)
        {
            textosSequencia[textosSequencia.Count - 1].texto.SetActive(false);
            yield return StartCoroutine(FazerFadeInOut());
        }

        Debug.Log("Sequência de introdução finalizada!");
    }

    IEnumerator FazerFadeInOut()
    {
        yield return new WaitForSeconds(atrasoFade);

        float t = 0f;

        // FADE IN (0 ? 1)
        while (t < tempoFade)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t / tempoFade);
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // Espera um pouco com o fade totalmente ativo
        yield return new WaitForSeconds(esperaEntreFades);
        imagemFundo.SetActive(false);
        blur.SetActive(false);
        // FADE OUT (1 ? 0)
        t = 0f;
        while (t < tempoFade)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, t / tempoFade);
            yield return null;
        }
        fadeCanvas.alpha = 0f;
    }
}
