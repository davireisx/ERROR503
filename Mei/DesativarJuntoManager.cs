using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // ✅ novo sistema de input
using UnityEngine.EventSystems;

public class DesativarJuntoManager : MonoBehaviour
{
    [System.Serializable]
    public class ParDeObjetos
    {
        public GameObject objetoPrincipal;
        public GameObject objetoVinculado;
    }

    [Header("Pares de Objetos (Principal → Vinculado)")]
    public List<ParDeObjetos> pares = new List<ParDeObjetos>();

    [Header("Objetivos e Check")]
    public GameObject objetoCheck;
    public GameObject objetivo2;
    public GameObject objetivo3;
    public float duracaoCheck = 1.5f;

    [Header("Piscada e Interação")]
    public List<SpriteRenderer> objetosPiscantes;
    public float velocidadePiscada = 1f;

    [Header("Objeto de Interação (com Collider)")]
    public GameObject objetoInteracao; // alvo clicável/toque

    [Header("Diálogo ao Interagir")]
    public GameObject dialogo;

    [Header("Sons")]
    public AudioSource relarTrem;     // 🔊 som ao relar
    public AudioSource barulhoTrem;   // 🔊 som 1.5s depois

    private Dictionary<GameObject, GameObject> mapa = new Dictionary<GameObject, GameObject>();
    private bool checkAtivado = false;
    private bool piscando = false;

    private bool interacaoRealizada = false; // ✅ impede segunda interação

    private BoxCollider2D colliderInteracao;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        // Cria o dicionário
        foreach (var par in pares)
        {
            if (par.objetoPrincipal != null && par.objetoVinculado != null)
                mapa[par.objetoPrincipal] = par.objetoVinculado;
        }

        // Configura o collider do objeto de interação
        if (objetoInteracao != null)
        {
            colliderInteracao = objetoInteracao.GetComponent<BoxCollider2D>();
            if (colliderInteracao == null)
                colliderInteracao = objetoInteracao.AddComponent<BoxCollider2D>();

            colliderInteracao.isTrigger = true;
            colliderInteracao.enabled = false;
        }
    }

    void Update()
    {
        // Verifica objetos
        foreach (var par in mapa)
        {
            if (par.Key != null && !par.Key.activeSelf && par.Value.activeSelf)
                par.Value.SetActive(false);
        }

        // Se todos desativaram, ativa o check
        if (!checkAtivado && TodosVinculadosDesativados())
            StartCoroutine(AtivarCheckTemporario());

        // Quando o objetivo3 ativa, começa piscada
        if (objetivo3 != null && objetivo3.activeSelf && !piscando)
            StartCoroutine(PiscarSprites());

        // Detecta toque/clique
        DetectarInteracaoInputSystem();
    }

    void DetectarInteracaoInputSystem()
    {
        // ❌ se a interação já foi feita, não faz mais nada
        if (interacaoRealizada)
            return;

        if (colliderInteracao == null || !colliderInteracao.enabled)
            return;

        // Usa o novo sistema de Pointer (mouse + toque)
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);

            if (colliderInteracao.OverlapPoint(worldPos))
            {
                interacaoRealizada = true; // ✅ trava futuras interações
                colliderInteracao.enabled = false; // ✅ desativa o collider
                StartCoroutine(TocarSons());
                Interagir();
            }
        }
    }

    IEnumerator TocarSons()
    {
        if (relarTrem != null)
            relarTrem.Play();

        yield return new WaitForSeconds(1.5f);

        if (barulhoTrem != null)
            barulhoTrem.Play();
    }

    bool TodosVinculadosDesativados()
    {
        foreach (var par in mapa)
        {
            if (par.Value != null && par.Value.activeSelf)
                return false;
        }
        return true;
    }

    IEnumerator AtivarCheckTemporario()
    {
        checkAtivado = true;

        if (objetoCheck != null)
            objetoCheck.SetActive(true);

        yield return new WaitForSeconds(duracaoCheck);

        if (objetivo2 != null)
            objetivo2.SetActive(false);
        if (objetivo3 != null)
            objetivo3.SetActive(true);

        if (objetoCheck != null)
            objetoCheck.SetActive(false);
    }

    IEnumerator PiscarSprites()
    {
        piscando = true;

        if (colliderInteracao != null)
            colliderInteracao.enabled = true;

        Color corBranca = Color.white;
        Color corPreta = Color.black;

        while (true)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * velocidadePiscada;
                foreach (var sr in objetosPiscantes)
                    if (sr != null)
                        sr.color = Color.Lerp(corPreta, corBranca, t);
                yield return null;
            }

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * velocidadePiscada;
                foreach (var sr in objetosPiscantes)
                    if (sr != null)
                        sr.color = Color.Lerp(corBranca, corPreta, t);
                yield return null;
            }
        }
    }

    void Interagir()
    {
        // ✅ Ativa o diálogo ao interagir (uma única vez)
        if (dialogo != null)
            dialogo.SetActive(true);
    }
}
