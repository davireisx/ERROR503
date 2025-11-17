using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FugirArcadeCipher : MonoBehaviour
{
    [System.Serializable]
    public class WaypointInfo
    {
        public Transform waypoint;
        public GameObject personagemAlvo;
        public AudioSource somImpacto;
        [Tooltip("Ângulo de impacto do personagem. Pode ser positivo ou negativo.")]
        public float anguloImpacto = 75f;
        [HideInInspector] public bool impactoExecutado = false;
    }

    [Header("Referências Principais")]
    public GameObject cipher;
    public List<WaypointInfo> waypoints = new();
    public Transform waypointFinal;

    [Header("Configurações de Movimento")]
    public float velocidade = 15f;
    public float rotacaoVelocidade = 800f;

    [Header("Aparição")]
    public Vector3 escalaInicial = new Vector3(0.1f, 0.1f, 1f);
    public Vector3 escalaFinal = new Vector3(1f, 1f, 1f);
    public float piscarVelocidade = 20f;

    [Header("Som e Efeitos")]
    public AudioSource somAparicao;
    public GameObject efeito;
    [Tooltip("Tempo entre cada personagem se levantar no final.")]
    public float tempoEntreLevantadas = 0.25f;

    [Header("Finalização")]
    public GameObject dialogo;     // GameObject do diálogo a ser ativado
    public Animator cenarioDS;     // Animator do cenário a ser desativado

    private SpriteRenderer sr;
    private float escalaProgresso = 0f;

    private void Start()
    {
        if (cipher != null)
            cipher.SetActive(false);

        StartCoroutine(AparicaoEFuga());
    }

    private IEnumerator AparicaoEFuga()
    {
        if (cipher == null || waypoints.Count == 0)
            yield break;

        cipher.SetActive(true);
        efeito.SetActive(true);

        if (somAparicao != null)
            somAparicao.Play();

        sr = cipher.GetComponent<SpriteRenderer>();
        if (sr == null)
            yield break;

        cipher.transform.localScale = escalaInicial;
        sr.color = new Color(0, 0, 0, 0);

        float angulo = 0f;
        int waypointIndex = 0;

        // Movimento principal do Cipher
        while (waypointIndex < waypoints.Count)
        {
            WaypointInfo info = waypoints[waypointIndex];
            if (info.waypoint != null)
            {
                Vector3 posAnterior = cipher.transform.position;
                cipher.transform.position = Vector3.MoveTowards(
                    cipher.transform.position,
                    info.waypoint.position,
                    velocidade * Time.deltaTime
                );

                // Crescimento gradual
                float distanciaPercorrida = Vector3.Distance(posAnterior, cipher.transform.position);
                escalaProgresso += distanciaPercorrida / 5f;
                escalaProgresso = Mathf.Clamp01(escalaProgresso);
                cipher.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, escalaProgresso);

                // Rotação e piscar
                angulo += rotacaoVelocidade * Time.deltaTime;
                cipher.transform.rotation = Quaternion.Euler(0, 0, angulo);

                float pingPong = Mathf.PingPong(Time.time * piscarVelocidade, 1f);
                sr.color = Color.Lerp(Color.black, Color.white, pingPong);

                // Impacto ao chegar no waypoint
                if (!info.impactoExecutado && Vector3.Distance(cipher.transform.position, info.waypoint.position) < 0.1f)
                {
                    info.impactoExecutado = true;

                    if (info.somImpacto != null) info.somImpacto.Play();
                    if (info.personagemAlvo != null)
                        ImpactarPersonagem(info.personagemAlvo, info.anguloImpacto);
                }

                if (Vector3.Distance(cipher.transform.position, info.waypoint.position) < 0.1f)
                    waypointIndex++;
            }

            yield return null;
        }

        // Vai até o ponto final
        if (waypointFinal != null)
        {
            while (Vector3.Distance(cipher.transform.position, waypointFinal.position) > 0.05f)
            {
                Vector3 posAnterior = cipher.transform.position;
                cipher.transform.position = Vector3.MoveTowards(
                    cipher.transform.position,
                    waypointFinal.position,
                    velocidade * Time.deltaTime
                );

                float distanciaPercorrida = Vector3.Distance(posAnterior, cipher.transform.position);
                escalaProgresso += distanciaPercorrida / 5f;
                escalaProgresso = Mathf.Clamp01(escalaProgresso);
                cipher.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, escalaProgresso);

                angulo += rotacaoVelocidade * Time.deltaTime;
                cipher.transform.rotation = Quaternion.Euler(0, 0, angulo);

                float pingPong = Mathf.PingPong(Time.time * piscarVelocidade, 1f);
                sr.color = Color.Lerp(Color.black, Color.white, pingPong);

                yield return null;
            }

            cipher.transform.position = waypointFinal.position;
        }

        // Efeito visual e rotação final do Cipher
        yield return StartCoroutine(SuavizarRotacaoFinal(cipher.transform, 0.5f));
        cipher.transform.rotation = Quaternion.identity;
        sr.color = Color.white;
        cipher.transform.localScale = escalaFinal;

        // --- Todos os personagens levantam um por um ---
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i].personagemAlvo != null)
            {
                StartCoroutine(SuavizarRotacaoFinal(waypoints[i].personagemAlvo.transform, 0.5f));
                yield return new WaitForSeconds(tempoEntreLevantadas);
            }
        }

        // Desativa o efeito final
        efeito.SetActive(false);

        // --- Ativa o diálogo e desativa o animator do cenário ---
        if (cenarioDS != null)
            cenarioDS.enabled = false;

        if (dialogo != null)
            dialogo.SetActive(true);
    }

    private void ImpactarPersonagem(GameObject alvo, float angulo)
    {
        if (alvo == null) return;
        alvo.transform.rotation = Quaternion.Euler(0, 0, angulo);
    }

    private IEnumerator SuavizarRotacaoFinal(Transform alvo, float duracao)
    {
        Quaternion rotInicial = alvo.rotation;
        Quaternion rotFinal = Quaternion.identity;
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);
            alvo.rotation = Quaternion.Lerp(rotInicial, rotFinal, t);
            yield return null;
        }

        alvo.rotation = Quaternion.identity;
    }
}
