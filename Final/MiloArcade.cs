using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonagemTransicao : MonoBehaviour
{
    [Header("Referências")]
    public Transform personagem;            // Personagem principal (defina no Inspector)
    public List<Transform> spawnPoints;     // Lista de pontos (em ordem)
    public AudioSource audioArcade;     // Lista de pontos (em ordem)

    [Header("Novo Personagem e Spawn")]
    public GameObject novoPersonagem;       // Novo personagem que será ativado
    public GameObject objetoAtivar;       // Novo personagem que será ativado
    public Transform spawnPointNovo;        // Local onde o novo personagem aparecerá

    [Header("Fade e Câmera")]
    public CanvasGroup fadeCanvas;
    public float velocidadeFade = 1f;
    public CameraManagerEsdras cam;

    [Header("Configurações de Movimento")]
    public float velocidade = 2f;
    public float velocidadeRotacao = 90f;
    public float tempoEspera = 1f;

    [Header("Escala")]
    public float escalaFinal = 1.5f;
    public float duracaoEscala = 3f;

    [Header("Piscar")]
    public float velocidadePiscar = 5f;     // ciclos por segundo

    private bool fadeIniciado = false;
    private SpriteRenderer sr;

    private void Start()
    {
        if (personagem != null)
            sr = personagem.GetComponent<SpriteRenderer>();

        StartCoroutine(Movimentar());
    }

    private IEnumerator Movimentar()
    {
        if (personagem == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("Personagem ou spawnpoints não definidos!");
            yield break;
        }

        yield return new WaitForSeconds(tempoEspera);

        Vector3 escalaInicial = personagem.localScale;
        Vector3 escalaAlvo = new Vector3(escalaFinal, escalaFinal, escalaFinal);
        float tempoTotal = 0f;
        float tempoPiscar = 0f;

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            Transform ponto = spawnPoints[i];
            Vector3 posInicial = personagem.position;
            float distancia = Vector3.Distance(posInicial, ponto.position);
            float duracao = distancia / velocidade;
            float t = 0f;

            while (t < duracao)
            {
                t += Time.deltaTime;
                float progresso = Mathf.SmoothStep(0, 1, t / duracao);
                personagem.position = Vector3.Lerp(posInicial, ponto.position, progresso);

                // Rotação contínua
                personagem.Rotate(0, 0, velocidadeRotacao * Time.deltaTime);

                // Aumenta escala aos poucos
                tempoTotal += Time.deltaTime;
                float perc = Mathf.Clamp01(tempoTotal / duracaoEscala);
                personagem.localScale = Vector3.Lerp(escalaInicial, escalaAlvo, perc);

                // Piscar sprite
                if (sr != null)
                {
                    tempoPiscar += Time.deltaTime * velocidadePiscar * Mathf.PI * 2;
                    float v = Mathf.Sin(tempoPiscar); // -1 ? 1
                    if (v < 0) sr.color = Color.black;
                    else if (v < 0.5f) sr.color = Color.white;
                    else sr.color = new Color(1, 1, 1, 0); // transparente
                }

                // Inicia o fade ao atingir o segundo waypoint
                if (i == 1 && !fadeIniciado)
                {
                    fadeIniciado = true;
                    StartCoroutine(FadeInTransicao());
                }

                yield return null;
            }
        }

        // Rotação final suave até 0
        float anguloAtual = personagem.eulerAngles.z;
        float destino = (anguloAtual > 0) ? 360f : 0f;
        float tempoRotacao = 0f;
        float duracaoRotFinal = 1.5f;

        while (tempoRotacao < duracaoRotFinal)
        {
            tempoRotacao += Time.deltaTime;
            float angulo = Mathf.Lerp(anguloAtual, destino, tempoRotacao / duracaoRotFinal);
            personagem.rotation = Quaternion.Euler(0, 0, angulo);
            yield return null;
        }

        personagem.rotation = Quaternion.Euler(0, 0, 0);

        // Restaura cor final do personagem
        if (sr != null)
            sr.color = Color.white;
    }

    private IEnumerator FadeInTransicao()
    {
        if (fadeCanvas == null)
        {
            Debug.LogWarning("Canvas de fade não atribuído!");
            yield break;
        }

        float duracaoFade = 1.0f;
        float tempo = 0f;

        audioArcade.Play();
        // FADE IN
        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, tempo / duracaoFade);
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // Teleporta novo personagem e desativa antigo
        if (novoPersonagem != null && spawnPointNovo != null)
        {
            novoPersonagem.transform.position = spawnPointNovo.position;

            if (personagem != null)
                personagem.gameObject.SetActive(false);

            if (cam != null)
                cam.SetScenarioBounds(1);

            if (novoPersonagem != null)
                novoPersonagem.SetActive(true);
        }

        yield return new WaitForSeconds(0.5f);

        // FADE OUT
        tempo = 0f;
        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, tempo / duracaoFade);
            yield return null;
        }
        fadeCanvas.alpha = 0f;
        yield return new WaitForSeconds(0.8f);
        objetoAtivar.SetActive(true);
    }
}
