using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FugirArcadeSegunda : MonoBehaviour
{
    [Header("Referências")]
    public GameObject personagem;              // Personagem que vai fugir
    public List<Transform> waypoints = new();  // Lista de pontos de destino (em ordem)
    public GameObject novoPersonagem;          // Outro personagem que reage ao final
    public GameObject dialogo;
    public GameObject efeito;

    [Header("Configurações de Movimento")]
    public float velocidade = 3f;              // Velocidade de movimento
    public float rotacaoVelocidade = 180f;     // Velocidade da rotação

    [Header("Efeitos Visuais")]
    public float tempoCrescimento = 1f;        // Tempo para atingir o tamanho final
    public Vector3 escalaFinal = new Vector3(1f, 1f, 1f);
    public float piscarVelocidade = 10f;       // Velocidade de piscar

    [Header("Sons (opcional)")]
    public AudioSource somAparicao;
    public AudioSource somFinal;               // Som ao chegar no último waypoint

    private void Start()
    {
        if (personagem != null)
            personagem.SetActive(false);

        StartCoroutine(AparicaoEFuga());
    }

    private IEnumerator AparicaoEFuga()
    {
        if (personagem == null || waypoints.Count == 0)
            yield break;

        // Ativa personagem e reinicia transformações
        personagem.SetActive(true);
        personagem.transform.rotation = Quaternion.identity;

        efeito.SetActive(true);
        if (somAparicao != null)
            somAparicao.Play();

        SpriteRenderer sr = personagem.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        // Começa pequeno, invisível e preto
        Vector3 escalaInicial = new Vector3(0.1f, 0.1f, 1f);
        personagem.transform.localScale = escalaInicial;

        Color c = Color.black;
        c.a = 0f;
        sr.color = c;

        float tempo = 0f;
        float angulo = 0f;

        // Crescimento inicial com rotação e piscar
        while (tempo < tempoCrescimento)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / tempoCrescimento);

            // Crescimento
            personagem.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, progresso);

            // Piscar e alpha
            float alpha = progresso;
            float blink = Mathf.Sin(Time.time * piscarVelocidade);
            Color baseColor = blink > 0
                ? Color.Lerp(Color.black, Color.white, blink)
                : Color.Lerp(Color.white, Color.clear, -blink);
            baseColor.a *= alpha;
            sr.color = baseColor;

            // Rotação constante
            angulo += rotacaoVelocidade * Time.deltaTime;
            personagem.transform.rotation = Quaternion.Euler(0, 0, angulo);

            yield return null;
        }

        // Corrige estado visual antes de mover
        personagem.transform.localScale = escalaFinal;
        sr.color = Color.white;

        // Agora começa a se mover pelos waypoints imediatamente
        foreach (Transform destino in waypoints)
        {
            if (destino == null) continue;

            while (Vector3.Distance(personagem.transform.position, destino.position) > 0.05f)
            {
                // Movimento suave
                personagem.transform.position = Vector3.MoveTowards(
                    personagem.transform.position,
                    destino.position,
                    velocidade * Time.deltaTime
                );

                // Rotação contínua durante o percurso
                angulo += rotacaoVelocidade * Time.deltaTime;
                personagem.transform.rotation = Quaternion.Euler(0, 0, angulo);

                // Piscar leve enquanto se move
                float blink = Mathf.Sin(Time.time * (piscarVelocidade / 2f));
                Color moveColor = blink > 0
                    ? Color.Lerp(Color.white, Color.gray, blink)
                    : Color.Lerp(Color.gray, Color.white, -blink);
                sr.color = moveColor;

                yield return null;
            }

            // Corrige a posição exata
            personagem.transform.position = destino.position;
        }

        // Finaliza com cor e rotação normal
        personagem.transform.rotation = Quaternion.identity;
        sr.color = Color.white;

        // ?? Toca o som final ao chegar no último waypoint
        if (somFinal != null)
            somFinal.Play();

        // ?? Efeito de colisão visual
        yield return StartCoroutine(ColisaoVisual());
    }

    private IEnumerator ColisaoVisual()
    {
        if (personagem == null || novoPersonagem == null)
            yield break;

        float tempo = 0f;
        float duracao = 1.5f;
        float anguloMax = 90f;

        // Aplica rotação instantânea de impacto
        personagem.transform.rotation = Quaternion.Euler(0, 0, anguloMax);
        novoPersonagem.transform.rotation = Quaternion.Euler(0, 0, -anguloMax);

        yield return new WaitForSeconds(duracao);

        // Retorna suavemente à rotação original
        while (tempo < 0.5f)
        {
            tempo += Time.deltaTime;
            float t = tempo / 0.5f;

            personagem.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(anguloMax, 0f, t));
            novoPersonagem.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(-anguloMax, 0f, t));

            yield return null;
        }

        // Garante que fiquem exatamente retos
        personagem.transform.rotation = Quaternion.identity;
        novoPersonagem.transform.rotation = Quaternion.identity;
        efeito.SetActive(false);
        yield return new WaitForSeconds(1f);
        dialogo.SetActive(true);   
    }
}
