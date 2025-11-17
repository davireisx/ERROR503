using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SenhaControllerDentroMuseu : MonoBehaviour
{
    [Header("C?mera")]
    public CameraManagerEsdras cameraManager;

    [Header("UI")]
    public Text visor;
    public GameObject imagemFechadura;
    public GameObject fechar;
    public GameObject joystick;
    public GameObject HUD;
    public Text checkText;
    public GameObject objetivos3;
    public GameObject objetivos4;
    public CanvasGroup fadeCanvasGroup;

    [Header("Senha")]
    public string senhaCorreta;
    private string codigoDigitado = "";
    public int limiteDigitos = 6;
    private bool bloqueado = false;

    [Header("Transi??o")]
    public Transform jogador;
    public Transform spawnPoint;

    [Header("?udios")]
    public AudioClip somSucesso;
    public AudioClip somErro;


    [System.Serializable]
    public class ConfiguracaoDigito
    {
        public Button botao;
        public Text texto;
        public Color corFeedback = Color.green;
        public AudioClip somClique;
        public bool isOK = false;
    }

    public List<ConfiguracaoDigito> configuracoes = new List<ConfiguracaoDigito>();
    public AudioSource audioSource;
    private Dictionary<Button, Coroutine> feedbacks = new Dictionary<Button, Coroutine>();

    void Start()
    {
        AtualizarVisor();

        if (joystick != null)
            joystick.SetActive(false);

        if (checkText != null)
            checkText.gameObject.SetActive(false);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        // Configura listeners nos bot?es
        foreach (var cfg in configuracoes)
        {
            if (cfg.botao != null)
            {
                ConfiguracaoDigito cfgCapturado = cfg;
                cfg.botao.onClick.AddListener(() => OnBotaoClicado(cfgCapturado));
            }
        }
    }

    private void OnBotaoClicado(ConfiguracaoDigito cfg)
    {
        if (bloqueado) return;

        string valor = cfg.texto.text;

        if (!feedbacks.ContainsKey(cfg.botao) || feedbacks[cfg.botao] == null)
            feedbacks[cfg.botao] = StartCoroutine(FeedbackDigito(cfg));

        if (char.IsDigit(valor, 0))
            DigitarNumero(valor);
        else if (valor == "?")
            Apagar();
        else if (valor == "OK")
            ValidarSenha();
    }

    public void DigitarNumero(string numero)
    {
        if (bloqueado) return;

        if (codigoDigitado.Length < limiteDigitos)
        {
            codigoDigitado += numero;
            AtualizarVisor();
        }
    }

    public void Apagar()
    {
        if (bloqueado) return;

        if (codigoDigitado.Length > 0)
        {
            codigoDigitado = codigoDigitado.Substring(0, codigoDigitado.Length - 1);
            AtualizarVisor();
        }
    }

    public void ValidarSenha()
    {
        if (bloqueado || codigoDigitado.Length < limiteDigitos)
        {
            ConfiguracaoDigito okCfg = configuracoes.Find(c => c.isOK);
            if (okCfg != null)
            {
                if (!feedbacks.ContainsKey(okCfg.botao) || feedbacks[okCfg.botao] == null)
                    feedbacks[okCfg.botao] = StartCoroutine(FeedbackBotaoOKIncompleto(okCfg));
            }
            return;
        }

        ConfiguracaoDigito okCfgCompleto = configuracoes.Find(c => c.isOK);
        if (okCfgCompleto != null)
        {
            if (!feedbacks.ContainsKey(okCfgCompleto.botao) || feedbacks[okCfgCompleto.botao] == null)
                feedbacks[okCfgCompleto.botao] = StartCoroutine(FeedbackBotaoOKCompleto(okCfgCompleto));
        }

        if (codigoDigitado == senhaCorreta)
            StartCoroutine(Sucesso());
        else
            StartCoroutine(Erro());
    }

    private void AtualizarVisor()
    {
        string texto = "";

        for (int i = 0; i < codigoDigitado.Length; i++)
            texto += "  " + codigoDigitado[i] + "  ";

        int asteriscosRestantes = limiteDigitos - codigoDigitado.Length;
        for (int i = 0; i < asteriscosRestantes; i++)
            texto += "  *  ";

        visor.text = texto.TrimEnd();
    }

    private IEnumerator FeedbackDigito(ConfiguracaoDigito cfg)
    {
        if (cfg.somClique != null && audioSource != null)
            audioSource.PlayOneShot(cfg.somClique);

        Color corOriginalTexto = cfg.texto.color;
        Color corOriginalBotao = cfg.botao.image.color;

        cfg.texto.color = cfg.corFeedback;
        cfg.botao.image.color = cfg.corFeedback;

        yield return new WaitForSeconds(0.2f);

        cfg.texto.color = corOriginalTexto;
        cfg.botao.image.color = corOriginalBotao;

        feedbacks[cfg.botao] = null;
    }

    private IEnumerator FeedbackBotaoOKIncompleto(ConfiguracaoDigito cfg)
    {
        Color corOriginalBotao = cfg.botao.image.color;

        for (int i = 0; i < 2; i++)
        {
            cfg.botao.image.color = cfg.corFeedback;
            yield return new WaitForSeconds(0.15f);
            cfg.botao.image.color = corOriginalBotao;
            yield return new WaitForSeconds(0.15f);
        }

        feedbacks[cfg.botao] = null;
    }

    private IEnumerator FeedbackBotaoOKCompleto(ConfiguracaoDigito cfg)
    {
        if (cfg.somClique != null && audioSource != null)
            audioSource.PlayOneShot(cfg.somClique);

        Color corOriginalBotao = cfg.botao.image.color;
        Color corOriginalTexto = cfg.texto.color;

        for (int i = 0; i < 2; i++)
        {
            cfg.botao.image.color = cfg.corFeedback;
            cfg.texto.color = cfg.corFeedback;
            yield return new WaitForSeconds(0.15f);
            cfg.botao.image.color = corOriginalBotao;
            cfg.texto.color = corOriginalTexto;
            yield return new WaitForSeconds(0.15f);
        }

        feedbacks[cfg.botao] = null;
    }

    private IEnumerator Erro()
    {
        bloqueado = true;

        if (audioSource != null && somErro != null)
            audioSource.PlayOneShot(somErro);

        for (int i = 0; i < 4; i++)
        {
            visor.text = "";
            yield return new WaitForSeconds(0.15f);
            AtualizarVisor();
            yield return new WaitForSeconds(0.15f);
        }

        visor.text = "     E    R    R    0    R";
        yield return new WaitForSeconds(1f);

        codigoDigitado = "";
        AtualizarVisor();
        bloqueado = false;
    }

    private IEnumerator Sucesso()
    {
        bloqueado = true;

        if (audioSource != null && somSucesso != null)
            audioSource.PlayOneShot(somSucesso);

        if (fechar != null)
            fechar.SetActive(false);

        if (imagemFechadura != null)
            imagemFechadura.SetActive(false);

        if (joystick != null)
            joystick.SetActive(true);

        if (checkText != null)
            checkText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        if (HUD != null)
            HUD.SetActive(false);

        if (fadeCanvasGroup != null)
            yield return StartCoroutine(Fade(0f, 1f, 0.75f));

        if (cameraManager != null)
            cameraManager.SetScenarioBounds(5);

        if (jogador != null && spawnPoint != null)
            jogador.position = spawnPoint.position;

        if (fadeCanvasGroup != null)
            yield return StartCoroutine(Fade(1f, 0f, 0.75f));

        if (checkText != null)
            checkText.gameObject.SetActive(false);

        if (objetivos3 != null)
            objetivos3.SetActive(false);

        if (objetivos4 != null)
            objetivos4.SetActive(true);

        if (HUD != null)
            HUD.SetActive(true);

        bloqueado = false;
        codigoDigitado = "";
        AtualizarVisor();
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = endAlpha;
    }
}