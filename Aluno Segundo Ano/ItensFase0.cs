using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ItensFase0 : MonoBehaviour
{
    [Header("Configurações")]
    public Transform player;

    [Tooltip("Distância em que o item começa a piscar (efeito visual).")]
    public float rangeVisual = 5f;

    [Tooltip("Distância real de interação (deve ser menor que o visual).")]
    public float rangeProximidade = 2f;

    [Header("Audio")]
    public AudioSource item;

    [Header("Referências Visuais")]
    public GameObject hud;
    public GameObject check;
    public GameObject checkAntes;
    public GameObject setas;
    public Color highlightColor = Color.yellow;

    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;

    private Color originalColor;
    private bool playerInVisualRange = false;
    private bool playerInProximityRange = false;
    private bool interactionComplete = false;

    void Start()
    {
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        else
            Debug.LogWarning($"[{gameObject.name}] SpriteRenderer não atribuído!");

        if (hud != null)
            hud.SetActive(false);

        if (player == null)
            Debug.LogError("Player não atribuído!");
    }

    void Update()
    {
        VerificarDistancias();
        AplicarBrilho();

        if (interactionComplete) return;

        // ---------------------- INTERAÇÃO POR CLIQUE ----------------------
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TentarInteragir();

        // ---------------------- INTERAÇÃO POR TOQUE -----------------------
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            TentarInteragir();

        // ---------------------- INTERAÇÃO POR TECLA SPACE -----------------
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            InteragirDireto(); // <-- interação pelo teclado (sem Raycast)
    }

    void VerificarDistancias()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        playerInVisualRange = distance <= rangeVisual;
        playerInProximityRange = distance <= rangeProximidade;
    }

    void AplicarBrilho()
    {
        if (spriteRenderer == null) return;

        if (playerInVisualRange && !interactionComplete)
        {
            float pulse = Mathf.PingPong(Time.time * 2f, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, highlightColor, pulse);
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    void TentarInteragir()
    {
        if (interactionComplete || !playerInProximityRange)
            return;

        Vector2 screenPos = Vector2.zero;
        bool inputDetected = false;

        // Detecta posição da interação (mouse ou toque)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPos = Mouse.current.position.ReadValue();
            inputDetected = true;
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            inputDetected = true;
        }

        if (!inputDetected) return;

        // Converte posição para o mundo
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        // Faz raycast 2D
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity);

        Debug.DrawRay(worldPos, Vector2.one * 0.1f, Color.red, 1f);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            Debug.Log("Interação confirmada por toque/clique!");
            Interagir();
        }
        else
        {
            Collider2D[] overlaps = Physics2D.OverlapPointAll(worldPos);
            foreach (Collider2D col in overlaps)
            {
                if (col.gameObject == gameObject)
                {
                    Debug.Log("Interação detectada via OverlapPoint!");
                    Interagir();
                    break;
                }
            }
        }
    }

    // ---------------------- MÉTODO PARA INTERAÇÃO PELO TECLADO (SPACE) ----------------------
    void InteragirDireto()
    {
        if (interactionComplete || !playerInProximityRange)
            return;

        Debug.Log("Interação confirmada pelo teclado (SPACE)!");
        Interagir();
    }

    void Interagir()
    {
        if (interactionComplete) return;

        interactionComplete = true;

        if (item != null)
            item.Play();

        if (check != null) check.SetActive(true);
        if (checkAntes != null) checkAntes.SetActive(false);

        if (setas != null) setas.SetActive(false);

        StartCoroutine(ExecutarCheckComDelay());
    }

    IEnumerator ExecutarCheckComDelay()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    IEnumerator ShowImageAfterDelay()
    {
        yield return new WaitForSeconds(0f);
        if (hud != null)
            hud.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangeVisual);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangeProximidade);
    }
}
