using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class WireDragComLimite : MonoBehaviour
{
    [Header("Referências")]
    public Transform pontoFixo;
    public Transform holderVisual;
    public SpriteRenderer parteVisual;
    public Transform pontaFinal;

    [Header("Configurações")]
    public float distanciaMaxima = 3f;
    public float anguloMax = 60f;
    public float sensibilidade = 0.7f;

    [Header("Ângulo Inicial (Escolha no Inspector)")]
    [Tooltip("Define o ângulo inicial do fio. Pode ser 0°, 90° ou 270°")]
    public AnguloInicialTipo anguloInicial = AnguloInicialTipo._270;

    public enum AnguloInicialTipo
    {
        _0 = 0,
        _90 = 90,
        _270 = 270
    }

    [Header("Destinos de Conexão")]
    public List<Collider2D> destinos = new List<Collider2D>();
    public Collider2D destinoCorreto;

    [Header("Luz/Audio")]
    public GameObject luz;
    public AudioSource audioFio;

    private Camera cam;
    private bool foiConectadoAutomaticamente = false;
    private bool estaArrastando = false;
    private bool conectado = false;
    private bool conectadoNoErrado = false;
    private bool conexaoTravada = false;
    private float tamanhoInicial;
    private Collider2D destinoAtual = null;
    private Collider2D meuCollider;

    public static WireDragComLimite fioSendoArrastado = null;
    private static readonly Dictionary<Collider2D, bool> destinosOcupados = new Dictionary<Collider2D, bool>();

    void Awake()
    {
        cam = Camera.main;
        tamanhoInicial = parteVisual.size.x;
        meuCollider = GetComponent<Collider2D>();

        // Registrar destinos
        foreach (var d in destinos)
        {
            if (d != null && !destinosOcupados.ContainsKey(d) && !d.CompareTag("NaoToca"))
                destinosOcupados[d] = false;
        }
        if (destinoCorreto != null && !destinosOcupados.ContainsKey(destinoCorreto))
            destinosOcupados[destinoCorreto] = false;

        // ?? Define o ângulo inicial com base na escolha no Inspector
        float anguloZ = (float)anguloInicial;
        holderVisual.rotation = Quaternion.Euler(0f, 0f, anguloZ);

        // Define a direção inicial de acordo com o ângulo escolhido
        Vector3 direcaoBase = Vector3.down; // padrão
        if (anguloInicial == AnguloInicialTipo._0) direcaoBase = Vector3.right;
        else if (anguloInicial == AnguloInicialTipo._90) direcaoBase = Vector3.up;
        else if (anguloInicial == AnguloInicialTipo._270) direcaoBase = Vector3.down;

        AtualizarVisual(direcaoBase, tamanhoInicial);
    }

    void Update()
    {
        if (conexaoTravada) return;

        Vector3 inputMundo = Vector3.zero;
        bool pressionando = false;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            inputMundo = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            pressionando = true;
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            inputMundo = cam.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            pressionando = true;
        }

        inputMundo.z = 0f;

        if (fioSendoArrastado != null && fioSendoArrastado != this && pressionando)
            return;

        if (pressionando && !estaArrastando && (conectado || conectadoNoErrado) && !foiConectadoAutomaticamente)
        {
            Collider2D hit = Physics2D.OverlapPoint(inputMundo);
            if (hit != null && hit.transform.IsChildOf(transform))
            {
                LiberarDestinoAtual();
                estaArrastando = true;
                conectado = false;
                conectadoNoErrado = false;
                fioSendoArrastado = this;
                if (luz != null) luz.SetActive(false);
            }
        }

        if (pressionando)
        {
            if (!estaArrastando && !conectado && !conectadoNoErrado)
            {
                Collider2D hit = Physics2D.OverlapPoint(inputMundo);
                if (hit != null && hit.transform.IsChildOf(transform))
                {
                    if (fioSendoArrastado == null)
                    {
                        estaArrastando = true;
                        fioSendoArrastado = this;
                    }
                }
            }

            if (estaArrastando)
            {
                Vector3 dir = inputMundo - pontoFixo.position;

                // Base do ângulo depende do anguloInicial
                Vector3 baseDir = Vector3.down;
                if (anguloInicial == AnguloInicialTipo._0) baseDir = Vector3.right;
                else if (anguloInicial == AnguloInicialTipo._90) baseDir = Vector3.up;
                else if (anguloInicial == AnguloInicialTipo._270) baseDir = Vector3.down;

                float angulo = Vector3.SignedAngle(baseDir, dir, Vector3.forward);
                angulo = Mathf.Clamp(angulo, -anguloMax, anguloMax);

                Vector3 dirLimitado = Quaternion.Euler(0, 0, angulo) * baseDir;
                float distanciaTotal = Vector3.Distance(pontoFixo.position, inputMundo) * sensibilidade;
                float distancia = Mathf.Clamp(distanciaTotal, 2f, distanciaMaxima);

                AtualizarVisual(dirLimitado, distancia);

                if (meuCollider != null)
                    meuCollider.enabled = false;
            }
        }
        else if (estaArrastando)
        {
            estaArrastando = false;
            fioSendoArrastado = null;

            if (meuCollider != null)
                meuCollider.enabled = true;

            FiacaoRoboInvertido fiacao = FindObjectOfType<FiacaoRoboInvertido>();
            if (fiacao != null && fiacao.lightObject != null)
                fiacao.lightObject.SetActive(false);

            if (pontaFinal == null) return;

            if (destinoCorreto != null &&
                destinoCorreto.OverlapPoint(pontaFinal.position) &&
                !destinosOcupados[destinoCorreto] &&
                !ColidiuComOutroFio() &&
                !destinoCorreto.CompareTag("NaoToca"))
            {
                ConectarNoDestino(destinoCorreto);
                return;
            }

            foreach (var destino in destinos)
            {
                if (destino == null || destino == destinoCorreto ||
                    (destinosOcupados.ContainsKey(destino) && destinosOcupados[destino]) ||
                    destino.CompareTag("NaoToca")) continue;

                if (destino.OverlapPoint(pontaFinal.position) && !ColidiuComOutroFio())
                {
                    ConectarNoDestino(destino);
                    return;
                }
            }

            StartCoroutine(VoltarParaOrigem());
        }
    }

    void AtualizarVisual(Vector3 direcao, float comprimento)
    {
        float anguloZ = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        holderVisual.eulerAngles = new Vector3(0f, 0f, anguloZ);
        parteVisual.size = new Vector2(comprimento, parteVisual.size.y);

        if (pontaFinal != null)
            pontaFinal.localPosition = new Vector3(parteVisual.size.x, 0f, 0f);
    }

    IEnumerator VoltarParaOrigem()
    {
        LiberarDestinoAtual();

        float tempo = 0f;
        float duracao = 0.25f;
        float tamanhoAtual = parteVisual.size.x;

        while (tempo < 1f)
        {
            tempo += Time.deltaTime / duracao;
            float novoTamanho = Mathf.Lerp(tamanhoAtual, tamanhoInicial, tempo);
            parteVisual.size = new Vector2(novoTamanho, parteVisual.size.y);
            if (pontaFinal != null)
                pontaFinal.localPosition = new Vector3(parteVisual.size.x, 0f, 0f);
            yield return null;
        }

        parteVisual.size = new Vector2(tamanhoInicial, parteVisual.size.y);
        holderVisual.rotation = Quaternion.Euler(0f, 0f, (float)anguloInicial);
        if (pontaFinal != null)
            pontaFinal.localPosition = new Vector3(tamanhoInicial, 0f, 0f);
    }

    // Resto das funções (ConectarNoDestino, LiberarDestinoAtual, etc) permanece igual.
    void ConectarNoDestino(Collider2D destino)
    {
        if (destino.CompareTag("NaoToca")) return;
        destinoAtual = destino;

        if (destino == destinoCorreto)
        {
            conectado = true;
            if (luz != null) luz.SetActive(true);
        }
        else conectadoNoErrado = true;

        destinosOcupados[destino] = true;
        audioFio.Play();
    }

    void LiberarDestinoAtual()
    {
        if (destinoAtual != null && destinosOcupados.ContainsKey(destinoAtual))
        {
            destinosOcupados[destinoAtual] = false;
            destinoAtual = null;
        }
        conectadoNoErrado = false;
        conectado = false;
    }

    bool ColidiuComOutroFio()
    {
        if (pontaFinal == null) return false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(pontaFinal.position, 0.1f);
        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != gameObject && hit.GetComponent<WireDragComLimite>() != null)
                return true;
        }
        return false;
    }
}
