using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class CartãoInteração : MonoBehaviour
{
    [Header("Configurações")]
    public float interactionRange = 3f;
    public Transform player;
    public Arquiteto ativarMovimento;

    [Header("Ações de Interação")]
    public GameObject objetoParaDesativar1;
    public GameObject objetoParaDesativar2;
    public GameObject cartaoAtiva;
    public GameObject cartaoDesativa;

    [Header("Textos")]
    public GameObject fundo1;
    public GameObject fundo2;
    public GameObject check;

    [Header("Joystick")]
    public Image joystickImage;

    [Header("Brilho")]
    public SpriteRenderer spriteRenderer;
    public Color highlightColor = Color.yellow;

    private Color originalColor;
    private bool foiClicado = false;
    private bool playerInRange = false;

    void Start()
    {
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        else
            Debug.LogWarning($"[{gameObject.name}] SpriteRenderer não atribuído!");

        if (player == null)
            Debug.LogError("Player não atribuído!");
    }

    void Update()
    {
        if (foiClicado || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        AplicarBrilho();

        // Clique com mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 clickPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            VerificarCliqueOuToque(clickPos);
        }

        // Toque em mobile
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            VerificarCliqueOuToque(touchPos);
        }
    }

    void VerificarCliqueOuToque(Vector2 worldPos)
    {
        if (!playerInRange || foiClicado) return;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            Interagir();
        }
    }

    void Interagir()
    {

        foiClicado = true;
        interactionRange = 0f;

        joystickImage.enabled = false;
        objetoParaDesativar1?.SetActive(false);
        objetoParaDesativar2?.SetActive(false);
        cartaoAtiva?.SetActive(true);
        ativarMovimento.IniciarMovimento();


        StartCoroutine(ExecutarCheckComDelay());
    }


    IEnumerator ExecutarCheckComDelay()
    {
        if (check != null)
            check.gameObject.SetActive(true);

        // Aguarda 1.5 segundos (você pode ajustar o tempo)
        yield return new WaitForSeconds(1.5f);

        if (check != null)
            check.gameObject.SetActive(false);

        if (fundo1 != null)
            fundo1.gameObject.SetActive(false);

        if (fundo2 != null)
            fundo2.gameObject.SetActive(true);
    }


    void AplicarBrilho()
    {
        if (spriteRenderer == null) return;

        if (playerInRange && !foiClicado)
        {
            float pulse = Mathf.PingPong(Time.time * 2f, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, highlightColor, pulse);
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
