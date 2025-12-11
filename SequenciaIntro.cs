using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SequenciaIntro : MonoBehaviour
{
    [Header("🎬 Elementos da Cena")]
    public Text texto1;
    public Text texto2;
    public Image imagemFinal;

    [Header("⚙️ Configurações de Tempo")]
    public float tempoFade = 1.5f;
    public float intervaloEntreTextos = 3f;
    public float tempoEsperaImagem = 10f;

    [Header("🔁 Cena e Fases para Reset")]
    public string nomeDaFaseParaCarregar;   // Nome da cena final
    public string fase1Resetar;
    public string fase2Resetar;
    public string fase3Resetar;

    private void Start()
    {
        // Inicializa todos invisíveis
        SetAlpha(texto1, 0f);
        SetAlpha(texto2, 0f);
        SetAlpha(imagemFinal, 0f);

        // Inicia a sequência
        StartCoroutine(ExecutarSequencia());
    }

    private IEnumerator ExecutarSequencia()
    {
        // Texto 1
        yield return StartCoroutine(FadeIn(texto1));
        yield return new WaitForSeconds(intervaloEntreTextos);

        // Texto 2
        yield return StartCoroutine(FadeIn(texto2));
        yield return new WaitForSeconds(intervaloEntreTextos);

        // Imagem
        yield return StartCoroutine(FadeIn(imagemFinal));
        yield return new WaitForSeconds(tempoEsperaImagem);

        // Reseta fases (apenas registra ação - pode personalizar depois com PlayerPrefs ou SaveManager)
        Debug.Log($"🔄 Resetando fases: {fase1Resetar}, {fase2Resetar}, {fase3Resetar}");

        // Troca de cena
        SceneManager.LoadScene(nomeDaFaseParaCarregar);
    }

    private IEnumerator FadeIn(Graphic elemento)
    {
        float t = 0f;
        while (t < tempoFade)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / tempoFade);
            SetAlpha(elemento, alpha);
            yield return null;
        }
        SetAlpha(elemento, 1f);
    }

    private void SetAlpha(Graphic elemento, float alpha)
    {
        if (elemento != null)
        {
            Color cor = elemento.color;
            cor.a = alpha;
            elemento.color = cor;
        }
    }
}
