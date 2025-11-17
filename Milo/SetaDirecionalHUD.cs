using UnityEngine;

public class SetaDirecionalHUD : MonoBehaviour
{
    [Header("Controle de cenário")]
    public int indexCenario; // Defina no Inspector

    [Header("Referências")]
    public Transform alvo;
    public Transform jogador;

    [Header("Setas Direcionais")]
    public GameObject setaCima;
    public GameObject setaBaixo;
    public GameObject setaDireita;
    public GameObject setaEsquerda;
    public GameObject setaCimaDireita;
    public GameObject setaCimaEsquerda;
    public GameObject setaBaixoDireita;
    public GameObject setaBaixoEsquerda;

    void Update()
    {
        if (!gameObject.activeSelf) return;
        if (alvo == null || jogador == null)
            return;

        Vector2 direcao = (alvo.position - jogador.position).normalized;

        int x = Mathf.RoundToInt(direcao.x);
        int y = Mathf.RoundToInt(direcao.y);

        // Desativa todas as setas antes de ativar a correta
        DesativarTodasAsSetas();

        if (x == 0 && y > 0)
            setaCima.SetActive(true);
        else if (x > 0 && y > 0)
            setaCimaDireita.SetActive(true);
        else if (x > 0 && y == 0)
            setaDireita.SetActive(true);
        else if (x > 0 && y < 0)
            setaBaixoDireita.SetActive(true);
        else if (x == 0 && y < 0)
            setaBaixo.SetActive(true);
        else if (x < 0 && y < 0)
            setaBaixoEsquerda.SetActive(true);
        else if (x < 0 && y == 0)
            setaEsquerda.SetActive(true);
        else if (x < 0 && y > 0)
            setaCimaEsquerda.SetActive(true);
    }

    private void DesativarTodasAsSetas()
    {
        setaCima.SetActive(false);
        setaBaixo.SetActive(false);
        setaDireita.SetActive(false);
        setaEsquerda.SetActive(false);
        setaCimaDireita.SetActive(false);
        setaCimaEsquerda.SetActive(false);
        setaBaixoDireita.SetActive(false);
        setaBaixoEsquerda.SetActive(false);
    }

    public void VerificarAtivacao(int cenarioAtual)
    {
        gameObject.SetActive(cenarioAtual == indexCenario);
    }
}
