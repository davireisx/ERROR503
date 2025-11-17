using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Guarita : MonoBehaviour
{
    [Header("Imagens")]
    public GameObject VSCODE;
    public GameObject protocolo;
    public GameObject botaoDebuggar;
    public GameObject acertos;
    public GameObject guaritaGeral;

    [Header("DialogoFinal")]
    public GameObject dialogoFinal;
    public Dialogo dialogoController; // referência pelo Inspector


    [Header("Objetivos e Checks")]
    public GameObject hud;
    public GameObject objetivos2;
    public GameObject check2;
    public GameObject fundo1;
    public GameObject fundo2;
    public GameObject fundomonitor;
    public GameObject objetivos3;
    public GameObject check3;
    public GameObject objetivos4;
    public GameObject check4;

    [Header("Protocolo - Mecânica")]
    public List<Text> linhasBinarias;
    public Image protocoloImage;
    public Text textoFinal;
    public float velocidadeDigitacao = 0.0001f;

    [Header("Debug - Mecânica")]
    public Image debugImage;
    public Text textoDebug;
    public List<Graphic> componentesParaDesaparecer;
    public List<Graphic> componentesParaAparecer;
    public Text mensagemSucesso;

    [Header("Traps que vão piscar")]
    public List<SpriteRenderer> trapsPiscam;
    public GameObject trapCollider1;
    public GameObject trapCollider2;
    public GameObject trapCollider3;
    public GameObject trapCollider4;

    [Header("Referências")]
    public Arquiteto arquitetas;
    public Joystick joystick;
    public CanvasGroup fade;
    public AudioSource audioTeclando;
    public AudioSource audioPendrive;
    public AudioSource audioDebug;
    public AudioSource audioCodigoDesaparecendo;
    public AudioSource audioCodigoAparecendo;
    public AudioSource audioCodigoFim;
    public AudioSource audioCatracaDesativa;
    public float velocidadeFade;
    public float tempoPiscada = 0.2f;
    public float trapPiscar = 3f;
    public float intervaloPiscada = 0.2f;

    private bool iniciarFade = false;
    private bool iniciarProtocolo = false;
    private bool protocoloCompleto = false;

    void Start()
    {
        InicializarComponentes();
    }

    void InicializarComponentes()
    {
        if (protocoloImage != null)
            protocoloImage.color = new Color(protocoloImage.color.r, protocoloImage.color.g, protocoloImage.color.b, 0f);

        if (protocolo != null)
            protocolo.SetActive(false);

        foreach (Text linha in linhasBinarias)
            if (linha != null) linha.gameObject.SetActive(false);

        if (textoFinal != null) textoFinal.gameObject.SetActive(false);
        if (botaoDebuggar != null) botaoDebuggar.SetActive(false);
        if (debugImage != null) debugImage.gameObject.SetActive(false);
        if (textoDebug != null) textoDebug.gameObject.SetActive(false);

        foreach (Graphic componente in componentesParaAparecer)
            if (componente != null) componente.gameObject.SetActive(false);

        if (mensagemSucesso != null) mensagemSucesso.gameObject.SetActive(false);

        if (objetivos2 != null) objetivos2.SetActive(false);
        if (check2 != null) check2.SetActive(false);
        if (objetivos3 != null) objetivos3.SetActive(false);
        if (check3 != null) check3.SetActive(false);
    }

    public void Inicio()
    {
        joystick.gameObject.SetActive(false);

        if (fade != null)
        {
            fade.alpha = 1f;
            iniciarFade = true;
        }
    }

    void Update()
    {
        if (iniciarFade) ProcessarFade();
        if (iniciarProtocolo && protocoloImage != null) ProcessarProtocolo();
    }

    void ProcessarFade()
    {
        check2.SetActive(true);
        objetivos2.SetActive(false);
        fundo1.SetActive(false);
        fade.alpha -= Time.deltaTime * velocidadeFade;

        if (fade.alpha <= 0f)
        {
            fade.alpha = 0f;
            iniciarFade = false;
            fade.gameObject.SetActive(false);
            fundomonitor.SetActive(true);
            if (objetivos3 != null) objetivos3.SetActive(true);
        }
    }

    void ProcessarProtocolo()
    {
        StartCoroutine(AtivarComponentesHUD());
        Color currentColor = protocoloImage.color;
        float newAlpha = currentColor.a + (Time.deltaTime * velocidadeFade);
        protocoloImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

        if (protocoloImage.color.a >= 1f && !protocoloCompleto)
        {
            protocoloImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
            iniciarProtocolo = false;
            protocoloCompleto = true;
            StartCoroutine(SequenciaLinhasBinarias());
        }
    }

    public void Ativar()
    {
        audioPendrive.Play();
        Protocolo();
    }

    public void Protocolo()
    {
        if (protocoloImage != null)
        {
            if (protocolo != null) protocolo.SetActive(true);
            protocoloImage.color = new Color(protocoloImage.color.r, protocoloImage.color.g, protocoloImage.color.b, 0f);
            iniciarProtocolo = true;
        }
    }

    private IEnumerator SequenciaLinhasBinarias()
    {
        yield return StartCoroutine(DigitarLinhas());
        yield return StartCoroutine(PiscarVerde());
        yield return StartCoroutine(ManterVerde());
        yield return StartCoroutine(ProcessarTextoFinal());
        yield return StartCoroutine(FadeOutProtocolo());
    }

    private IEnumerator FadeOutProtocolo()
    {
        yield return StartCoroutine(FadeGraphic(protocoloImage, 1f, 0f, 1f));
        yield return new WaitForSeconds(1.5f);
        protocolo.SetActive(false);

        if (botaoDebuggar != null)
        {
            botaoDebuggar.SetActive(true);
            if (debugImage != null) debugImage.gameObject.SetActive(true);
            if (textoDebug != null) textoDebug.gameObject.SetActive(true);
        }
    }

    public IEnumerator AtivarComponentesHUD()
    {
        check3.SetActive(true);
        yield return new WaitForSeconds(9.5f);
        objetivos3.SetActive(false);
        objetivos4.SetActive(true);
    }

    public void AcionarDebug()
    {
        audioDebug.Play();
        StartCoroutine(SequenciaDebug());
    }

    private IEnumerator SequenciaDebug()
    {
        audioCodigoDesaparecendo.Play();
        check4.SetActive(true);

        if (botaoDebuggar != null) botaoDebuggar.SetActive(false);

        foreach (Graphic componente in componentesParaDesaparecer)
        {
            if (componente != null)
            {
                componente.gameObject.SetActive(true);
                yield return StartCoroutine(FadeGraphic(componente, 1f, 0f, 0.25f));
                componente.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.05f);
            }
        }

        audioCodigoDesaparecendo.Stop();
        yield return new WaitForSeconds(1f);

        audioCodigoAparecendo.Play();
        for (int i = 0; i < componentesParaAparecer.Count; i++)
        {
            Graphic componente = componentesParaAparecer[i];
            if (componente != null)
            {
                componente.gameObject.SetActive(true);
                float targetAlpha = (i == componentesParaAparecer.Count - 1) ? 0.25f : 1f;
                yield return StartCoroutine(FadeGraphic(componente, 0f, targetAlpha, 0.25f));
                yield return new WaitForSeconds(0.05f);
            }
        }

        audioCodigoAparecendo.Stop();
        audioCodigoFim.Play();

        if (mensagemSucesso != null)
        {
            mensagemSucesso.gameObject.SetActive(true);
            mensagemSucesso.color = new Color(mensagemSucesso.color.r, mensagemSucesso.color.g, mensagemSucesso.color.b, 1f);

            string textoOriginal = mensagemSucesso.text.Trim();
            if (!string.IsNullOrEmpty(textoOriginal))
            {
                mensagemSucesso.text = "";
                foreach (char letra in textoOriginal)
                {
                    mensagemSucesso.text += letra;
                    if (velocidadeDigitacao > 0f)
                        yield return new WaitForSeconds(velocidadeDigitacao);
                }
            }
        }

        yield return new WaitForSeconds(2f);

        if (fade != null)
        {
            fade.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvas(fade, 0f, 1f, 1f));

            if (guaritaGeral != null) guaritaGeral.SetActive(false);
            fundomonitor.SetActive(false);
            fundo2.SetActive(true);

            fade.gameObject.SetActive(false);

            StartCoroutine(IniciarPiscada());
        }
    }

    private IEnumerator IniciarPiscada()
    {
        hud.SetActive(false);
        audioCatracaDesativa.Play();
        StartCoroutine(PiscarSprites());
        yield return new WaitForSeconds(2.5f);
        trapCollider1.SetActive(false);
        trapCollider2.SetActive(false);
        trapCollider3.SetActive(false);
        trapCollider4.SetActive(false);
        audioCatracaDesativa.Stop();
        dialogoFinal.SetActive(true);
        if (dialogoController != null)
        {
            bool dialogoTerminou = false;
            dialogoController.OnDialogoTerminado += () => dialogoTerminou = true;
            yield return new WaitUntil(() => dialogoTerminou);
        }
        else
        {
            yield return new WaitForSeconds(5f);
        }
        joystick.gameObject.SetActive(true);

        arquitetas.IniciarMovimento();
    }

    private IEnumerator PiscarSprites()
    {
        if (trapsPiscam == null || trapsPiscam.Count == 0) yield break;

        float tempoTotal = 0f;
        Color preto = new Color(0f, 0f, 0f, 120f / 255f);
        Color branco = new Color(1f, 1f, 1f, 120f / 255f);
        Color transparente = new Color(0f, 0f, 0f, 0f);

        Color[] cicloNormal = { preto, branco, transparente };
        int indexNormal = 0;

        while (tempoTotal < trapPiscar)
        {
            Color corAtual = cicloNormal[indexNormal];
            for (int i = 0; i < trapsPiscam.Count; i++)
            {
                if (trapsPiscam[i] != null)
                {
                    if (i == trapsPiscam.Count - 1 && corAtual == branco)
                        trapsPiscam[i].color = preto;
                    else
                        trapsPiscam[i].color = corAtual;
                }
            }

            indexNormal = (indexNormal + 1) % cicloNormal.Length;
            yield return new WaitForSeconds(intervaloPiscada);
            tempoTotal += intervaloPiscada;
        }

        foreach (SpriteRenderer sr in trapsPiscam)
            if (sr != null) sr.gameObject.SetActive(false);
    }

    private IEnumerator DigitarLinhas()
    {
        audioTeclando.Play();
        foreach (Text linha in linhasBinarias)
        {
            if (linha != null)
            {
                linha.gameObject.SetActive(true);
                string textoOriginal = linha.text;
                linha.text = "";
                foreach (char letra in textoOriginal)
                {
                    linha.text += letra;
                    if (velocidadeDigitacao > 0f)
                        yield return new WaitForSeconds(velocidadeDigitacao);
                }
                yield return new WaitForSeconds(0.005f);
            }
        }
    }

    private IEnumerator PiscarVerde()
    {
        audioTeclando.Stop();
        for (int i = 0; i < 2; i++)
        {
            foreach (Text linha in linhasBinarias) if (linha != null) linha.color = Color.green;
            yield return new WaitForSeconds(tempoPiscada);
            foreach (Text linha in linhasBinarias) if (linha != null) linha.gameObject.SetActive(false);
            yield return new WaitForSeconds(tempoPiscada);
            foreach (Text linha in linhasBinarias) if (linha != null) linha.gameObject.SetActive(true);
        }
    }

    private IEnumerator ManterVerde()
    {
        foreach (Text linha in linhasBinarias) if (linha != null) linha.color = Color.green;
        yield return new WaitForSeconds(2f);
        foreach (Text linha in linhasBinarias) if (linha != null) linha.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ProcessarTextoFinal()
    {
        audioTeclando.Stop();
        if (textoFinal != null)
        {
            textoFinal.gameObject.SetActive(true);
            string textoOriginalFinal = textoFinal.text;
            textoFinal.text = "";

            foreach (char letra in textoOriginalFinal)
            {
                textoFinal.text += letra;
                if (velocidadeDigitacao > 0f)
                    yield return new WaitForSeconds(velocidadeDigitacao);
            }

            yield return new WaitForSeconds(2f);

            yield return StartCoroutine(FadeGraphic(textoFinal, 1f, 0f, 0.5f));
            textoFinal.gameObject.SetActive(false);
        }
    }

    // Helpers
    private IEnumerator FadeGraphic(Graphic g, float start, float end, float duration)
    {
        float t = 0f;
        Color c = g.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, t / duration);
            g.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
        g.color = new Color(c.r, c.g, c.b, end);
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float start, float end, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }
        cg.alpha = end;
    }
}
