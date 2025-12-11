using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using UnityEngine.InputSystem; 
using System.Collections;


public class MenuInicial : MonoBehaviour
{
    [Header("Referências")]
    public Button botaoIniciar;
    public CanvasGroup fadePanel;
    public Image imagem;

    [Header("Configurações")]
    public float duracaoFade = 2.5f;
    public string nomeCenaDestino;
    public float amplitudeMovimento = 10f;
    public float velocidadeMovimento = 2f;

    private bool clicado = false;
    private Vector3 posicaoInicial;

    private void Start()
    {
        // Painel de fade começa invisível
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.gameObject.SetActive(false);
        }

        // Guarda posição inicial da imagem
        if (imagem != null)
            posicaoInicial = imagem.rectTransform.anchoredPosition;

        // Inicia a animação flutuante
        StartCoroutine(AnimarImagem());

        // 🔹 MANTIDO -> CLONADO NADA: Clique e toque continuam funcionando via botão
        if (botaoIniciar != null)
            botaoIniciar.onClick.AddListener(OnCliqueBotao);
    }

    private void Update()
    {
        // 🟢 ADICIONADO -> SPACE FUNCIONA TAMBÉM, mas sem remover clique ou toque
        if (!clicado && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OnCliqueBotao(); // Mesma função que o botão usa
        }
    }

    private void OnCliqueBotao()
    {
        if (clicado) return;
        clicado = true;

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            StartCoroutine(FazerFadeIn());
        }
    }

    private IEnumerator FazerFadeIn()
    {
        float tempo = 0f;

        Color corInicial = Color.white;
        Color corFinal = new Color(18f / 255f, 1f, 0f); // Verde neon (#12FF00)

        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;

            fadePanel.alpha = Mathf.Lerp(0f, 1f, tempo / duracaoFade);

            if (imagem != null)
                imagem.color = Color.Lerp(corInicial, corFinal, tempo / duracaoFade);

            yield return null;
        }

        fadePanel.alpha = 1f;
        if (imagem != null)
            imagem.color = corFinal;

        yield return new WaitForSeconds(0.2f);

        if (!string.IsNullOrEmpty(nomeCenaDestino))
            SceneManager.LoadScene(nomeCenaDestino);
        else
            Debug.LogWarning("⚠ Nenhum nome de cena definido em 'nomeCenaDestino'!");
    }

    private IEnumerator AnimarImagem()
    {
        while (true)
        {
            if (imagem != null)
            {
                float novaY = posicaoInicial.y + Mathf.Sin(Time.time * velocidadeMovimento) * amplitudeMovimento;
                imagem.rectTransform.anchoredPosition = new Vector2(posicaoInicial.x, novaY);
            }
            yield return null;
        }
    }
}
