using System.Collections;
using UnityEngine;

public class DelayComeco : MonoBehaviour
{
    [Header("Objeto a ser ativado após 3 segundos")]
    public GameObject objetoParaAtivar;

    [Header("Tempo de espera (em segundos)")]
    public float tempoDeEspera = 3f;

    private void Start()
    {
        StartCoroutine(EsperarEAtivar());
    }

    IEnumerator EsperarEAtivar()
    {
        yield return new WaitForSeconds(tempoDeEspera);
        objetoParaAtivar.SetActive(true);
    }
}
