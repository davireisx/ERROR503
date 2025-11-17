using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Adicione esta linha para usar componentes de UI

public class Personagem : MonoBehaviour
{
    // Referências no cenário
    [Header("Referências de Cenário")]
    [SerializeField] private GameObject trapcenario; // Objeto da armadilha
    [SerializeField] private Transform trap; // Ponto de ativação para o personagem

    // Configurações de movimento
    [Header("Configurações de Movimento")]
    [SerializeField] private float speed; // Velocidade de movimento
    [SerializeField] private float triggerDistance = 1.0f; // Distância necessária para ativar o ponto de interação

    // Referência ao joystick da UI
    [Header("Referência do Joystick")]
    [SerializeField] private Joystick joystick; // Arraste o joystick da UI para cá no Inspector

    // Threshold para considerar o joystick parado
    [SerializeField] private float joystickThreshold = 0.1f; // Valor mínimo para considerar movimento

    // Estado de diálogo
    private bool isInDialogue = false; // Controla se o personagem está em um diálogo
    private bool hasTriggeredDialogue = false; // Verifica se o diálogo foi acionado

    // Componentes internos
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // Direção de movimento
    private Vector2 lastDirection = Vector2.zero; // Última direção de movimento

    // Música
    [Header("Configurações de Áudio")]
    [SerializeField] private AudioSource audioSource; // Música de fundo

    void Start()
    {
        // Inicializa os componentes
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Inicializa o componente de áudio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // Faz a música repetir
        audioSource.Play(); // Inicia a música
    }

    void Update()
    {
        // Se o personagem estiver em um diálogo, ele não pode se mover
        if (isInDialogue)
        {
            rb.linearVelocity = Vector2.zero; // Para o movimento do personagem
            return; // Bloqueia o restante do código
        }

        // Verifica se o personagem atingiu o ponto de ativação para ativar a armadilha
        if (!hasTriggeredDialogue && Vector2.Distance(transform.position, trap.position) <= triggerDistance)
        {
            hasTriggeredDialogue = true;
        }

        // Verifica a interação com o inimigo e o estado do diálogo
        CheckEnemyDialogueState();

        // Captura o movimento do joystick da UI
        Vector2 direction = new Vector2(joystick.Horizontal, joystick.Vertical);

        // Se a magnitude do vetor for menor que o threshold, considera parado
        if (direction.magnitude < joystickThreshold)
        {
            direction = Vector2.zero; // Define a direção como zero (parado)
        }

        lastDirection = direction; // Atualiza a última direção do movimento

        // Aplica a velocidade ao movimento do personagem
        rb.linearVelocity = direction.normalized * speed;

        // Gerencia as animações com base na direção do movimento
        UpdateMovementAnimation(direction);

        // Verifica a distância entre o personagem e o trap, e ativa a armadilha se estiver dentro do limite
        if (Vector2.Distance(transform.position, trap.position) <= triggerDistance)
        {
            ActivateTrap();
        }
    }

    // Reseta as camadas de animação (usado para alternar animações)
    private void ResetLayers()
    {
        anim.SetLayerWeight(0, 0); // Camada de animação para baixo (front)
        anim.SetLayerWeight(1, 0); // Camada de animação para cima (back)
        anim.SetLayerWeight(2, 0); // Camada de animação lateral (side)
    }

    // Atualiza a animação de movimento com base na direção
    private void UpdateMovementAnimation(Vector2 direction)
    {
        if (direction.magnitude < joystickThreshold) // Se estiver parado
        {
            ResetLayers();
            anim.SetBool("walking", false); // Define a animação como idle
            return;
        }

        // Verifica se o movimento vertical é dominante
        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            ResetLayers();
            if (direction.y > 0)
            {
                anim.SetLayerWeight(1, 1); // Ativa animação de movimento para cima (back)
            }
            else
            {
                anim.SetLayerWeight(0, 1); // Ativa animação de movimento para baixo (front)
            }
        }
        else // Movimento horizontal é dominante
        {
            ResetLayers();
            anim.SetLayerWeight(2, 1); // Ativa animação de movimento lateral (side)
            spriteRenderer.flipX = direction.x < 0; // Inverte o sprite conforme a direção
        }

        anim.SetBool("walking", true); // Define a animação como "walking"
    }

    // Verifica a interação com o inimigo e atualiza o estado do diálogo
    private void CheckEnemyDialogueState()
    {
        EnemyMovement enemy = Object.FindFirstObjectByType<EnemyMovement>();
        if (enemy != null && enemy.IsAtLastWaypoint() && Vector2.Distance(transform.position, enemy.transform.position) <= enemy.interactionRange)
        {
            Dialogue dialogue = Object.FindFirstObjectByType<Dialogue>();
            if (dialogue != null)
            {
                dialogue.isSecondDialogueAvailable = true;
            }
        }
        else
        {
            Dialogue dialogue = Object.FindFirstObjectByType<Dialogue>();
            if (dialogue != null)
            {
                dialogue.isSecondDialogueAvailable = false;
            }
        }
    }

    // Ativa a armadilha quando o personagem chega no ponto de ativação
    private void ActivateTrap()
    {
        trapcenario.SetActive(true); // Ativa a armadilha no cenário
        Debug.Log("Trap ativado!"); // Mensagem de confirmação no console

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // Define o estado de idle do personagem (parado olhando para frente)
    public void SetIdleFront()
    {
        ResetLayers();
        anim.SetLayerWeight(0, 1); // Ativa animação de idle para baixo (frente)
        anim.SetBool("walking", false); // Garante que a animação de movimento pare
    }

    // Função para bloquear ou liberar o movimento dependendo do estado de diálogo
    public void SetDialogueState(bool isInDialogue)
    {
        this.isInDialogue = isInDialogue;

        if (isInDialogue)
        {
            rb.linearVelocity = Vector2.zero; // Para completamente o movimento
            SetIdleFront(); // Garante que a animação seja idle
        }
    }
}