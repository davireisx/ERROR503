using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMei : MonoBehaviour
{
    private Vector2 moveInput;
    private bool inputLocked = false;
    private bool bloqueioInteracao = false;
    private Rigidbody2D rb;

    private int vida;
    private bool podeTomarDano = true;
    private bool sofrendoKnockback = false;

    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Joystick joystick;

    [Header("Sistema de Vida")]
    [SerializeField] private int vidaMaxima = 3;
    [SerializeField] private float tempoInvencivel = 1f;
    [SerializeField] private float intervaloPiscar = 0.15f;

    [Header("Áudio")]
    [SerializeField] private AudioSource audioDano;
    [SerializeField] private AudioSource audioVida;

    [Header("Componentes")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string telaGameOver;

    private SpriteRenderer[] allSprites;
    private Color[] coresOriginais;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 10f;
    [SerializeField] private float duracaoKnockback = 0.25f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        allSprites = GetComponentsInChildren<SpriteRenderer>(true);
        coresOriginais = new Color[allSprites.Length];
        for (int i = 0; i < allSprites.Length; i++)
            coresOriginais[i] = allSprites[i].color;

        spriteRenderer = allSprites[0];
        vida = vidaMaxima;
    }

    private void Update()
    {
        // 🔸 Quando joystick está desativado ou interação ativa
        if (!joystick.gameObject.activeInHierarchy || bloqueioInteracao)
        {
            moveInput = Vector2.zero;
            if (!sofrendoKnockback) // 🔹 Não zera velocidade durante knockback
                rb.linearVelocity = Vector2.zero;
            if (anim) anim.SetBool("walking", false);
            return;
        }

        // 🔸 Durante knockback - apenas atualiza animação, mas não movimento
        if (sofrendoKnockback)
        {
            if (anim) anim.SetBool("walking", false);
            return;
        }

        // 🔸 Durante input travado (sem knockback)
        if (inputLocked)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            if (anim) anim.SetBool("walking", false);
            return;
        }

        // 🔸 Movimentação normal
        moveInput = new Vector2(joystick.Horizontal, joystick.Vertical);
        if (anim)
            anim.SetBool("walking", moveInput.magnitude > 0.1f);

        FlipCharacter();
    }

    private void FixedUpdate()
    {
        // 🔹 Durante knockback, NÃO interfere na física - deixa o Rigidbody fazer seu trabalho
        if (sofrendoKnockback) return;

        if (inputLocked || bloqueioInteracao)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (moveInput.magnitude > 0.1f)
        {
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void FlipCharacter()
    {
        if (moveInput.x < 0) spriteRenderer.flipX = true;
        else if (moveInput.x > 0) spriteRenderer.flipX = false;
    }

    // === Sistema de Dano ===
    public void TomarDano(int dano, Vector2 direcao)
    {
        if (!podeTomarDano) return;

        vida = Mathf.Clamp(vida - dano, 0, vidaMaxima);
        Debug.Log($"Vida: {vida}/{vidaMaxima}");

        if (audioDano && !audioDano.isPlaying)
            audioDano.Play();

        if (vida <= 0)
        {
            Morrer();
        }
        else
        {
            StartCoroutine(EfeitoDano());
            StartCoroutine(AplicarKnockback(direcao));
        }
    }

    private IEnumerator AplicarKnockback(Vector2 direcao)
    {
        podeTomarDano = false;
        sofrendoKnockback = true;
        inputLocked = true;

        // 🔹 Limpa qualquer velocidade anterior
        rb.linearVelocity = Vector2.zero;

        // 🔹 Configuração para knockback mais efetivo
        rb.linearDamping = 0f;
        rb.gravityScale = 0f;

        // 🔹 Aplica impulso real na direção contrária
        direcao.Normalize();
        Vector2 impulso = direcao * forcaKnockback;
        rb.AddForce(impulso, ForceMode2D.Impulse);

        if (anim) anim.SetBool("walking", false);

        // 🔹 Aguarda a duração do knockback
        yield return new WaitForSeconds(duracaoKnockback);

        // 🔹 Para o movimento suavemente após o knockback
        rb.linearVelocity = Vector2.zero;
        sofrendoKnockback = false;
        inputLocked = false;

        // 🔹 Tempo de invencibilidade após o knockback
        yield return new WaitForSeconds(tempoInvencivel - duracaoKnockback);
        podeTomarDano = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && podeTomarDano)
        {
            Vector2 dir = (transform.position - other.transform.position).normalized;
            TomarDano(1, dir);
        }

        if (other.CompareTag("Vida") && vida < vidaMaxima)
        {
            vida = Mathf.Clamp(vida + 1, 0, vidaMaxima);
            if (audioVida) audioVida.Play();
            Destroy(other.gameObject);
        }
    }

    private void Morrer()
    {
        Debug.Log("Jogador morreu!");
        StartCoroutine(MudarCenaAposMorte());
    }

    private IEnumerator MudarCenaAposMorte()
    {
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(telaGameOver);
    }

    private IEnumerator EfeitoDano()
    {
        float tempo = 0f;
        while (tempo < tempoInvencivel)
        {
            foreach (var sr in allSprites)
                sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(intervaloPiscar);
            tempo += intervaloPiscar;
        }
        foreach (var sr in allSprites)
            sr.enabled = true;
    }

    // 🔹 Método para controlar o bloqueio de interação (para diálogos, etc.)
    public void SetBloqueioInteracao(bool bloqueado)
    {
        bloqueioInteracao = bloqueado;
        if (bloqueado)
        {
            moveInput = Vector2.zero;
            if (!sofrendoKnockback)
                rb.linearVelocity = Vector2.zero;
        }
    }
}