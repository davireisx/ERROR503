using UnityEngine;
using UnityEngine.UI;

public class Botao1 : MonoBehaviour
{
    [Header("Configuração do Botão")]
    public Button interactButton;       // Botão de interação
    public float detectionRadius;       // Raio de detecção da área
    public Transform player;            // Referência ao jogador (Player)
    private bool playerInRange = false; // Flag para detectar se o jogador está na área

    [Header("Configuração do Jogo")]
    public GameObject weapon;           // Referência ao GameObject da arma
    public DesafioGerador1 desafioGerador; // Referência ao script do desafio
    public SpawnElias spawnManager;     // Referência ao SpawnManager
    public float delayMovimentoInimigos = 2f; // Tempo de atraso para o movimento dos inimigos

    void Start()
    {
        // Inicializa o estado do botão e da arma
        interactButton.gameObject.SetActive(false);
        if (weapon != null)
        {
            weapon.SetActive(false); // Garante que a arma esteja desativada no início
        }
    }

    void Update()
    {
        // Verifica a distância entre o jogador e o botão
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        if (distanceToPlayer <= detectionRadius && !playerInRange)
        {
            playerInRange = true;
            interactButton.gameObject.SetActive(true); // Exibe o botão
            Debug.Log("BOTÃO 1 APARECENDO.");
        }
        else if (distanceToPlayer > detectionRadius && playerInRange)
        {
            playerInRange = false;
            interactButton.gameObject.SetActive(false); // Oculta o botão
            Debug.Log("BOTÃO 1 NÃO APARECENDO.");
        }
    }

    public void OnButtonClick1()
    {
        Debug.Log("Primeiro botão pressionado! Ação iniciada.");

        // Ativa o spawn de inimigos
        spawnManager.AtivarSpawn();

        // Define o destino dos inimigos para o Gerador 1
        spawnManager.SetDestinationToGerador1();

        // Inicia o desafio do gerador
        desafioGerador.IniciarDesafio();

        // Ativa a arma, caso exista
        if (weapon != null)
        {
            weapon.SetActive(true);
            Debug.Log("Arma ativada!");
        }

        // Desativa permanentemente o botão e seu GameObject
        interactButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        // Desenha o raio de detecção no editor para visualização
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
