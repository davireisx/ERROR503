using UnityEngine;

public class ComponenteLancado : MonoBehaviour
{
    public Transform[] caminho;
    public float velocidade = 5f;
    public float velocidadeRotacao = 360f;

    public System.Action<ComponenteLancado> OnChegouNoFinal;

    public bool ChegouNoFinal => chegouNoFinal;
    private bool chegouNoFinal = false;
    private int indexAtual = 0;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    void Update()
    {
        if (!chegouNoFinal && spriteRenderer != null && spriteRenderer.enabled)
        {
            MoverComponente();
        }
    }

    void MoverComponente()
    {
        if (caminho == null || caminho.Length == 0) return;

        Transform destino = caminho[indexAtual];
        transform.position = Vector3.MoveTowards(transform.position, destino.position, velocidade * Time.deltaTime);
        transform.Rotate(Vector3.forward * velocidadeRotacao * Time.deltaTime);

        if (Vector3.Distance(transform.position, destino.position) < 0.1f)
        {
            indexAtual++;
            if (indexAtual >= caminho.Length)
            {
                chegouNoFinal = true;

                // ?? Ao chegar no final, muda a layer
                if (spriteRenderer != null)
                {
                    spriteRenderer.sortingLayerName = "Foreground";
                    spriteRenderer.sortingOrder = 1;
                }

                OnChegouNoFinal?.Invoke(this);
            }
        }
    }

    public void IniciarMovimento()
    {
        chegouNoFinal = false;
        indexAtual = 0;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;

            // ?? Ao iniciar o lançamento, muda para layer "Objetos"
            spriteRenderer.sortingLayerName = "Objetos";
            spriteRenderer.sortingOrder = 4;
        }
    }
}
