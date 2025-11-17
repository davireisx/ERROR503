using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CenaComponentesManager : MonoBehaviour
{
    [Header("Referências Principais")]
    public GameObject mei;                       // Jogadora
    public Transform waypoint;                   // Ponto de destino
    public GameObject joystick;                  // HUD do joystick
    public GameObject hud;                       // HUD principal

    [Header("Componentes para piscar")]
    public List<GameObject> listComponentes;     // Lista dos componentes que vão piscar

    [Header("Outros Elementos")]
    public GameObject dialogoMei2;               // Próximo diálogo
    public AudioSource pegarComponentes;         // Som de pegar componentes

    [Header("Configurações de movimento")]
    public float velocidade = 2f;                // Velocidade da Mei
    public float distanciaParada = 0.1f;         // Distância para considerar chegada ao waypoint

    [Header("Opções visuais")]
    public bool flipSpriteAoMover = true;        // Vira sprite pela escala X
    public SpriteRenderer meiSpriteRenderer;    // opcional, se quiser setar diretamente

    private Animator animator;
    private bool indoParaWaypoint = false;
    private readonly int walkingHash = Animator.StringToHash("walking"); // evita string toda hora

    void Start()
    {
        // Desativa HUD e joystick
        if (joystick != null) joystick.SetActive(false);
        if (hud != null) hud.SetActive(false);

        // Tenta pegar o Animator: primeiro no próprio GameObject, depois em filhos
        if (animator == null && mei != null)
            animator = mei.GetComponent<Animator>();

        if (animator == null && mei != null)
            animator = mei.GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogError("[CenaComponentesManager] Animator não encontrado na 'mei' ou em seus filhos. Adicione um Animator ou arraste-o no inspector.");

        // Se o usuario não arrumou o SpriteRenderer explicitamente, tenta pegar um
        if (meiSpriteRenderer == null && mei != null)
            meiSpriteRenderer = mei.GetComponentInChildren<SpriteRenderer>();

        // Verificações básicas
        if (mei == null) Debug.LogError("[CenaComponentesManager] 'mei' não atribuída no inspector.");
        if (waypoint == null) Debug.LogError("[CenaComponentesManager] 'waypoint' não atribuída no inspector.");
        if (listComponentes == null || listComponentes.Count == 0) Debug.LogWarning("[CenaComponentesManager] 'listComponentes' vazio - nada vai piscar.");

        // Inicia movimento
        StartCoroutine(MoverAteWaypoint());
    }

    IEnumerator MoverAteWaypoint()
    {
        if (mei == null || waypoint == null)
            yield break;

        // Marca que está indo
        indoParaWaypoint = true;

        // Se animator existir, aciona walking (garantimos set durante movimento)
        if (animator != null)
            animator.SetBool(walkingHash, true);

        while (Vector2.Distance((Vector2)mei.transform.position, (Vector2)waypoint.position) > distanciaParada)
        {
            // Move
            Vector3 novaPos = Vector3.MoveTowards(mei.transform.position, waypoint.position, velocidade * Time.deltaTime);
            mei.transform.position = novaPos;

            // Mantém o parâmetro de animação ativo enquanto se move (alguns controllers precisam de atualização contínua)
            if (animator != null)
                animator.SetBool(walkingHash, true);

            // Virar o sprite conforme direção
            if (flipSpriteAoMover && meiSpriteRenderer != null)
            {
                float dir = waypoint.position.x - mei.transform.position.x;
                if (Mathf.Abs(dir) > 0.01f)
                {
                    Vector3 s = meiSpriteRenderer.transform.localScale;
                    s.x = Mathf.Sign(dir) * Mathf.Abs(s.x);
                    meiSpriteRenderer.transform.localScale = s;
                }
            }
            else if (flipSpriteAoMover && meiSpriteRenderer == null)
            {
                // tenta usar transform.localScale do objeto principal se não houver SpriteRenderer separado
                float dir = waypoint.position.x - mei.transform.position.x;
                if (Mathf.Abs(dir) > 0.01f)
                {
                    Vector3 s = mei.transform.localScale;
                    s.x = Mathf.Sign(dir) * Mathf.Abs(s.x);
                    mei.transform.localScale = s;
                }
            }

            yield return null;
        }

        // Parou de andar: desliga walking
        if (animator != null)
            animator.SetBool(walkingHash, false);

        indoParaWaypoint = false;

        // Piscar e pegar componentes
        StartCoroutine(PiscarEColetar());
    }

    IEnumerator PiscarEColetar()
    {
        float duracao = 1.5f;
        float tempo = 0f;
        List<SpriteRenderer> sprites = new List<SpriteRenderer>();

        // Coleta todos os SpriteRenderers
        foreach (var obj in listComponentes)
        {
            if (obj == null) continue;
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
                sprites.Add(sr);
            else
                Debug.LogWarning($"[CenaComponentesManager] GameObject '{obj.name}' não tem SpriteRenderer.");
        }

        // Piscar branco ? preto
        while (tempo < duracao)
        {
            float t = Mathf.PingPong(Time.time * 5f, 1f); // Pisca rápido
            Color cor = Color.Lerp(Color.white, Color.black, t);

            foreach (var sr in sprites)
                sr.color = cor;

            tempo += Time.deltaTime;
            yield return null;
        }

        // Reseta a cor final para branco
        foreach (var sr in sprites)
            sr.color = Color.white;

        // Toca o som
        if (pegarComponentes != null)
            pegarComponentes.Play();

        foreach (var obj in listComponentes)
            if (obj != null) obj.SetActive(false);

        // Espera um pouco (ajuste se quiser ouvir o áudio inteiro)
        yield return new WaitForSeconds(0.5f);



        joystick.SetActive(true);

        // Ativa o diálogo
        if (dialogoMei2 != null)
            dialogoMei2.SetActive(true);
    }
}
