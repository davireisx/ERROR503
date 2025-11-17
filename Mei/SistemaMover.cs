using UnityEngine;

public class SistemaMover : MonoBehaviour
{
    public MoveSystem[] pieces; // Todas as peças do jogo

    public void CheckAllPieces()
    {
        Debug.Log("?? Verificando todas as peças..."); // Mensagem inicial para ver se o GameManager foi chamado

        bool allCorrect = true;

        foreach (MoveSystem piece in pieces)
        {
            if (!piece.IsCorrect()) // Se alguma peça estiver errada...
            {
                allCorrect = false;
                Debug.Log($"? Peça {piece.gameObject.name} está errada!");
                break;
            }
        }

        if (allCorrect)
        {
            Debug.Log("? Todas as peças foram colocadas corretamente! Parabéns! ??");
        }
        else
        {
            Debug.Log("? Algumas peças estão no lugar errado! Reiniciando...");
            ResetAllPieces();
        }
    }

    private void ResetAllPieces()
    {
        Debug.Log("?? Resetando todas as peças...");

        foreach (MoveSystem piece in pieces)
        {
            piece.ResetPosition();
        }
    }
}
