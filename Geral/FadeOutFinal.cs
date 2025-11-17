using UnityEngine;
using System.Collections;

public class FadeOutFinal : MonoBehaviour
{
    [Header("Configurações do Fade")]
    [SerializeField] private float fadeDuration = 2f; // Duração do fade em segundos
    [SerializeField] private bool disableAfterFade = false; // Se deve desabilitar o objeto após o fade

    private CanvasGroup canvasGroup;

    void Start()
    {
        // Busca o CanvasGroup automaticamente
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup não encontrado no objeto!");
            return;
        }

        // Garante que o alpha comece em 1
        canvasGroup.alpha = 1f;

        // Inicia o fade out automaticamente
        StartFadeOut();
    }

    // Método público para iniciar o fade out
    public void StartFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    // Corrotina que executa o fade out
    private IEnumerator FadeOutCoroutine()
    {
        Debug.Log("Iniciando fade out...");

        float currentTime = 0f;
        float startAlpha = canvasGroup.alpha;

        // Executa o fade ao longo do tempo especificado
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, currentTime / fadeDuration);
            yield return null;
        }

        // Garante que o alpha termine exatamente em 0
        canvasGroup.alpha = 0f;

        Debug.Log("Fade out completo! Alpha: " + canvasGroup.alpha);

        // Desativa o objeto apenas se a opção estiver marcada
        if (disableAfterFade)
        {
            gameObject.SetActive(false);
        }
    }

    // Método para configurar a duração programaticamente
    public void SetFadeDuration(float duration)
    {
        fadeDuration = duration;
    }
}