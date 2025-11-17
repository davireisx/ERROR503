using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SistemaDialogoDoisPersonagens : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public float interactionRange = 3f;
    public GameObject joystick;
    public GameObject botaoInteragir;
    public GameObject hud;

    [Header("Caixas de Diálogo")]
    public GameObject caixaPersonagem1; // Ex: Caixa azul
    public GameObject caixaPersonagem2; // Ex: Caixa vermelha
    public Button botaoAvancar1;
    public Button botaoAvancar2;
    public Text check;

    [Header("Falas")]
    public GameObject[] falas; // Todas as falas em sequência
    public int ultimoIndicePersonagem1 = 2; // Definido no Inspector

    [Header("Fade Final")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;
    public float fadeStayTime = 5f;


    private int falaAtual = 0;
    private bool dialogoAtivo = false;
    private bool playerInRange = false;

    void Start()
    {
        DesativarFalas();

        caixaPersonagem1?.SetActive(false);
        caixaPersonagem2?.SetActive(false);
        botaoInteragir?.SetActive(false);

        if (botaoAvancar1 != null) botaoAvancar1.onClick.AddListener(AvancarFala);
        if (botaoAvancar2 != null) botaoAvancar2.onClick.AddListener(AvancarFala);
    }

    void Update()
    {
        if (player == null) return;

        float distancia = Vector2.Distance(transform.position, player.position);
        playerInRange = distancia <= interactionRange;

        if (!dialogoAtivo)
            botaoInteragir?.SetActive(playerInRange);

    }

    public void IniciarDialogo()
    {
        falaAtual = 0;
        dialogoAtivo = true;

        botaoInteragir?.SetActive(false);
        hud?.SetActive(false);
        joystick?.SetActive(false);

        AtualizarCaixaDialogo();
        MostrarFalaAtual();
    }

    void AvancarFala()
    {
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
        botaoInteragir.SetActive(false);
        hud?.SetActive(true);
        caixaPersonagem1?.SetActive(false);
        caixaPersonagem2?.SetActive(false);
        joystick?.SetActive(false);

        check.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        hud.SetActive(false);
        check.gameObject.SetActive(false);

        dialogoAtivo = false;

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = true; // se quiser bloquear cliques
            fadeGroup.interactable = false;

            float tempo = 0f;
            while (tempo < fadeDuration)
            {
                tempo += Time.deltaTime;
                fadeGroup.alpha = Mathf.Lerp(0f, 1f, tempo / fadeDuration);
                yield return null;
            }

            fadeGroup.alpha = 1f;
        }
    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
