using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    // Configurações do inimigo
    [Header("Configurações do Inimigo")]
    public float speed = 3f; // Velocidade do inimigo
    public float interactionRange = 2f; // Distância de interação
    public bool loopWaypoints = true; // Define se o inimigo deve reiniciar o percurso ao final
    public List<Transform> waypoints; // Lista de waypoints

    private int currentWaypointIndex = 0; // Índice do waypoint atual
    private bool isAtLastWaypoint = false; // Verifica se o inimigo está no último waypoint
    private bool hasTalked = false; // Indica se o diálogo foi concluído

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Obtém os componentes necessários
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (waypoints.Count == 0)
        {
            Debug.LogError("Nenhum waypoint definido para o inimigo!");
        }

        // Assina o evento de término de diálogo
        DialogueControl dc = Object.FindFirstObjectByType<DialogueControl>();
        dc.OnDialogueEnd += OnDialogueFinished;
    }

    void Update()
    {
        // Move o inimigo apenas após o término do diálogo
        if (hasTalked && waypoints.Count > 0)
        {
            MoveToWaypoint();
        }
        else
        {
            StopMoving();
        }
    }

    private void MoveToWaypoint()
    {
        if (currentWaypointIndex >= waypoints.Count - 1 && !loopWaypoints)
        {
            StopMoving();
            isAtLastWaypoint = true;
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector2 direction = (targetWaypoint.position - transform.position).normalized;

        rb.linearVelocity = direction * speed;
        UpdateAnimation(direction);

        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.2f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Count - 1)
            {
                if (!loopWaypoints)
                {
                    StopMoving();
                    return;
                }
                else
                {
                    currentWaypointIndex = 0;
                }
            }
        }
    }

    private void StopMoving()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
        anim?.SetBool("IsMoving", false);
    }

    private void UpdateAnimation(Vector2 direction)
    {
        anim?.SetBool("IsMoving", true);

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            anim?.SetBool("IsSide", true);
            anim?.SetBool("IsBack", false);
            anim?.SetBool("IsFront", false);
            spriteRenderer.flipX = direction.x < 0;
        }
        else
        {
            anim?.SetBool("IsBack", direction.y > 0);
            anim?.SetBool("IsFront", direction.y <= 0);
            anim?.SetBool("IsSide", false);
        }
    }

    public void OnDialogueFinished()
    {
        Debug.Log("Diálogo finalizado! O inimigo agora pode se mover.");
        hasTalked = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        Gizmos.color = Color.red;
        if (waypoints != null && waypoints.Count > 1)
        {
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        foreach (var waypoint in waypoints)
        {
            Gizmos.DrawSphere(waypoint.position, 0.2f);
        }
    }

    public bool IsAtLastWaypoint()
    {
        return isAtLastWaypoint;
    }
}