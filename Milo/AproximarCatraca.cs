using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Catraca))]
public class AproximarCatraca : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private Button botaoInteracao;
    [SerializeField] private Transform jogador;
    [SerializeField] private float raioInteracao = 2.5f;
    [SerializeField] private GameObject painelMinigame;
    [SerializeField] private DesafioDasCores desafioDasCores;
    [SerializeField] private CatracaManager catracaManager;

    [Header("Audio Botão")]
    [SerializeField] private AudioSource audioBotao;

    private Camera mainCam;
    private RectTransform botaoRect;
    private Catraca catraca;

    void Start()
    {
        mainCam = Camera.main;
        botaoRect = botaoInteracao.GetComponent<RectTransform>();
        catraca = GetComponent<Catraca>();

        if (botaoInteracao == null || painelMinigame == null)
        {
            Debug.LogError("AproximarCatraca: referência perdida!");
            enabled = false;
            return;
        }

        if (desafioDasCores == null)
            desafioDasCores = painelMinigame.GetComponent<DesafioDasCores>();

        botaoInteracao.gameObject.SetActive(false);

        botaoInteracao.onClick.AddListener(OnBotaoClicked);
    }

    void Update()
    {
        float dist = Vector3.Distance(jogador.position, transform.position);

        // Oculta se estiver longe
        if (dist > raioInteracao)
        {
            botaoInteracao.gameObject.SetActive(false);
            return;
        }

        // Debug console

        // Verifica se a catraca deve ativar botão
        bool podeAtivar = catraca.DeveAtivarBotao();

        // --- REMOVIDA a checagem de TodasEstaoCorrompidas() ---
        // Agora cada catraca decide individualmente se mostra o botão
        if (!podeAtivar)
        {
            botaoInteracao.gameObject.SetActive(false);
            return;
        }

        // Posiciona e mostra
        Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position);
        botaoRect.position = screenPos + new Vector3(0, 50f, 0);

        if (!botaoInteracao.gameObject.activeSelf)
            botaoInteracao.gameObject.SetActive(true);
    }

    public void OnBotaoClicked()
    {
        audioBotao.Play();
        botaoInteracao.gameObject.SetActive(false);
        painelMinigame.SetActive(true);
        Catraca catracaAtual = GetComponent<Catraca>();

        catracaAtual.Consertar();
        desafioDasCores.IniciarMinigame(catracaAtual);
    }

    void OnDrawGizmos()
    {
        if (jogador != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, raioInteracao);
            Gizmos.DrawLine(transform.position, jogador.position);
        }
    }
}
