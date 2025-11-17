using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DisparoCodigo : MonoBehaviour
{
    [Header("Configurações")]
    public float speed = 20f;
    public float tempoDestruicao = 5f;
    public Color corCorrompida;

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private float tempoVida;
    private bool targetReached = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Suaviza o movimento
        tempoVida = 0f;
    }

    public void SetTarget(Vector2 target)
    {
        targetPosition = target;
        UpdateMovement();
    }

    void UpdateMovement()
    {
        Vector2 direcao = (targetPosition - (Vector2)transform.position).normalized;

        // Atualiza velocidade e rotação
        rb.linearVelocity = direcao * speed;

        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);
    }

    void Update()
    {
        if (targetReached) return;

        tempoVida += Time.deltaTime;

        // Atualiza o movimento continuamente
        UpdateMovement();

        // Verifica se atingiu o alvo ou tempo máximo
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        if (distanceToTarget < 0.5f || tempoVida >= tempoDestruicao)
        {
            targetReached = true;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Catraca"))
        {
            Catraca catraca = other.GetComponent<Catraca>();
            if (catraca != null)
            {
                catraca.SetColor(corCorrompida);
                Destroy(gameObject);
            }
        }
    }
}