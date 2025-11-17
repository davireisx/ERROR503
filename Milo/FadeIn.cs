using UnityEngine;
using System.Collections;

public class FadeIn : MonoBehaviour
{
    [Header("Configuração do Fade")]
    public CanvasGroup canvasGroup;   // CanvasGroup do cenário
    public GameObject objetoAlvo;     // GameObject que dispara o fade
    public float tempoFade = 1f;      // duração do fade

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // começa transparente e bloqueia interação
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        if (objetoAlvo == null)
            objetoAlvo = gameObject;

        StartCoroutine(VerificarAtivacao());
    }

    private IEnumerator VerificarAtivacao()
    {
        // espera até o objetoAlvo estar ativo
        while (!objetoAlvo.activeInHierarchy)
            yield return null;

        // inicia fade in do alpha 0 para 1
        yield return StartCoroutine(Fade(0f, 1f));

        // libera interação quando terminar
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private IEnumerator Fade(float start, float end)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / tempoFade;
            canvasGroup.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }

        canvasGroup.alpha = end;
    }
}
