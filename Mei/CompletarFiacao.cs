using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CompletarFiacao : MonoBehaviour
{
    [Header("Objetos a verificar")]
    public GameObject[] objetosParaVerificar;

    [Header("Referências para troca")]
    public Transform player;
    public Transform novoSpawnPoint;
    public GameObject joystick;
    public CameraManagerEsdras cameraManager;
    public Image telaFade;

    [Header("Desativar Script Após Troca")]
    public GameObject objetoComInteragir;
    public GameObject HUD;
    public GameObject ativa;

    [Header("Vagão para pintar")]
    public GameObject vagao;

    [Header("Fiação (novo objeto)")]
    public GameObject fiacao;
    public GameObject fiacaoNovaImagem;

    [Header("Configurações")]
    public float fadeDuration = 1f;
    public int novoCenarioIndex = 2;

    private bool trocaFeita = false;

    void Start()
    {
        if (objetosParaVerificar.Length != 4)
            Debug.LogWarning("Você precisa atribuir exatamente 4 objetos para verificação.");

        if (telaFade != null)
        {
            Color c = telaFade.color;
            c.a = 0;
            telaFade.color = c;
            telaFade.gameObject.SetActive(false);
        }

        if (fiacaoNovaImagem != null)
            fiacaoNovaImagem.SetActive(false);
    }

    void Update()
    {
        if (trocaFeita) return;

        bool todosAtivos = true;
        foreach (GameObject obj in objetosParaVerificar)
        {
            if (obj == null || !obj.activeSelf)
            {
                todosAtivos = false;
                break;
            }
        }

        if (todosAtivos)
        {
            StartCoroutine(FazerTransicao());
            trocaFeita = true;
        }
    }

    IEnumerator FazerTransicao()
    {
        telaFade.gameObject.SetActive(true);
        yield return StartCoroutine(FadeIn());

        player.position = novoSpawnPoint.position;

        // ?? RESTAURA o tamanho da câmera salvo anteriormente
        if (Camera.main != null && PlayerPrefs.HasKey("UltimoCameraSize"))
        {
            float sizeSalvo = PlayerPrefs.GetFloat("UltimoCameraSize");
            Camera.main.orthographicSize = sizeSalvo;
            Debug.Log($"Tamanho da câmera restaurado: {sizeSalvo}");
        }

        if (cameraManager != null)
            cameraManager.SetScenarioBounds(novoCenarioIndex);

        player.gameObject.SetActive(true);
        ativa.SetActive(false);

        if (joystick != null) joystick.SetActive(true);

        if (objetoComInteragir != null)
        {
            InteragirTrocaCenario interagir = objetoComInteragir.GetComponent<InteragirTrocaCenario>();
            if (interagir != null)
            {
                interagir.enabled = false;
                Debug.Log("Script InteragirTrocaCenario desativado do objeto original.");
            }
        }

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FadeOut());

        telaFade.gameObject.SetActive(false);
        HUD.gameObject.SetActive(true);
        joystick.gameObject.SetActive(true);

        Debug.Log("Troca de cenário concluída.");

        SpriteRenderer srVagao = vagao != null ? vagao.GetComponent<SpriteRenderer>() : null;
        if (srVagao != null)
            yield return StartCoroutine(PiscarVagao(srVagao));
    }

    IEnumerator PiscarVagao(SpriteRenderer srVagao)
    {
        float tempo = 0f;
        float duracao = 1.5f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime * 5f;
            float t = Mathf.PingPong(Time.time * 5f, 1f);
            srVagao.color = Color.Lerp(Color.black, Color.white, t);
            yield return null;
        }

        srVagao.color = Color.white;

        if (fiacaoNovaImagem != null)
            fiacaoNovaImagem.SetActive(true);

        if (fiacao != null)
        {
            InteragirTrocaCenario interagirFiacao = fiacao.GetComponent<InteragirTrocaCenario>();
            if (interagirFiacao != null)
                interagirFiacao.enabled = true;
        }

        Debug.Log("Vagão piscou e fiação ativada com sucesso!");
    }

    IEnumerator FadeIn()
    {
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
