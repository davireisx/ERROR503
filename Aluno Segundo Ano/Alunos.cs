using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Alunos : MonoBehaviour
{
    private Vector2 moveInput;
    private Vector2 ultimaDirecao = Vector2.down;
    private Vector2 direcaoAnterior = Vector2.zero;
    private bool isFacingLeft = false;

    private bool inputLocked = false;          // usado no knockback
    private float inputLockTimer = 0f;
    private bool bloqueioInteracao = false;    // bloqueia leitura do joystick durante interação

    private Rigidbody2D rb;
    private int vida;
    private bool podeTomarDano = true;
    private bool dentroDoInimigo = false;
    private float tempoParaProximoDano = 0f;

    [Header("Movimentação do Jogador")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Joystick joystick;

    [Header("Sistema de Vida")]
    [SerializeField] private int vidaMaxima = 3;
   // [SerializeField] private Image[] coracoesHUD;
   // [SerializeField] private Sprite coracaoCheio;
    //[SerializeField] private Sprite coracaoVazio;
    [SerializeField] private float tempoInvencivel = 1f;
    [SerializeField] private float intervaloPiscar = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioDano;
    [SerializeField] private AudioSource audioVida;

    [Header("Componentes")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer; // referência principal (primeiro renderer da lista)
    [SerializeField] private string telaGameOver;
    private Color corOriginal;

    // === NOVO: controla todos os sprites do player ===
    private SpriteRenderer[] allSprites;
    private Color[] coresOriginais;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 8f;
    [SerializeField] private float duracaoKnockback = 0.5f;
    [SerializeField] private AnimationCurve curvaKnockback;

    [Header("Cenário Atual")]
    public int cenarioAtual;   // Qual cenário o player está agora


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Pega TODOS os SpriteRenderers do player (objeto e filhos, inclusive inativos)
        allSprites = GetComponentsInChildren<SpriteRenderer>(true);
        if (allSprites == null || allSprites.Length == 0)
            Debug.LogError("Nenhum SpriteRenderer encontrado no player!");

        // Guarda cores originais para restaurar depois
        coresOriginais = new Color[allSprites.Length];
        for (int i = 0; i < allSprites.Length; i++)
            coresOriginais[i] = allSprites[i].color;

        // Mantém referência principal (se for usada em outros lugares)
        spriteRenderer = allSprites[0];
        corOriginal = spriteRenderer.color;

        if (joystick == null)
            Debug.LogError("Joystick não está configurado!");

        vida = vidaMaxima;
        //AtualizarHUDVida();
        InicializarCurvaKnockback();
    }

    private void Update()
    {

        // Liberação automática do lock do knockback
        if (inputLocked)
        {
            inputLockTimer -= Time.deltaTime;
            if (inputLockTimer <= 0f)
                inputLocked = false;
        }

        // Se estiver em interação ou com input travado, NÃO ler o joystick
        if (bloqueioInteracao || inputLocked)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput = new Vector2(joystick.Horizontal, joystick.Vertical);

            if (moveInput.magnitude > 0.1f)
                direcaoAnterior = moveInput.normalized;
        }

        VerificarContatoComInimigo();
        if (anim) anim.SetBool("walking", moveInput.magnitude > 0.1f);
        FlipCharacter();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (inputLocked || bloqueioInteracao)
        {
            // garante parada total
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (moveInput.magnitude > 0.1f)
        {
            ultimaDirecao = moveInput.normalized;
            float suavizacao = Mathf.Clamp01(moveInput.magnitude);
            float inputModulado = Mathf.Pow(suavizacao, 1.5f); // curva exponencial
            Vector2 direcaoFinal = moveInput.normalized * inputModulado;
            rb.MovePosition(rb.position + direcaoFinal * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void FlipCharacter()
    {
        if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true;
            isFacingLeft = true;
        }
        else if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false;
            isFacingLeft = false;
        }
    }

    private void VerificarContatoComInimigo()
    {
        if (dentroDoInimigo && podeTomarDano)
        {
            TomarDano(1);
        }
    }

   //* private void AtualizarHUDVida()
    //{
       // for (int i = 0; i < coracoesHUD.Length; i++)
       //{
        //    coracoesHUD[i].sprite = i < vida ? coracaoCheio : coracaoVazio;
        //    coracoesHUD[i].enabled = i < vidaMaxima;
        //}
   // } 

    public void TomarDano(int dano)
    {
        if (!podeTomarDano) return;

        vida = Mathf.Clamp(vida - dano, 0, vidaMaxima);
        if (audioDano) audioDano.Play();
        //AtualizarHUDVida();
        Debug.Log($"Vida: {vida}/{vidaMaxima}");

        if (vida <= 0) 
        {
            Morrer();
        }
            
        else 
        {
            StartCoroutine(EfeitoDano());
            
        }
    }

    // === helpers de cor/visibilidade para todos os sprites ===
    private void SetColorAll(Color c)
    {
        for (int i = 0; i < allSprites.Length; i++)
            allSprites[i].color = c;
    }

    private void SetEnabledAll(bool enabled)
    {
        for (int i = 0; i < allSprites.Length; i++)
            allSprites[i].enabled = enabled;
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < allSprites.Length; i++)
            allSprites[i].color = coresOriginais[i];
    }

    private IEnumerator EfeitoDano()
    {
        podeTomarDano = false;
        float tempo = 0f;


        while (tempo < tempoInvencivel)
        {

            // preto
            SetColorAll(Color.black);
            yield return new WaitForSeconds(intervaloPiscar);

            // invisível (desabilitado visualmente)
            SetEnabledAll(false);
            yield return new WaitForSeconds(intervaloPiscar);

            // reaparece em branco (contraste)
            SetEnabledAll(true);
            SetColorAll(Color.white);
            yield return new WaitForSeconds(intervaloPiscar);

            // volta cor original
            RestoreOriginalColors();
            yield return new WaitForSeconds(intervaloPiscar);

            tempo += intervaloPiscar * 4f;
        }

        SetEnabledAll(true);
        RestoreOriginalColors();
        podeTomarDano = true;
    }

    private void Morrer()
    {
        Debug.Log("Jogador morreu!");
        StartCoroutine(MudarCenaAposMorte());
    }

    private IEnumerator MudarCenaAposMorte()
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(telaGameOver);
    }

    public void RecuperarVida(int quantidade)
    {
        vida = Mathf.Clamp(vida + quantidade, 0, vidaMaxima);
        if (audioVida) audioVida.Play();
        //AtualizarHUDVida();
    }

    private IEnumerator AplicarKnockback(Vector2 direcao)
    {
        LockInput(duracaoKnockback);

        float timer = 0f;
        Vector2 startPos = rb.position;

        while (timer < duracaoKnockback)
        {
            float t = timer / duracaoKnockback;
            float intensidade = curvaKnockback.Evaluate(t); // 0 a 1
                                                            // Movimenta suavemente, reduz a força para não exagerar no celular
            Vector2 deslocamento = direcao * forcaKnockback * intensidade * Time.fixedDeltaTime * 0.5f;
            rb.MovePosition(rb.position + deslocamento);

            timer += Time.deltaTime;
            yield return null;
        }
    }


    private void LockInput(float duration)
    {
        inputLocked = true;
        inputLockTimer = duration;
    }

    private void InicializarCurvaKnockback()
    {
        if (curvaKnockback == null || curvaKnockback.length == 0)
        {
            curvaKnockback = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 1f, 0f, 2f),
                new Keyframe(1f, 0f)
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            dentroDoInimigo = true;
            tempoParaProximoDano = 0f;

            if (podeTomarDano)
            {
                // 1) Usa a última direção do player => empurra para trás
                Vector2 dir = -ultimaDirecao;

                // 2) Se estava parado, calcula pelo ponto de contato mais próximo
                if (dir == Vector2.zero)
                {
                    Vector2 contato = other.bounds.ClosestPoint(transform.position);
                    dir = ((Vector2)transform.position - contato).normalized;
                }

                // 3) Fallback
                if (dir == Vector2.zero)
                    dir = ((Vector2)transform.position - (Vector2)other.transform.position).normalized;

                StartCoroutine(AplicarKnockback(dir));
            }
        }

        if (other.CompareTag("Vida"))
        {
            if (vida < vidaMaxima)
            {
                RecuperarVida(1);
                Destroy(other.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            dentroDoInimigo = false;
    }

    // ==== Acessores / Controles de interação ====
    public bool IsFacingLeft() => isFacingLeft;
    public bool IsMoving() => moveInput.magnitude > 0.1f;
    public void SetInput(Vector2 input) => moveInput = input;
    public Vector2 GetCurrentInput() => moveInput;

    public void TeleportarPara(Vector2 novaPosicao) => transform.position = novaPosicao;

    public void PararMovimento()
    {
        moveInput = Vector2.zero;
        if (anim) anim.SetBool("walking", false);
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public void LiberarMovimento()
    {
        // libera QUALQUER bloqueio
        bloqueioInteracao = false;

        // limpa estados para recomeçar sem “arrasto”
        moveInput = Vector2.zero;
        direcaoAnterior = Vector2.zero;

        rb.linearVelocity = Vector2.zero;
        if (anim) anim.SetBool("walking", false);
    }

    // NOVO: chamados pela UI/Interação
    public void ComecarInteracao()
    {
        bloqueioInteracao = true;
        PararMovimento();
    }

    public void TerminarInteracao()
    {
        LiberarMovimento();
    }
}
