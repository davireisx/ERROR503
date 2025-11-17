using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerPersonagensSaemArcade : MonoBehaviour
{
    [Header("Referências")]
    public GameObject player;         // Personagem principal
    public Transform spawnPoint;      // Ponto de destino do player
    public GameObject efeitoFade;     // Objeto de fade (ativado e desativado)
    public AudioSource audioEfeito;     // Objeto de fade (ativado e desativado)

    [Header("NPCs e Spawns")]
    public List<GameObject> npcs;     // Lista de NPCs
    public List<Transform> spawnsNpc; // Lista de spawns correspondentes

    [Header("Configuração de Movimento")]
    public float playerSpeed = 2f;      // Velocidade de movimento do player
    public float npcSpeed = 1f;         // Velocidade de movimento dos NPCs
    public Animator animator;           // Animator do player
    public float fadeSpeed = 1.5f;      // Velocidade do esmaecimento
    public float piscarVelocidade = 5f; // Velocidade do piscar

    [Header("Configuração de Rotação")]
    public float rotationSpeed = 180f; // Velocidade de rotação em graus/segundo
    public bool rotacaoContinua = true; // Se true = gira, se false = balança

    [Header("Diálogo")]
    public GameObject dialogo;
    

    private void Start()
    {
        if (player != null && animator == null)
            animator = player.GetComponent<Animator>();

        if (efeitoFade != null)
            efeitoFade.SetActive(false);

        // Inicializa NPCs invisíveis
        foreach (GameObject npc in npcs)
        {
            if (npc != null)
            {
                SpriteRenderer sr = npc.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 0f;
                    sr.color = c;
                }
            }
        }

        StartCoroutine(ExecutarSequencia());
    }

    private IEnumerator ExecutarSequencia()
    {
        if (efeitoFade != null)
            efeitoFade.SetActive(true);

        // Garante que o player esteja virado para a direita
        if (player != null)
        {
            Vector3 escala = player.transform.localScale;
            escala.x = Mathf.Abs(escala.x); // sempre positivo (direita)
            player.transform.localScale = escala;
        }

        // Move o player até o destino
        while (player != null && spawnPoint != null &&
               Vector3.Distance(player.transform.position, spawnPoint.position) > 0.05f)
        {
            if (animator != null)
                animator.SetBool("walking", true);

            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                spawnPoint.position,
                playerSpeed * Time.deltaTime
            );

            yield return null;
        }


        if (animator != null)
            animator.SetBool("walking", false);

        audioEfeito.Play();

        // Inicia movimentação dos NPCs
        for (int i = 0; i < npcs.Count; i++)
        {
            if (npcs[i] != null && spawnsNpc.Count > i && spawnsNpc[i] != null)
            {
                StartCoroutine(MoverNpc(npcs[i], spawnsNpc[i]));
            }
        }

        yield return new WaitForSeconds(2.5f);

        if (efeitoFade != null)
            efeitoFade.SetActive(false);

        // Ativa o diálogo
        if (dialogo != null)
            dialogo.SetActive(true);
    }

    private IEnumerator MoverNpc(GameObject npc, Transform destino)
    {
        SpriteRenderer sr = npc.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float anguloAtual = 0f;

        // Escala inicial e final
        Vector3 escalaInicial = new Vector3(0.1f, 0.1f, 1f);
        Vector3 escalaFinal = new Vector3(0.35f, 0.35f, 1f);
        npc.transform.localScale = escalaInicial;

        float tempoCrescimento = 1f; // tempo em segundos para crescer totalmente
        float t = 0f;

        while (Vector3.Distance(npc.transform.position, destino.position) > 0.05f)
        {
            // Movimento
            npc.transform.position = Vector3.MoveTowards(
                npc.transform.position,
                destino.position,
                npcSpeed * Time.deltaTime
            );

            // Rotação
            if (rotacaoContinua)
            {
                anguloAtual += rotationSpeed * Time.deltaTime;
                npc.transform.rotation = Quaternion.Euler(0, 0, anguloAtual);
            }
            else
            {
                float rotacao = Mathf.Sin(Time.time * 2f) * rotationSpeed;
                npc.transform.rotation = Quaternion.Euler(0, 0, rotacao);
            }

            // Crescimento gradual
            t += Time.deltaTime / tempoCrescimento;
            npc.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, Mathf.Clamp01(t));

            // Esmaecer + piscar
            Color c = sr.color;
            float alpha = Mathf.Lerp(c.a, 1f, fadeSpeed * Time.deltaTime);
            float piscar = (Mathf.Sin(Time.time * piscarVelocidade) * 0.25f) + 0.75f;
            c.a = alpha * piscar;
            sr.color = c;

            yield return null;
        }

        // Final: posição, rotação, escala e alpha fixos
        npc.transform.position = destino.position;
        npc.transform.rotation = Quaternion.identity;
        npc.transform.localScale = escalaFinal;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }

}
