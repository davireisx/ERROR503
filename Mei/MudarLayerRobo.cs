using UnityEngine;
using System.Collections;

public class MudarLayerRobo : MonoBehaviour
{
    [Header("Referências")]
    public SpriteRenderer roboRenderer;
    public GameObject robo;               // Objeto do robô
    public Transform waypoint;            // Ponto de referência

    [Header("Configurações")]
    public float tempoMudanca = 3f;        // Duração da mudança
    public float distanciaDeteccao = 0.1f; // Distância mínima para considerar que chegou

    
   
    private bool alterando = false;

    void Start()
    {
        if (robo != null)
        {
            roboRenderer = robo.GetComponent<SpriteRenderer>();
            
        }
    }

    void Update()
    {
        if (robo == null || waypoint == null || roboRenderer == null)
            return;

        float distancia = Vector2.Distance(robo.transform.position, waypoint.position);

        if (distancia <= distanciaDeteccao && !alterando)
        {
            StartCoroutine(MudarSortingTemporariamente());
        }
    }

    IEnumerator MudarSortingTemporariamente()
    {
        alterando = true;

        // Salva estado original
        string sortingOriginal = roboRenderer.sortingLayerName;
        int orderOriginal = roboRenderer.sortingOrder;

        // Desativa o LayerSistema
  

        // Aplica a nova layer e ordem
        roboRenderer.sortingLayerName = "Objetos";
        roboRenderer.sortingOrder = 4;

        // Espera o tempo configurado
        yield return new WaitForSeconds(tempoMudanca);

        // Restaura o estado original
        roboRenderer.sortingLayerName = sortingOriginal;
        roboRenderer.sortingOrder = orderOriginal;

        // Reativa o LayerSistema

        alterando = false;
    }
}
