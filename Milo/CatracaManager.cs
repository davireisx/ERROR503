using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

using static Catraca;

public class CatracaManager : MonoBehaviour
{
    [Header("Contadores de HUD")]
    public Text textoFacil;
    public Text textoMedio;
    public Text textoDificil;

    [Header("Checks de confirmação")]
    public GameObject checkFacil;
    public GameObject checkMedio;
    public GameObject checkDificil;
    public GameObject HUD;
    public GameObject hudFinal;

    [Header("Check geral de vitória")]
    public GameObject checkGeral;

    [Header("Fade")]
    public CanvasGroup fadeCanvasGroup;
    public Image fadeImage;
    public float fadeDuration = 2f;
    public GameObject joystick;

    [Header("Objetos finais a desativar")]
    public GameObject trapCatraca;
    public GameObject arquiteto;

    [Header("DialogoFinal")]
    public GameObject dialogoFinal;
    public Dialogo dialogoController; // referência ao Dialogo

    [Header("Cenas")]
    public AudioSource audioCatracas;
    public AudioSource audioDano;
    public AudioSource audioFinal;
    public string nomeCenaVitoria;
    public string nomeCenaDerrota;

    [Header("Arquiteto - Transformação Final")]
    public Sprite novoSpriteArquiteto; // Sprite que o Arquiteto usará no final

    private Dictionary<Dificuldade, int> resolvidas = new Dictionary<Dificuldade, int>();
    private Dictionary<Dificuldade, int> falhas = new Dictionary<Dificuldade, int>();

    private Color corVitoria = new Color(230f / 255f, 240f / 255f, 255f / 255f); // E6F0FF
    private bool dialogoAtivado = false; // controla se o diálogo já foi disparado

    private void Start()
    {
        resolvidas[Dificuldade.Facil] = 0;
        resolvidas[Dificuldade.Medio] = 0;
        resolvidas[Dificuldade.Dificil] = 0;

        falhas[Dificuldade.Facil] = 0;
        falhas[Dificuldade.Medio] = 0;
        falhas[Dificuldade.Dificil] = 0;

        AtualizarHUD();
    }

    public void CatracaResolvida(Dificuldade dificuldade)
    {
        if (resolvidas[dificuldade] == 0) // conta só a primeira de cada tipo
        {
            resolvidas[dificuldade]++;
            AtualizarHUD();
            VerificarVitoria();

            switch (dificuldade)
            {
                case Dificuldade.Facil: if (checkFacil != null) checkFacil.SetActive(true); break;
                case Dificuldade.Medio: if (checkMedio != null) checkMedio.SetActive(true); break;
                case Dificuldade.Dificil: if (checkDificil != null) checkDificil.SetActive(true); break;
            }
        }
    }

    public void CatracaFalhou(Dificuldade dificuldade)
    {
        falhas[dificuldade]++;
        if (resolvidas[dificuldade] == 0 && falhas[dificuldade] >= 2)
        {
            StartCoroutine(DerrotaFinal());
        }
    }

    IEnumerator DerrotaFinal()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(nomeCenaDerrota);
    }

    void AtualizarHUD()
    {
        if (textoFacil) textoFacil.text = $"Fácil:   {resolvidas[Dificuldade.Facil]}  /  1";
        if (textoMedio) textoMedio.text = $"Médio:   {resolvidas[Dificuldade.Medio]}  /  1";
        if (textoDificil) textoDificil.text = $"Difícil:   {resolvidas[Dificuldade.Dificil]}  /  1";
    }

    void VerificarVitoria()
    {
        if (resolvidas[Dificuldade.Facil] >= 1 &&
            resolvidas[Dificuldade.Medio] >= 1 &&
           resolvidas[Dificuldade.Dificil] >= 1)
        {
            if (trapCatraca) trapCatraca.SetActive(false);
            audioCatracas.Play();

            if (checkGeral != null) checkGeral.SetActive(true);

            Catraca[] catracas = FindObjectsOfType<Catraca>();
            foreach (Catraca c in catracas)
            {
                c.SetColor(corVitoria);
            }

            joystick.SetActive(false);

            StartCoroutine(PiscarEDiminuirEAtivarDialogo());
       }
    }

    IEnumerator PiscarEDiminuirEAtivarDialogo()
    {
        
        // Agora apenas pisca o Arquiteto e muda o sprite (sem diminuir)
        yield return StartCoroutine(PiscarArquiteto());

        // Depois ativa o diálogo final
        dialogoAtivado = true;
        yield return StartCoroutine(AtivarDialogoFinal());
    }

    IEnumerator PiscarArquiteto()
    {
        if (arquiteto == null) yield break;
        audioDano.Play();
        HUD.SetActive(false);
 
        yield return new WaitForSeconds(1f);
        hudFinal.SetActive(true);

        SpriteRenderer sr = arquiteto.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Animator anim = arquiteto.GetComponent<Animator>();
        if (anim != null) anim.enabled = false; // desativa animação enquanto pisca

        float duracao = 1.5f; // duração total do efeito
        float tempoDecorrido = 0f;

        float intervaloPiscada = 0.2f;
        float proxTroca = 0f;
        bool corBranca = true;

        while (tempoDecorrido < duracao)
        {
            // Pisca alternando entre preto e branco
            if (tempoDecorrido >= proxTroca)
            {
                sr.color = corBranca ? Color.black : Color.white;
                corBranca = !corBranca;
                proxTroca += intervaloPiscada;
            }

            tempoDecorrido += Time.deltaTime;
            yield return null;
        }

        // Ao final, deixa ele branco
        sr.color = Color.white;

        // Troca o sprite final, se definido
        if (novoSpriteArquiteto != null)
        {
            sr.sprite = novoSpriteArquiteto;
        }
    }

    IEnumerator AtivarDialogoFinal()
    {
        yield return new WaitForSeconds(2f);
        dialogoAtivado = true;
        StartCoroutine(VitoriaFinal());
    }

    IEnumerator VitoriaFinal()
    {
        bool dialogoTerminou = false;

        if (dialogoFinal != null)
        {
            dialogoFinal.SetActive(true);

            if (dialogoController != null)
            {
                dialogoController.OnDialogoTerminado += () => dialogoTerminou = true;
                yield return new WaitUntil(() => dialogoTerminou);
            }
            else
            {
                yield return new WaitForSeconds(5f);
            }
        }

        audioFinal.Play();
        yield return StartCoroutine(FadeIn());

        SceneManager.LoadScene(nomeCenaVitoria);
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
    }

    public bool TodasEstaoCorrompidas()
    {
        Catraca[] catracas = FindObjectsOfType<Catraca>();
        foreach (Catraca c in catracas)
        {
            if (!c.DeveAtivarBotao())
                return false;
        }
        return true;
    }
}
