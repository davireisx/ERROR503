using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerTrocaCena : MonoBehaviour
{
    [Header("Referências")]
    public GameObject hud;
    public GameObject joystick;

    [Header("Configuração da Cena")]
    public string nomeCenaDestino; // Nome da cena para carregar

    void OnEnable()
    {
        // Desativa HUD
        if (hud != null) hud.SetActive(false);

        // Desativa Joystick
        if (joystick != null) joystick.SetActive(false);

        // Troca de cena
        if (!string.IsNullOrEmpty(nomeCenaDestino))
        {
            SceneManager.LoadScene(nomeCenaDestino);
        }
        else
        {
            Debug.LogWarning("Nenhuma cena configurada em ManagerTrocaCena!");
        }
    }
}
