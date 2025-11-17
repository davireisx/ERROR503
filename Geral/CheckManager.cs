using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckManager : MonoBehaviour
{
    [Header("Objetos que precisam estar ativos")]
    public List<GameObject> objetosParaChecar = new List<GameObject>();

    [Header("Referências")]
    public GameObject checkFinal;
    public GameObject fundo2;
    public GameObject fundo3;
    public NPCInteracao npc;
    public GameObject npcPisca;

    private bool sequenciaIniciada = false;

    void Update()
    {
        if (!sequenciaIniciada && TodosAtivos())
        {
            sequenciaIniciada = true;
            StartCoroutine(SequenciaFinal());
        }
    }

    private bool TodosAtivos()
    {
        foreach (GameObject obj in objetosParaChecar)
        {
            if (obj == null || !obj.activeSelf)
                return false;
        }
        return true;
    }

    IEnumerator SequenciaFinal()
    {
        // Ativa o check final
        if (checkFinal != null) checkFinal.SetActive(true);

        // Espera 2 segundos
        yield return new WaitForSeconds(3);

        // Troca fundos
        if (fundo2 != null) fundo2.SetActive(false);
        if (fundo3 != null) fundo3.SetActive(true);

        // Habilita NPC piscando
        if (npcPisca != null) npcPisca.SetActive(true);

        // Escuta evento de término do diálogo
        if (npc != null)
        {
            npc.OnDialogoTerminado += DialogoFinalizado;
            Debug.Log("CheckManager: Esperando NPC terminar o diálogo...");
        }
        else
        {
            Debug.LogWarning("NPCInteracao não atribuído no CheckManager.");
        }
    }

    private void DialogoFinalizado()
    {
        Debug.Log("CheckManager: Diálogo terminou.");
        if (npc != null)
            npc.OnDialogoTerminado -= DialogoFinalizado;

        // ?? O FadeManager vai cuidar do fade quando o objeto for ativado
    }
}
