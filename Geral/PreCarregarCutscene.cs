using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class PreCarregarCutscene : MonoBehaviour
{
    [Header("Nome do vídeo dentro da pasta StreamingAssets")]
    public string videoFileName = "cutscene.mp4";

    [Header("Mostrar logs no console (debug opcional)")]
    public bool mostrarLogs = true;

    private string origem;
    private string destino;

    void Start()
    {
        StartCoroutine(PreCarregarVideo());
    }

    public IEnumerator PreCarregarVideo()
    {
        origem = Path.Combine(Application.streamingAssetsPath, videoFileName);
        destino = Path.Combine(Application.persistentDataPath, videoFileName);

        if (File.Exists(destino))
        {
            if (mostrarLogs)
                Debug.Log($"? Vídeo '{videoFileName}' já está carregado em cache.");
            yield break;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (mostrarLogs)
            Debug.Log($"?? Copiando '{videoFileName}' para cache...");

        using (UnityWebRequest www = UnityWebRequest.Get(origem))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(destino, www.downloadHandler.data);
                if (mostrarLogs)
                    Debug.Log($"? Vídeo '{videoFileName}' copiado com sucesso para: {destino}");
            }
            else
            {
                Debug.LogWarning($"?? Falha ao copiar '{videoFileName}': {www.error}");
            }
        }
#else
        File.Copy(origem, destino, true);
        if (mostrarLogs)
            Debug.Log($"? Vídeo '{videoFileName}' copiado (modo Editor/PC).");
#endif
    }
}
