using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Arquiteto : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float waypointThreshold = 0.1f;
    public GameObject posicaoInicialMovimento;

    [Header("Configurações de Tiro")]
    public GameObject projetilVerdePrefab;
    public GameObject projetilAmareloPrefab;
    public GameObject projetilVermelhoPrefab;
    public Transform pontoDeTiro;
    public float tempoDeEspera = 3f;
    public Transform[] alvosPossiveis;

    [Header("Comportamento Final")]
    public bool pararNoUltimoWaypoint = true;

    [Header("Referências")]
    public CatracaManager catraca;
    public Animator animator;
    public GameObject trap;
    public AudioSource audioAtaque;
    public AudioSource audioTiro;
    public SpriteRenderer spriteRenderer;

    [Header("Interação")]
    public float raioInteracao = 2f;
    public Transform player;

    [Header("Configurações de Efeito Visual")]
    public Sprite spriteFinal; // Sprite que será aplicado no final do efeito

    // Estados internos
    private bool podeAtirar = false;
    private bool arquitetoVasco = false;
    private bool comandoRecebido = false;
    private bool indoParaPosicaoInicial = false;
    private bool jogadorNoRaio = false;
    private bool estaTeleportando = false;

    private int currentWaypointIndex = 0;
    private bool isMovingForward = true;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    private Transform ultimoAlvoAtirado = null;
    private List<Transform> alvosDisponiveis = new();

    void Start()
    {
  
        ResetarAlvosDisponiveis();
        // Configura estado inicial
        SetAnimatorState(idle: true, some: false, aparece: false, atacando: false);
    }

    void Update()
    {
        if (!comandoRecebido) return;

        if (indoParaPosicaoInicial && !estaTeleportando)
        {
            IniciarTeleporteParaPosicaoInicial();
        }
        else if (podeAtirar && !estaTeleportando)
        {
            if (waypoints.Length == 0) return;

            if (isWaiting)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= tempoDeEspera)
                {
                    isWaiting = false;
                    // Ao terminar de esperar, inicia teleporte para próximo waypoint
                    StartCoroutine(TeleporteParaProximoWaypoint());
                }
            }
        }
    }

    // ---------------- SISTEMA DE ANIMAÇÃO ----------------
    void SetAnimatorState(bool idle, bool some, bool aparece, bool atacando)
    {
        animator.SetBool("idle", idle);
        animator.SetBool("some", some);
        animator.SetBool("aparece", aparece);
        animator.SetBool("Atacando", atacando);
    }

    // ---------------- SISTEMA DE TELEPORTE ----------------

    void IniciarTeleporteParaPosicaoInicial()
    {
        estaTeleportando = true;
        indoParaPosicaoInicial = false;
        StartCoroutine(SequenciaTeleporteInicial());
    }

    IEnumerator SequenciaTeleporteInicial()
    {
        Debug.Log("Iniciando sequência de teleporte inicial...");

        // Fase 1: Animação "some" (1 segundo)
        SetAnimatorState(idle: false, some: true, aparece: false, atacando: false);

        Debug.Log("Animação SOME iniciada");
        yield return new WaitForSeconds(1f);
        Debug.Log("Animação SOME concluída");

        // Teleporte instantâneo para posição inicial
        if (posicaoInicialMovimento != null)
        {
            transform.position = posicaoInicialMovimento.transform.position;
            Debug.Log("Teleporte concluído para posição inicial");
        }

        // Fase 2: Animação "aparece" (1 segundo)
        SetAnimatorState(idle: false, some: false, aparece: true, atacando: false);
   
        Debug.Log("Animação APARECE iniciada");
        yield return new WaitForSeconds(1f);
        Debug.Log("Animação APARECE concluída");
        audioAtaque.Play();
        // Fase 3: Volta para idle
        SetAnimatorState(idle: true, some: false, aparece: false, atacando: false);

        estaTeleportando = false;
        podeAtirar = true;

        Debug.Log("Sequência de teleporte inicial finalizada");

        // Inicia imediatamente o ataque no primeiro waypoint
        StartCoroutine(IniciarAtaqueNoWaypoint());
    }

    IEnumerator TeleporteParaProximoWaypoint()
    {
        // VERIFICA SE É O ÚLTIMO WAYPOINT ANTES DE TELETRANSPORTAR
        int nextIndex = isMovingForward ? currentWaypointIndex + 1 : currentWaypointIndex - 1;

        if (pararNoUltimoWaypoint && nextIndex >= waypoints.Length)
        {
            // CHEGOU NO ÚLTIMO WAYPOINT - PARA TUDO E VAI PARA IDLE
            Debug.Log("Último waypoint alcançado - parando e indo para idle");
            podeAtirar = false;
            isWaiting = false;
            SetAnimatorState(idle: true, some: false, aparece: false, atacando: false);
            yield break;
        }

        estaTeleportando = true;

        Debug.Log("Iniciando teleporte para próximo waypoint...");

        // FASE 1: Atacando false → Some true
        SetAnimatorState(idle: false, some: true, aparece: false, atacando: false);

        Debug.Log("Transição: Atacando false → Some true");
        yield return new WaitForSeconds(1f);

        // Teleporte para próximo waypoint
        GetNextWaypoint();

        // VERIFICA SE AINDA PODE ATIRAR APÓS O TELETRANSPORTE
        if (!podeAtirar)
        {
            SetAnimatorState(idle: true, some: false, aparece: false, atacando: false);
            estaTeleportando = false;
            yield break;
        }

        if (currentWaypointIndex < waypoints.Length && waypoints[currentWaypointIndex] != null)
        {
            transform.position = waypoints[currentWaypointIndex].position;
            Debug.Log("Teleportado para waypoint: " + currentWaypointIndex);
        }

        // FASE 2: Some false → Aparece true
        SetAnimatorState(idle: false, some: false, aparece: true, atacando: false);

        Debug.Log("Transição: Some false → Aparece true");
        yield return new WaitForSeconds(1f);

        // FASE 3: Aparece false → Atacando true
        SetAnimatorState(idle: false, some: false, aparece: false, atacando: true);

        Debug.Log("Transição: Aparece false → Atacando true");

        estaTeleportando = false;
        audioAtaque.Play();
        // Inicia o ataque após aparecer
        yield return new WaitForSeconds(1f);
        AtirarParaAlvoAleatorio();

        yield return new WaitForSeconds(0.5f);

        // Mantém no estado de atacando e inicia espera
        isWaiting = true;
        waitTimer = 0f;
    }

    IEnumerator IniciarAtaqueNoWaypoint()
    {
        // Vai direto para o estado de atacando no waypoint atual
        SetAnimatorState(idle: false, some: false, aparece: false, atacando: true);

        Debug.Log("Iniciando ataque no waypoint: " + currentWaypointIndex);

        yield return new WaitForSeconds(1f);
        AtirarParaAlvoAleatorio();

        yield return new WaitForSeconds(0.5f);

        // Inicia espera para próximo teleporte
        isWaiting = true;
        waitTimer = 0f;
    }

    void GetNextWaypoint()
    {
        if (isMovingForward)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (pararNoUltimoWaypoint)
                {
                    podeAtirar = false;
                    isWaiting = false;
                    Debug.Log("ÚLTIMO WAYPOINT: Parando arquiteto");
                }
                else
                {
                    isMovingForward = false;
                    currentWaypointIndex = waypoints.Length - 2;
                }
            }
        }
        else
        {
            currentWaypointIndex--;
            if (currentWaypointIndex < 0)
            {
                isMovingForward = true;
                currentWaypointIndex = 1;
            }
        }
    }

    // ---------------- TIRO ----------------

    void AtirarParaAlvoAleatorio()
    {
        if (!podeAtirar || pontoDeTiro == null || alvosDisponiveis.Count == 0) return;

        int randomIndex = Random.Range(0, alvosDisponiveis.Count);
        Transform alvoAtual = alvosDisponiveis[randomIndex];

        ultimoAlvoAtirado = alvoAtual;
        alvosDisponiveis.RemoveAt(randomIndex);

        GameObject projetilPrefab = GetProjetilPrefabForAlvo(alvoAtual);
        if (projetilPrefab != null)
        {
            GameObject projetil = Instantiate(projetilPrefab, pontoDeTiro.position, Quaternion.identity);
            audioTiro.Play();

            if (!projetil.TryGetComponent(out DisparoCodigo dc))
                dc = projetil.AddComponent<DisparoCodigo>();

            dc.SetTarget(alvoAtual.position);
        }

        if (alvosDisponiveis.Count == 0)
            ResetarAlvosDisponiveis();
    }

    GameObject GetProjetilPrefabForAlvo(Transform alvo)
    {
        int index = System.Array.IndexOf(alvosPossiveis, alvo);
        return index switch
        {
            0 or 4 => projetilVerdePrefab,
            1 or 3 => projetilAmareloPrefab,
            2 or 5 => projetilVermelhoPrefab,
            _ => projetilVerdePrefab
        };
    }

    void ResetarAlvosDisponiveis()
    {
        alvosDisponiveis.Clear();
        foreach (Transform alvo in alvosPossiveis)
            if (alvo != null && alvo != ultimoAlvoAtirado)
                alvosDisponiveis.Add(alvo);

        if (alvosDisponiveis.Count == 0 && alvosPossiveis.Length > 0)
            alvosDisponiveis.AddRange(alvosPossiveis);
    }

    // ---------------- EFEITO VISUAL ATUALIZADO ----------------

    // ---------------- EFEITO VISUAL ATUALIZADO ----------------

    public IEnumerator PiscarDuranteReducao(float duracao = 2.5f)
    {
        Debug.Log("=== INICIANDO EFEITO PISCAR ===");

        // Verifica se o SpriteRenderer está disponível
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer não atribuído!");
            yield break;
        }

        // Desativa outras animações durante o efeito
        SetAnimatorState(idle: false, some: false, aparece: false, atacando: false);

        // Para qualquer movimento ou ação
        podeAtirar = false;
        comandoRecebido = false;
        isWaiting = false;
        estaTeleportando = false;

        float tempoDecorrido = 0f;
        Color corOriginal = spriteRenderer.color;

        Debug.Log("Iniciando efeito de piscar por " + duracao + " segundos");

        // Fase 1: piscar entre cor original e preto por 2.5 segundos
        while (tempoDecorrido < duracao)
        {
            // Alterna entre cor original e preto (piscar rápido)
            float velocidadePiscar = 8f; // Frequência fixa para teste
            if (Mathf.FloorToInt(tempoDecorrido * velocidadePiscar) % 2 == 0)
            {
                spriteRenderer.color = corOriginal;
            }
            else
            {
                spriteRenderer.color = Color.black;
            }

            tempoDecorrido += Time.deltaTime;
            yield return null;
        }

        // Fase 2: Aplica o sprite final
        if (spriteFinal != null)
        {
            spriteRenderer.sprite = spriteFinal;
            spriteRenderer.color = Color.white; // Garante cor normal
            Debug.Log("Sprite final aplicado após piscar");
        }
        else
        {
            Debug.LogWarning("SpriteFinal não atribuído!");
        }

        arquitetoVasco = true;

        // Mantém o arquiteto parado no estado final
        SetAnimatorState(idle: true, some: false, aparece: false, atacando: false);

        Debug.Log("=== EFEITO PISCAR CONCLUÍDO ===");
    }

    public void IniciarMovimento()
    {
        if (comandoRecebido) return;

        comandoRecebido = true;
        indoParaPosicaoInicial = true;

        // Para a animação idle e prepara para o teleporte
        SetAnimatorState(idle: false, some: false, aparece: false, atacando: false);

        trap.SetActive(true);

        Debug.Log("Movimento iniciado - indo para teleporte");
    }

    public void Vasco() => arquitetoVasco = true;

    // ---------------- DEBUG ----------------

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);

        if (posicaoInicialMovimento != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(posicaoInicialMovimento.transform.position, Vector3.one * 0.5f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raioInteracao);
    }
}