using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Junkinho : MonoBehaviour
{
    [Header("Configurações de Spawn")]
    public bool spawnAleatorio = true;
    public Transform spawnPointEspecifico;

    [Header("Alvos")]
    public Transform personagemPrincipal;
    public Transform veneno;

    [Header("Configurações de Patrulha")]
    public List<Transform> waypoints = new List<Transform>();
    public float speed = 3f;
    public float waitTime = 1f;
    public float minDistanceToWaypoint = 0.1f;

    [Header("Comportamento Inteligente")]
    [Range(0f, 2f)]
    public float pesoPerseguicao = 1.5f;
    [Range(0f, 1f)]
    public float pesoFuga = 0.2f;
    public float distanciaDetecao = 8f;
    public float distanciaPerseguicaoAgressiva = 12f;

    [Header("Comportamento Visual")]
    public bool rotateToFaceMovement = true;
    public float rotationLerp = 10f;

    [Header("Sistema de Morte")]
    public VerificadorWaves leva;
    public Transform spawnPointMorte;
    public float velocidadeBonusMorte = 10f;
    public Color corPiscada1 = Color.white;
    public Color corPiscada2 = Color.red;
    public float velocidadePiscada = 5f;
    public GameObject efeitoMorte;
    public float tempoAntesEfeito = 1.5f;
    public float tempoEfeitoAtivo = 1.5f;

    [Header("Input System")]
    public InputActionReference spawnAction;
    public VerificadorWaves verificaMorteJunkinho;

    // Variáveis privadas
    private Transform currentTarget;
    private Transform lastTarget;
    private bool isMoving = false;
    private bool isWaiting = false;
    private bool emMorte = false;
    private bool patrulhaAtiva = false;
    private Dictionary<string, List<string>> connections = new Dictionary<string, List<string>>();
    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private float velocidadeOriginal;
    private Animator animator;
    private bool chegouSpawnMorte = false;

    void Awake()
    {
        InitializeComponents();
    }

    void OnDestroy()
    {
        CleanupInput();
    }

    void Start()
    {
        InitializeJunkinho();
    }

    void Update()
    {
        UpdateJunkinhoState();
        DrawDebugVisuals();
    }

    #region Inicialização
    void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null) corOriginal = spriteRenderer.color;
        velocidadeOriginal = speed;

        SetupInputSystem();
        SetupEfeitoMorte();
    }

    void SetupInputSystem()
    {
        if (spawnAction != null)
        {
            spawnAction.action.Enable();
            spawnAction.action.performed += OnSpawnInput;
        }
    }

    void SetupEfeitoMorte()
    {
        if (efeitoMorte != null)
            efeitoMorte.SetActive(false);
    }

    void CleanupInput()
    {
        if (spawnAction != null)
            spawnAction.action.performed -= OnSpawnInput;
    }

    void InitializeJunkinho()
    {
        if (animator != null)
            animator.SetBool("idle", true);

        if (waypoints.Count == 0)
        {
            Debug.LogWarning("Nenhum waypoint configurado!");
            enabled = false;
            return;
        }

        SetupConnections();
        SpawnJunkinho();
        StartCoroutine(SequenciaSpawnInicial());
    }
    #endregion

    #region Sequência de Spawn Inicial
    IEnumerator SequenciaSpawnInicial()
    {
        Debug.Log("Iniciando sequência de spawn...");

        // 1. Spawn no waypoint 8
        Transform waypoint8 = waypoints.Find(wp => wp != null && wp.name == "8");
        if (waypoint8 != null)
        {
            transform.position = waypoint8.position;
            currentTarget = waypoint8;
            Debug.Log($"Spawnou no waypoint 8");
        }


        // 2. Ir para o waypoint 1
        Transform waypoint1 = waypoints.Find(wp => wp != null && wp.name == "1");
        if (waypoint1 != null)
        {
            Debug.Log("Indo para waypoint 1...");
            yield return StartCoroutine(MoverParaWaypoint(waypoint1));
            currentTarget = waypoint1;
        }

        // 3. Ir para o waypoint 2 (spawn final)
        Transform waypoint2 = waypoints.Find(wp => wp != null && wp.name == "2");
        if (waypoint2 != null)
        {
            Debug.Log("Indo para waypoint 2 (spawn final)...");
            yield return StartCoroutine(MoverParaWaypoint(waypoint2));
            currentTarget = waypoint2;
        }

        // 4. Iniciar patrulha normal
        Debug.Log("Sequência de spawn concluída. Iniciando patrulha normal...");
        patrulhaAtiva = true;
        StartCoroutine(PatrolRoutine());
    }

    IEnumerator MoverParaWaypoint(Transform target)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = target.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (Vector3.Distance(transform.position, targetPosition) > minDistanceToWaypoint)
        {
            float distanceCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distanceCovered / journeyLength;

            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            if (rotateToFaceMovement)
            {
                Vector3 direction = (targetPosition - transform.position).normalized;
                if (direction.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerp);
                }
            }

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
    #endregion

    #region Atualização
    void UpdateJunkinhoState()
    {
        if (!emMorte && leva != null && leva.concluidos > 0 && patrulhaAtiva)
            IniciarMorte();

        if (emMorte && spriteRenderer != null && !chegouSpawnMorte)
            UpdatePiscadaMorte();
    }

    void UpdatePiscadaMorte()
    {
        float alpha = Mathf.PingPong(Time.time * velocidadePiscada, 1f);
        spriteRenderer.color = Color.Lerp(corPiscada1, corPiscada2, alpha);
    }

    void DrawDebugVisuals()
    {
        if (currentTarget != null)
        {
            Color corLinha = emMorte ? Color.red : (PersonagemMuitoProximo() ? Color.yellow : Color.green);
            Debug.DrawLine(transform.position, currentTarget.position, corLinha);
        }

        if (personagemPrincipal != null)
        {
            Color corPersonagem = PersonagemMuitoProximo() ? Color.yellow : Color.blue;
            Debug.DrawLine(transform.position, personagemPrincipal.position, corPersonagem);
        }

        if (veneno != null)
            Debug.DrawLine(transform.position, veneno.position, Color.red);
    }
    #endregion

    #region Sistema de Morte - CORRIGIDO
    void IniciarMorte()
    {
        if (emMorte) return;

        emMorte = true;
        patrulhaAtiva = false;
        speed = velocidadeOriginal + velocidadeBonusMorte;
        Debug.Log($"JUNKINHO ENTROU EM MODO DE MORTE! Velocidade: {speed}");

        StopAllCoroutines();
        StartCoroutine(SequenciaMorte());
    }

    IEnumerator SequenciaMorte()
    {
        // 1. Primeiro ir para o waypoint 3
        Transform waypoint3 = waypoints.Find(wp => wp != null && wp.name == "3");
        if (waypoint3 != null)
        {
            Debug.Log("Indo para waypoint 3...");
            yield return StartCoroutine(IrParaWaypointRespeitandoConexoes(waypoint3));
        }

        // 2. Depois ir para o waypoint 4
        Transform waypoint4 = waypoints.Find(wp => wp != null && wp.name == "4");
        if (waypoint4 != null)
        {
            Debug.Log("Indo para waypoint 4...");
            yield return StartCoroutine(IrParaWaypointRespeitandoConexoes(waypoint4));
        }

        // 3. Depois ir para o waypoint 5
        Transform waypoint5 = waypoints.Find(wp => wp != null && wp.name == "5");
        if (waypoint5 != null)
        {
            Debug.Log("Indo para waypoint 5...");
            yield return StartCoroutine(IrParaWaypointRespeitandoConexoes(waypoint5));
        }

        // 4. Depois ir para o waypoint 6
        Transform waypoint6 = waypoints.Find(wp => wp != null && wp.name == "6");
        if (waypoint6 != null)
        {
            Debug.Log("Indo para waypoint 6...");
            yield return StartCoroutine(IrParaWaypointRespeitandoConexoes(waypoint6));
        }

        // 5. Finalmente ir para o waypoint 7
        Transform waypoint7 = waypoints.Find(wp => wp != null && wp.name == "7");
        if (waypoint7 != null)
        {
            Debug.Log("Indo para waypoint 7...");
            yield return StartCoroutine(IrParaWaypointRespeitandoConexoes(waypoint7));
        }
        else
        {
            Debug.LogError("Waypoint 7 não encontrado!");
        }

        // 6. Por último ir para o spawn point de morte
        if (spawnPointMorte != null)
        {
            Debug.Log("Indo para spawn point de morte...");
            yield return StartCoroutine(MoverParaWaypointMorte(spawnPointMorte));
            yield return StartCoroutine(ExecutarMorteCompleta());
        }
        else
        {
            Debug.LogError("Spawn point de morte não configurado!");
        }
    }

    IEnumerator ExecutarMorteCompleta()
    {
        chegouSpawnMorte = true;

        if (spriteRenderer != null)
            spriteRenderer.color = corOriginal;

        if (animator != null)
        {
            animator.SetBool("morte", true);
            Debug.Log("Animação de morte ativada!");
        }

        yield return new WaitForSeconds(tempoAntesEfeito);

        if (efeitoMorte != null)
        {
            efeitoMorte.SetActive(true);
            Debug.Log("Efeito de morte ativado!");
        }

        yield return new WaitForSeconds(tempoEfeitoAtivo);

        if (efeitoMorte != null)
            efeitoMorte.SetActive(false);

        Debug.Log("JUNKINHO MORREU!");
        gameObject.SetActive(false);

        if (verificaMorteJunkinho != null)
            verificaMorteJunkinho.VerificarTodos();
    }
    #endregion

    #region Navegação e Pathfinding
    IEnumerator IrParaWaypointRespeitandoConexoes(Transform destinoFinal)
    {
        List<Transform> caminho = EncontrarCaminho(currentTarget, destinoFinal);

        if (caminho == null || caminho.Count == 0)
        {
            Debug.LogWarning($"Não foi possível encontrar caminho para {destinoFinal.name}!");
            yield return StartCoroutine(EncontrarCaminhoFallback(destinoFinal));
            yield break;
        }

        Debug.Log($"Caminho válido encontrado: {string.Join(" -> ", caminho.ConvertAll(wp => wp.name))}");

        for (int i = 1; i < caminho.Count; i++)
        {
            Transform waypoint = caminho[i];
            Debug.Log($"Movendo para: {waypoint.name} (etapa {i}/{caminho.Count - 1})");

            yield return StartCoroutine(MoverParaWaypointMorte(waypoint));
            currentTarget = waypoint;
        }
    }

    List<Transform> EncontrarCaminho(Transform inicio, Transform destino)
    {
        if (inicio == null || destino == null) return null;

        var fila = new Queue<Transform>();
        var veioDe = new Dictionary<Transform, Transform>();
        var visitados = new HashSet<Transform>();

        fila.Enqueue(inicio);
        visitados.Add(inicio);
        veioDe[inicio] = null;

        while (fila.Count > 0)
        {
            Transform atual = fila.Dequeue();

            if (atual == destino)
                return ReconstruirCaminho(veioDe, destino);

            string nomeAtual = atual.name;

            if (connections.ContainsKey(nomeAtual))
            {
                foreach (string nomeVizinho in connections[nomeAtual])
                {
                    Transform vizinho = waypoints.Find(wp => wp != null && wp.name == nomeVizinho);

                    if (vizinho != null && !visitados.Contains(vizinho) && EhConexaoValida(atual, vizinho))
                    {
                        visitados.Add(vizinho);
                        veioDe[vizinho] = atual;
                        fila.Enqueue(vizinho);
                    }
                }
            }
        }

        Debug.LogWarning($"Não foi possível encontrar caminho de {inicio.name} para {destino.name}");
        return null;
    }

    List<Transform> ReconstruirCaminho(Dictionary<Transform, Transform> veioDe, Transform destino)
    {
        var caminho = new List<Transform>();
        Transform atual = destino;

        while (atual != null)
        {
            caminho.Insert(0, atual);
            atual = veioDe[atual];
        }

        return CaminhoEhValido(caminho) ? caminho : null;
    }

    bool EhConexaoValida(Transform de, Transform para)
    {
        if (de == null || para == null) return false;
        string nomeDe = de.name;
        return connections.ContainsKey(nomeDe) && connections[nomeDe].Contains(para.name);
    }

    bool CaminhoEhValido(List<Transform> caminho)
    {
        if (caminho == null || caminho.Count < 2) return true;

        for (int i = 0; i < caminho.Count - 1; i++)
        {
            if (!EhConexaoValida(caminho[i], caminho[i + 1]))
            {
                Debug.LogError($"Conexão inválida: {caminho[i].name} -> {caminho[i + 1].name}");
                return false;
            }
        }

        return true;
    }

    IEnumerator EncontrarCaminhoFallback(Transform destinoFinal)
    {
        Debug.Log("Usando método fallback...");

        Transform atual = currentTarget;
        int tentativas = 0;
        const int maxTentativas = 20;

        while (atual != destinoFinal && tentativas < maxTentativas)
        {
            tentativas++;
            Transform proximo = EncontrarProximoMaisPerto(atual, destinoFinal);

            if (proximo == null || proximo == atual)
            {
                Debug.LogError($"Fallback falhou em {atual.name}");
                yield break;
            }

            Debug.Log($"Fallback: {atual.name} -> {proximo.name}");
            yield return StartCoroutine(MoverParaWaypointMorte(proximo));
            currentTarget = atual = proximo;

        }

        if (tentativas >= maxTentativas)
            Debug.LogError("Fallback excedeu tentativas!");
    }

    Transform EncontrarProximoMaisPerto(Transform atual, Transform destino)
    {
        if (atual == null || destino == null) return null;

        string nomeAtual = atual.name;
        Transform melhorProximo = null;
        float menorDistancia = Mathf.Infinity;

        if (connections.ContainsKey(nomeAtual))
        {
            foreach (string nomeVizinho in connections[nomeAtual])
            {
                Transform vizinho = waypoints.Find(wp => wp != null && wp.name == nomeVizinho);
                if (vizinho != null && vizinho != lastTarget)
                {
                    float distancia = Vector3.Distance(vizinho.position, destino.position);
                    if (distancia < menorDistancia)
                    {
                        menorDistancia = distancia;
                        melhorProximo = vizinho;
                    }
                }
            }
        }

        return melhorProximo;
    }

    IEnumerator MoverParaWaypointMorte(Transform target)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = target.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (Vector3.Distance(transform.position, targetPosition) > minDistanceToWaypoint)
        {
            float distanceCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distanceCovered / journeyLength;

            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            if (rotateToFaceMovement)
            {
                Vector3 direction = (targetPosition - transform.position).normalized;
                if (direction.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerp);
                }
            }

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
    #endregion

    #region Comportamento Inteligente
    bool PersonagemMuitoProximo() => personagemPrincipal != null &&
        Vector3.Distance(transform.position, personagemPrincipal.position) <= distanciaPerseguicaoAgressiva;

    bool PersonagemProximo() => personagemPrincipal != null &&
        Vector3.Distance(transform.position, personagemPrincipal.position) <= distanciaDetecao;

    bool VenenoProximo() => veneno != null &&
        Vector3.Distance(transform.position, veneno.position) <= distanciaDetecao;

    void SetupConnections()
    {
        connections.Clear();
        connections.Add("1", new List<string> { "2", "8" });
        connections.Add("2", new List<string> { "1", "3", "7" });
        connections.Add("3", new List<string> { "2", "4" });
        connections.Add("4", new List<string> { "3", "5", "9" });
        connections.Add("5", new List<string> { "4", "6" });
        connections.Add("6", new List<string> { "5", "7" });
        connections.Add("7", new List<string> { "2", "6" });
        connections.Add("8", new List<string> { "1", "9" });
        connections.Add("9", new List<string> { "4", "8" });
    }

    List<Transform> GetValidNextWaypoints()
    {
        var validWaypoints = new List<Transform>();
        if (currentTarget == null || emMorte || !patrulhaAtiva) return validWaypoints;

        string currentName = currentTarget.name;
        if (!connections.ContainsKey(currentName)) return validWaypoints;

        foreach (string connectedName in connections[currentName])
        {
            Transform connectedWaypoint = waypoints.Find(wp => wp != null && wp.name == connectedName);
            if (connectedWaypoint != null && connectedWaypoint != lastTarget)
                validWaypoints.Add(connectedWaypoint);
        }

        if (validWaypoints.Count == 0)
        {
            foreach (string connectedName in connections[currentName])
            {
                Transform connectedWaypoint = waypoints.Find(wp => wp != null && wp.name == connectedName);
                if (connectedWaypoint != null)
                    validWaypoints.Add(connectedWaypoint);
            }
        }

        return validWaypoints;
    }

    Transform GetMelhorWaypointInteligente()
    {
        var waypointsValidos = GetValidNextWaypoints();
        if (waypointsValidos.Count == 0) return currentTarget;

        Transform melhorWaypoint = waypointsValidos[0];
        float melhorPontuacao = -Mathf.Infinity;

        foreach (Transform wp in waypointsValidos)
        {
            float pontuacao = CalcularPontuacaoWaypoint(wp);
            if (pontuacao > melhorPontuacao)
            {
                melhorPontuacao = pontuacao;
                melhorWaypoint = wp;
            }
        }

        return melhorWaypoint;
    }

    float CalcularPontuacaoWaypoint(Transform waypoint)
    {
        float pontuacao = 0f;

        if (personagemPrincipal != null)
        {
            float distancia = Vector3.Distance(waypoint.position, personagemPrincipal.position);
            float multiplier = PersonagemMuitoProximo() ? 2f : 1f;
            pontuacao += (1f - Mathf.Clamp01(distancia / distanciaDetecao)) * pesoPerseguicao * multiplier;
        }

        if (veneno != null)
        {
            float distancia = Vector3.Distance(waypoint.position, veneno.position);
            pontuacao += Mathf.Clamp01(distancia / distanciaDetecao) * pesoFuga;
        }

        pontuacao += Random.Range(-0.05f, 0.05f);
        return pontuacao;
    }
    #endregion

    #region Patrol e Movimento
    IEnumerator PatrolRoutine()
    {

        while (true)
        {
            if (emMorte || !patrulhaAtiva) yield break;

            float tempoEspera = PersonagemMuitoProximo() ? waitTime * 0.3f : waitTime;

            if (!isWaiting)
            {
                isWaiting = true;
                yield return new WaitForSeconds(tempoEspera);
                isWaiting = false;
            }

            bool devePerseguir = PersonagemProximo() || PersonagemMuitoProximo() || VenenoProximo();
            Transform novoAlvo = devePerseguir ? GetMelhorWaypointInteligente() : GetWaypointAleatorio();

            if (novoAlvo != null && novoAlvo != currentTarget)
            {
                lastTarget = currentTarget;
                currentTarget = novoAlvo;
                yield return StartCoroutine(MoveToTarget(currentTarget));
            }
            else
            {
                yield return new WaitForSeconds(tempoEspera);
            }
        }
    }

    Transform GetWaypointAleatorio()
    {
        var validos = GetValidNextWaypoints();
        return validos.Count > 0 ? validos[Random.Range(0, validos.Count)] : currentTarget;
    }

    IEnumerator MoveToTarget(Transform target)
    {
        if (emMorte || !patrulhaAtiva) yield break;

        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = target.position;
        float journeyLength = Vector3.Distance(startPos, targetPos);
        float startTime = Time.time;

        float velocidadeAtual = PersonagemMuitoProximo() ? speed * 1.3f : speed;

        while (Vector3.Distance(transform.position, targetPos) > minDistanceToWaypoint)
        {
            if (emMorte || !patrulhaAtiva) yield break;

            float distanceCovered = (Time.time - startTime) * velocidadeAtual;
            float fraction = distanceCovered / journeyLength;

            transform.position = Vector3.Lerp(startPos, targetPos, fraction);

            if (rotateToFaceMovement)
            {
                Vector3 direction = (targetPos - transform.position).normalized;
                if (direction.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion targetRot = Quaternion.Euler(0, 0, angle);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotationLerp);
                }
            }

            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }
    #endregion

    #region Spawn e Respawn
    void OnSpawnInput(InputAction.CallbackContext context)
    {
        if (context.performed) Respawn();
    }

    void SpawnJunkinho()
    {
        Transform waypoint8 = waypoints.Find(wp => wp != null && wp.name == "8");
        if (waypoint8 != null)
        {
            transform.position = waypoint8.position;
            currentTarget = waypoint8;
        }
        else
        {
            Transform spawnPoint = spawnPointEspecifico != null ? spawnPointEspecifico :
                spawnAleatorio ? waypoints[Random.Range(0, waypoints.Count)] : waypoints[0];
            transform.position = spawnPoint.position;
            currentTarget = spawnPoint;
        }

        ResetEstadoMorte();
    }

    void ResetEstadoMorte()
    {
        emMorte = false;
        chegouSpawnMorte = false;
        patrulhaAtiva = false;
        speed = velocidadeOriginal;

        if (spriteRenderer != null) spriteRenderer.color = corOriginal;
        if (animator != null) animator.SetBool("morte", false);
        if (efeitoMorte != null) efeitoMorte.SetActive(false);
    }

    public void Respawn()
    {
        StopAllCoroutines();
        isMoving = false;
        isWaiting = false;
        lastTarget = null;

        ResetEstadoMorte();
        SpawnJunkinho();
        StartCoroutine(SequenciaSpawnInicial());
    }

    public void SpawnEmPosicao(Transform novaPosicao)
    {
        StopAllCoroutines();
        isMoving = false;
        isWaiting = false;
        lastTarget = null;

        ResetEstadoMorte();
        transform.position = novaPosicao.position;
        currentTarget = novaPosicao;
        patrulhaAtiva = true;
        StartCoroutine(PatrolRoutine());
    }
    #endregion

    #region Debug e Gizmos
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        DrawWaypoints();
        DrawSpawnMorte();
        DrawCurrentTarget();
        DrawDetectionZones();
        DrawConnections();
    }

    void DrawWaypoints()
    {
        Gizmos.color = Color.yellow;
        foreach (var wp in waypoints)
        {
            if (wp == null) continue;
            Gizmos.DrawSphere(wp.position, 0.2f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(wp.position + Vector3.up * 0.4f, wp.name);
#endif
        }
    }

    void DrawSpawnMorte()
    {
        if (spawnPointMorte != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(spawnPointMorte.position, Vector3.one * 0.5f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(spawnPointMorte.position + Vector3.up * 0.8f, "SPAWN MORTE");
#endif
        }
    }

    void DrawCurrentTarget()
    {
        if (Application.isPlaying && currentTarget != null)
        {
            Gizmos.color = emMorte ? Color.red : (PersonagemMuitoProximo() ? Color.yellow : Color.green);
            Gizmos.DrawWireSphere(currentTarget.position, 0.3f);
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    void DrawDetectionZones()
    {
        Gizmos.color = new Color(0f, 0f, 1f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, distanciaDetecao);

        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, distanciaPerseguicaoAgressiva);
    }

    void DrawConnections()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.6f);

        if (Application.isPlaying && connections.Count > 0)
        {
            foreach (var connection in connections)
            {
                Transform startWP = waypoints.Find(wp => wp != null && wp.name == connection.Key);
                if (startWP == null) continue;

                foreach (string connectedName in connection.Value)
                {
                    Transform endWP = waypoints.Find(wp => wp != null && wp.name == connectedName);
                    if (endWP != null)
                        Gizmos.DrawLine(startWP.position, endWP.position);
                }
            }
        }
        else
        {
            DrawConnection("1", "2");
            DrawConnection("1", "8");
            DrawConnection("2", "3");
            DrawConnection("2", "7");
            DrawConnection("3", "4");
            DrawConnection("4", "5");
            DrawConnection("4", "9");
            DrawConnection("5", "6");
            DrawConnection("6", "7");
            DrawConnection("8", "9");
        }
    }

    void DrawConnection(string wp1, string wp2)
    {
        Transform t1 = waypoints.Find(w => w != null && w.name == wp1);
        Transform t2 = waypoints.Find(w => w != null && w.name == wp2);
        if (t1 != null && t2 != null)
            Gizmos.DrawLine(t1.position, t2.position);
    }
    #endregion
}