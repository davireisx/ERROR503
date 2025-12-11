using System.Collections;
using UnityEngine;

public class MordidaPretinha : MonoBehaviour
{
    [Header("Referências do cachorro")]
    public Transform cachorro;         // Transform do cachorro
    public Animator animator;          // Animator do cachorro
    public Transform waypointMordida;  // Destino da mordida
    public Transform waypointInicio;   // Posição inicial para voltar
    public AudioSource mordidaAudio;   // Áudio da mordida
    public AudioSource latidoAudio;    // Áudio do latido

    [Header("Referências do Cipher")]
    public SpriteRenderer cipherRenderer;

    [Header("Referência do diálogo")]
    public GameObject dialogo;

    [Header("Configurações")]
    public float velocidadeCachorro = 3f;
    public float duracaoMordida = 1.5f;     // ?? Duração da animação de mordida
    public float duracaoPiscarCipher = 3f;  // ? Duração total do piscar do Cipher
    public Color corPiscar = Color.red;

    private Color corOriginalCipher;
    private bool estaExecutando = false;
    private Vector3 escalaOriginal;

    public void Start()
    {
        // Guarda a escala original para os flips
        escalaOriginal = cachorro.localScale;
        AtivarMordida();
    }

    public void AtivarMordida()
    {
        if (cachorro == null || animator == null || waypointMordida == null ||
            waypointInicio == null || cipherRenderer == null || dialogo == null)
        {
            Debug.LogWarning("Faltando referências no MordidaPretinha!");
            return;
        }

        if (estaExecutando) return;

        corOriginalCipher = cipherRenderer.color;
        StartCoroutine(ExecutarMordida());
    }

    private IEnumerator ExecutarMordida()
    {
        estaExecutando = true;

        Debug.Log($"Iniciando movimento para mordida: {cachorro.position} -> {waypointMordida.position}");

        // 1?? LATIDO PARA WALKING: walking true e latido false
        if (latidoAudio != null)
        {
            latidoAudio.Play();
            yield return new WaitForSeconds(0.5f); // Pequeno delay para o latido
        }

        animator.SetBool("walking", true);
        animator.SetBool("latido", false);

        // Move até o waypoint de mordida
        float distancia = Vector3.Distance(cachorro.position, waypointMordida.position);

        while (distancia > 0.1f)
        {
            cachorro.position = Vector3.MoveTowards(
                cachorro.position,
                waypointMordida.position,
                velocidadeCachorro * Time.deltaTime
            );

            distancia = Vector3.Distance(cachorro.position, waypointMordida.position);
            yield return null;
        }

        Debug.Log("Chegou no waypoint de mordida!");

        // 2?? WALKING PARA MORDIDA: mordida true e walking false
        animator.SetBool("walking", false);
        animator.SetBool("mordida", true);

        // Toca áudio da mordida
        if (mordidaAudio != null)
            mordidaAudio.Play();

        // 3?? Executa mordida (1.5s) e piscar (3s) ao mesmo tempo
        StartCoroutine(PiscarCipherCoroutine()); // piscar paralelo
        yield return new WaitForSeconds(duracaoMordida); // mordida dura 1.5s

        // Após a animação de mordida, ela permanece mordendo até piscar acabar
        // Mantém mordida ativa até o piscar terminar
        yield return new WaitForSeconds(duracaoPiscarCipher - duracaoMordida);

        // 4?? Finaliza mordida
        animator.SetBool("mordida", false);
        animator.SetBool("walking", true);

        if (mordidaAudio != null)
            mordidaAudio.Stop();

        // 5?? Ativa diálogo
        dialogo.SetActive(true);

        // 6?? VOLTA PARA O WAYPOINT INICIAL
        Debug.Log("Iniciando volta para posição inicial");

        // Flip no X para virar o cachorro
        Vector3 novaEscala = escalaOriginal;
        novaEscala.x *= -1; // Inverte no eixo X
        cachorro.localScale = novaEscala;

        // Move de volta para o waypoint inicial (já está em walking)
        distancia = Vector3.Distance(cachorro.position, waypointInicio.position);
        while (distancia > 0.1f)
        {
            cachorro.position = Vector3.MoveTowards(
                cachorro.position,
                waypointInicio.position,
                velocidadeCachorro * Time.deltaTime
            );
            distancia = Vector3.Distance(cachorro.position, waypointInicio.position);
            yield return null;
        }

        Debug.Log("Chegou no waypoint inicial!");

        // 7?? WALKING PARA IDLE
        animator.SetBool("walking", false);
        animator.SetBool("idle", true);

        // Restaura orientação original
        cachorro.localScale = escalaOriginal;

        estaExecutando = false;
    }

    // ? Piscar o Cipher por 3 segundos
    private IEnumerator PiscarCipherCoroutine()
    {
        float tempo = 0f;
        while (tempo < duracaoPiscarCipher)
        {
            tempo += Time.deltaTime;
            float lerp = Mathf.PingPong(tempo * 2f, 1f);
            cipherRenderer.color = Color.Lerp(corOriginalCipher, corPiscar, lerp);
            yield return null;
        }
        cipherRenderer.color = corOriginalCipher;
    }

    // Gizmos (visual debug)
    private void OnDrawGizmos()
    {
        if (cachorro != null)
        {
            if (waypointMordida != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(cachorro.position, waypointMordida.position);
                Gizmos.DrawSphere(waypointMordida.position, 0.2f);
            }

            if (waypointInicio != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(cachorro.position, waypointInicio.position);
                Gizmos.DrawSphere(waypointInicio.position, 0.2f);
            }
        }
    }

    [ContextMenu("Testar Mordida")]
    public void TestarMordida()
    {
        if (Application.isPlaying)
            AtivarMordida();
        else
            Debug.Log("Execute no Play Mode para testar");
    }
}
