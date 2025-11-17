using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // para mudar de cena

public class ManagerFinalEstacao : MonoBehaviour
{
    [Header("Objetos a controlar")]
    public GameObject hud;           // ?? Novo objeto HUD (pai do joystick, barras, etc.)
    public GameObject joystick;
    public GameObject personGame;
    public GameObject objetoCheck;
    public GameObject fadePanel;     // precisa ter um CanvasGroup
    public CanvasGroup fadeCanvasGroup;

    [Header("Configuração do Fade")]
    public float duracaoFade = 1.5f;

    [Header("Som e Transição")]
    //public AudioSource somFinal;     // ?? som tocado quando o personagem for desativado
    public string proximaCena;       // ?? nome da cena a carregar após o fade

    private bool hudDesativado = false;

    void Start()
    {
        // Garante que o painel tenha um CanvasGroup
        if (fadePanel != null)
        {
            fadeCanvasGroup = fadePanel.GetComponent<CanvasGroup>();
            if (fadeCanvasGroup == null)
                fadeCanvasGroup = fadePanel.AddComponent<CanvasGroup>();

            fadeCanvasGroup.alpha = 0f; // começa invisível
        }
    }

    void OnEnable()
    {


        // Começa o fade in
        if (fadePanel != null && fadeCanvasGroup != null)
            StartCoroutine(FadeIn());

        // Depois de 1 segundo, ativa o personagem e o check
        StartCoroutine(AtivarPersonagemECheck());
    }

    IEnumerator AtivarPersonagemECheck()
    {

        if (objetoCheck != null)
            objetoCheck.SetActive(true);

        yield return new WaitForSeconds(1f);

        if (personGame != null)
        {
            personGame.SetActive(false);

           
        }
    }

    IEnumerator FadeIn()
    {
        float tempo = 0f;
        hudDesativado = false;

        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / duracaoFade;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progresso);

            // Quando o fade estiver na metade (alpha ? 0.5)
            if (!hudDesativado && progresso >= 0.5f)
            {
                hudDesativado = true;
                DesativarHUD();
            }

            yield return null;
        }

        fadeCanvasGroup.alpha = 1f; // garante opacidade total no fim

        // ?? Troca de cena após o fade completo
        if (!string.IsNullOrEmpty(proximaCena))
        {
            SceneManager.LoadScene(proximaCena);
        }
    }

    void DesativarHUD()
    {
        // ?? Desativa o HUD completo se existir
        if (hud != null)
        {
            hud.SetActive(false);
            return;
        }

        // ?? Ou desativa elementos individuais (fallback)
        if (joystick != null)
            joystick.SetActive(false);

        if (personGame != null)
            personGame.SetActive(false);
    }
}
