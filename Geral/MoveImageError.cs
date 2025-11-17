using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MoverImagemErrorFlex : MonoBehaviour
{
    [Header("Referências")]
    public GameObject error;
    public GameObject continuar;
    public GameObject logo;

    [Header("Alvo do movimento (opcional)")]
    public GameObject alvoParaMover;

    [Header("Configuração do movimento")]
    public float duracao = 3f;
    public float velocidade = 3f;
    public float amplitude = 20f;
    public float tempoTransicaoFinal = 0.6f;

    private RectTransform alvoRect;
    private Vector2 posInicialRect;
    private Transform alvoTrans;
    private Vector3 posInicialTrans;
    private bool usandoRect = false;
    private Button botaoLogo;
    private Coroutine coroutineMovimento;
    private bool movimentoContinuo = false;
    private bool podeMover = false;

    void OnEnable()
    {
        // Identifica o alvo do movimento
        GameObject alvo = alvoParaMover != null ? alvoParaMover : this.gameObject;
        alvoRect = alvo.GetComponent<RectTransform>();

        if (alvoRect != null)
        {
            usandoRect = true;
            posInicialRect = alvoRect.anchoredPosition;
        }
        else
        {
            usandoRect = false;
            alvoTrans = alvo.transform;
            posInicialTrans = alvoTrans.localPosition;
        }

        // Configura o botão mas DESABILITA O COMPONENTE BUTTON no início
        if (logo != null)
        {
            botaoLogo = logo.GetComponent<Button>();
            if (botaoLogo == null)
                botaoLogo = logo.GetComponentInChildren<Button>();

            if (botaoLogo != null)
            {
                botaoLogo.enabled = false;
                botaoLogo.onClick.RemoveAllListeners();
                botaoLogo.onClick.AddListener(OnLogoClick);
            }
        }

        movimentoContinuo = false;
        podeMover = true;
        coroutineMovimento = StartCoroutine(MoverPorTempo());
    }

    IEnumerator MoverPorTempo()
    {
        float tempo = 0f;

        // Movimento sobe/desce por 3s
        while (tempo < duracao)
        {
            if (!podeMover) yield break;

            float deslocamento = Mathf.Sin(Time.time * velocidade) * amplitude;

            if (usandoRect)
                alvoRect.anchoredPosition = posInicialRect + new Vector2(0, deslocamento);
            else
                alvoTrans.localPosition = posInicialTrans + new Vector3(0, deslocamento, 0);

            tempo += Time.deltaTime;
            yield return null;
        }

        // Finaliza e inicia a nova fase
        FinalizarAnimacao();
    }

    void FinalizarAnimacao()
    {
        if (error != null)
            error.SetActive(false);
        if (continuar != null)
            continuar.SetActive(true);

        // Habilita o botão
        if (botaoLogo != null)
            botaoLogo.enabled = true;

        // Assim que continuar for ativado, começa movimento a partir da posição atual
        StartCoroutine(ReiniciarMovimentoSuavemente());
    }

    IEnumerator ReiniciarMovimentoSuavemente()
    {
        // Captura posição atual
        Vector2 posAtualRect = usandoRect ? alvoRect.anchoredPosition : Vector2.zero;
        Vector3 posAtualTrans = usandoRect ? Vector3.zero : alvoTrans.localPosition;

        float tempo = 0f;
        float deslocamentoAtual = usandoRect ? posAtualRect.y - posInicialRect.y : posAtualTrans.y - posInicialTrans.y;

        // Transição suave para continuar o movimento
        while (tempo < tempoTransicaoFinal)
        {
            float deslocamento = Mathf.Lerp(deslocamentoAtual, amplitude, tempo / tempoTransicaoFinal);
            if (usandoRect)
                alvoRect.anchoredPosition = posInicialRect + new Vector2(0, deslocamento);
            else
                alvoTrans.localPosition = posInicialTrans + new Vector3(0, deslocamento, 0);

            tempo += Time.deltaTime;
            yield return null;
        }

        // Inicia movimento contínuo partindo de onde parou
        movimentoContinuo = true;
        StartCoroutine(MovimentoContinuo());
    }

    IEnumerator MovimentoContinuo()
    {
        // Parte da posição atual (sem pular)
        float tempo = 0f;

        // Calcula a fase inicial baseado na posição atual
        float deslocamentoAtual = usandoRect ? alvoRect.anchoredPosition.y - posInicialRect.y : alvoTrans.localPosition.y - posInicialTrans.y;
        float faseInicial = Mathf.Asin(Mathf.Clamp(deslocamentoAtual / amplitude, -1f, 1f));

        while (movimentoContinuo)
        {
            tempo += Time.deltaTime * velocidade;
            float deslocamento = Mathf.Sin(tempo + faseInicial) * amplitude;

            if (usandoRect)
                alvoRect.anchoredPosition = posInicialRect + new Vector2(0, deslocamento);
            else
                alvoTrans.localPosition = posInicialTrans + new Vector3(0, deslocamento, 0);

            yield return null;
        }
    }

    void OnLogoClick()
    {
        movimentoContinuo = false;
        podeMover = false;

        if (coroutineMovimento != null)
        {
            StopCoroutine(coroutineMovimento);
            coroutineMovimento = null;
        }

        MudarCorContinuar();
    }

    void MudarCorContinuar()
    {
        if (continuar == null) return;

        if (ColorUtility.TryParseHtmlString("#E06D69", out Color novaCor))
        {
            Image img = continuar.GetComponent<Image>();
            if (img != null)
            {
                img.color = novaCor;
                return;
            }

            Text txt = continuar.GetComponent<Text>();
            if (txt != null)
                txt.color = novaCor;
        }
    }

    void OnDisable()
    {
        movimentoContinuo = false;
        podeMover = false;
        if (coroutineMovimento != null)
        {
            StopCoroutine(coroutineMovimento);
            coroutineMovimento = null;
        }
    }
}
