using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // Input System novo

public class InteragirPortaDigital : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public float interactionRange = 3f;
    public GameObject joystick;

    [Header("UI - Botões e Painéis")]
    public GameObject painelDialogo1;
    public GameObject botaoPular1;
    public GameObject painelDialogo2;
    public GameObject botaoPular2;
    public GameObject painelDialogo3;
    public GameObject botaoPular3;
    public GameObject painelDialogo4;
    public GameObject botaoPular4;
    public GameObject painelDialogo5;
    public GameObject botaoPular5;
    public GameObject tutorial;
    public GameObject botaoFecharPoder;
    public GameObject fechar;
    public GameObject painelDigital;
    public GameObject placaEmbaralhada;
    public GameObject placaNormal;
    public SpriteRenderer porta;

    [Header("Audio")]
    public AudioSource somFechadura;
    public AudioSource somErroFechadura;
    public AudioSource on;
    public AudioSource off;

    [Header("Brilho")]
    public Color highlightColor = Color.yellow;
    private Color originalColor;

    [Header("HUD")]
    public GameObject hud;
    public GameObject objetivos2;
    public GameObject objetivos3;


    [Header("Falas")]
    public GameObject[] falas1; // personagem início
    public GameObject[] falas2; // inimigo 1
    public GameObject[] falas3; // personagem final
    public GameObject[] falas4; // inimigo pós tutorial
    public GameObject[] falas5; // personagem final final

    private int falaAtual = 0;
    private bool playerInRange = false;
    private bool imageActive = false;
    private bool dialogo1Ativo = false;
    private bool jaAbriuPrimeiroDialogo = false;
    private bool concluiuDialogo2 = false;
    private bool concluiuDialogoFinal = false;
    private bool jaAbriuPainelDigital = false;
    private int momentoPorta = 1;
    private bool portaPodeBrilhar = true;


    void Start()
    {
        momentoPorta = 1;
        porta = GetComponent<SpriteRenderer>();
        if (porta != null)
            originalColor = porta.color;
        else
            Debug.LogWarning($"[{gameObject.name}] SpriteRenderer não encontrado!");

        if (player == null) Debug.LogError($"[{gameObject.name}] Player não atribuído!");

        // Desativa tudo
        fechar?.SetActive(false);
        painelDialogo1?.SetActive(false);
        painelDialogo2?.SetActive(false);
        painelDialogo3?.SetActive(false);
        painelDialogo4?.SetActive(false);
        painelDialogo5?.SetActive(false);
        botaoPular1?.SetActive(false);
        botaoPular2?.SetActive(false);
        botaoPular3?.SetActive(false);
        botaoPular4?.SetActive(false);
        botaoPular5?.SetActive(false);
        tutorial?.SetActive(false);
        botaoFecharPoder?.SetActive(false);
        painelDigital?.SetActive(false);
        joystick?.SetActive(true);
        hud?.SetActive(true);


        DesativarFalas(falas1);
        DesativarFalas(falas2);
        DesativarFalas(falas3);
        DesativarFalas(falas4);
        DesativarFalas(falas5);
    }

    void Update()
    {

        VerificarDistancia();
        AplicarBrilho();

        // Suporte para toque (mobile)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            VerificarCliqueOuToque(touchPos);
        }

        // Suporte para mouse (PC)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 clickPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            VerificarCliqueOuToque(clickPos);
        }

        // Verifica se algum painel está ativo
        bool algumPainelAtivo =
            (painelDialogo1 != null && painelDialogo1.activeSelf) ||
            (painelDialogo2 != null && painelDialogo2.activeSelf) ||
            (painelDialogo3 != null && painelDialogo3.activeSelf) ||
            (painelDialogo4 != null && painelDialogo4.activeSelf) ||
            (painelDialogo5 != null && painelDialogo5.activeSelf) ||
            (tutorial != null && tutorial.activeSelf) ||
            (painelDigital != null && painelDigital.activeSelf);

        // Ativa ou desativa o HUD com base nisso
        if (hud != null)
        {
            hud.SetActive(!algumPainelAtivo);
        }

        if (player == null) return;

    }

    void VerificarDistancia()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;
    }

    void AplicarBrilho()
    {
        if (porta == null) return;

        if (playerInRange && !imageActive && portaPodeBrilhar == true)
        {
            float pulse = Mathf.PingPong(Time.time * 2f, 1f);
            porta.color = Color.Lerp(originalColor, highlightColor, pulse);
        }
        else
        {
            porta.color = originalColor;
        }
    }


    void VerificarCliqueOuToque(Vector2 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == this.gameObject && playerInRange && momentoPorta == 1)
        {
            Abrir();
        }

        else if (hit.collider != null && hit.collider.gameObject == this.gameObject && playerInRange && momentoPorta == 2)
        {
            Abrir2();
        }
    }

    public void Abrir()
    {
        somErroFechadura.Play();
        portaPodeBrilhar = false;
        falaAtual = 0;
        dialogo1Ativo = true;
        painelDialogo1?.SetActive(true);
        botaoPular1?.SetActive(true);
        joystick?.SetActive(false);
        MostrarFalaAtual(falas1);
        hud?.SetActive(false);
        momentoPorta = 3;
    }

    public void Abrir2()
    {
        somFechadura.Play();
        portaPodeBrilhar = false;
        imageActive = true;
        painelDigital.SetActive(true);
        fechar?.SetActive(true);
        joystick?.gameObject.SetActive(false);
        hud?.SetActive(false);
    }

    public void AoClicarPular1()
    {
        on.Play();
        if (++falaAtual < falas1.Length)
            MostrarFalaAtual(falas1);
        else
        {
            DesativarFalas(falas1);
            painelDialogo1?.SetActive(false);
            botaoPular1?.SetActive(false);

            falaAtual = 0;
            painelDialogo2?.SetActive(true);
            botaoPular2?.SetActive(true);
            MostrarFalaAtual(falas2);
        }
    }

    public void AoClicarPular2()
    {
        on.Play();
        if (++falaAtual < falas2.Length)
            MostrarFalaAtual(falas2);
        else
        {
            DesativarFalas(falas2);
            painelDialogo2?.SetActive(false);
            botaoPular2?.SetActive(false);

            falaAtual = 0;
            painelDialogo3?.SetActive(true);
            botaoPular3?.SetActive(true);
            MostrarFalaAtual(falas3);
        }
    }

    public void AoClicarPular3()
    {
        on.Play();
        if (++falaAtual < falas3.Length)
            MostrarFalaAtual(falas3);
        else
        {
            DesativarFalas(falas3);
            painelDialogo3?.SetActive(false);
            botaoPular3?.SetActive(false);

            tutorial?.SetActive(true);
            botaoFecharPoder?.SetActive(true);
            joystick?.SetActive(false);

            dialogo1Ativo = false;
            jaAbriuPrimeiroDialogo = true;
            concluiuDialogo2 = true;
        }
    }

    public void AoClicarFecharPoder()
    {
        off.Play();
        interactionRange = 0;
        botaoFecharPoder?.SetActive(false);
        tutorial?.SetActive(false);
        painelDialogo1?.SetActive(false);
        painelDialogo2?.SetActive(false);
        painelDialogo3?.SetActive(false);
        painelDialogo4?.SetActive(false);
        painelDialogo5?.SetActive(false);

        // Desativa brilho e força branco
        portaPodeBrilhar = true;
        porta.color = Color.white;
        originalColor = Color.white;

        // Define que o próximo clique usará Abrir2()
        momentoPorta = 2;

        StartCoroutine(AguardarFecharPoder());
    }


    private IEnumerator AguardarFecharPoder()
    {
        yield return new WaitForSeconds(1.5f);

        falaAtual = 0;
        painelDialogo4?.SetActive(true);
        botaoPular4?.SetActive(true);
        objetivos2.SetActive(false);
        objetivos3.SetActive(true);
        MostrarFalaAtual(falas4);
    }

    public void AoClicarFecharFechadura()
    {
        FecharPainelDigital();
    }

    public void AoClicarPular4()
    {
        on.Play();
        if (++falaAtual < falas4.Length)
            MostrarFalaAtual(falas4);
        else
        {
            DesativarFalas(falas4);
            painelDialogo4?.SetActive(false);
            botaoPular4?.SetActive(false);

            falaAtual = 0;
            painelDialogo5?.SetActive(true);
            botaoPular5?.SetActive(true);
            MostrarFalaAtual(falas5);
        }
    }

    public void AoClicarPular5()
    {
        on.Play();
        if (++falaAtual < falas5.Length)
            MostrarFalaAtual(falas5);
        else
        {
            DesativarFalas(falas5);
            painelDialogo5?.SetActive(false);
            botaoPular5?.SetActive(false);
            joystick?.SetActive(true);
            imageActive = false; // ? ESSENCIAL!
            falaAtual = 0;       // ? por segurança

            portaPodeBrilhar = true;
            concluiuDialogoFinal = true;
            interactionRange = 13;
        }
    }



    void MostrarFalaAtual(GameObject[] lista)
    {
        for (int i = 0; i < lista.Length; i++)
            if (lista[i] != null)
                lista[i].SetActive(i == falaAtual);
    }

    void DesativarFalas(GameObject[] lista)
    {
        foreach (var f in lista) if (f != null) f.SetActive(false);
    }

    public void AbrirPainelDigital()
    {
        somFechadura.Play();
        painelDigital?.SetActive(true);
        joystick?.SetActive(false);
        fechar?.SetActive(true);
        jaAbriuPainelDigital = true;
    }



    public void FecharPainelDigital()
    {
        painelDigital?.SetActive(false);
        joystick?.SetActive(true);
        objetivos2.SetActive(false);
        objetivos3.SetActive(true);
        fechar?.SetActive(false);

        // Permitir que o botão apareça novamente se estiver no range
        jaAbriuPainelDigital = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}