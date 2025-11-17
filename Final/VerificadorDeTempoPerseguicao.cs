using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class VerificadorDeTempoPerseguicao : MonoBehaviour
{
    [Header("Referências")]
    public JezabelController vilao;
    public Alunos player;
    public Text textoTimer;
    public CanvasGroup fadeCanvas;
    public float duracaoFade = 1.5f;

    [Header("Configurações de Tempo")]
    public int tempoMaximo = 10; // contador inteiro
    public Color corNormal = Color.white;
    public Color corAlerta = Color.red;

    private float tempoRestante;
    private bool gameOverAtivo = false;

    void Start()
    {
        tempoRestante = tempoMaximo;

        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;

        AtualizarTexto();

        if (vilao != null)
            vilao.OnDispararProximoWaypoint += ReiniciarTimer;
    }

    void Update()
    {
        if (gameOverAtivo) return;

        tempoRestante -= Time.deltaTime;
        AtualizarTexto();

        if (tempoRestante <= 0)
        {
            StartCoroutine(GameOver());
        }
    }

    void ReiniciarTimer()
    {
        tempoRestante = tempoMaximo;
        AtualizarTexto();
    }

    void AtualizarTexto()
    {
        if (textoTimer == null) return;

        int tempoInt = Mathf.CeilToInt(tempoRestante);
        int minutos = tempoInt / 60;
        int segundos = tempoInt % 60;

        // Exibe no formato 00:10, 00:09, etc.
        textoTimer.text = string.Format("{0:00} : {1:00}", minutos, segundos);

        textoTimer.color = tempoInt <= 5 ? corAlerta : corNormal;
    }

    IEnumerator GameOver()
    {
        gameOverAtivo = true;

        if (fadeCanvas != null)
        {
            float tempo = 0f;
            while (tempo < duracaoFade)
            {
                tempo += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(0f, 1f, tempo / duracaoFade);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("GameOverFinal");
    }

    private void OnDestroy()
    {
        if (vilao != null)
            vilao.OnDispararProximoWaypoint -= ReiniciarTimer;
    }
}
