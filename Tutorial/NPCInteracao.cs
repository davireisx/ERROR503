using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class NPCInteracao : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public GameObject dialogo;
    public GameObject check;
    public SpriteRenderer spriteRenderer;
    public AudioSource audioToque;

    [Header("Distâncias")]
    public float distanciaVisual = 5f;
    public float distanciaInteracao = 2f;

    [Header("Cores das zonas (Gizmos)")]
    public Color corVisual = Color.yellow;
    public Color corInteracao = Color.green;

    [Header("Piscar")]
    public float velocidadePiscar = 5f;
    public Color corPiscar = Color.black;

    private bool piscando = false;
    private bool interagiu = false;

    // Evento disparado quando o diálogo termina
    public event Action OnDialogoTerminado;

    void Update()
    {
        if (interagiu) return;

        float distanciaPlayer = Vector2.Distance(player.position, transform.position);

        // Zona visual: começa a piscar
        if (distanciaPlayer <= distanciaVisual && !piscando)
        {
            StartCoroutine(Piscar());
        }
        else if (distanciaPlayer > distanciaVisual && piscando)
        {
            StopAllCoroutines();
            piscando = false;
            spriteRenderer.color = Color.white;
        }

        // Clique ou toque
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            VerificarCliqueOuToque(touchPos);
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 clickPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            VerificarCliqueOuToque(clickPos);
        }
    }

    private System.Collections.IEnumerator Piscar()
    {
        piscando = true;
        while (true)
        {
            float t = Mathf.PingPong(Time.time * velocidadePiscar, 1f);
            spriteRenderer.color = Color.Lerp(Color.white, corPiscar, t);
            yield return null;
        }
    }

    private void VerificarCliqueOuToque(Vector2 pos)
    {
        if (interagiu) return;

        float distanciaPlayer = Vector2.Distance(player.position, transform.position);
        if (distanciaPlayer > distanciaInteracao) return;

        float distanciaClique = Vector2.Distance(pos, transform.position);
        if (distanciaClique <= distanciaInteracao)
        {
            audioToque.Play();
            AtivarDialogo();
        }
    }

    private void AtivarDialogo()
    {
        interagiu = true;
        StopAllCoroutines();
        spriteRenderer.color = Color.white;
        dialogo.SetActive(true);
        check.SetActive(true);
    }

    // Chame este método no fim das falas do diálogo
    public void EncerrarDialogo()
    {
        OnDialogoTerminado?.Invoke();
}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = corVisual;
        Gizmos.DrawWireSphere(transform.position, distanciaVisual);

        Gizmos.color = corInteracao;
        Gizmos.DrawWireSphere(transform.position, distanciaInteracao);
    }
}
