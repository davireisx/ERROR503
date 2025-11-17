using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevaNocaute : MonoBehaviour
{
    [Header("Objetos a serem monitorados")]
    public List<GameObject> objetosParaMonitorar;

    [Header("Referência ao Robo")]
    public RoboController robo; // referência ao seu script do robô

    [Header("UI")]
    public GameObject check;
    public GameObject objetivos1;
    public GameObject objetivos2;
    private bool todosDestruidos = false; // garante que só execute uma vez

    void Update()
    {
        if (todosDestruidos || objetosParaMonitorar == null || objetosParaMonitorar.Count == 0)
            return;

        // Remove todos os objetos destruídos da lista
        objetosParaMonitorar.RemoveAll(obj => obj == null);

        // Se a lista estiver vazia, todos foram destruídos
        if (objetosParaMonitorar.Count == 0)
        {
            todosDestruidos = true;

            if (robo != null)
                robo.LevarNocaute();

            StartCoroutine(ChamarUI());

            Debug.Log("Todos os objetos foram destruídos! Ação executada.");
        }
    }

    IEnumerator ChamarUI()
    {
        check.SetActive(true);
        yield return new WaitForSeconds(1f);
        objetivos1.SetActive(false);
        objetivos2.SetActive(true);
        check.SetActive(false);
    }
}
