using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LiberaMov : MonoBehaviour
{
    [Header("Referências Principais")]
    public GameObject joystick;
    public List<GameObject> componentes;
    public Transform player;
    public float distanciaInteracao = 2f;
    public AudioSource audioSource;

    private Camera mainCamera;

    void OnEnable()
    {
        joystick.SetActive(true);
        mainCamera = Camera.main;

        foreach (GameObject c in componentes)
        {
            if (c != null)
            {
                SpriteRenderer sr = c.GetComponent<SpriteRenderer>();
                if (sr != null)
                    StartCoroutine(Piscar(sr));

                Collider2D col = c.GetComponent<Collider2D>();
                if (col == null)
                    col = c.AddComponent<BoxCollider2D>();

                col.isTrigger = true;
            }
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            DetectarClique(Mouse.current.position.ReadValue());
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            DetectarClique(Touchscreen.current.primaryTouch.position.ReadValue());
    }

    void DetectarClique(Vector2 posTela)
    {
        Vector2 posMundo = mainCamera.ScreenToWorldPoint(posTela);
        Collider2D hit = Physics2D.OverlapPoint(posMundo);

        if (hit != null && componentes.Contains(hit.gameObject))
        {
            float dist = Vector2.Distance(player.position, hit.transform.position);
            if (dist <= distanciaInteracao)
                StartCoroutine(Interagir(hit.gameObject));
        }
    }

    IEnumerator Interagir(GameObject objeto)
    {
        if (audioSource != null)
            audioSource.Play();

        SpriteRenderer sr = objeto.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;

        Collider2D col = objeto.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        yield return null;
    }

    IEnumerator Piscar(SpriteRenderer sr)
    {
        while (sr != null && sr.enabled)
        {
            sr.color = Color.black;
            yield return new WaitForSeconds(0.3f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.3f);
        }
    }

    // --- GIZMOS DE INTERAÇÃO ---
    void OnDrawGizmos()
    {
        if (player == null || componentes == null)
            return;

        foreach (GameObject c in componentes)
        {
            if (c == null)
                continue;

            // Linha entre o player e o componente
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(player.position, c.transform.position);

            // Círculo de interação
            Color areaColor = new Color(0f, 0.6f, 1f, 0.15f); // Azul claro translúcido
            Gizmos.color = areaColor;
            Gizmos.DrawSphere(c.transform.position, distanciaInteracao);

            // Contorno do círculo
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(c.transform.position, distanciaInteracao);

            // Nome do componente (útil no editor)
#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.Label(c.transform.position + Vector3.up * 0.5f, c.name);
#endif
        }

        // Indicador do player
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, 0.2f);
    }
}
