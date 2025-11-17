using System.Collections.Generic;
using UnityEngine;

public class VagaoManager : MonoBehaviour
{
    [Header("Lista de Vagões Corrompidos")]
    public List<GameObject> vagoes;

    [Header("Objetivos")]
    public GameObject objetivo1;
    public GameObject objetivo2;

    [Range(0f, 0.05f)]
    public float toleranciaCor = 0.01f;

    [HideInInspector] public HashSet<GameObject> vagoesPretosFixos = new HashSet<GameObject>();

    private bool todosBrancosDetectados = false;
    private int indiceVagaoAtivo = -1;

    void Start()
    {

        if (objetivo1 != null) objetivo1.SetActive(true);
        if (objetivo2 != null) objetivo2.SetActive(false);
    }

    void Update()
    {
        if (!todosBrancosDetectados && TodosVagoesBrancos())
        {
            todosBrancosDetectados = true;
            AtualizarObjetivos();
            AtivarProximoVagao(); // primeiro vagão começa a piscar
        }
    }

    bool TodosVagoesBrancos()
    {
        foreach (GameObject vagao in vagoes)
        {
            if (vagao == null) continue;
            if (!VagaoEstaBranco(vagao)) return false;
        }
        return true;
    }

    bool VagaoEstaBranco(GameObject vagao)
    {
        SpriteRenderer sr = vagao.GetComponent<SpriteRenderer>();
        return sr != null && CorEhBranca(sr.color);
    }

    bool CorEhBranca(Color cor)
    {
        return Mathf.Abs(cor.r - 1f) <= toleranciaCor &&
               Mathf.Abs(cor.g - 1f) <= toleranciaCor &&
               Mathf.Abs(cor.b - 1f) <= toleranciaCor;
    }

    void AtualizarObjetivos()
    {
        if (objetivo1 != null) objetivo1.SetActive(false);
        if (objetivo2 != null) objetivo2.SetActive(true);
    }

    public void VagaoConcluido(GameObject vagao)
    {
        // Ativar todos os vagoes pretos que foram corrompidos corretamente
        AtivarVagoesPretosCorrompidos();
        AtivarProximoVagao();
    }

    public void RegistrarVagaoPretoFixo(GameObject vagaoPreto)
    {
        if (!vagoesPretosFixos.Contains(vagaoPreto))
        {
            vagoesPretosFixos.Add(vagaoPreto);
            // Ativar imediatamente quando for registrado
            if (vagaoPreto != null)
            {
                vagaoPreto.SetActive(true);
                SpriteRenderer sr = vagaoPreto.GetComponent<SpriteRenderer>();
                if (sr) sr.color = Color.white;
            }
        }
    }

    private void AtivarProximoVagao()
    {
        indiceVagaoAtivo++;
        if (indiceVagaoAtivo >= vagoes.Count) return;

        GameObject vagaoDaVez = vagoes[indiceVagaoAtivo];
        if (vagaoDaVez == null) return;

        VagaoSistema script = vagaoDaVez.GetComponent<VagaoSistema>();
        if (script != null)
        {
            script.enabled = true;
            script.SetInterativo(true);       // apenas o vagão da vez é interativo
            script.AtivarPiscarInicio();      // apenas ele pisca
        }
    }

    // Método para verificar se um vagão preto já foi corrompido corretamente
    public bool VagaoPretoJaCorrompido(GameObject vagaoPreto)
    {
        return vagoesPretosFixos.Contains(vagaoPreto);
    }

    // Método para ativar todos os vagoes pretos que foram corrompidos
    private void AtivarVagoesPretosCorrompidos()
    {
        foreach (GameObject vagaoPreto in vagoesPretosFixos)
        {
            if (vagaoPreto != null)
            {
                vagaoPreto.SetActive(true);
                SpriteRenderer sr = vagaoPreto.GetComponent<SpriteRenderer>();
                if (sr) sr.color = Color.white;
            }
        }
    }
}