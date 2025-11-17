using System.Collections;
using UnityEngine;

public class CameraEventoManager : MonoBehaviour
{
    [Header("Referências")]
    public CameraManagerEsdras cameraManager;   // Referência ao script da câmera
    public GameObject check;                    // Objeto que será ativado
    public GameObject hud;                      // HUD que será desativado
    public GameObject dialogo;                  // Diálogo que será ativado
    public GameObject joystick;                  // Diálogo que será ativado

    [Header("Tempos de Evento")]
    public float tempoParaAtivarCheck = 1f;     // Tempo antes de ativar o check
    public float duracaoCheckAtivo = 1.5f;      // Quanto tempo o check fica ativo
    public float delayAtivarDialogo = 1f;       // Tempo entre desativar HUD e ativar diálogo

    private bool sequenciaIniciada = false;
    private int ultimoCenario = -1;

    void Update()
    {
        if (cameraManager == null || cameraManager.cameraScript == null)
            return;

        // Aqui descobrimos qual cenário está ativo com base nos limites atuais da câmera
        int cenarioAtual = DetectarCenarioAtual();

        // Quando o cenário for o 4 e ainda não iniciou a sequência
        if (!sequenciaIniciada && cenarioAtual == 4)
        {
            sequenciaIniciada = true;
            StartCoroutine(SequenciaEventos());
        }

        ultimoCenario = cenarioAtual;
    }

    private int DetectarCenarioAtual()
    {
        // Detecta o cenário ativo comparando os limites atuais da câmera com os armazenados no CameraManagerEsdras
        var cam = cameraManager.cameraScript;

        if (Mathf.Approximately(cam.globalMinX, cameraManager.minX1 + cameraManager.offsetX) &&
            Mathf.Approximately(cam.globalMaxX, cameraManager.maxX1 + cameraManager.offsetX))
            return 1;

        if (Mathf.Approximately(cam.globalMinX, cameraManager.minX2 + cameraManager.offsetX) &&
            Mathf.Approximately(cam.globalMaxX, cameraManager.maxX2 + cameraManager.offsetX))
            return 2;

        if (Mathf.Approximately(cam.globalMinX, cameraManager.minX3 + cameraManager.offsetX) &&
            Mathf.Approximately(cam.globalMaxX, cameraManager.maxX3 + cameraManager.offsetX))
            return 3;

        if (Mathf.Approximately(cam.globalMinX, cameraManager.minX4 + cameraManager.offsetX) &&
            Mathf.Approximately(cam.globalMaxX, cameraManager.maxX4 + cameraManager.offsetX))
            return 4;

        if (Mathf.Approximately(cam.globalMinX, cameraManager.minX5 + cameraManager.offsetX) &&
            Mathf.Approximately(cam.globalMaxX, cameraManager.maxX5 + cameraManager.offsetX))
            return 5;

        return -1;
    }

    private IEnumerator SequenciaEventos()
    {
        // Espera o tempo configurado e ativa o check
        joystick.SetActive(false);

        yield return new WaitForSeconds(tempoParaAtivarCheck);
        check.SetActive(true);
        

        // Mantém o check ativo
        yield return new WaitForSeconds(duracaoCheckAtivo);
        check.SetActive(false);
        

        // Desativa o HUD
        if (hud != null)
            hud.SetActive(false);


        // Espera e ativa o diálogo
        yield return new WaitForSeconds(delayAtivarDialogo);
        if (dialogo != null)
            dialogo.SetActive(true);
    }
}
