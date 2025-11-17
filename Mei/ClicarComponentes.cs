using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class ClicarComponentes : MonoBehaviour
{
    [Header("Interação")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private GameObject painelInteracao;

    private Transform playerTransform;
    private Collider2D colisor;
    private AudioSource audioSource;
    private bool clicado = false;

    void Awake()
    {
        colisor = GetComponent<Collider2D>();
        colisor.enabled = false;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        VerificarCliqueDoUsuario();
    }

    void VerificarCliqueDoUsuario()
    {
        if (playerTransform == null || colisor == null || !colisor.enabled || clicado)
            return;

        Vector2 toqueOuClique = Vector2.zero;
        bool clicou = false;

        // Toque (mobile)
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.isPressed && touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(touch.position.ReadValue());
                toqueOuClique = new Vector2(worldPos.x, worldPos.y);
                clicou = true;
            }
        }

        // Clique (mouse)
        if (!clicou && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            toqueOuClique = new Vector2(worldPos.x, worldPos.y);
            clicou = true;
        }

        if (!clicou) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(toqueOuClique, interactionRange);
        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.gameObject == this.gameObject)
            {
                float distancia = Vector2.Distance(transform.position, playerTransform.position);
                if (distancia <= interactionRange && painelInteracao != null)
                {
                    var playerScript = playerTransform.GetComponent<Alunos>();
                    if (playerScript != null)
                    {
                        // BLOQUEIA leitura do joystick e para o movimento imediatamente
                        playerScript.ComecarInteracao();
                    }

                    // Ativa o painel de interação
                    StartCoroutine(FazerTransicao());

                    // Som + desabilita re-clique
                    if (!clicado && audioSource != null)
                    {
                        clicado = true;
                        audioSource.PlayOneShot(audioSource.clip);
                        colisor.enabled = false;
                    }

                    Debug.Log("Painel ativado via clique/toque.");
                }
                else
                {
                    Debug.Log("Clique detectado, mas fora do alcance de interação.");
                }
                break;
            }
        }
    }

    IEnumerator FazerTransicao()
    {
        yield return new WaitForSeconds(0.2f);
        painelInteracao.SetActive(true);
    }

    public void AtivarInteracao() => colisor.enabled = true;
    public void DesativarInteracao() => colisor.enabled = false;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
