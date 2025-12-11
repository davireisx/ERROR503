using UnityEngine;

public class DesativarTutorialAoIniciar : MonoBehaviour
{
    [Header("Arraste aqui o objeto Tutorial")]
    public GameObject tutorial;

    void Start()
    {
        if (tutorial != null)
        {
            tutorial.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Objeto 'tutorial' não foi atribuído no inspector!");
        }
    }
}
