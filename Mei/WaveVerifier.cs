using UnityEngine;

public class VerificadorWaves : MonoBehaviour
{
    [Header("UI")]
    public GameObject hud;
    public GameObject hudJUNKO;
    public GameObject joystick;

    [Header("Componentes que serão verificados")]
    public ComponenteLancado[] componentes;

    [Header("Piscar Componentes")]
    public bool piscar;
    public Color corDaPiscada = new Color(0f, 0f, 0f, 0.5f);
    public float velocidadePiscada = 5f;
    public ComponenteLancado[] componentesPiscarAtaque;

    [Header("Clicar Componentes")]
    public ClicarComponentes clicarComponentes;
    public Collider2D[] podeClicar;

    public int concluidos = 0;
    private bool piscando = false;

    void Start()
    {
        hud.SetActive(false);
        joystick.SetActive(false);

        foreach (var comp in componentes)
        {
            if (comp != null && comp.ChegouNoFinal)
            {
                concluidos++;
            }
            else if (comp != null)
            {
                comp.OnChegouNoFinal += ComponenteFinalizou;
            }
        }

        VerificarTodos();
    }

    private void Update()
    {
        if (!piscando || !piscar) return;

        foreach (var comp in componentesPiscarAtaque)
        {
            if (comp == null) continue;

            SpriteRenderer sr = comp.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            Color corOriginal = Color.white; // Pode ajustar se quiser pegar sr.color inicial
            float alpha = Mathf.PingPong(Time.time * velocidadePiscada, 1f);
            sr.color = Color.Lerp(corOriginal, corDaPiscada, alpha);
        }
    }

    private void ComponenteFinalizou(ComponenteLancado comp)
    {
        concluidos++;
    }

    public void VerificarTodos()
    {
        if (concluidos >= componentes.Length)
        {
            // Ativa HUD e joystick
            hud.SetActive(true);
            hudJUNKO.SetActive(false);
            joystick.SetActive(true);

            // Começa a piscar
            if (piscar && componentesPiscarAtaque != null)
                piscando = true;

            // Ativa os colliders para clique
            if (podeClicar != null)
            {
                foreach (var col in podeClicar)
                {
                    if (col != null)
                        col.enabled = true;
                }
            }

            // Se tiver script de clique, habilita também
            if (clicarComponentes != null)
                clicarComponentes.enabled = true;
        }
    }
}
