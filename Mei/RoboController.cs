using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;

public class RoboController : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string nome;
        public Transform[] waypoints;
        public ComponenteLancado[] componentes;
        public bool ataqueNoInicio;
        public bool pararNoFinal;
    }

    [Header("Configurações")]
    public Wave[] waves;
    public Animator animator;
    public GameObject player;
    public Transform playerPos;
    public Joystick joystickPlayer;
    public CameraManagerEsdras cameraManager;
    public float moveSpeed = 2f;

    [Header("Configurações Nocaute")]
    public float rangeDePiscada = 5f;
    public float rangeDeInteracao = 2f;
    public Color corDaPiscada = new Color(1f, 0f, 0f, 0.5f);
    public float velocidadePiscada = 3f;

    [Header("HUD")]
    public AudioSource audioRoboBaguncaComponentes;
    public AudioSource audioRoboNocaute;
    public GameObject HUD;
    public GameObject HUDFINAL;
    public GameObject dialogoFinal;
    public GameObject objetivos1;
    public GameObject objetivos2;
    public GameObject objetivos3;
    public GameObject check;
    public GameObject tampa1;
    public GameObject tampa2;
    public Transform spawnFiacao1;
    public Transform spawnFiacao2;

    [Header("Configurações")]
    public Image telaFade;
    public float fadeDuration;


    private bool estaNocauteado = false;
    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private bool piscando = false;
    private bool segundaOrdemRecebida = false;

    private Transform playerTransform;
    private Collider2D colisor;


    private int contadorNocautes = 0;
    private bool jaClicouNoNocaute = false; // controla spam
    private bool podeContinuar = false;
    private Coroutine correnteCoroutine;
    public static float tamanhoOriginalCamera = -1f;


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        corOriginal = spriteRenderer.color;
        colisor = GetComponent<Collider2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        correnteCoroutine = StartCoroutine(ControlarWaves());
    }

    private void Update()
    {
        if (estaNocauteado)
        {
            AtualizarPiscada();
            VerificarCliqueDoUsuario();
        }
    }

    public void LevarNocaute()
    {
        if (estaNocauteado) return;

        contadorNocautes++;

        estaNocauteado = true;
        jaClicouNoNocaute = false;

        animator.SetTrigger("Nocaute");

        if (contadorNocautes == 1)
        {
            tampa1.SetActive(true);
        }

        gameObject.tag = "Untagged";

        if (contadorNocautes == 2)
        {
            // Segundo nocaute: desativa collider e faz fade final
            if (colisor != null)
                colisor.enabled = true;
            tampa2.SetActive(true);
            piscando = true; // para piscada
            //StartCoroutine(FadeBrancoParaPreto());
        }
        else
        {
            // Primeiro nocaute: mantém piscando
            if (colisor != null)
                colisor.enabled = true;
            piscando = true;
        }
    }




    void AtualizarPiscada()
    {
        if (!piscando || spriteRenderer == null || playerTransform == null)
            return;

        float distancia = Vector2.Distance(transform.position, playerTransform.position);
        if (distancia <= rangeDePiscada)
        {
            float alpha = Mathf.PingPong(Time.time * velocidadePiscada, 1f);
            spriteRenderer.color = Color.Lerp(corOriginal, corDaPiscada, alpha);
        }
        else
        {
            spriteRenderer.color = corOriginal;
        }
    }

    void VerificarCliqueDoUsuario()
    {
        if (playerTransform == null || colisor == null || !colisor.enabled || !estaNocauteado)
            return;

        Vector2 toqueOuClique = Vector2.zero;
        bool clicou = false;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            toqueOuClique = Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            clicou = true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            toqueOuClique = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            clicou = true;
        }

        if (!clicou) return;

        Collider2D colisorTocado = Physics2D.OverlapPoint(toqueOuClique);
        if (colisorTocado == colisor)
        {
            float distancia = Vector2.Distance(playerTransform.position, transform.position);
            if (distancia <= rangeDeInteracao)
            {
                // Impede spam
                if (jaClicouNoNocaute) return;
                jaClicouNoNocaute = true;

                Debug.Log("Jogador clicou no robô dentro da área de interação!");

                if (contadorNocautes == 1) // Primeiro nocaute
                {
                    audioRoboNocaute.Play();
                    StartCoroutine(FazerTransicao1());
                }
                else if (contadorNocautes >= 2) // Segundo nocaute ou mais
                {
                    audioRoboNocaute.Play();
                    StartCoroutine(FazerTransicao2());
                }
            }
            else
            {
                Debug.Log("Jogador clicou no robô, mas está FORA da área de interação.");
            }
        }

    }

    IEnumerator ControlarWaves()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            Wave waveAtual = waves[i];
            Debug.Log($"Iniciando wave: {waveAtual.nome}");

            // Ataque no início da wave
            if (waveAtual.ataqueNoInicio)
            {
                animator.SetTrigger("Idle");
                if (i == 0)
                    yield return new WaitForSeconds(0.5f);

                yield return StartCoroutine(AtacarELancarComponentes(waveAtual.componentes));
                animator.SetTrigger("Idle");
            }

            // Wave 1: esperar segunda ordem antes de mover
            if (i == 1)
            {
                Debug.Log("Wave 1: aguardando segunda ordem...");
                yield return new WaitUntil(() => segundaOrdemRecebida);
                Debug.Log("Segunda ordem recebida, iniciando movimento da wave 1.");
            }

            // Movimentação pelos waypoints
            if (waveAtual.waypoints != null && waveAtual.waypoints.Length > 0)
                yield return StartCoroutine(SeguirWaypoints(waveAtual.waypoints, waveAtual.nome));


            // Ataque no final da wave
            if (!waveAtual.ataqueNoInicio)
            {
                yield return StartCoroutine(AtacarELancarComponentes(waveAtual.componentes));
                animator.SetTrigger("Idle");
            }

            // Pausar se necessário
            if (waveAtual.pararNoFinal)
            {
                Debug.Log($"Parando após terminar a wave: {waveAtual.nome}");
                yield return new WaitUntil(() => podeContinuar);
                Debug.Log("Segunda ordem recebida, continuando...");
            }
        }

        Debug.Log("Todas as waves concluídas!");
    }

    public void DarSegundaOrdem()
    {
        estaNocauteado = false; // <- aqui
        piscando = false;
        if (spriteRenderer != null)
            spriteRenderer.color = corOriginal;

        if (animator != null)
            animator.SetTrigger("Idle");

        colisor.enabled = true;
        gameObject.tag = "Enemy";

        joystickPlayer.gameObject.SetActive(true);
        check.gameObject.SetActive(false);
        HUD.gameObject.SetActive(false);
        objetivos1.gameObject.SetActive(true);
        objetivos2.gameObject.SetActive(false);
        objetivos3.gameObject.SetActive(false);

        podeContinuar = true;
        segundaOrdemRecebida = true;
    }


    IEnumerator AtacarELancarComponentes(ComponenteLancado[] comps)
    {
        bool todosChegaram = false;
        int restantes = comps != null ? comps.Length : 0;

        if (restantes > 0)
        {
            foreach (var comp in comps)
            {
                comp.OnChegouNoFinal += (c) =>
                {
                    restantes--;
                    if (restantes <= 0)
                        todosChegaram = true;
                };
            }
        }
        else
        {
            todosChegaram = true;
        }

        animator.SetBool("Atacando", true);

        if (comps != null)
        {
            foreach (var comp in comps)
            {
                comp.IniciarMovimento();
            }
        }

        yield return new WaitForSeconds(3f);
        animator.SetBool("Atacando", false);

        while (!todosChegaram)
            yield return null;
    }

    IEnumerator SeguirWaypoints(Transform[] waypoints, string nomeWave)
    {
        int index = 0;
        if (audioRoboBaguncaComponentes == null)
        {
            audioRoboBaguncaComponentes = gameObject.AddComponent<AudioSource>();
            audioRoboBaguncaComponentes.playOnAwake = false;
        }

        while (index < waypoints.Length)
        {
            Transform alvo = waypoints[index];
            transform.position = Vector3.MoveTowards(transform.position, alvo.position, moveSpeed * Time.deltaTime);

            // Quando chegar perto do waypoint
            if (Vector3.Distance(transform.position, alvo.position) < 0.05f)
            {
                // Só toca se for o terceiro waypoint e a wave estiver na lista especificada
                if (index == 2 &&
                    (nomeWave == "PRIMEIRO ATAQUE - Wave1" ||
                     nomeWave == "PRIMEIRO ATAQUE - Wave2" ||
                     nomeWave == "SEGUNDO ATAQUE - Wave3" ||
                     nomeWave == "SEGUNDO ATAQUE - Wave4" ||
                     nomeWave == "SEGUNDO ATAQUE - Wave5"))
                {
                    if (!audioRoboBaguncaComponentes.isPlaying)
                    {
                        audioRoboBaguncaComponentes.Play();
                        Debug.Log("Som tocado no terceiro waypoint da wave: " + nomeWave);
                    }
                }

                index++; // passa para o próximo waypoint
            }

            yield return null;
        }
    }



    IEnumerator FazerTransicao1()
    {
        joystickPlayer.gameObject.SetActive(false);
        check.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        telaFade.gameObject.SetActive(true);
        yield return StartCoroutine(FadeIn());
        // ?? Salva o tamanho atual e aumenta +5
        if (cameraManager != null && Camera.main != null)
        {
            tamanhoOriginalCamera = Camera.main.orthographicSize;
            Camera.main.orthographicSize = tamanhoOriginalCamera + 0.5f;
        }

        // Move player e ativa controles
        cameraManager.SetScenarioBounds(2);
        check.gameObject.SetActive(false);
        HUD.SetActive(true);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FadeOut());
        playerPos.position = spawnFiacao1.position;
        player.gameObject.SetActive(false);

        objetivos2.gameObject.SetActive(false);
        objetivos3.gameObject.SetActive(true);


        telaFade.gameObject.SetActive(false);
    }

    IEnumerator FazerTransicao2()
    {
        joystickPlayer.gameObject.SetActive(false);
        check.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        telaFade.gameObject.SetActive(true);
        yield return StartCoroutine(FadeIn());
        // ?? Salva o tamanho atual e aumenta +5
        if (cameraManager != null && Camera.main != null)
        {
            tamanhoOriginalCamera = Camera.main.orthographicSize;
            Camera.main.orthographicSize = tamanhoOriginalCamera + 3f;
        }


        // Move player e ativa controles
        cameraManager.SetScenarioBounds(3);
        check.gameObject.SetActive(false);
        HUD.SetActive(true);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FadeOut());
        playerPos.position = spawnFiacao2.position;
        player.gameObject.SetActive(false);

        objetivos2.gameObject.SetActive(false);
        objetivos3.gameObject.SetActive(true);


        telaFade.gameObject.SetActive(false);
    }


    public IEnumerator FadeBrancoParaPreto()
    {
        colisor.enabled = false;
        piscando = false;

        Color corInicial = Color.white;
        Color corFinal = Color.black;

        float duracao = 2f; // tempo do fade
        float t = 0f;

        while (t < duracao)
        {
            t += Time.deltaTime;
            float progresso = t / duracao;
            spriteRenderer.color = Color.Lerp(corInicial, corFinal, progresso);
            yield return null;
        }

        spriteRenderer.color = corFinal;
        HUD.SetActive(false);
        HUDFINAL.SetActive(true);
        yield return new WaitForSeconds(1f);
        dialogoFinal.SetActive(true);
    }

    IEnumerator FadeIn()
    {
        telaFade.gameObject.SetActive(true);    
        float t = 0;
        Color c = telaFade.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            telaFade.color = c;
            yield return null;
        }
        c.a = 1;
        telaFade.color = c;
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        Color c = telaFade.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            telaFade.color = c;
            yield return null;
        }
        c.a = 0;
        telaFade.color = c;
    }
}
