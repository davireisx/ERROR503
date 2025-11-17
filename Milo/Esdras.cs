using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class Esdras : MonoBehaviour
{
    [Header("Movimentação do Jogador")]
    [SerializeField] private float moveSpeed; // Velocidade de movimento do jogador
    private Vector2 moveInput; // Input de movimento do jogador

    [Header("Joystick")]
    [SerializeField] private Joystick joystick; // Referência ao joystick no Canvas

    [Header("Componentes do Jogador")]
    [SerializeField] private Animator anim; // Componente Animator para animações
    [SerializeField] private SpriteRenderer spriteRenderer; // Componente SpriteRenderer para o sprite

    private bool isFacingLeft = false; // Controle para indicar se o jogador está virado para a esquerda
    private bool inputLocked = false;
    private float inputLockTimer = 0f;

    private void Start()
    {
        // Inicializa os componentes necessários
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (joystick == null)
        {
            Debug.LogError("Joystick não está configurado! Arraste o joystick para o campo no Inspector.");
        }
    }

    private void Update()
    {
        HandleMovement(); // Gerencia o movimento do jogador
    }

    public void LockInput(float duration)
    {
        inputLocked = true;
        inputLockTimer = duration;
    }


    public void HandleMovement()
    {
        if (inputLocked)
        {
            inputLockTimer -= Time.deltaTime;
            if (inputLockTimer <= 0f)
            {
                inputLocked = false;
            }
            anim.SetBool("walking", false);
            return;
        }

        if (joystick != null)
        {
            // Obtém o input do joystick
            moveInput.x = joystick.Horizontal;
            moveInput.y = joystick.Vertical;

            // Move o jogador com base na velocidade e no input
            transform.Translate(moveInput * Time.deltaTime * moveSpeed);

            // Atualiza o parâmetro de animação para indicar movimento
            anim.SetBool("walking", Mathf.Abs(moveInput.x) > 0 || Mathf.Abs(moveInput.y) > 0);

            // Atualiza o lado para o qual o jogador está virado
            FlipCharacter();
        }
        else
        {
            Debug.LogWarning("Joystick não configurado. O jogador não irá se mover.");
        }
    }


    private void FlipCharacter()
    {
        if (moveInput.x < 0)
        {
            // Virar para a esquerda
            spriteRenderer.flipX = true;
            isFacingLeft = true;
        }
        else if (moveInput.x > 0)
        {
            // Virar para a direita
            spriteRenderer.flipX = false;
            isFacingLeft = false;
        }
    }
    public bool IsFacingLeft()
    {
        // Retorna true se o jogador estiver virado para a esquerda
        return isFacingLeft;
    }

    public void SetInput(Vector2 input)
    {
        moveInput = input;
    }


    public Vector2 GetCurrentInput()
    {
        return moveInput;
    }


    public bool IsMoving()
    {
        return Mathf.Abs(moveInput.x) > 0.1f || Mathf.Abs(moveInput.y) > 0.1f;
    }


}