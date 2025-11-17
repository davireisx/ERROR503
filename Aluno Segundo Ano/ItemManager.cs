using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ItemManager : MonoBehaviour
{
    [Header("Itens da Fase 0")]
    public GameObject[] itens;        // Coloque aqui os itens no Inspector

    [Header("Objetos de Feedback")]
    public GameObject checkGeral;
    public GameObject fundo1;
    public GameObject fundo2;
    public GameObject salaDS;

    private bool finalizado = false;

    void Update()
    {
        if (finalizado) return;

        // Verifica se todos os itens já foram destruídos
        bool todosDestruidos = true;
        foreach (GameObject item in itens)
        {
            if (item != null) // ainda existe na cena
            {
                todosDestruidos = false;
                break;
            }
        }

        if (todosDestruidos)
        {
            StartCoroutine(Finalizar());
        }
    }

    IEnumerator Finalizar()
    {
        finalizado = true;

        if (checkGeral != null) checkGeral.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        if (fundo1 != null) fundo1.SetActive(false);
        if (fundo2 != null) fundo2.SetActive(true);
        if (salaDS != null) salaDS.SetActive(true);

        Debug.Log("[ItemManager] Todos os itens foram coletados!");
    }
}
