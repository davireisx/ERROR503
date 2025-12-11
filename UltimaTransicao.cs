using UnityEngine;
using UnityEngine.SceneManagement;

public class UltimaTransicao : MonoBehaviour
{
    [Header("Referência do Canvas de Fade")]
    public CanvasGroup fadeCanvas; // Canvas com CanvasGroup
    public float fadeSpeed = 1f;   // Velocidade do fade
    public string cenaVitoria;
    public AudioSource audioFinal;

    private bool iniciandoFade = false;

    void OnEnable()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f; // Começa transparente
            audioFinal.Play();
            iniciandoFade = true;
        }
        else
        {
            Debug.LogWarning("UltimaTransicao: Nenhum CanvasGroup atribuído!");
        }
    }

    void Update()
    {
        if (iniciandoFade && fadeCanvas != null)
        {
            fadeCanvas.alpha += Time.deltaTime * fadeSpeed;

            if (fadeCanvas.alpha >= 1f)
            {
                fadeCanvas.alpha = 1f;
                iniciandoFade = false;
                SceneManager.LoadScene(cenaVitoria);
            }
        }
    }
}
