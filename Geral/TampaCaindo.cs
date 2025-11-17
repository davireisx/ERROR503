using UnityEngine;

public class TampaCaindo : MonoBehaviour
{
    [Header("Referências")]
    public Transform waypoint; // O destino final da tampa
    public float velocidadeMovimento = 2f; // Velocidade de descida
    public float velocidadeRotacao = 180f; // Velocidade de rotação no ar
    public float duracaoRotacaoFinal = 1f; // Tempo para completar o giro final de 180°

    private bool ativado = false;
    private bool chegouNoWaypoint = false;
    private float tempoRotacaoFinal = 0f;
    private float rotacaoInicialY;
    private float rotacaoFinalY;

    void OnEnable()
    {
        // Quando o script é ativado, começa o movimento
        ativado = true;
        chegouNoWaypoint = false;
        tempoRotacaoFinal = 0f;
    }

    void Update()
    {
        if (!ativado) return;

        if (!chegouNoWaypoint)
        {
            // Movimento suave até o waypoint
            transform.position = Vector3.MoveTowards(transform.position, waypoint.position, velocidadeMovimento * Time.deltaTime);

            // Rotação contínua no eixo Y durante a queda
            transform.Rotate(Vector3.up * velocidadeRotacao * Time.deltaTime);

            // Verifica se chegou no destino
            float distancia = Vector3.Distance(transform.position, waypoint.position);
            if (distancia <= 0.05f)
            {
                chegouNoWaypoint = true;

                // Define os ângulos iniciais e finais para interpolação suave até 180°
                rotacaoInicialY = transform.eulerAngles.y;
                rotacaoFinalY = Mathf.Round(rotacaoInicialY / 360f) * 360f + 180f; // Garante múltiplos de 180°
            }
        }
        else
        {
            // Transição suave para o ângulo de 180° no eixo Y
            tempoRotacaoFinal += Time.deltaTime / duracaoRotacaoFinal;
            float novoY = Mathf.LerpAngle(rotacaoInicialY, rotacaoFinalY, tempoRotacaoFinal);
            transform.rotation = Quaternion.Euler(0f, novoY, 0f);

            if (tempoRotacaoFinal >= 1f)
            {
                // Finalizou o giro
                ativado = false;
            }
        }
    }
}
