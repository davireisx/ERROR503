using UnityEngine;

public class AudioNoWaypoint : MonoBehaviour
{
    public Transform personagem;       // Referência ao seu personagem
    public Transform waypoint;         // O waypoint que o personagem deve alcançar
    public float distanciaParaAtivar = 1.0f; // Distância mínima para ativar o som
    public AudioSource audioSource;    // AudioSource que será reproduzido

    private bool jaTocou = false;     // Evita tocar várias vezes

    void Update()
    {
        if (!jaTocou && personagem != null && Vector3.Distance(personagem.position, waypoint.position) <= distanciaParaAtivar)
        {
            if (audioSource != null)
            {
                audioSource.Play();
                jaTocou = true;
            }
            else
            {
                Debug.LogWarning("AudioSource não atribuído!");
            }
        }
    }
}
