using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiacaoRoboInvertido : MonoBehaviour
{
    [System.Serializable]
    public class FioAnimado
    {
        public FioAlmoxarifado fio;
        public Transform destinoCorreto;
    }

    [Header("Lista de fios e seus destinos")]
    public List<FioAnimado> fiosSequenciais = new List<FioAnimado>();

    [Header("Configuração de animação")]
    public float tempoEntreFios = 0.5f;
    public float duracaoAnimacao = 0.5f;

    [Header("Referência da luz")]
    public GameObject lightObject;

    private void Start()
    {
        if (lightObject != null)
            lightObject.SetActive(true);

        foreach (var fioData in fiosSequenciais)
        {
            if (fioData.fio != null && fioData.destinoCorreto != null)
            {
                fioData.fio.ConectarAutomaticamente(fioData.destinoCorreto);
            }
        }

        StartCoroutine(AnimarFiosSequencialmente());
    }

    private IEnumerator AnimarFiosSequencialmente()
    {
        foreach (var fioData in fiosSequenciais)
        {
            yield return StartCoroutine(EsticarFio(fioData.fio, fioData.destinoCorreto));
            yield return new WaitForSeconds(tempoEntreFios);
        }
    }

    private IEnumerator EsticarFio(FioAlmoxarifado fio, Transform destino)
    {
        if (fio == null || destino == null || fio.pontoFixo == null ||
            fio.parteVisual == null || fio.holderVisual == null) yield break;

        var (pontoFixo, holderVisual, parteVisual, pontaFinal) =
            (fio.pontoFixo, fio.holderVisual, fio.parteVisual, fio.pontaFinal);

        Vector3 direcaoGlobal = destino.position - pontoFixo.position;
        float anguloZ = Mathf.Atan2(direcaoGlobal.y, direcaoGlobal.x) * Mathf.Rad2Deg;
        holderVisual.rotation = Quaternion.Euler(0f, 0f, anguloZ);

        float distanciaInicial = parteVisual.size.x;
        float distanciaFinal = Vector3.Distance(pontoFixo.position, destino.position);
        float larguraVisual = parteVisual.size.y;
        float tempo = 0f;

        while (tempo < duracaoAnimacao)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracaoAnimacao);

            float comprimentoMundo = Mathf.Lerp(distanciaInicial, distanciaFinal, progresso);
            Vector3 dirMundo = (destino.position - pontoFixo.position).normalized;
            Vector3 posLocal = holderVisual.InverseTransformPoint(pontoFixo.position + dirMundo * comprimentoMundo);
            float comprimentoLocal = Mathf.Max(posLocal.x, 0.1f);

            parteVisual.size = new Vector2(comprimentoLocal, larguraVisual);
            if (pontaFinal != null)
                pontaFinal.localPosition = new Vector3(comprimentoLocal, 0f, 0f);

            yield return null;
        }

        if (pontaFinal != null)
            pontaFinal.position = destino.position;

        fio.SincronizarVisualComPontaFinal();
        fio.ConectarAutomaticamente(destino);
    }
}
