using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogoEstacaoDebora : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public GameObject joystick;
    public GameObject hud;

    [Header("Caixas de Diálogo")]
    public GameObject caixaPersonagem1;
    public GameObject caixaPersonagem2;
    public Button botaoAvancar1;
    public Button botaoAvancar2;
    public AudioSource next;

    [Header("Falas")]
    public GameObject[] falas;
    public bool[] falasDoPersonagem1;

    [Header("Configuração do Diálogo Automático")]
    public float tempoAntesDeIniciar = 1f;

    private int falaAtual = 0;
    private bool dialogoAtivo = false;

    // Callback para ser chamado quando o diálogo terminar
    public System.Action onDialogoFinalizado;

    void Start()
    {
        DesativarFalas();
        caixaPersonagem1?.SetActive(false);
        caixaPersonagem2?.SetActive(false);
    }

    /// <summary>
    /// Inicia o diálogo normalmente
    /// </summary>
    public void IniciarDialogo()
    {
        falaAtual = 0;
        dialogoAtivo = true;

        hud?.SetActive(false);
        joystick?.SetActive(false);

        AtualizarCaixaDialogo();
        MostrarFalaAtual();
    }

    /// <summary>
    /// Inicia o diálogo e dispara um callback ao final
    /// </summary>
    public void IniciarDialogoComCallback(System.Action callback)
    {
        onDialogoFinalizado = callback;
        IniciarDialogo();
    }

    public void AvancarFala()
    {
        if (!dialogoAtivo) return;

        next?.Play();
        falaAtual++;

        if (falaAtual < falas.Length)
        {
            AtualizarCaixaDialogo();
            MostrarFalaAtual();
        }
        else
        {
            // Finalizou diálogo
            DesativarFalas();
            caixaPersonagem1?.SetActive(false);
            caixaPersonagem2?.SetActive(false);
            dialogoAtivo = false;

            hud?.SetActive(true);
            joystick?.SetActive(true);

            // Dispara evento de finalização
            onDialogoFinalizado?.Invoke();
            onDialogoFinalizado = null;
        }
    }

    void AtualizarCaixaDialogo()
    {
        if (falaAtual >= falasDoPersonagem1.Length) return;

        bool personagem1Falando = falasDoPersonagem1[falaAtual];

        caixaPersonagem1?.SetActive(personagem1Falando);
        caixaPersonagem2?.SetActive(!personagem1Falando);

        botaoAvancar1?.gameObject.SetActive(personagem1Falando);
        botaoAvancar2?.gameObject.SetActive(!personagem1Falando);
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
}
