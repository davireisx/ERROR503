using UnityEngine;

public class AtivarMovRobo : MonoBehaviour
{
    public CompletarFiacaoInversa ativaRobo;
    public GameObject HUDJUNKO;
    void Start()
    {
        ativaRobo.DepoisDialogo();
        HUDJUNKO.SetActive(true);
    }


}
