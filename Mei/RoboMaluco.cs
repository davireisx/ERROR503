using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class RoboMaluco : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string nome;
        public ComponenteLancado[] componentes;
    }

    [Header("Configura??es B?sicas")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waitTimeAtWaypoint = 0.5f;

    [Header("Waves de Componentes")]
    public Wave[] waves;

    [Header("Refer?ncias")]
    public GerenciadorDeComponentesLancados gerenciador;

    private Animator animator;
    private bool estaNocauteado = false;
    private bool estaSeguindoWaypoints = false;
    private int currentWaypointIndex = 0;

    // Controle de progresso
    private int componentesRestantesWave0 = 0;
    private bool iniciouSegundoAtaque = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Ativa os GameObjects dos componentes, mas o SpriteRenderer ? desativado internamente
        foreach (var wave in waves)
        {
            foreach (var comp in wave.componentes)
            {
                if (comp != null)
                {
                    comp.gameObject.SetActive(true); // Para garantir que o Start() do componente rode
                }
            }
        }

        StartCoroutine(SequenciaComportamento());
    }

    void AtivarWave(int index)
    {
        if (index >= 0 && index < waves.Length)
        {
            var wave = waves[index];

            if (index == 0) // Wave 0
            {
                componentesRestantesWave0 = wave.componentes.Length;

                foreach (var componente in wave.componentes)
                {
                    if (componente != null)
                    {
                        componente.OnChegouNoFinal += QuandoComponenteFinalizarWave0;
                        componente.IniciarMovimento();
                    }
                }
            }
            else if (index == 1) // Wave 1 (n?o segue waypoints depois)
            {
                foreach (var componente in wave.componentes)
                {
                    if (componente != null)
                    {
                        componente.OnChegouNoFinal += QuandoComponenteFinalizarWave1;
                        componente.IniciarMovimento();
                    }
                }
            }
        }
    }

    void QuandoComponenteFinalizarWave0(ComponenteLancado componente)
    {
        componente.OnChegouNoFinal -= QuandoComponenteFinalizarWave0;
        componentesRestantesWave0--;

        //componente.AtivarPiscada(); // Ativa piscada + collider ao chegar

        if (componentesRestantesWave0 <= 0)
        {
            Debug.Log("Todos os componentes da Wave 0 chegaram ao destino.");

            if (gerenciador != null && gerenciador.joystick != null)
            {
                gerenciador.joystick.SetActive(true); // ? Ativa o joystick aqui
            }

            StartCoroutine(SeguirWaypointsEAtacarWave2());
        }
    }

    void QuandoComponenteFinalizarWave1(ComponenteLancado componente)
    {
        componente.OnChegouNoFinal -= QuandoComponenteFinalizarWave1;
    }

    void AtivarComponentesParaInteracao()
    {
        foreach (var wave in waves)
        {
            foreach (var componente in wave.componentes)
            {
                if (componente != null)
                {
                
                }
            }
        }

        if (gerenciador != null)
        {
            if (gerenciador.joystick != null)
                gerenciador.joystick.SetActive(true);

            if (gerenciador.hud != null)
                gerenciador.hud.SetActive(true);
        }

        Debug.Log("Todos os componentes prontos para intera??o.");
    }

    void IniciarAtaque()
    {
        animator.SetBool("Atacando", true);
        Debug.Log("Robo iniciou ATAQUE!");
    }


    void FinalizarAtaque()
    {
        animator.SetBool("Atacando", false);
        animator.SetTrigger("Idle");
        Debug.Log("Robo finalizou ATAQUE.");
    }

    public void LevarNocaute()
    {
        Debug.Log("Robo levou nocaute!");
        animator.SetTrigger("Nocaute");
        estaNocauteado = true;
        StopCoroutine(nameof(SeguirWaypoints));
        estaSeguindoWaypoints = false;
    }

    public bool EstaAtacando()
    {
        return animator.GetBool("Atacando");
    }


    IEnumerator SequenciaComportamento()
    {
        yield return new WaitForSeconds(1.5f);

        // Primeiro ataque com Wave 0
        IniciarAtaque();
        AtivarWave(0);
        yield return new WaitForSeconds(3f); // Tempo de ataque
        FinalizarAtaque();

        // Espera todos os componentes da Wave 0 terminarem (o restante ser? iniciado no callback)
    }

    IEnumerator SeguirWaypoints()
    {
        estaSeguindoWaypoints = true;

        while (currentWaypointIndex < waypoints.Length)
        {
            Transform alvo = waypoints[currentWaypointIndex];

            while (Vector2.Distance(transform.position, alvo.position) > 0.1f)
            {
                if (!estaNocauteado)
                {
                    transform.position = Vector2.MoveTowards(transform.position, alvo.position, moveSpeed * Time.deltaTime);
                }
                yield return null;
            }

            yield return new WaitForSeconds(waitTimeAtWaypoint);
            currentWaypointIndex++;
        }

        estaSeguindoWaypoints = false;
        currentWaypointIndex = 0;
    }

    IEnumerator SeguirWaypointsEAtacarWave2()
    {
        yield return StartCoroutine(SeguirWaypoints());

        if (!iniciouSegundoAtaque)
        {
            iniciouSegundoAtaque = true;

            IniciarAtaque();
            AtivarWave(1); // Inicia o segundo ataque
            yield return new WaitForSeconds(3f); // Espera a anima??o de ataque
            FinalizarAtaque();

            // Agora sim: ativa piscada, collider, joystick e HUD
            AtivarComponentesParaInteracao();
        }
    }


    void OnDrawGizmosSelected()
    {
        if (waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                }
            }

            if (waypoints[waypoints.Length - 1] != null)
            {
                Gizmos.DrawSphere(waypoints[waypoints.Length - 1].position, 0.2f);
            }
        }
    }
}