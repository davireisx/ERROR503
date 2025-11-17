using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;
using System; // Para Action
using System.Collections.Generic;

public class DialogoAutomatico : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public GameObject joystick;
    public GameObject hud;

    [Header("Caixas de Diálogo")]
    public GameObject caixaPersonagem1;
    public GameObject caixaPersonagem2;
    public AudioSource next;

    [Header("Falas")]
    public GameObject[] falas;
    public bool[] falasDoPersonagem1;

    [Header("Configuração do Diálogo Automático")]
    public float tempoAntesDeIniciar = 1f;
    public List<float> tempoDeCadaFala = new List<float>(); // ?? cada fala tem seu próprio tempo

    [Header("Próximo Diálogo")]
    public GameObject segundoDialogo; // ?? será ativado quando este acabar

    private int falaAtual = 0;
    private bool dialogoAtivo = false;

    // Evento que avisa quando o diálogo terminou
    public event Action OnDialogoTerminado;

    public void Start()
    {
        DesativarFalas();

        caixaPersonagem1?.SetActive(false);
        caixaPersonagem2?.SetActive(false);

        StartCoroutine(IniciarDialogoAutomaticamente());
    }

    IEnumerator IniciarDialogoAutomaticamente()
    {
        yield return new WaitForSeconds(tempoAntesDeIniciar);
        IniciarDialogo();
    }

    public void IniciarDialogo()
    {
        falaAtual = 0;
        dialogoAtivo = true;

        hud?.SetActive(false);
        joystick?.SetActive(false);

        StartCoroutine(MostrarDialogoSequencial());
    }

    IEnumerator MostrarDialogoSequencial()
    {
        while (falaAtual < falas.Length)
        {
            AtualizarCaixaDialogo();
            MostrarFalaAtual();

            next?.Play();

            // usa o tempo da lista, se não existir usa 3s padrão
            float tempo = (falaAtual < tempoDeCadaFala.Count) ? tempoDeCadaFala[falaAtual] : 3f;
            yield return new WaitForSeconds(tempo);

            falaAtual++;
        }

        // Quando acabar
        DesativarFalas();
        caixaPersonagem1?.SetActive(false);
        caixaPersonagem2?.SetActive(false);
        dialogoAtivo = false;

        hud?.SetActive(true);
        joystick?.SetActive(true);

        // Ativa o próximo diálogo
        if (segundoDialogo != null)
            segundoDialogo.SetActive(true);

        // Dispara evento quando acabar
        OnDialogoTerminado?.Invoke();
    }

    void AtualizarCaixaDialogo()
    {
        if (falaAtual >= falasDoPersonagem1.Length) return;

        bool personagem1Falando = falasDoPersonagem1[falaAtual];

        caixaPersonagem1?.SetActive(personagem1Falando);
        caixaPersonagem2?.SetActive(!personagem1Falando);
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
