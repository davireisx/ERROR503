using UnityEngine;
using System.Collections;

public class PersonagemComBala : MonoBehaviour
{
    [Header("Referência do Personagem")]
    public GameObject personagem;
    public Transform spawnPoint;
    public float velocidadePersonagem = 2f;
    public Animator anim;

    [Header("Item Comunicador")]
    public GameObject comunicador;
    public Animator comunicadorAnim;

    [Header("Configuração da Bala")]
    public GameObject bala;
    public Transform waypointBala;
    public float velocidadeInicialBala = 1f;
    public float velocidadeMaxBala = 10f;
    public float tempoCrescimento = 2f;
    public Animator balaAnim;

    [Header("Efeito da Faísca")]
    public GameObject faisca;

    [Header("Efeitos Sonoros")]
    public AudioSource audioDisparo;
    public AudioSource audioImpacto;

    [Header("Diálogo do Vilão")]
    public GameObject dialogo;

    [Header("GameObject Pré-Tiro (mostrado por 3s)")]
    public GameObject preTiroObj; // <- Novo campo para o objeto que aparece por 3s

    private bool personagemChegou = false;
    private bool balaDisparada = false;
    private Vector3 escalaInicialBala;
    private Vector3 escalaFinalBala = new Vector3(5f, 5f, 1f);

    private void Start()
    {
        if (personagem == null)
        {
            Debug.LogWarning("? Nenhum personagem atribuído no inspector!");
            return;
        }

        // Configura a bala
        if (bala != null)
        {
            bala.SetActive(false);
            escalaInicialBala = bala.transform.localScale * 0.2f;
            bala.transform.localScale = escalaInicialBala;
        }

        // Desativa os objetos no início
        if (faisca != null) faisca.SetActive(false);
        if (dialogo != null) dialogo.SetActive(false);
        if (preTiroObj != null) preTiroObj.SetActive(false);

        // Animação inicial
        if (anim != null)
            anim.SetBool("walking", true);

        // Comunicador
        if (comunicadorAnim != null)
        {
            comunicadorAnim.SetBool("carrega", false);
            comunicadorAnim.SetBool("carregado", false);
        }
    }

    private void Update()
    {
        if (personagem == null || spawnPoint == null) return;

        if (!personagemChegou)
        {
            // Enquanto caminha, o comunicador carrega
            if (comunicadorAnim != null)
            {
                comunicadorAnim.SetBool("carrega", true);
                comunicadorAnim.SetBool("carregado", false);
            }

            // Movimento até o ponto
            personagem.transform.position = Vector3.MoveTowards(
                personagem.transform.position,
                spawnPoint.position,
                velocidadePersonagem * Time.deltaTime
            );

            // Verifica chegada
            if (Vector3.Distance(personagem.transform.position, spawnPoint.position) < 0.1f)
            {
                personagemChegou = true;

                // Para animação de andar
                if (anim != null)
                    anim.SetBool("walking", false);

                // Atualiza estados do comunicador
                if (comunicadorAnim != null)
                {
                    comunicadorAnim.SetBool("carrega", false);
                    comunicadorAnim.SetBool("carregado", true);
                }

                // ? Ativa o objeto por 3 segundos antes do disparo
                StartCoroutine(EsperarAntesDoDisparo());
            }
        }
    }

    private IEnumerator EsperarAntesDoDisparo()
    {
        if (preTiroObj != null)
        {
            preTiroObj.SetActive(true);
            yield return new WaitForSeconds(1f);
            preTiroObj.SetActive(false);
        }

        // Depois dos 3 segundos, dispara normalmente
        if (!balaDisparada && bala != null)
            StartCoroutine(DispararBala());
    }

    private IEnumerator DispararBala()
    {
        balaDisparada = true;
        bala.SetActive(true);

        if (audioDisparo != null)
            audioDisparo.Play();

        if (faisca != null)
        {
            faisca.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            faisca.SetActive(false);
        }

        float tempo = 0f;

        while (bala != null && Vector3.Distance(bala.transform.position, waypointBala.position) > 0.1f)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / tempoCrescimento);

            bala.transform.localScale = Vector3.Lerp(escalaInicialBala, escalaFinalBala, t);
            float velocidadeAtual = Mathf.Lerp(velocidadeInicialBala, velocidadeMaxBala, t);

            bala.transform.position = Vector3.MoveTowards(
                bala.transform.position,
                waypointBala.position,
                velocidadeAtual * Time.deltaTime
            );

            yield return null;
        }

        if (bala != null)
        {
            bala.transform.position = waypointBala.position;

            if (audioImpacto != null)
                audioImpacto.Play();

            if (balaAnim != null)
                balaAnim.SetBool("finalizada", true);

            if (dialogo != null)
                dialogo.SetActive(true);

            yield return new WaitForSeconds(0.5f);
            bala.SetActive(false);
        }
    }
}
