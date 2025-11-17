using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class DialogoComFade : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public GameObject joystick;
    public GameObject hud;
    public GameObject fundo1;
    public GameObject fundo2;
    public GameObject textoFinal; // Objeto que contém o Text

    [Header("Caixas de Diálogo")]
    public GameObject caixaPersonagem1;
    public GameObject caixaPersonagem2;
    public Button botaoAvancar1;
    public Button botaoAvancar2;
    public AudioSource next;

    [Header("Falas")]
    public GameObject[] falas;
    public bool[] falasDoPersonagem1;

    [Header("Configuração do Diálogo Automático")]
    public float tempoAntesDeIniciar = 1f;

    [Header("Fade")]
    public CanvasGroup fadeCanvas; // Fade da tela
    public float duracaoFadeTela = 2f;
    public float duracaoFadeTexto = 1.5f;
    public float tempoTextoVisivel = 3f;
    public string cenaDestino = "Menu";

    private Text textoUI;
    private int falaAtual = 0;
    private bool dialogoAtivo = false;

    public event Action OnDialogoTerminado;

    void Start()
    {
        DesativarFalas();

        caixaPersonagem1?.SetActive(false);
        caixaPersonagem2?.SetActive(false);

        // Garante que o fade da tela comece invisível
        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;

        // Pega o componente Text e deixa invisível
        if (textoFinal != null)
        {
            textoUI = textoFinal.GetComponent<Text>();
            if (textoUI != null)
            {
                Color cor = textoUI.color;
                cor.a = 0f;
                textoUI.color = cor;
            }
            textoFinal.SetActive(false);
        }

        StartCoroutine(IniciarDialogoAutomaticamente());
    }

    IEnumerator IniciarDialogoAutomaticamente()
    {
        yield return new WaitForSeconds(tempoAntesDeIniciar);
        IniciarDialogo();
    }

    public void IniciarDialogo()
    {
        falaAtual = 0;
        dialogoAtivo = true;

        hud?.SetActive(false);
        joystick?.SetActive(false);

        AtualizarCaixaDialogo();
        MostrarFalaAtual();
    }

    public void AvancarFala()
    {
        next?.Play();
        falaAtual++;

        if (falaAtual < falas.Length)
        {
            AtualizarCaixaDialogo();
            MostrarFalaAtual();
        }
        else
        {
            // Fim do diálogo
            DesativarFalas();
            caixaPersonagem1?.SetActive(false);
            caixaPersonagem2?.SetActive(false);
            dialogoAtivo = false;

            hud?.SetActive(true);
            fundo1?.SetActive(false);
            fundo2?.SetActive(true);
            joystick?.SetActive(false);

            OnDialogoTerminado?.Invoke();

            if (fadeCanvas != null)
                StartCoroutine(FazerFadeCompleto());
        }
    }

    IEnumerator FazerFadeCompleto()
    {
        // FADE IN da tela (0 ? 1)
        float t = 0f;
        while (t < duracaoFadeTela)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t / duracaoFadeTela);
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // Espera meio segundo antes de mostrar o texto
        yield return new WaitForSeconds(0.5f);

        // FADE IN do texto (alpha 0 ? 1)
        if (textoUI != null)
        {
            textoFinal.SetActive(true);
            Color cor = textoUI.color;
            t = 0f;
            while (t < duracaoFadeTexto)
            {
                t += Time.deltaTime;
                cor.a = Mathf.Lerp(0f, 1f, t / duracaoFadeTexto);
                textoUI.color = cor;
                yield return null;
            }
            cor.a = 1f;
            textoUI.color = cor;

            // Mantém o texto visível
            yield return new WaitForSeconds(tempoTextoVisivel);

            // FADE OUT do texto (alpha 1 ? 0)
            t = 0f;
            while (t < duracaoFadeTexto)
            {
                t += Time.deltaTime;
                cor.a = Mathf.Lerp(1f, 0f, t / duracaoFadeTexto);
                textoUI.color = cor;
                yield return null;
            }
            cor.a = 0f;
            textoUI.color = cor;
            textoFinal.SetActive(false);
        }

        // Espera um pouco antes de trocar de cena
        yield return new WaitForSeconds(0.5f);

        if (!string.IsNullOrEmpty(cenaDestino))
            SceneManager.LoadScene(cenaDestino);
    }

    void AtualizarCaixaDialogo()
    {
        if (falaAtual >= falasDoPersonagem1.Length) return;

        bool personagem1Falando = falasDoPersonagem1[falaAtual];

        caixaPersonagem1?.SetActive(personagem1Falando);
        caixaPersonagem2?.SetActive(!personagem1Falando);

        botaoAvancar1?.gameObject.SetActive(personagem1Falando);
        botaoAvancar2?.gameObject.SetActive(!personagem1Falando);
    }

    void MostrarFalaAtual()
    {
        for (int i = 0; i < falas.Length; i++)
        {
            if (falas[i] != null)
                falas[i].SetActive(i == falaAtual);
        }
    }

    void DesativarFalas()
    {
        foreach (var fala in falas)
            if (fala != null) fala.SetActive(false);
    }
}
