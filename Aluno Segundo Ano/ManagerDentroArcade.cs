using System.Collections;
using UnityEngine;

public class ManagerDentroArcade : MonoBehaviour
{
    [Header("Referências")]
    public GameObject objetoPrincipal;
    public GameObject telaVerde;
    public GameObject telaVermelha;
    public GameObject milo;
    public GameObject mei;
    public GameObject dialogo;

    [Header("Fade")]
    public float fadeDuration = 0.5f;

    private SpriteRenderer miloRenderer;
    private SpriteRenderer meiRenderer;

    private bool previousActive;
    private bool alreadyTriggered = false;

    private void Awake()
    {
        if (milo != null) miloRenderer = milo.GetComponent<SpriteRenderer>();
        if (mei != null) meiRenderer = mei.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        previousActive = objetoPrincipal != null ? objetoPrincipal.activeSelf : false;

        if (telaVerde != null) telaVerde.SetActive(false);
        if (telaVermelha != null) telaVermelha.SetActive(false);
        if (milo != null) milo.SetActive(false);
        if (mei != null) mei.SetActive(false);
        if (dialogo != null) dialogo.SetActive(false);
    }

    private void Update()
    {
        if (alreadyTriggered || objetoPrincipal == null) return;

        bool currentActive = objetoPrincipal.activeSelf;

        if (previousActive && !currentActive)
        {
            alreadyTriggered = true;
            StartCoroutine(Sequencia());
        }

        previousActive = currentActive;
    }

    private IEnumerator Sequencia()
    {
        float tempo = 0f;
        bool estado = false;
        if (telaVerde != null) telaVerde.SetActive(false);
        if (telaVermelha != null) telaVermelha.SetActive(false);

        while (tempo < 0.7f)
        {
            estado = !estado;
            if (telaVerde != null) telaVerde.SetActive(estado);
            if (telaVermelha != null) telaVermelha.SetActive(!estado);

            yield return new WaitForSeconds(0.2f);
            tempo += 0.2f;
        }

        if (telaVerde != null) telaVerde.SetActive(false);
        if (telaVermelha != null) telaVermelha.SetActive(true);

        if (milo != null) milo.SetActive(true);
        if (mei != null) mei.SetActive(true);

        float alphaTarget = 130f / 255f; // ~0.51
        float t = 0f;

        if (miloRenderer != null) miloRenderer.color = new Color(miloRenderer.color.r, miloRenderer.color.g, miloRenderer.color.b, 0f);
        if (meiRenderer != null) meiRenderer.color = new Color(meiRenderer.color.r, meiRenderer.color.g, meiRenderer.color.b, 0f);

        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, alphaTarget, t / fadeDuration);
            if (miloRenderer != null) miloRenderer.color = new Color(miloRenderer.color.r, miloRenderer.color.g, miloRenderer.color.b, alpha);
            if (meiRenderer != null) meiRenderer.color = new Color(meiRenderer.color.r, meiRenderer.color.g, meiRenderer.color.b, alpha);

            t += Time.deltaTime;
            yield return null;
        }

        if (miloRenderer != null) miloRenderer.color = new Color(miloRenderer.color.r, miloRenderer.color.g, miloRenderer.color.b, alphaTarget);
        if (meiRenderer != null) meiRenderer.color = new Color(meiRenderer.color.r, meiRenderer.color.g, meiRenderer.color.b, alphaTarget);

        yield return new WaitForSeconds(0.8f);

        if (dialogo != null) dialogo.SetActive(true);
    }
}
