using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeInScene : MonoBehaviour
{
    [Header("Referências")]
    public CanvasGroup fadeCanvas; // CanvasGroup do painel de fade

    [Header("Configurações")]
    public float fadeDuration = 2f; // Tempo para o fade chegar em alpha 1
    public string nomeCenaDestino; // Nome da cena que será carregada ao final

    private bool isFading = false;

    void OnEnable()
    {
        // Garante que o fade comece do alpha 0
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            StartCoroutine(FazerFadeIn());
        }
    }

    private System.Collections.IEnumerator FazerFadeIn()
    {
        if (isFading) yield break;
        isFading = true;

        float tempo = 0f;

        while (tempo < fadeDuration)
        {
            tempo += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, tempo / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 1f;

        // Espera um pequeno instante e então muda de cena
        yield return new WaitForSeconds(0.3f);

        if (!string.IsNullOrEmpty(nomeCenaDestino))
        {
            SceneManager.LoadScene(nomeCenaDestino);
        }
    }
}
