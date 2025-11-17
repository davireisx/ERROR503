using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SenhaController : MonoBehaviour
{
    [Header("Câmera")]
    public CameraManagerEsdras cameraManager;

    [Header("UI")]
    public Text visor;
    public GameObject fadePanel;
    public GameObject imagemFechadura;
    public GameObject fechar;
    public GameObject joystick;
    public GameObject hud; // HUD geral
    public GameObject objetivos1;
    public GameObject objetivos2;
    public Text checkText;

    [Header("Áudios de resultado")]
    public AudioClip somSucesso;
    public AudioClip somErro;

    [Header("Feedback")]
    public GameObject textoCheck; // "CHECK"
    private CanvasGroup fadeCanvasGroup;

    [System.Serializable]
    public class ConfiguracaoDigito
    {
        public Button botao;               // Botão do dígito
        public Text texto;                 // Texto dentro do botão
        public Color corFeedback = Color.green;
        public AudioClip somClique;
        public bool isOK = false;
    }

    [Header("Configuração de cada dígito")]
    public List<ConfiguracaoDigito> configuracoes = new List<ConfiguracaoDigito>();
    public AudioSource audioSource;

    [Header("Senha")]
    public string senhaCorreta;
    private string codigoDigitado = "";
    public int limiteDigitos = 4;
    public float escalaPlayer = 0.68f;

    [Header("Teleportação")]
    public Transform jogador;
    public Transform spawnPoint;

    private bool bloqueado = false;

    // Controle de feedback para cada botão
    private Dictionary<Button, Coroutine> feedbacks = new Dictionary<Button, Coroutine>();

    void Start()
    {
        AtualizarVisor();

        // Configura os listeners de clique para cada botão
        foreach (var cfg in configuracoes)
        {
            if (cfg.botao != null)
            {
                ConfiguracaoDigito cfgCapturado = cfg; // captura corretamente a variável
                cfg.botao.onClick.AddListener(() => OnBotaoClicado(cfgCapturado));
            }
        }

        if (joystick != null)
            joystick.SetActive(false);

        if (checkText != null)
            checkText.gameObject.SetActive(false);

        if (fadePanel != null)
        {
            fadeCanvasGroup = fadePanel.GetComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        Debug.Log(codigoDigitado.Length);
    }

    private void OnBotaoClicado(ConfiguracaoDigito cfg)
    {
        if (bloqueado) return;

        string valor = cfg.texto.text;

        
            // 🔹 Números ou backspace
            if (!feedbacks.ContainsKey(cfg.botao) || feedbacks[cfg.botao] == null)
                feedbacks[cfg.botao] = StartCoroutine(FeedbackDigito(cfg));

            if (char.IsDigit(valor, 0))
                DigitarNumero(valor);
            else if (valor == "←")
                Apagar();
        else if (valor == "OK")
            ValidarSenha();
        

    }




    private void DigitarNumero(string numero)
    {
        if (bloqueado) return;

        if (codigoDigitado.Length < limiteDigitos)
        {
            codigoDigitado += numero[0]; // Adiciona apenas o primeiro caractere
            Debug.Log($"Digitado: {codigoDigitado}, Length: {codigoDigitado.Length}");
            AtualizarVisor();
        }
    }

    private IEnumerator FeedbackDigito(ConfiguracaoDigito cfg)
    {
        if (cfg.somClique != null && audioSource != null)
            audioSource.PlayOneShot(cfg.somClique);

        // pega as cores originais
        Color corOriginalTexto = cfg.texto.color;
        Color corOriginalBotao = cfg.botao.image.color;

        // aplica cor de feedback
        cfg.texto.color = cfg.corFeedback;
        cfg.botao.image.color = cfg.corFeedback;

        yield return new WaitForSeconds(0.2f); // intervalo do piscar

        // volta cores originais
        cfg.texto.color = corOriginalTexto;
        cfg.botao.image.color = corOriginalBotao;

        feedbacks[cfg.botao] = null; // libera botão pra novo clique
    }

    private IEnumerator FeedbackBotaoOKIncompleto(ConfiguracaoDigito cfg)
    {
        Debug.Log("Incompleto!");
        // 🔹 Sem som para OK incompleto
        Color corOriginalBotao = cfg.botao.image.color;

        // 🔹 Apenas o botão pisca (texto não)
        for (int i = 0; i < 2; i++)
        {
            cfg.botao.image.color = cfg.corFeedback;
            yield return new WaitForSeconds(0.15f);
            cfg.botao.image.color = corOriginalBotao;
            yield return new WaitForSeconds(0.15f);
        }

        feedbacks[cfg.botao] = null; // libera botão
    }

    // Pisca botão + texto, toca som e valida senha
    private IEnumerator FeedbackBotaoOKCompleto(ConfiguracaoDigito cfg)
    {
        Color corOriginalBotao = cfg.botao.image.color;
        Color corOriginalTexto = cfg.texto.color;

        // Apenas feedback visual
        for (int i = 0; i < 2; i++)
        {
            cfg.botao.image.color = cfg.corFeedback;
            cfg.texto.color = cfg.corFeedback;
            yield return new WaitForSeconds(0.15f);
            cfg.botao.image.color = corOriginalBotao;
            cfg.texto.color = corOriginalTexto;
            yield return new WaitForSeconds(0.15f);
        }

        feedbacks[cfg.botao] = null; // libera botão
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
            return;

        if (codigoDigitado == senhaCorreta)
        {
            StartCoroutine(Sucesso());
        }
        else
        {
            StartCoroutine(Erro());
        }
    }

    private void AtualizarVisor()
    {
        // Sempre mostra 4 posições
        char[] display = new char[limiteDigitos];

        for (int i = 0; i < limiteDigitos; i++)
        {
            if (i < codigoDigitado.Length)
                display[i] = codigoDigitado[i];
            else
                display[i] = '*';
        }

        // Monta a string final do visor
        visor.text = $"  {display[0]}     {display[1]}     {display[2]}     {display[3]}";
    }

    private IEnumerator Erro()
    {
        if (audioSource != null && somErro != null)
            audioSource.PlayOneShot(somErro);

        ConfiguracaoDigito okCfg = configuracoes.Find(c => c.isOK);
        if (okCfg != null)
            yield return StartCoroutine(FeedbackBotaoOKIncompleto(okCfg));

        bloqueado = true;

        for (int i = 0; i < 4; i++)
        {
            visor.text = "";
            yield return new WaitForSeconds(0.15f);
            AtualizarVisor();
            yield return new WaitForSeconds(0.15f);
        }

        visor.text = " E R R 0 R";
        yield return new WaitForSeconds(1f);

        codigoDigitado = "";
        AtualizarVisor();
        bloqueado = false;
    }

    private IEnumerator Sucesso()
    {
        bloqueado = true;

        // Toca som de sucesso uma vez
        if (audioSource != null && somSucesso != null)
            audioSource.PlayOneShot(somSucesso);

        ConfiguracaoDigito okCfg = configuracoes.Find(c => c.isOK);
        if (okCfg != null)
            yield return StartCoroutine(FeedbackBotaoOKCompleto(okCfg));

        bloqueado = true;

        if (fechar != null) fechar.SetActive(false);
        if (imagemFechadura != null) imagemFechadura.SetActive(false);
        if (hud != null) hud.SetActive(true);
        if (textoCheck != null) textoCheck.SetActive(true);

        yield return new WaitForSeconds(1f);

        if (hud != null) hud.SetActive(false);
        if (textoCheck != null) textoCheck.SetActive(false);

        if (fadeCanvasGroup != null)
            yield return StartCoroutine(Fade(0f, 1.5f, 1.5f));

        if (jogador != null && spawnPoint != null)
        {
            jogador.position = spawnPoint.position;
            jogador.localScale = new Vector3(escalaPlayer, escalaPlayer, jogador.localScale.z);
            jogador.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y, 0f);
        }

        if (cameraManager != null)
            cameraManager.SetScenarioBounds(2);

        if (fadeCanvasGroup != null)
            yield return StartCoroutine(Fade(1.5f, 0f, 1.5f));
        if (joystick != null) joystick.SetActive(true);
        if (hud != null) hud.SetActive(true);
        if (objetivos1 != null) objetivos1.SetActive(false);
        if (objetivos2 != null) objetivos2.SetActive(true);

        codigoDigitado = "";
        bloqueado = false;
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