using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FugirArcade : MonoBehaviour
{
    [Header("Personagens Principais")]
    public GameObject personagem1;
    public GameObject personagem2;
    public GameObject personagem3;
    public GameObject personagem4;
    public GameObject dialogo;
    public GameObject efeito;

    private Animator anim1;
    private Animator anim2;
    private Animator anim3;
    private Animator anim4;

    [Header("Waypoints")]
    public Transform[] waypoints1;
    public Transform[] waypoints2;
    public Transform[] waypoints3;
    public Transform[] waypoints4;

    [Header("Configurações")]
    public float velocidade = 3f;

    private int personagensFinalizados = 0;

    [Header("Novo Personagem")]
    public GameObject novoPersonagem;          // Novo personagem que surge
    public Transform spawnDestino;             // Ponto para onde ele irá
    public AudioSource somAparicao;            // Som ao aparecer
    public float velocidadeNovo = 2f;          // Velocidade de movimento
    public float rotacaoVelocidade = 180f;     // Velocidade da rotação
    public float piscarVelocidade = 5f;        // Velocidade do piscar
    public float tempoCrescimento = 1.2f;      // Tempo para atingir o tamanho final
    public Vector3 escalaFinal = new Vector3(0.4f, 0.4f, 1f);

    void Start()
    {
        // Inicializa animators
        anim1 = personagem1.GetComponent<Animator>();
        anim2 = personagem2.GetComponent<Animator>();
        anim3 = personagem3.GetComponent<Animator>();
        anim4 = personagem4.GetComponent<Animator>();

        // Desativa temporariamente o script "Alunos" do quarto personagem
        Alunos alunoScript = personagem4.GetComponent<Alunos>();
        if (alunoScript != null)
            alunoScript.enabled = false;

        // Inicia movimento dos personagens
        if (waypoints1.Length > 0) StartCoroutine(MoverPersonagem(personagem1, anim1, waypoints1));
        if (waypoints2.Length > 0) StartCoroutine(MoverPersonagem(personagem2, anim2, waypoints2));
        if (waypoints3.Length > 0) StartCoroutine(MoverPersonagem(personagem3, anim3, waypoints3));
        if (waypoints4.Length > 0) StartCoroutine(MoverPersonagem(personagem4, anim4, waypoints4, alunoScript));

        // Inicia o novo personagem
        if (novoPersonagem != null && spawnDestino != null)
            StartCoroutine(AparicaoNovoPersonagem());
    }

    private IEnumerator MoverPersonagem(GameObject personagem, Animator animator, Transform[] waypoints, Alunos alunoScript = null)
    {
        if (waypoints.Length == 0) yield break;

        SpriteRenderer sr = personagem.GetComponent<SpriteRenderer>();
        string originalLayer = sr.sortingLayerName;

        bool isThird = personagem == personagem3;
        if (isThird) sr.sortingLayerName = "Componentes";

        animator.SetBool("walking", true);

        for (int i = 0; i < waypoints.Length; i++)
        {
            Vector3 startPos = personagem.transform.position;
            Vector3 endPos = waypoints[i].position;
            float distance = Vector3.Distance(startPos, endPos);

            if (distance < 0.01f) continue;

            float t = 0f;
            while (t < 1f)
            {
                t += (velocidade / distance) * Time.deltaTime;
                personagem.transform.position = Vector3.Lerp(startPos, endPos, Mathf.Min(t, 1f));

                Vector3 dir = endPos - personagem.transform.position;
                if (dir.x != 0)
                    personagem.transform.localScale = new Vector3(Mathf.Sign(dir.x) * Mathf.Abs(personagem.transform.localScale.x),
                        personagem.transform.localScale.y, personagem.transform.localScale.z);

                yield return null;
            }

            personagem.transform.position = endPos;
        }

        if (isThird) sr.sortingLayerName = originalLayer;
        animator.SetBool("walking", false);
        personagensFinalizados++;

        // Reativa o script "Alunos" após terminar
        if (alunoScript != null)
            alunoScript.enabled = true;
    }

    private IEnumerator AparicaoNovoPersonagem()
    {
        efeito.SetActive(true);
        novoPersonagem.SetActive(true);

        if (somAparicao != null)
            somAparicao.Play();

        SpriteRenderer sr = novoPersonagem.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        // Começa pequeno e invisível (alpha 0)
        Vector3 escalaInicial = new Vector3(0.1f, 0.1f, 1f);
        novoPersonagem.transform.localScale = escalaInicial;

        Color c = Color.black;
        c.a = 0f;
        sr.color = c;

        float tempo = 0f;
        float angulo = 0f;

        while (Vector3.Distance(novoPersonagem.transform.position, spawnDestino.position) > 0.05f)
        {
            // Movimento até o destino
            novoPersonagem.transform.position = Vector3.MoveTowards(
                novoPersonagem.transform.position,
                spawnDestino.position,
                velocidadeNovo * Time.deltaTime
            );

            // Rotação contínua
            angulo += rotacaoVelocidade * Time.deltaTime;
            novoPersonagem.transform.rotation = Quaternion.Euler(0, 0, angulo);

            // Crescimento gradual
            tempo += Time.deltaTime / tempoCrescimento;
            float progresso = Mathf.Clamp01(tempo);
            novoPersonagem.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, progresso);

            // Alpha aumenta junto com o crescimento
            float alpha = Mathf.Lerp(0f, 1f, progresso);

            // Piscar preto ? branco ? transparente, influenciado pelo alpha
            float blink = Mathf.Sin(Time.time * piscarVelocidade);
            Color baseColor;

            if (blink > 0)
                baseColor = Color.Lerp(Color.black, Color.white, blink);
            else
                baseColor = Color.Lerp(Color.white, Color.clear, -blink);

            // Aplica alpha gradual
            baseColor.a *= alpha;
            sr.color = baseColor;

            yield return null;
        }

        // Finaliza fixo
        novoPersonagem.transform.position = spawnDestino.position;
        novoPersonagem.transform.rotation = Quaternion.identity;
        novoPersonagem.transform.localScale = escalaFinal;

        Color finalColor = Color.white;
        finalColor.a = 1f;
        sr.color = finalColor;

        efeito.SetActive(false);
        yield return new WaitForSeconds(0.8f);
        dialogo.SetActive(true);
    }

}
