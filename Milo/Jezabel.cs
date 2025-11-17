using System;
using System.Collections;
using UnityEngine;

public class JezabelController : MonoBehaviour
{
    public CenarioDoJezabel[] cenarios;
    public float velocidade = 3f;
    public float distanciaMinima = 0.2f;
    public Alunos player;
    public float duracaoFade = 1.5f;

    private int indiceCenario = 0;
    private int indiceWaypoint = 0;
    private bool estaParado = false;
    private bool aguardandoJogador = false;
    private SpriteRenderer sprite;
    private Animator anim;

    public event Action OnDispararProximoWaypoint;

    [System.Serializable]
    public class CenarioDoJezabel
    {
        public string nomeCenario;
        public int numeroCenario;
        public Transform[] waypoints;
        public Transform nextSpawnJezabel;
        public bool[] pararNoWaypoint;
    }

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        IniciarCenario(indiceCenario);
    }

    void Update()
    {
        if (indiceCenario >= cenarios.Length) return;

        var cenarioAtual = cenarios[indiceCenario];

        if (aguardandoJogador)
        {
            if (player != null && player.cenarioAtual == cenarioAtual.numeroCenario)
            {
                aguardandoJogador = false;
                OnDispararProximoWaypoint?.Invoke();
                Debug.Log("Player chegou, Jezabel vai continuar.");
            }
            else
            {
                AtualizarAnimacao(false);
                return;
            }
        }

        if (estaParado)
        {
            AtualizarAnimacao(false);
            return;
        }

        if (indiceWaypoint < cenarioAtual.waypoints.Length)
        {
            Transform alvo = cenarioAtual.waypoints[indiceWaypoint];
            Vector3 direcao = (alvo.position - transform.position).normalized;

            // === Movimento ===
            transform.position += direcao * velocidade * Time.deltaTime;

            // === Flip no eixo X conforme direção ===
            if (direcao.x > 0.05f)
                sprite.flipX = false; // indo para direita
            else if (direcao.x < -0.05f)
                sprite.flipX = true;  // indo para esquerda

            // === Animação de movimento ===
            AtualizarAnimacao(true);

            if (Vector3.Distance(transform.position, alvo.position) < distanciaMinima)
            {
                StartCoroutine(ChecarParada(cenarioAtual, indiceWaypoint));
                indiceWaypoint++;
            }
        }
        else
        {
            AtualizarAnimacao(false);

            if (cenarioAtual.nextSpawnJezabel != null)
                StartCoroutine(TeleportarComAnimacao(cenarioAtual.nextSpawnJezabel.position));
            else
                AvancarCenario();
        }
    }

    IEnumerator TeleportarComAnimacao(Vector3 novoLocal)
    {
        estaParado = true;
        AtualizarAnimacao(false);

        if (sprite != null)
        {
            float tempo = 0f;
            Color original = sprite.color;
            while (tempo < duracaoFade)
            {
                tempo += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, tempo / duracaoFade);
                sprite.color = new Color(original.r, original.g, original.b, alpha);
                yield return null;
            }
        }

        transform.position = novoLocal;

        if (sprite != null)
        {
            float tempo = 0f;
            Color original = sprite.color;
            while (tempo < duracaoFade)
            {
                tempo += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, tempo / duracaoFade);
                sprite.color = new Color(original.r, original.g, original.b, alpha);
                yield return null;
            }
        }

        estaParado = false;
        AtualizarAnimacao(false);
        AvancarCenario();
    }

    void AvancarCenario()
    {
        indiceCenario++;
        indiceWaypoint = 0;

        if (indiceCenario < cenarios.Length)
        {
            aguardandoJogador = true;
            IniciarCenario(indiceCenario);
        }
    }

    void IniciarCenario(int indice)
    {
        if (indice >= cenarios.Length) return;
        var cenario = cenarios[indice];
        indiceWaypoint = 0;
        Debug.Log("Jezabel iniciou cenário: " + cenario.nomeCenario);
    }

    IEnumerator ChecarParada(CenarioDoJezabel cenario, int index)
    {
        if (cenario.pararNoWaypoint.Length > index && cenario.pararNoWaypoint[index])
        {
            estaParado = true;
            AtualizarAnimacao(false);
            Debug.Log("Jezabel parou no waypoint " + index);
            yield return new WaitForSeconds(3f);
            estaParado = false;
        }
    }

    // ---------------------------
    // Controle da animação
    // ---------------------------
    void AtualizarAnimacao(bool andando)
    {
        if (anim != null)
            anim.SetBool("walking", andando);
    }

    // ---------------------------
    // Gizmos (visualização)
    // ---------------------------
    void OnDrawGizmos()
    {
        if (cenarios == null) return;
        Gizmos.color = Color.yellow;

        foreach (var cenario in cenarios)
        {
            if (cenario == null || cenario.waypoints == null) continue;

            for (int i = 0; i < cenario.waypoints.Length - 1; i++)
            {
                if (cenario.waypoints[i] != null && cenario.waypoints[i + 1] != null)
                    Gizmos.DrawLine(cenario.waypoints[i].position, cenario.waypoints[i + 1].position);
            }
        }
    }
}
