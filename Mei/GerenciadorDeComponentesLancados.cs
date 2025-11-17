using UnityEngine;
using System.Collections.Generic;

public class GerenciadorDeComponentesLancados : MonoBehaviour
{
    public ComponenteLancado[] componentes;
    public GameObject joystick; 
    public GameObject hud; // atribua via Inspector

    private List<ComponenteLancado> chegaram = new List<ComponenteLancado>();

    public void NotificarChegada(ComponenteLancado componente)
    {
        if (!chegaram.Contains(componente))
            chegaram.Add(componente);

        if (chegaram.Count == componentes.Length)
        {
            Debug.Log("? TODOS os componentes chegaram. Liberando joystick.");

            if (joystick != null)
                joystick.SetActive(true);
        }
    }

   
}
