using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class SistemaDialogoDoisPersonagens : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public float interactionRange = 3f;
    public GameObject joystick;
    public GameObject hud;
    public AudioSource next;
    public AudioSource cliquePortal;
    public AudioSource durantePortal;

    [Header("Transição de Fase")]
    public GameObject personagemParaDesaparecer;
    public string proximaCenaNome;


    [Header("Caixas de Diálogo")]
    public GameObject caixaPersonagem1;
    public GameObject caixaPersonagem2;
    public Button botaoAvancar1;
    public Button botaoAvancar2;
    public Text check;

    [Header("Falas")]
    public GameObject[] falas;
    public int ultimoIndicePersonagem1 = 2;

    [Header("Fade Final")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;
    public float fadeStayTime = 5f;

    [Header("Brilho")]
    public Color highlightColor = Color.yellow;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private int falaAtual = 0;
    private bool dialogoAtivo = false;
    private bool playerInRange = false;

    void Start()
    {
        DesativarFalas();

        caixaPersonagem1?.SetActive(false);
        caixaPersonagem2?.SetActive(false);

        if (botaoAvancar1 != null) botaoAvancar1.onClick.AddListener(AvancarFala);
        if (botaoAvancar2 != null) botaoAvancar2.onClick.AddListener(AvancarFala);

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        else
            Debug.LogWarning($"[{gameObject.name}] SpriteRenderer não encontrado!");

        if (player == null) Debug.LogError("Player não atribuído!");
    }

    void Update()
    {
        if (player == null) return;

        float distancia = Vector2.Distance(transform.position, player.position);
        playerInRange = distancia <= interactionRange;

        AplicarBrilho();

        // Clique mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 clickPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            VerificarCliqueOuToque(clickPos);
        }

        // Toque mobile
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            VerificarCliqueOuToque(touchPos);
        }
    }

    void AplicarBrilho()
    {
        if (spriteRenderer == null) return;

        if (playerInRange && !dialogoAtivo)
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
        if (!playerInRange || dialogoAtivo) return;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            cliquePortal.Play();
            IniciarDialogo();
        }
    }

    public void IniciarDialogo()
    {
        falaAtual = 0;
        dialogoAtivo = true;

        hud?.SetActive(false);
        joystick?.SetActive(false);

        AtualizarCaixaDialogo();
        MostrarFalaAtual();
    }

    void AvancarFala()
    {
        next.Play();
        falaAtual++;

        if (falaAtual < falas.Length)
        {
            AtualizarCaixaDialogo();
            MostrarFalaAtual();
        }
        else
        {
            StartCoroutine(FinalizarComFade());
        }
    }

    void AtualizarCaixaDialogo()
    {
        bool falaDoPersonagem1 = falaAtual <= ultimoIndicePersonagem1;

        caixaPersonagem1?.SetActive(falaDoPersonagem1);
        caixaPersonagem2?.SetActive(!falaDoPersonagem1);
    }

    void MostrarFalaAtual()
    {
        for (int i = 0; i < falas.Length; i++)
        {
            if (falas[i] != null)
                falas[i].SetActive(i == falaAtual);
        }
    }

    void DesativarFalas()
    {
        foreach (var fala in falas)
            if (fala != null) fala.SetActive(false);
    }

    IEnumerator FinalizarComFade()
    {
        DesativarFalas();

        interactionRange = 0;
        hud?.SetActive(true);
        caixaPersonagem1?.SetActive(false);
        caixaPersonagem2?.SetActive(false);
        joystick?.SetActive(false);
        check.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        hud?.SetActive(false);
        check.gameObject.SetActive(false);

        dialogoAtivo = false;

        durantePortal.Play();
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = true;
            fadeGroup.interactable = false;

            float tempo = 0f;
            while (tempo < fadeDuration)
            {
                tempo += Time.deltaTime;
                fadeGroup.alpha = Mathf.Lerp(0f, 1f, tempo / fadeDuration);
                yield return null;
            }

            fadeGroup.alpha = 1f;

            // Aqui: esconde personagem e troca de fase
            if (personagemParaDesaparecer != null)
            {
                personagemParaDesaparecer.SetActive(false);
            }

            yield return new WaitForSeconds(fadeStayTime); // tempo extra se quiser

            // Carrega próxima cena
            if (!string.IsNullOrEmpty(proximaCenaNome))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(proximaCenaNome);
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
