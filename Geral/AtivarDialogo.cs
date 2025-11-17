using UnityEngine;

public class AtivarDialogoNoWaypoint : MonoBehaviour
{
    [Header("Referências")]
    public Transform personagem;
    public Transform waypoint;
    public GameObject dialogo;

    [Header("Configurações")]
    public float distanciaAtivacao = 1.0f; // distância para considerar que chegou

    private bool dialogoAtivado = false;

    void Update()
    {
        if (!dialogoAtivado)
        {
            float distancia = Vector2.Distance(personagem.position, waypoint.position);

            if (distancia <= distanciaAtivacao)
            {
                dialogo.SetActive(true);
                dialogoAtivado = true;
            }
        }
    }
}
