using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialIntroducao : MonoBehaviour
{
    [Header("Elementos iniciais")]
    public GameObject fundoTotal; // Painel com CanvasGroup
    public CanvasGroup fadeCanvas; // ?? CanvasGroup do fundoTotal (para o fade)
    public GameObject HUD;
    public GameObject joystick;
    public GameObject dialogoAutoma;
    public AudioSource audioPular;

    [Header("Elementos intermediários")]
    public GameObject[] textos; // textos mostrados um a um

    [Header("Tempo de exibição por fala (ajustável no Inspector)")]
    [Tooltip("Cada valor corresponde a uma fala do array de textos.")]
    public float[] tempoPorFala = { 2.0f, 2.5f, 3f, 3.5f, 4.5f, 3.0f };

    [Header("Fundos e botões extras")]
    public GameObject fundo0;
    public GameObject fundo1;
    public GameObject checkFundo1;
    public Button botaoPular;

    [Header("Tempo inicial antes do tutorial começar")]
    public float tempoInicial = 2f;

    [Header("Configuração do Fade")]
    public float duracaoFadeInicial = 1.5f; // ?? Tempo do fade de 1 ? 0 no início

    private bool pulou = false;

    void Start()
    {
        if (tempoPorFala.Length < textos.Length)
        {
            Debug.LogWarning("? O array tempoPorFala tem menos valores que o número de textos. Usando 2s como fallback.");
        }

        // ?? Garante que o fadeCanvas comece visível (alpha 1)
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1f;
        }
        else
        {
            Debug.LogError("? Nenhum CanvasGroup atribuído em 'fadeCanvas'! Atribua o CanvasGroup do fundoTotal no Inspector.");
        }

        StartCoroutine(SequenciaIntro());
    }

    IEnumerator SequenciaIntro()
    {
        fundoTotal.SetActive(true);
        HUD.SetActive(false);
        joystick.SetActive(false);

        // ?? Executa o fade de 1 ? 0 no início
        yield return StartCoroutine(FazerFadeInicial());

        yield return new WaitForSeconds(tempoInicial);

        // Mostra os textos em sequência
        for (int i = 0; i < textos.Length; i++)
        {
            textos[i].SetActive(true);

            if (i == 2)
            {
                HUD.SetActive(true);
            }

            if (i == textos.Length - 1)
            {
                fundo0.SetActive(false);
                fundo1.SetActive(true);
            }

            float tempo = (i < tempoPorFala.Length) ? tempoPorFala[i] : 2f;
            yield return new WaitForSeconds(tempo);

            if (i < textos.Length - 1)
            {
                textos[i].SetActive(false);
            }

            if (pulou) yield break;
        }

        Debug.Log("? Tutorial finalizado (aguardando botão).");
    }

    IEnumerator FazerFadeInicial()
    {
        if (fadeCanvas == null)
            yield break;

        float tempo = 0f;
        while (tempo < duracaoFadeInicial)
        {
            tempo += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, tempo / duracaoFadeInicial);
            yield return null;
        }

        fadeCanvas.alpha = 0f; // Garante final exato
    }

    public void BotaoPularOnClick()
    {
        if (!pulou)
        {
            audioPular.Play();
            pulou = true;
            StartCoroutine(PularCoroutine());
        }
    }

    IEnumerator PularCoroutine()
    {
        checkFundo1.SetActive(true);
        yield return new WaitForSeconds(2f);

        HUD.SetActive(false);
        dialogoAutoma.SetActive(true);
        fundoTotal.SetActive(false);

        Debug.Log("?? Tutorial encerrado pelo botão.");
    }
}
