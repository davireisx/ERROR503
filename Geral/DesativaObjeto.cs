using UnityEngine;

public class DesativaObjeto : MonoBehaviour
{
    public GameObject objeto;
    public GameObject objeto2;
    public GameObject setas;
    void Start()
    {
        
    }

    public void Desativa()
    {
        objeto.SetActive(false);
        objeto2.SetActive(true);
        setas.SetActive(false);
    }

}
