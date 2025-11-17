using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CompletarFiacaoInversa : MonoBehaviour
{
    [Header("Fios a verificar")]
    public FioAlmoxarifado[] fios; // Atribua todos os fios no Inspector

    [Header("Refer?ncias para troca")]
    public Transform player;
    public Transform novoSpawnPoint;
    public GameObject joystick;
    public CameraManagerEsdras cameraManager;
    public Image telaFade;
    public RoboController robo;
    public GameObject HUD;
    public GameObject HUDJUNKO;
    public GameObject check;
    public GameObject dialogo;
    public GameObject junkrat;
    public GameObject tampa1;

    [Header("Configurações")]
    public float fadeDuration = 1f;
    public int novoCenarioIndex = 2;
    public bool segundaFiacao;

    private bool trocaFeita = false;

    void Start()
    {
        if (fios.Length == 0)
        {
            Debug.LogWarning("Nenhum fio atribu?do no array de fios.");
        }

        if (telaFade != null)
        {
            Color c = telaFade.color;
            c.a = 0;
            telaFade.color = c;
            telaFade.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (trocaFeita) return;

        bool todosConectadosErrado = true;

        foreach (FioAlmoxarifado fio in fios)
        {
            if (fio == null)
            {
                todosConectadosErrado = false;
                break;
            }

            // Se n?o est? conectado ou est? conectado no correto, falha a condi??o
            if (!fioConectadoErrado(fio))
            {
                todosConectadosErrado = false;
                break;
            }
        }

        if (todosConectadosErrado)
        {
            StartCoroutine(FazerTransicao());
            trocaFeita = true;
        }
    }

    bool fioConectadoErrado(FioAlmoxarifado fio)
    {
        // Ele est? conectado, mas n?o no destino correto
        return fio != null &&
               fio.GetEstaConectado() &&
               !fio.EstaNoDestinoCorreto();
    }

    IEnumerator FazerTransicao()
    {
        tampa1.SetActive(false);
        check.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        telaFade.gameObject.SetActive(true);
        yield return StartCoroutine(FadeIn());

        // ?? Restaura tamanho original da câmera
        if (Camera.main != null && RoboController.tamanhoOriginalCamera > 0)
        {
            Camera.main.orthographicSize = RoboController.tamanhoOriginalCamera;
            Debug.Log($"[CompletarFiacaoInversa] Câmera restaurada para tamanho original: {RoboController.tamanhoOriginalCamera}");
        }

        // Move player e ativa controles
        player.position = novoSpawnPoint.position;
        player.gameObject.SetActive(true);
        if (cameraManager != null) cameraManager.SetScenarioBounds(novoCenarioIndex);
        if (joystick != null) joystick.SetActive(true);
        if (HUD != null) HUD.SetActive(true);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FadeOut());

        dialogo.SetActive(true);

        if (segundaFiacao == true)
        {
            robo.StartCoroutine(robo.FadeBrancoParaPreto());
        }

        telaFade.gameObject.SetActive(false);

        Debug.Log("? Vitória invertida: jogador bagunçou tudo corretamente!");
    }


    public void DepoisDialogo()
    {
            junkrat.SetActive(true);
            robo.DarSegundaOrdem();
            HUDJUNKO.SetActive(true);

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