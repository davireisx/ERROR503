using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCipherIntro : MonoBehaviour
{
    [Header("Referências")]
    public GameObject cipher;
    public GameObject telaVerde;
    public GameObject telaVermelha;
    public List<GameObject> personagens; // Lista de objetos que vão aparecer no fade in
    public GameObject dialogo;

    [Header("Fade")]
    public float fadeDuration = 1.5f; // Tempo de fade-in dos personagens

    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    private const float targetAlpha = 140f / 255f; // 0 ? 140 no range 0-255

    private void Start()
    {
        // Garante que Cipher começa desativado
        if (cipher != null)
            cipher.SetActive(false);

        // Garante que os personagens começam invisíveis (alpha 0)
        foreach (var personagem in personagens)
        {
            if (personagem != null)
            {
                personagem.SetActive(true); // precisa estar ativo para mexer no SpriteRenderer
                var renderer = personagem.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Color cor = renderer.color;
                    cor.a = 0f;
                    renderer.color = cor;
                    renderers.Add(renderer);
                }
            }
        }

        // Garante que diálogo começa desativado
        if (dialogo != null)
            dialogo.SetActive(false);

        StartCoroutine(ExecutarSequencia());
    }

    private IEnumerator ExecutarSequencia()
    {
        // --- Etapa 1: Piscar telas ---
        float tempo = 0f;
        bool estado = false;
        if (telaVerde != null) telaVerde.SetActive(false);
        if (telaVermelha != null) telaVermelha.SetActive(false);

        while (tempo < 0.7f)
        {
            estado = !estado;
            if (telaVerde != null) telaVerde.SetActive(estado);
            if (telaVermelha != null) telaVermelha.SetActive(!estado);

            yield return new WaitForSeconds(0.15f); // velocidade do piscar
            tempo += 0.15f;
        }

        // --- Etapa 2: Apenas tela verde ativada ---
        if (telaVerde != null) telaVerde.SetActive(true);
        if (telaVermelha != null) telaVermelha.SetActive(false);

        // --- Etapa 3: Fade in dos personagens ---
        float t = 0f;
        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, targetAlpha, t / fadeDuration);

            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    Color cor = renderer.color;
                    cor.a = alpha;
                    renderer.color = cor;
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Garante alpha final = 140/255
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                Color cor = renderer.color;
                cor.a = targetAlpha;
                renderer.color = cor;
            }
        }

        // --- Etapa 4: Ativar diálogo ---
        if (dialogo != null)
            dialogo.SetActive(true);
    }
}
