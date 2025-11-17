using UnityEngine;
using UnityEngine.InputSystem; // Input System novo
using System.Collections;
using System.Collections.Generic;


public class InteragirMonitor : MonoBehaviour
{
    [Header("Configurações de Interação")]
    public Transform player;
    public float interactionRange = 3f;

    [Header("Referências de UI")]
    public GameObject imagePanel;
    public GameObject HUD;
    public SpriteRenderer spriteRenderer;
    public Guarita guarita;

    [Header("Brilho")]
    public Color highlightColor = Color.yellow;

    [Header("Áudio")]
    public AudioSource audioSource; // <--- Arraste o AudioSource aqui no Inspector

    private Collider2D col;
    private Color originalColor;


    private bool playerInRange = false;
    private bool imageActive = false;

    void Start()
    {

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        else
            Debug.LogWarning($"[{gameObject.name}] SpriteRenderer não encontrado!");

        col = GetComponent<Collider2D>(); // <--- pega o collider 2D
        if (col == null)
            Debug.LogWarning($"[{gameObject.name}] Collider2D não encontrado!");

        if (player == null) Debug.LogError("Player não atribuído!");
        if (audioSource == null) Debug.LogWarning($"[{gameObject.name}] AudioSource não atribuído!");

        imagePanel?.SetActive(false);
        HUD?.SetActive(true);
    }
    void Update()
    {
        VerificarDistancia();
        AplicarBrilho();

        // Suporte para toque (mobile)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            VerificarCliqueOuToque(touchPos);
        }

        // Suporte para mouse (PC)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 clickPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            VerificarCliqueOuToque(clickPos);
        }
    }

    void VerificarDistancia()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;
    }

    void AplicarBrilho()
    {
        if (spriteRenderer == null) return;

        if (playerInRange && !imageActive)
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
        if (hit.collider != null && hit.collider.gameObject == this.gameObject && playerInRange)
        {
            StartCoroutine(Som());
        }
    }

    private IEnumerator Som ()
    {
        audioSource.Play();
        yield return new WaitForSeconds(0.1f);
        Abrir();
    }

    public void Abrir()
    {

        guarita.Inicio();

        imageActive = true;
        imagePanel?.SetActive(true);

        if (col != null)
            col.enabled = false; // Desativa o collider
    }

    public void Fechar()
    {
        imageActive = false;
        imagePanel?.SetActive(false);

        HUD?.SetActive(true);

        if (col != null)
            col.enabled = true; // Reativa o collider
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
