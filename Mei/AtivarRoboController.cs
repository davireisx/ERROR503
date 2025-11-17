using UnityEngine;
using System.Collections;

public class AtivarRoboController : MonoBehaviour
{
    [Header("Referências Principais")]
    public GameObject roboObject;     // Objeto que contém o script RoboController
    public GameObject junkinhoObject; // Objeto que contém o script Junkinho
    public GameObject HUD;
    public GameObject HUDJUNKO;
    public GameObject objetivo1;
    public GameObject objetivo2;
    public GameObject objectTrigger;

    public RoboController robo;
    public Junkinho junkinho;

    void Start()
    {
        // Verifica se os objetos foram atribuídos
        if (roboObject != null)
        {
            robo = roboObject.GetComponent<RoboController>();
        }

        if (junkinhoObject != null)
        {
            junkinho = junkinhoObject.GetComponent<Junkinho>();
        }

        // Se o script RoboController foi encontrado
        if (robo != null)
        {
            objetivo1.SetActive(false);
            objetivo2.SetActive(true);
            HUDJUNKO.SetActive(true);
            HUD.SetActive(false);
            objectTrigger.SetActive(true);

            robo.enabled = true; // Ativa o script RoboController
            StartCoroutine(Esperar()); // ? CORREÇÃO: parênteses necessários

            Debug.Log("RoboController foi ativado com sucesso!");
        }
        else
        {
            Debug.LogWarning("O objeto atribuído não possui o script RoboController!");
        }
    }

    IEnumerator Esperar()
    {
        yield return new WaitForSeconds(3f);

        if (junkinho != null)
        {
            junkinho.enabled = true;
            Debug.Log("Junkinho foi ativado após 1.5 segundos!");
        }
        else
        {
            Debug.LogWarning("O objeto atribuído não possui o script Junkinho!");
        }
    }
}
