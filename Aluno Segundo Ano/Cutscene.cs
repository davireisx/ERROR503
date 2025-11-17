using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class irCutscene2 : MonoBehaviour
{
    [SerializeField] private string cutsceneSceneName; // Nome da cena da cutscene
    [SerializeField] private CanvasGroup fadeCanvas;   // Referência ao CanvasGroup do Fade
    [SerializeField] private float fadeDuration = 1f;  // Tempo do fade
    [SerializeField] private GameObject joystick;      // Referência ao Joystick no HUD

    private bool isFading = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isFading)
        {
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        isFading = true;

        // Desativa o joystick
        if (joystick != null)
            joystick.SetActive(false);

        float elapsedTime = 0f;

        // Garante que o fade começa transparente
        fadeCanvas.alpha = 0f;
        fadeCanvas.gameObject.SetActive(true);

        // Fade In
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        // Carrega a cena
        SceneManager.LoadScene(cutsceneSceneName);
    }
}
