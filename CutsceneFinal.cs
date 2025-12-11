using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cachorro
{
    [Header("Referências do Cachorro")]
    public string nome;
    public Transform cachorro;
    public Transform waypointCachorro;
    public GameObject efeitoCachorro;
    public AudioSource somLatido;

    [Header("Configurações")]
    public float velocidade = 2f;

    [HideInInspector] public bool andando = false;
    [HideInInspector] public Coroutine zoomCoroutine;
}

public class CutsceneFinal : MonoBehaviour
{
    [Header("Referências")]
    public Camera mainCamera;
    public CameraSeguirEsdras cameraSeguir;
    public Transform cipher;
    public SpriteRenderer cipherRenderer;
    public Animator cipherAnim; // ?? Animator do Cipher
    public List<Transform> waypoints;
    public List<float> tempoDeEsperaPorWaypoint;
    public List<Cachorro> cachorros;
    public GameObject dialogo;

    [Header("Configurações de Zoom")]
    public float zoomFinal = 9f;
    public float zoomVoltar = 14f;
    public float zoomVelocidade = 2f;

    [Header("Configurações do Cipher")]
    public float velocidadeCipher = 3f;
    public float tempoAparecer = 1.5f;

    private float zoomInicial;
    private bool iniciouMovimento;
    private int waypointAtual = 0;
    private bool esperando = false;
    private bool trocouPraCachorro = false;
    private Coroutine zoomCoroutine;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (cameraSeguir == null) cameraSeguir = FindObjectOfType<CameraSeguirEsdras>();

        if (cipherRenderer != null)
        {
            Color c = cipherRenderer.color;
            c.a = 0f;
            cipherRenderer.color = c;
        }

        if (dialogo != null) dialogo.SetActive(false);

        StartCoroutine(CutsceneFluxo());
    }

    private IEnumerator CutsceneFluxo()
    {
        yield return null;
        zoomInicial = mainCamera.orthographicSize;

        // Primeiro zoom in no Cipher
        yield return StartCoroutine(AlterarZoomSuave(zoomFinal));
        yield return new WaitForSeconds(0.3f);

        // Fade-in do Cipher
        if (cipherRenderer != null)
        {
            float tempo = 0f;
            Color cor = cipherRenderer.color;
            while (tempo < tempoAparecer)
            {
                tempo += Time.deltaTime;
                cor.a = Mathf.Lerp(0f, 1f, tempo / tempoAparecer);
                cipherRenderer.color = cor;
                yield return null;
            }
            cor.a = 1f;
            cipherRenderer.color = cor;
        }

        yield return new WaitForSeconds(0.5f);

        if (cameraSeguir != null && cipher != null)
            cameraSeguir.player = cipher;

        iniciouMovimento = true;
    }

    void Update()
    {
        // Movimento do Cipher
        if (iniciouMovimento && !esperando && waypoints != null && waypointAtual < waypoints.Count && cipher != null)
        {
            Transform alvo = waypoints[waypointAtual];

            // ?? Ativa walking=true enquanto se move
            if (cipherAnim != null)
                cipherAnim.SetBool("walking", true);

            cipher.position = Vector3.MoveTowards(cipher.position, alvo.position, velocidadeCipher * Time.deltaTime);

            if (waypointAtual == 0 && !trocouPraCachorro)
            {
                if (Vector3.Distance(cipher.position, alvo.position) > 0.001f)
                {
                    trocouPraCachorro = true;
                    if (cachorros.Count > 0)
                        StartCoroutine(IniciarCachorrosSequencialmente());
                }
            }

            float distancia = Vector3.Distance(cipher.position, alvo.position);
            if (distancia < 0.1f)
                StartCoroutine(EsperarNoWaypoint());
        }

        // Movimento dos cachorros
        foreach (var dog in cachorros)
        {
            if (dog.andando && dog.cachorro != null && dog.waypointCachorro != null)
            {
                float dist = Vector3.Distance(dog.cachorro.position, dog.waypointCachorro.position);
                if (dist > 0.05f)
                {
                    dog.cachorro.position = Vector3.MoveTowards(dog.cachorro.position, dog.waypointCachorro.position, dog.velocidade * Time.deltaTime);
                }
                else
                {
                    dog.andando = false;

                    if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
                    zoomCoroutine = StartCoroutine(AlterarZoomSuave(zoomInicial));

                    Animator anim = dog.cachorro.GetComponent<Animator>();
                    if (anim != null)
                    {
                        anim.SetBool("walking", false);
                        anim.SetBool("latido", true);
                    }

                    if (dog == cachorros[0])
                        StartCoroutine(PrimeiroCachorroIdle(anim, dog));

                    if (dialogo != null) dialogo.SetActive(true);
                    if (dog.somLatido != null) dog.somLatido.Stop();

                    Debug.Log($"{dog.nome} chegou ao destino! Diálogo ativado!");
                }
            }
        }
    }

    private IEnumerator IniciarCachorrosSequencialmente()
    {
        for (int i = 0; i < cachorros.Count; i++)
        {
            yield return new WaitForSeconds(i * 1f);
            StartCoroutine(MudarCameraParaCachorro(cachorros[i]));
        }
    }

    private IEnumerator MudarCameraParaCachorro(Cachorro dog)
    {
        if (dog == null || dog.cachorro == null || cameraSeguir == null) yield break;

        cameraSeguir.player = dog.cachorro;
        zoomCoroutine = StartCoroutine(AlterarZoomSuave(zoomVoltar));

        if (dog.efeitoCachorro != null)
        {
            dog.efeitoCachorro.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            dog.efeitoCachorro.SetActive(false);
        }

        Animator anim = dog.cachorro.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("walking", true);
            anim.SetBool("latido", false);
        }

        if (dog.somLatido != null)
            dog.somLatido.Play();

        dog.andando = true;
    }

    private IEnumerator EsperarNoWaypoint()
    {
        if (esperando) yield break;
        esperando = true;

        float tempoEsperar = 1f;
        if (tempoDeEsperaPorWaypoint != null && waypointAtual < tempoDeEsperaPorWaypoint.Count)
            tempoEsperar = tempoDeEsperaPorWaypoint[waypointAtual];

        yield return new WaitForSeconds(tempoEsperar);

        waypointAtual++;

        // ?? Se chegou no último waypoint, para o movimento e desativa walking
        if (waypointAtual >= waypoints.Count)
        {
            iniciouMovimento = false;
            if (cipherAnim != null)
                cipherAnim.SetBool("walking", false);
        }

        esperando = false;
    }

    private IEnumerator AlterarZoomSuave(float targetSize)
    {
        float initialSize = mainCamera.orthographicSize;
        float duration = Mathf.Abs(targetSize - initialSize) / zoomVelocidade;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainCamera.orthographicSize = Mathf.Lerp(initialSize, targetSize, elapsed / duration);
            yield return null;
        }

        mainCamera.orthographicSize = targetSize;
    }

    private IEnumerator PrimeiroCachorroIdle(Animator anim, Cachorro dog)
    {
        yield return new WaitForSeconds(2f);

        if (anim != null)
        {
            anim.SetBool("latido", false);
            anim.SetBool("idle", true);
        }

        if (dog.somLatido != null)
            dog.somLatido.Stop();

        Debug.Log($"?? {dog.nome} mudou para idle após latir.");
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.15f);
            if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        if (cachorros != null)
        {
            Gizmos.color = Color.green;
            foreach (var dog in cachorros)
            {
                if (dog.waypointCachorro != null)
                    Gizmos.DrawSphere(dog.waypointCachorro.position, 0.15f);
            }
        }
    }
}
