using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerFinal : MonoBehaviour
{
    [Header("Personagens")]
    public GameObject personagem1;
    public GameObject personagem2;
    public GameObject personagem3;

    public Animator anim1;
    public Animator anim2;
    public Animator anim3;

    [Header("Waypoints")]
    public Transform[] waypoints1;
    public Transform[] waypoints2;
    public Transform[] waypoints3;

    [Header("Configurações")]
    public float velocidade = 3f; // Velocidade de movimento

    [Header("Objetos Ativados no Final")]
    public GameObject joystick;
    public GameObject arcade; // onde está o script NPCInteracao

    private int personagensFinalizados = 0;

    void Start()
    {
        // Inicializa animators
        anim1 = personagem1.GetComponent<Animator>();
        anim2 = personagem2.GetComponent<Animator>();
        anim3 = personagem3.GetComponent<Animator>();

        // Garante que joystick e arcade começam desativados
        if (joystick != null) joystick.SetActive(false);
        if (arcade != null) arcade.SetActive(false);

        // Posiciona no primeiro waypoint
        if (waypoints1.Length > 0) personagem1.transform.position = waypoints1[0].position;
        if (waypoints2.Length > 0) personagem2.transform.position = waypoints2[0].position;
        if (waypoints3.Length > 0) personagem3.transform.position = waypoints3[0].position;

        // Inicia movimentação com animação
        StartCoroutine(MoverPersonagem(personagem1, anim1, waypoints1));
        StartCoroutine(MoverPersonagem(personagem2, anim2, waypoints2));
        StartCoroutine(MoverPersonagem(personagem3, anim3, waypoints3));
    }

    private IEnumerator MoverPersonagem(GameObject personagem, Animator animator, Transform[] waypoints)
    {
        if (waypoints.Length == 0) yield break;

        animator.SetBool("walking", true);

        int index = 0;
        while (index < waypoints.Length)
        {
            Vector3 destino = waypoints[index].position;
            while (Vector3.Distance(personagem.transform.position, destino) > 0.05f)
            {
                personagem.transform.position = Vector3.MoveTowards(
                    personagem.transform.position,
                    destino,
                    velocidade * Time.deltaTime
                );
                yield return null;
            }
            personagem.transform.position = destino; // garante que chegou exatamente
            index++;
        }

        animator.SetBool("walking", false);

        // Marca personagem como finalizado
        personagensFinalizados++;

        // Quando todos terminarem -> ativa joystick e arcade
        if (personagensFinalizados >= 3)
        {
            if (joystick != null) joystick.SetActive(true);
            if (arcade != null) arcade.SetActive(true);
        }
    }
}
