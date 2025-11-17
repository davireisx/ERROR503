using UnityEngine;
using UnityEngine.SceneManagement;

public class BotaoContinuar : MonoBehaviour
{
    [Header("Referências")]
    public GameObject botaoContinuar;   // O botão "Continuar"
    public GameObject objetoAnimado;    // O objeto que vai subir e descer

    [Header("Movimento Vertical")]
    public float amplitude = 10f;       // Distância máxima de movimento
    public float velocidade = 2f;       // Velocidade do movimento

    private Vector3 posicaoInicial;

    void Start()
    {
        if (objetoAnimado != null)
            posicaoInicial = objetoAnimado.transform.localPosition;
    }

    void Update()
    {
        if (objetoAnimado != null)
        {
            float novaY = posicaoInicial.y + Mathf.Sin(Time.time * velocidade) * amplitude;
            objetoAnimado.transform.localPosition = new Vector3(posicaoInicial.x, novaY, posicaoInicial.z);
        }
    }

    public void ContinuarJogo()
    {
        string ultimaCena = SaveSystem.CarregarCenaSalva();

        if (!string.IsNullOrEmpty(ultimaCena))
        {
            SceneManager.LoadScene(ultimaCena);
        }
        else
        {
            SaveSystem.ResetarProgresso();
            SceneManager.LoadScene("Menu");
        }
    }

}
