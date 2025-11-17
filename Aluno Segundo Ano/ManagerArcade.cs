using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ManagerArcade : MonoBehaviour
{
    [Header("TrocarCenario")]
    public CanvasGroup fadeCanvas;         // CanvasGroup usado para o fade
    public float fadeDuration = 1.5f;      // Tempo de duração do fade
    public GameObject playerObj;
    public Transform player;               // Referência ao personagem
    public Transform spawnPoint;           // Local para onde o personagem será teleportado
    public CameraManagerEsdras camManager;
    public GameObject HUD;
    public GameObject joystick;
    public GameObject audioTocar;

    private bool ativo = false;

    // Método para iniciar o processo
    public void Start()
    {
        if (!ativo)
            StartCoroutine(FadeInETeleportar());
    }

    private IEnumerator FadeInETeleportar()
    {
        ativo = true;

        // Garante que o fadeCanvas está visível
        fadeCanvas.gameObject.SetActive(true);

        // Fade in
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // Teleporta o player
        if (player != null && spawnPoint != null)
        {
            player.position = spawnPoint.position;
        }

        camManager.SetScenarioBounds(2);
        audioTocar.SetActive(true);
        fadeCanvas.gameObject.SetActive(false);
        playerObj.SetActive(false);
        HUD.SetActive(false);
        joystick.SetActive(false);

        // Fade out de volta para a jogabilidade
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 0f;

        ativo = false;
    }
}
