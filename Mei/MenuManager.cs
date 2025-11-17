using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public string nomeCena1;
    public string nomeCena2;

    public Button cena1;
    public Button sair;

    private void Start()
    {
        // Vincula os botões aos métodos
        cena1.onClick.AddListener(OnBotao1Click);
        sair.onClick.AddListener(OnBotao3Click);
    }

    public void OnBotao1Click()
    {
        SceneManager.LoadScene(nomeCena1);
    }
    public void OnBotao3Click()
    {
        Application.Quit(); // Encerra o jogo (funciona apenas em builds)
        Debug.Log("Sair do jogo."); // Para testes no editor
    }
}
