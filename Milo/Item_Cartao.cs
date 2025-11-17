using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEditor;

public class ItemInteraction : MonoBehaviour
{
    

    [Header("Configurações")]
    public Transform player;
    public float range = 3f;


    [Header("Setas")]
    public GameObject seta1;
    public GameObject seta2;

    [Header("Audio")]
    public AudioSource item;

    [Header("Referências Visuais")]
    public GameObject hud;                   // Imagem que aparece após 3s
    public GameObject quadrado1;
    public GameObject quadrado2;
    public GameObject check;
    public InteragirMonitor pendriveInteracao;      // Arraste o script no Inspector
    public Color highlightColor = Color.yellow;  // Cor de brilho

    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;        // Defina no Inspector

    private Color originalColor;
    private bool playerInRange = false;
    private bool interactionComplete = false;

    void Start()
    {
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        else
            Debug.LogWarning($"[{gameObject.name}] SpriteRenderer não atribuído!");

        if (hud != null)
            hud.gameObject.SetActive(false);

        StartCoroutine(ShowImageAfterDelay());

        if (player == null)
            Debug.LogError("Player não atribuído!");
    }

    void Update()
    {
        VerificarDistancia();
        AplicarBrilho();

        if (interactionComplete) return;

        // Clique no PC
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 clickPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            VerificarCliqueOuToque(clickPos);
        }

        // Toque no Mobile
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            VerificarCliqueOuToque(touchPos);
        }
    }

    void VerificarDistancia()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= range;
    }

    void AplicarBrilho()
    {
        if (spriteRenderer == null) return;

        if (playerInRange && !interactionComplete)
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
        if (!playerInRange || interactionComplete) return;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            item.Play();
            Interagir();
        }
    }

    void Interagir()
    {
        interactionComplete = true;

        seta1.gameObject.SetActive(false);
        seta2.gameObject.SetActive(true);

        
        pendriveInteracao.interactionRange = 7f;



        check.gameObject.SetActive(true);


        StartCoroutine(ExecutarCheckComDelay());

    }

    IEnumerator ExecutarCheckComDelay()
    {
        check.gameObject.SetActive(true);

        // Aguarda 1.5 segundos (você pode ajustar o tempo)
        yield return new WaitForSeconds(0.6f);
        spriteRenderer.gameObject.SetActive(false);

        if (check != null)
            check.gameObject.SetActive(false);

        if (quadrado1 != null)
            quadrado1.gameObject.SetActive(false);

        if (quadrado2 != null)
            quadrado2.gameObject.SetActive(true);

    }
    IEnumerator ShowImageAfterDelay()
    {
        yield return new WaitForSeconds(0f);
        if (hud != null)
            hud.gameObject.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
