using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Referências")]
    public Camera mainCamera;
    public CanvasGroup fadeCanvas;
    public Transform spawnPoint;
    public GameObject personagemPrincipal;
    public GameObject objetoAtivar;
    public CameraManagerEsdras cam;

    [Header("Configurações de Zoom")]
    public float zoomDestino = 9.5f; // tamanho mínimo do zoom para o fade
    public float zoomVelocidade = 2f;

    [Header("Configurações de Fade")]
    public float fadeSpeed = 1.5f;

    private float zoomInicial;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Espera um frame para garantir que o CameraSeguirEsdras já ajustou o size
        StartCoroutine(IniciarCamera());
    }

    private IEnumerator IniciarCamera()
    {
        yield return null; // espera 1 frame
        zoomInicial = mainCamera.orthographicSize; // pega o size já ajustado
        StartCoroutine(ControlarCamera());
    }

    private IEnumerator ControlarCamera()
    {
        // 1 — ESPERA 1 SEGUNDO
        yield return new WaitForSeconds(1f);

        // 2 — DIMINUIR O ZOOM ATÉ 9.5
        while (mainCamera.orthographicSize > zoomDestino)
        {
            mainCamera.orthographicSize = Mathf.MoveTowards(mainCamera.orthographicSize, zoomDestino, zoomVelocidade * Time.deltaTime);
            yield return null;
        }
        mainCamera.orthographicSize = zoomDestino;

        // 3 — FADE IN
        float tempoFade = 0f;
        while (tempoFade < 1f)
        {
            tempoFade += Time.deltaTime * fadeSpeed;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, tempoFade);
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // 4 — TELEPORTA PERSONAGEM E SETA CENÁRIO
        if (personagemPrincipal != null && spawnPoint != null)
        {
            personagemPrincipal.transform.position = spawnPoint.position;
            personagemPrincipal.SetActive(false);
        }

        if (cam != null) cam.SetScenarioBounds(2);

        yield return new WaitForSeconds(0.3f); // pausa opcional

        // 5 — FADE OUT + ZOOM CRESCENDO ATÉ O SIZE INICIAL SALVO
        tempoFade = 0f;
        while (tempoFade < 1f || mainCamera.orthographicSize < zoomInicial)
        {
            tempoFade += Time.deltaTime * fadeSpeed;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, tempoFade);

            mainCamera.orthographicSize = Mathf.MoveTowards(mainCamera.orthographicSize, zoomInicial, zoomVelocidade * Time.deltaTime);

            yield return null;
        }

        fadeCanvas.alpha = 0f;
        mainCamera.orthographicSize = zoomInicial;

        // 6 — ATIVA O OBJETO FINAL
        if (objetoAtivar != null)
            objetoAtivar.SetActive(true);
    }
}
