using UnityEngine;
using System.Collections;

public class ManagerTiro : MonoBehaviour
{
    [Header("Item Comunicador")]
    public Animator comunicadorAnim;

    [Header("Configuração da Bala")]
    public GameObject bala;
    public Transform waypointBala;
    public float velocidadeBala = 10f;
    public Animator balaAnim;
    public float tempoCrescimento = 1.5f;
    public Vector3 escalaFinal = new Vector3(3f, 3f, 1f);

    [Header("Efeito da Faísca")]
    public GameObject faisca;

    [Header("Sons do Disparo")]
    public AudioSource audioDisparo;   // Som do disparo
    public AudioSource audioImpacto;   // ?? Som ao chegar no destino

    [Header("Transição após o disparo")]
    public GameObject transicao; // Objeto de transição a ser ativado no final

    private void Start()
    {
        StartCoroutine(Disparar());
    }

    private IEnumerator Disparar()
    {
        // Ativa faísca
        if (faisca != null)
        {
            faisca.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            faisca.SetActive(false);
        }

        // Animação do comunicador
        if (comunicadorAnim != null)
            comunicadorAnim.SetTrigger("disparo");

        // Som do disparo
        if (audioDisparo != null)
            audioDisparo.Play();

        // Movimento da bala
        if (bala != null && waypointBala != null)
        {
            bala.SetActive(true);

            Vector3 destino = waypointBala.position;
            Vector3 escalaInicial = bala.transform.localScale;
            float tempo = 0f;

            while (Vector3.Distance(bala.transform.position, destino) > 0.05f)
            {
                tempo += Time.deltaTime;
                float t = Mathf.Clamp01(tempo / tempoCrescimento);

                // Crescimento da bala ao longo do trajeto
                bala.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);

                // Movimento até o destino
                bala.transform.position = Vector3.MoveTowards(
                    bala.transform.position,
                    destino,
                    velocidadeBala * Time.deltaTime
                );

                yield return null;
            }

            // Bala chegou ao destino
            bala.transform.position = destino;

            // ?? Toca som de impacto
            if (audioImpacto != null)
                audioImpacto.Play();

            // Ativa animação "finalizada"
            if (balaAnim != null)
                balaAnim.SetBool("finalizada", true);

            // Espera 3 segundos
            yield return new WaitForSeconds(0.5f);

            // Desativa a bala
            bala.SetActive(false);

            // Ativa objeto de transição
            if (transicao != null)
                transicao.SetActive(true);
        }
    }
}
