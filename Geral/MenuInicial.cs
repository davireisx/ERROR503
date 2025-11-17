using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuInicial : MonoBehaviour
{
    [Header("Referências")]
    public Button botaoIniciar;          // Botão do menu
    public CanvasGroup fadePanel;        // Painel de fade (CanvasGroup)
    public Image imagem;                 // Imagem que muda de cor

    [Header("Configurações")]
    public float duracaoFade = 2.5f;     // Duração total do fade
    public string nomeCenaDestino;       // Cena que será carregada
    public float amplitudeMovimento = 10f;   // Quanto a imagem se move pra cima/baixo
    public float velocidadeMovimento = 2f;   // Velocidade do movimento da imagem

    private bool clicado = false;        // Impede múltiplos cliques
    private Vector3 posicaoInicial;      // Posição original da imagem

    private void Start()
    {
        // Garante que o painel começa invisível
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

        // Configura o clique único
        if (botaoIniciar != null)
            botaoIniciar.onClick.AddListener(OnCliqueBotao);
    }

    private void OnCliqueBotao()
    {
        if (clicado) return; // impede múltiplos cliques
        clicado = true;

        // Inicia o fade e a troca de cor
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            StartCoroutine(FazerFadeIn());
        }
    }

    private IEnumerator FazerFadeIn()
    {
        float tempo = 0f;

        Color corInicial = Color.white; // Branco (255,255,255)
        Color corFinal = new Color(18f / 255f, 1f, 0f); // Verde neon (#12FF00)

        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;

            // Fade do painel preto
            fadePanel.alpha = Mathf.Lerp(0f, 1f, tempo / duracaoFade);

            // Transição da cor da imagem
            if (imagem != null)
                imagem.color = Color.Lerp(corInicial, corFinal, tempo / duracaoFade);

            yield return null;
        }

        // Garante estado final
        fadePanel.alpha = 1f;
        if (imagem != null)
            imagem.color = corFinal;

        yield return new WaitForSeconds(0.2f);

        // Troca de cena
        if (!string.IsNullOrEmpty(nomeCenaDestino))
        {
            SceneManager.LoadScene(nomeCenaDestino);
        }
        else
        {
            Debug.LogWarning("? Nenhum nome de cena definido em 'nomeCenaDestino'!");
        }
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
