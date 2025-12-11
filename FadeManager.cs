using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    [Header("Objeto que dispara o fade")]
    public GameObject objetoAlvo; // quando for ativado, o fade começa

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public string cenaDestino = "Menu";

    private bool fadeIniciado = false;

    void Update()
    {
        if (!fadeIniciado && objetoAlvo != null && !objetoAlvo.activeSelf)
        {
            fadeIniciado = true;
            StartCoroutine(FazerFadeECarregarCena());
        }
    }

    IEnumerator FazerFadeECarregarCena()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
            fadeCanvas.alpha = 0f;

            float tempo = 0f;
            while (tempo < fadeDuration)
            {
                tempo += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(0, 1, tempo / fadeDuration);
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("FadeCanvas não atribuído no FadeManager.");
        }

        Debug.Log("FadeManager: Carregando cena " + cenaDestino);
        SceneManager.LoadScene(cenaDestino);
    }
}
