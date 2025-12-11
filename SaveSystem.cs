using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    private static string saveKey = "ultimaCena";

    public static void SalvarCenaAtual()
    {
        string cenaAtual = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(saveKey, cenaAtual);
        PlayerPrefs.Save();
        Debug.Log("Cena salva: " + cenaAtual);
    }

    public static string CarregarCenaSalva()
    {
        if (PlayerPrefs.HasKey(saveKey))
            return PlayerPrefs.GetString(saveKey);
        else
            return "";
    }

    public static void ResetarProgresso()
    {
        PlayerPrefs.DeleteKey(saveKey);
    }
}
