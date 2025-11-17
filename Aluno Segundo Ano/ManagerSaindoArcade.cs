using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerSaindoArcade : MonoBehaviour
{
    [Header("Referências")]
    public List<GameObject> objetosPiscantes; // Lista de objetos que piscam
    public GameObject player;
    public Transform spawnPoint;
    public GameObject dialogo;
    public CameraManagerEsdras camManager;
    public GameObject audioTocar;
    public AudioSource audioAlerta;
    public AudioSource audioCheck;

    [Header("Fade")]
    public CanvasGroup fadeCanvas; // CanvasGroup usado para fade
    public float fadeDuration = 1.5f;

    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    private Dictionary<SpriteRenderer, Color> coresOriginais = new Dictionary<SpriteRenderer, Color>();

    private void Start()
    {
        // Pega SpriteRenderers e guarda as cores originais
        foreach (var obj in objetosPiscantes)
        {
            if (obj != null)
            {
                var r = obj.GetComponent<SpriteRenderer>();
                if (r != null)
                {
                    renderers.Add(r);
                    coresOriginais[r] = r.color;
                }
            }
        }

        if (dialogo != null) dialogo.SetActive(false);

        // Garante que tela começa invisível
        if (fadeCanvas != null) fadeCanvas.alpha = 0f;

        StartCoroutine(ExecutarSequencia());
    }

    private IEnumerator ExecutarSequencia()
    {
        // --- Etapa 1: Piscando objetos (entre cor original e branco com mesmo alpha) ---
        float tempo = 0f;
        bool estado = false;

        while (tempo < 2f)
        {
            estado = !estado;

            foreach (var r in renderers)
            {
                if (r != null)
                {
                    Color corOriginal = coresOriginais[r];
                    if (estado)
                        r.color = new Color(1f, 1f, 1f, corOriginal.a); // Branco com alpha original
                    else
                        r.color = corOriginal;
                }
            }
            audioAlerta.Play();
            yield return new WaitForSeconds(0.2f);
            tempo += 0.2f;
        }

        audioAlerta.Stop();
        audioCheck.Play();
        // --- Etapa 2: Todos brancos no final (com alpha original) ---
        foreach (var r in renderers)
        {
            if (r != null)
            {
                float a = coresOriginais[r].a;
                r.color = new Color(1f, 1f, 1f, a);
            }
        }

        // Espera 2 segundos
        yield return new WaitForSeconds(2f);

        // --- Etapa 3: Fade In ---
        yield return StartCoroutine(Fade(0f, 1f));

        // --- Etapa 4: Teleporta player ---
        if (player != null && spawnPoint != null)
            player.transform.position = spawnPoint.position;

        if (camManager != null)
            camManager.SetScenarioBounds(1);

        if (player != null)
            player.SetActive(true);

            audioTocar.SetActive(false);

        // --- Etapa 5: Fade Out ---
        yield return StartCoroutine(Fade(1f, 0f));

        // --- Etapa 6: Ativar diálogo ---
        if (dialogo != null) dialogo.SetActive(true);
    }

    private IEnumerator Fade(float start, float end)
    {
        if (fadeCanvas == null) yield break;

        float t = 0f;
        while (t < fadeDuration)
        {
            fadeCanvas.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }

        fadeCanvas.alpha = end;
    }
}
