using UnityEngine;

public class PlayerCenario : MonoBehaviour
{
    public int cenarioAtual = 1; // Cenário inicial do player

    // Esse método pode ser chamado pelo CameraManager ou por triggers de cenário
    public void SetCenario(int numeroCenario)
    {
        cenarioAtual = numeroCenario;
        Debug.Log("Player agora está no cenário: " + numeroCenario);
    }
}
