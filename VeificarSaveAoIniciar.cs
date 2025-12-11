using UnityEngine;
using UnityEngine.SceneManagement;

public class VerificarSaveAoIniciar : MonoBehaviour
{
    void Start()
    {
        string ultimaCena = SaveSystem.CarregarCenaSalva();

        if (!string.IsNullOrEmpty(ultimaCena))
        {
            // Já existe progresso, vai para a tela de "Continuar"
            SceneManager.LoadScene("Save");
        }
        else
        {
            // Se não tiver progresso, segue o fluxo normal (menu ou intro)
            // Se quiser pular direto pra Fase1, basta colocar:
            // SceneManager.LoadScene("Fase1");
        }
    }
}
