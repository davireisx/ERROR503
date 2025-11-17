using System.Collections;
using UnityEngine;

public class VilaoCipherController : MonoBehaviour
{
    [Header("Referências")]
    public GameObject cipher;        // Referência ao vilão Cipher
    public Transform destino;        // Ponto de destino do movimento
    public GameObject HUD;           // HUD do jogador
    public GameObject joystick;      // Joystick do jogador

    [Header("Configurações de Movimento")]
    public float velocidade = 3f;    // Velocidade do movimento
    [Tooltip("Distância até o destino onde Cipher iniciará a animação de desaparecimento")]
    public float distanciaParaDesaparecer = 1f;

    [Header("Animação de Desaparecimento")]
    public float tempoFade = 1f;     // Tempo da animação de sumiço
    public AnimationCurve curvaFade = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Vector3 posicaoInicial;
    private bool iniciouDesaparecimento = false;
    private SpriteRenderer sprite;
    private Animator anim;           // Referência ao Animator

    void Start()
    {
        if (cipher == null)
        {
            Debug.LogError("? Nenhum Cipher foi atribuído no Inspector!");
            return;
        }

        sprite = cipher.GetComponent<SpriteRenderer>();
        anim = cipher.GetComponent<Animator>(); // pega o Animator

        posicaoInicial = cipher.transform.position;

        if (HUD) HUD.SetActive(false);
        if (joystick) joystick.SetActive(false);
    }

    void Update()
    {
        if (cipher == null || iniciouDesaparecimento) return;

        // Movimento até o destino
        Vector3 posicaoAntiga = cipher.transform.position;
        cipher.transform.position = Vector3.MoveTowards(cipher.transform.position, destino.position, velocidade * Time.deltaTime);

        // Detecta se está se movendo
        bool estaAndando = Vector3.Distance(posicaoAntiga, cipher.transform.position) > 0.001f;
        if (anim) anim.SetBool("walking", estaAndando);

        float distanciaTotal = Vector3.Distance(posicaoInicial, destino.position);
        float distanciaPercorrida = Vector3.Distance(posicaoInicial, cipher.transform.position);

        // Quando estiver no meio do caminho ou próximo do destino
        if (distanciaPercorrida >= distanciaTotal / 2f || Vector3.Distance(cipher.transform.position, destino.position) <= distanciaParaDesaparecer)
        {
            StartCoroutine(AnimarDesaparecimento());
        }
    }

    IEnumerator AnimarDesaparecimento()
    {
        iniciouDesaparecimento = true;

        if (anim) anim.SetBool("walking", false); // Para a animação antes de sumir

        float tempo = 0f;
        Color corInicial = sprite.color;

        // Efeito de fade out suave com curva personalizada
        while (tempo < tempoFade)
        {
            tempo += Time.deltaTime;
            float t = tempo / tempoFade;
            float alpha = curvaFade.Evaluate(t);
            sprite.color = new Color(corInicial.r, corInicial.g, corInicial.b, alpha);
            yield return null;
        }

        // Desativa o Cipher e ativa o HUD + joystick
        cipher.SetActive(false);

        if (HUD) HUD.SetActive(true);
        if (joystick) joystick.SetActive(true);
    }
}
