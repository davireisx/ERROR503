using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PersonagemInfo
{
    [Header("Referências do Personagem")]
    public string nome;
    public Transform personagem;
    public Animator animator;
    public SpriteRenderer renderer;

    [Header("Waypoints de Destino")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Configurações")]
    public float velocidade = 2f;
    public float tempoParaProximo = 1f;
    public float duracaoFade = 1f;
    public float distanciaParada = 0.05f;

    [HideInInspector]
    public bool ativo = false;
    [HideInInspector]
    public int currentWaypointIndex = 0;
}

public class PersonagensChegam : MonoBehaviour
{
    [Header("Lista de Personagens")]
    public List<PersonagemInfo> personagens = new List<PersonagemInfo>();

    private void OnEnable()
    {
        StartCoroutine(AtivarSequencia());
    }

    IEnumerator AtivarSequencia()
    {
        // Pequeno delay inicial para garantir que tudo está inicializado
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < personagens.Count; i++)
        {
            PersonagemInfo p = personagens[i];
            if (p.personagem == null) continue;

            p.ativo = true;
            StartCoroutine(ProcessarPersonagem(p));
            yield return new WaitForSeconds(p.tempoParaProximo);
        }
    }

    IEnumerator ProcessarPersonagem(PersonagemInfo p)
    {
        // Fase 1: Fade-in
        yield return StartCoroutine(FadeInPersonagem(p));

        // Fase 2: Movimento entre waypoints
        yield return StartCoroutine(MoverEntreWaypoints(p));

        // Fase 3: Finalização
        if (p.animator != null)
            p.animator.SetBool("walking", false);
    }

    IEnumerator FadeInPersonagem(PersonagemInfo p)
    {
        if (p.renderer != null)
        {
            Color startColor = p.renderer.color;
            startColor.a = 0f;
            p.renderer.color = startColor;

            float t = 0f;
            while (t < p.duracaoFade)
            {
                t += Time.deltaTime;
                Color c = p.renderer.color;
                c.a = Mathf.Lerp(0f, 1f, t / p.duracaoFade);
                p.renderer.color = c;
                yield return null;
            }

            Color finalColor = p.renderer.color;
            finalColor.a = 1f;
            p.renderer.color = finalColor;
        }
        yield return new WaitForEndOfFrame();
    }

    IEnumerator MoverEntreWaypoints(PersonagemInfo p)
    {
        if (p.animator != null)
            p.animator.SetBool("walking", true);

        for (int i = 0; i < p.waypoints.Count; i++)
        {
            if (p.waypoints[i] == null) continue;

            yield return StartCoroutine(MoverParaWaypoint(p, p.waypoints[i]));
        }
    }

    IEnumerator MoverParaWaypoint(PersonagemInfo p, Transform waypoint)
    {
        Vector3 target = waypoint.position;

        while (true)
        {
            if (!p.ativo || p.personagem == null)
                yield break;

            // Movimento suave e consistente em qualquer dispositivo
            p.personagem.position = Vector3.MoveTowards(
                p.personagem.position,
                target,
                p.velocidade * Time.deltaTime
            );

            // Sai do loop quando chegar
            if (Vector3.Distance(p.personagem.position, target) <= p.distanciaParada)
                break;

            // Flip
            Vector3 dir = target - p.personagem.position;
            if (p.renderer != null)
            {
                if (dir.x > 0.05f) p.renderer.flipX = false;
                else if (dir.x < -0.05f) p.renderer.flipX = true;
            }

            yield return null;
        }

        // Garante que fique exatamente no ponto
        p.personagem.position = target;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        foreach (PersonagemInfo p in personagens)
        {
            p.ativo = false;
        }
    }

   
}