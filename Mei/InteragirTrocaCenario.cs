using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class InteragirTrocaCenario : MonoBehaviour
{
    public enum CenarioDestino { Cenario3 = 3, Cenario4 = 4, Cenario5 = 5 }

    [Header("Configurações de Interação")]
    public Transform player;
    public float visualRange = 5f;
    public float interactionRange = 3f;

    [Header("Câmera e Spawn")]
    public CameraManagerEsdras cameraManager;
    public Transform spawnPoint;
    public GameObject joystick;
    public CenarioDestino destinoDoCenario;

    [Header("Fade (UI Image)")]
    public Image telaFade;
    public float fadeDuration = 1f;
    public GameObject HUD;
    public GameObject ativa;

    [Header("Tutorial (ativa após troca)")]
    public GameObject tutorial; // ?? adicionado
    private static bool tutorialJaAtivado = false; // ?? só ativa uma vez

    [Header("Visual")]
    public Color highlightColor = Color.yellow;

    [Header("Diálogo (opcional)")]
    public DialogoEstacaoDebora dialogoInicial;

    [Header("Som de Interação")]
    public AudioSource somInteracao;

    [Header("Gizmos")]
    public float gizmoSizeX = 2f;
    public float gizmoSizeY = 2f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool playerInVisualRange = false;
    private bool playerInInteractionRange = false;
    private static bool dialogoJaExecutado = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        if (telaFade != null)
        {
            Color c = telaFade.color;
            c.a = 0;
            telaFade.color = c;
            telaFade.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        VerificarDistancia();
        AplicarBrilho();

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            VerificarCliqueOuToque(touchPos);
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 clickPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            VerificarCliqueOuToque(clickPos);
        }
    }

    void VerificarDistancia()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        playerInVisualRange = distance <= visualRange;
        playerInInteractionRange = distance <= interactionRange;
    }

    void AplicarBrilho()
    {
        if (spriteRenderer == null) return;

        if (playerInVisualRange)
        {
            float pulse = Mathf.PingPong(Time.time * 2f, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, highlightColor, pulse);
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    void VerificarCliqueOuToque(Vector2 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == this.gameObject && playerInInteractionRange)
        {
            if (somInteracao != null)
                somInteracao.Play();

            if (!dialogoJaExecutado && dialogoInicial != null)
            {
                dialogoJaExecutado = true;
                dialogoInicial.IniciarDialogoComCallback(() =>
                {
                    StartCoroutine(FadeTrocaCenario());
                });
            }
            else
            {
                StartCoroutine(FadeTrocaCenario());
            }
        }
    }

    IEnumerator FadeTrocaCenario()
    {
        telaFade.gameObject.SetActive(true);
        HUD.gameObject.SetActive(false);
        yield return StartCoroutine(FadeIn());

        // --- TELEPORTE DO PLAYER ---
        if (Camera.main != null)
        {
            float sizeAtual = Camera.main.orthographicSize;
            PlayerPrefs.SetFloat("UltimoCameraSize", sizeAtual);
            PlayerPrefs.Save();
            Camera.main.orthographicSize = sizeAtual + 5f;
            Debug.Log($"Tamanho da câmera salvo: {sizeAtual} | Novo tamanho aplicado: {Camera.main.orthographicSize}");
        }

        player.position = spawnPoint.position;
        player.gameObject.SetActive(false);
        joystick?.SetActive(false);
        ativa?.SetActive(true);
        cameraManager?.SetScenarioBounds((int)destinoDoCenario);

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        // --- ATIVA TUTORIAL (SÓ UMA VEZ, APÓS TROCA DE CENÁRIO) ---
        if (!tutorialJaAtivado && tutorial != null)
        {
            tutorial.SetActive(true);
            tutorialJaAtivado = true;
        }

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FadeOut());
        telaFade.gameObject.SetActive(false);
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        Color c = telaFade.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            telaFade.color = c;
            yield return null;
        }
        c.a = 1;
        telaFade.color = c;
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        Color c = telaFade.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            telaFade.color = c;
            yield return null;
        }
        c.a = 0;
        telaFade.color = c;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visualRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
