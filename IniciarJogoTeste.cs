using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class IniciarJogoTeste : MonoBehaviour
{
    [Header("Referências")]
    public VideoPlayer videoPlayer;
    public GameObject videoContainer;
    public CanvasGroup fadeCanvasGroup;

    [Header("Configurações")]
    public string videoFileName = "cutscene.mp4";
    public string nextSceneName = "Scene2";
    public float fadeDuration = 1.5f;
    public float tempoMaximoEspera = 6f; // tempo máximo de espera do prepare

    private bool videoTerminado = false;

    void Start()
    {
        // Começa com a tela preta
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 1f;

        // Reset completo
        videoPlayer.Stop();
        videoPlayer.time = 0;
        videoPlayer.frame = 0;
        videoTerminado = false;

        // Caminho do vídeo
        string videoPath = ObterCaminhoVideo();

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;

        // Eventos
        videoPlayer.loopPointReached += OnVideoFinished;

        // Inicia a preparação com timeout
        StartCoroutine(PrepararCutsceneComTimeout());
    }

    string ObterCaminhoVideo()
    {
        // 1️⃣ Primeiro tenta carregar do cache local (caso tenha sido pré-carregado)
        string caminhoCache = Path.Combine(Application.persistentDataPath, videoFileName);
        if (File.Exists(caminhoCache))
        {
            Debug.Log($"🎬 Usando vídeo pré-carregado: {caminhoCache}");
            return caminhoCache;
        }

        // 2️⃣ Caso contrário, usa o caminho original da plataforma
#if UNITY_ANDROID && !UNITY_EDITOR
        return "jar:file://" + Application.dataPath + "!/assets/" + videoFileName;
#elif UNITY_IOS && !UNITY_EDITOR
        return Path.Combine(Application.streamingAssetsPath, videoFileName);
#else
        return Path.Combine(Application.streamingAssetsPath, videoFileName);
#endif
    }

    IEnumerator PrepararCutsceneComTimeout()
    {
        videoPlayer.Prepare();

        float tempo = 0f;
        while (!videoPlayer.isPrepared && tempo < tempoMaximoEspera)
        {
            tempo += Time.deltaTime;
            yield return null;
        }

        if (videoPlayer.isPrepared)
        {
            StartCoroutine(RestartFromBeginning());
        }
        else
        {
            Debug.LogWarning("⚠️ Cutscene não carregou a tempo — pulando para a próxima cena.");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator RestartFromBeginning()
    {
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        videoPlayer.Pause();
        videoPlayer.time = 0;
        videoPlayer.frame = 0;

        yield return new WaitForEndOfFrame();

        videoPlayer.Play();
        yield return new WaitUntil(() => videoPlayer.frame <= 1);

        StartCoroutine(FadeInFromBlack());
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (!videoTerminado)
        {
            videoTerminado = true;
            StartCoroutine(WaitAudioThenFade());
        }
    }

    IEnumerator FadeInFromBlack()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (t / fadeDuration));
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
    }

    IEnumerator WaitAudioThenFade()
    {
        float vol = 1f;
        while (vol > 0f)
        {
            vol -= Time.deltaTime / 0.5f;
            videoPlayer.SetDirectAudioVolume(0, Mathf.Clamp01(vol));
            yield return null;
        }

        yield return new WaitUntil(() => !videoPlayer.isPlaying);

        if (videoContainer != null)
            videoContainer.SetActive(false);

        StartCoroutine(FadeOutAndStartGame());
    }

    IEnumerator FadeOutAndStartGame()
    {
        yield return new WaitForSeconds(fadeDuration);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
        SceneManager.LoadScene(nextSceneName);
    }
}
