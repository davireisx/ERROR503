using System.Collections;
using UnityEngine;

public class ManagerDepoisMei : MonoBehaviour
{
    [Header("Referências")]
    public GameObject milo;
    public GameObject mei;
    public GameObject cipher;
    public GameObject dialogo;

    [Header("Fade")]
    public float fadeDuration = 0.5f;

    private SpriteRenderer miloRenderer;
    private SpriteRenderer meiRenderer;
    private SpriteRenderer cipherRenderer;

    private void Awake()
    {
        if (milo != null) miloRenderer = milo.GetComponent<SpriteRenderer>();
        if (mei != null) meiRenderer = mei.GetComponent<SpriteRenderer>();
        if (cipher != null) cipherRenderer = cipher.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(ExecutarTransicao());
    }

    private IEnumerator ExecutarTransicao()
    {
        if (milo == null || mei == null || cipher == null || dialogo == null)
            yield break;

        cipher.SetActive(true);
        if (cipherRenderer != null)
        {
            Color corCipher = cipherRenderer.color;
            corCipher.a = 0f;
            cipherRenderer.color = corCipher;
        }

        if (miloRenderer != null) miloRenderer.color = new Color(miloRenderer.color.r, miloRenderer.color.g, miloRenderer.color.b, 130f / 255f);
        if (meiRenderer != null) meiRenderer.color = new Color(meiRenderer.color.r, meiRenderer.color.g, meiRenderer.color.b, 130f / 255f);

        float alphaTarget = 130f / 255f; // ~0.51
        float t = 0f;

        while (t < fadeDuration)
        {
            float alphaOut = Mathf.Lerp(alphaTarget, 0f, t / fadeDuration);
            float alphaIn = Mathf.Lerp(0f, alphaTarget, t / fadeDuration);

            if (miloRenderer != null) miloRenderer.color = new Color(miloRenderer.color.r, miloRenderer.color.g, miloRenderer.color.b, alphaOut);
            if (meiRenderer != null) meiRenderer.color = new Color(meiRenderer.color.r, meiRenderer.color.g, meiRenderer.color.b, alphaOut);
            if (cipherRenderer != null) cipherRenderer.color = new Color(cipherRenderer.color.r, cipherRenderer.color.g, cipherRenderer.color.b, alphaIn);

            t += Time.deltaTime;
            yield return null;
        }

        if (miloRenderer != null) { miloRenderer.color = new Color(miloRenderer.color.r, miloRenderer.color.g, miloRenderer.color.b, 0f); milo.SetActive(false); }
        if (meiRenderer != null) { meiRenderer.color = new Color(meiRenderer.color.r, meiRenderer.color.g, meiRenderer.color.b, 0f); mei.SetActive(false); }
        if (cipherRenderer != null) cipherRenderer.color = new Color(cipherRenderer.color.r, cipherRenderer.color.g, cipherRenderer.color.b, alphaTarget);

        yield return new WaitForSeconds(1);
        dialogo.SetActive(true);
    }
}
