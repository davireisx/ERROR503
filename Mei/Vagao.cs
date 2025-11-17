using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class VagaoSistema : MonoBehaviour
{
    [Header("Gerenciador Principal")]
    public VagaoManager manager;

    [Header("Limites de Movimento")]
    public float minX = -10f, maxX = 10f;
    public float minY = -5f, maxY = 5f;

    [Header("Vagões Pretos (Encaixes)")]
    public GameObject[] vagoesPretos;
    public int pontoCorretoIndex = 0;
    public float distanciaMaximaEncaixe = 1.2f;

    [Header("Velocidade de Piscar")]
    [Range(0.1f, 10f)] public float velocidadePiscar = 2f;

    [Header("Referências")]
    public CameraSeguirEsdras cameraScript;
    public Transform player;
    public GameObject joystick;

    [Header("Área de Interação")]
    public Vector2 tamanhoAreaInteracao = Vector2.one;
    public Vector2 offsetAreaInteracao = Vector2.zero;

    [Header("Sons")]
    public AudioSource relarTrem;     // Som ao tocar o encaixe
    public AudioSource destinoTrem;   // Som ao soltar no destino correto

    private Vector3 posicaoOriginal;
    private Quaternion rotacaoOriginal;
    private bool estaArrastando = false;
    private Vector3 offsetArraste;
    private Camera mainCamera;
    private BoxCollider2D boxCollider;
    private CircleCollider2D triggerCol;
    private Vector3 posicaoAlvo;
    private float suavizacao = 15f;

    private Coroutine[] corrotinasPiscar;
    private Coroutine piscarInicioCorrotina;
    private SpriteRenderer srVagao;
    private bool interativo = false;

    // ?? Controle de som relar
    private float cooldownRelar = 0.2f;
    private float ultimoSomRelar = 0f;

    void Start()
    {
        posicaoOriginal = transform.position;
        rotacaoOriginal = transform.rotation;
        mainCamera = Camera.main;
        boxCollider = GetComponent<BoxCollider2D>();
        triggerCol = GetComponent<CircleCollider2D>();
        srVagao = GetComponent<SpriteRenderer>();

        if (vagoesPretos != null && vagoesPretos.Length > 0)
            corrotinasPiscar = new Coroutine[vagoesPretos.Length];
        else
            corrotinasPiscar = new Coroutine[0];

        SetInterativo(false);

        if (triggerCol != null)
            triggerCol.isTrigger = true;

        foreach (GameObject vagaoPreto in vagoesPretos)
        {
            if (vagaoPreto != null && !manager.VagaoPretoJaCorrompido(vagaoPreto))
                vagaoPreto.SetActive(false);
        }
    }

    void Update()
    {
        if (!estaArrastando && interativo)
        {
            Vector2? pos = ObterPosicaoEntrada();
            if (EntradaIniciada() && pos.HasValue && IsPointOverCollider(pos.Value))
                IniciarArraste(pos.Value);
        }
        else if (estaArrastando)
        {
            Vector2? pos = ObterPosicaoEntrada();
            if (pos.HasValue)
            {
                AtualizarPosicaoAlvo(pos.Value);
                transform.position = Vector3.Lerp(transform.position, posicaoAlvo, Time.deltaTime * suavizacao);
            }
            if (EntradaEncerrada()) SoltarVagao();
        }
    }

    public void SetInterativo(bool interativo)
    {
        this.interativo = interativo;
        if (boxCollider != null)
            boxCollider.enabled = interativo;
        if (triggerCol != null)
            triggerCol.enabled = interativo;
    }

    public void AtivarPiscarInicio()
    {
        if (piscarInicioCorrotina != null)
            StopCoroutine(piscarInicioCorrotina);

        if (srVagao != null) srVagao.color = Color.black;

        piscarInicioCorrotina = StartCoroutine(PiscarInicioVisual());
    }

    private IEnumerator PiscarInicioVisual()
    {
        Color cor1 = Color.white;
        Color cor2 = Color.black;

        while (interativo)
        {
            if (srVagao == null) yield break;
            srVagao.color = Color.Lerp(cor1, cor2, Mathf.PingPong(Time.time * velocidadePiscar, 1f));
            yield return null;
        }

        if (srVagao != null) srVagao.color = Color.black;
    }

    Vector2? ObterPosicaoEntrada()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return mainCamera.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            return mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return null;
    }

    bool EntradaIniciada()
    {
        return (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
    }

    bool EntradaEncerrada()
    {
        return (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            || (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame);
    }

    bool IsPointOverCollider(Vector2 point)
    {
        Rect area = new Rect((Vector2)transform.position + offsetAreaInteracao - tamanhoAreaInteracao / 2, tamanhoAreaInteracao);
        return area.Contains(point);
    }

    void IniciarArraste(Vector2 worldPos)
    {
        estaArrastando = true;
        offsetArraste = transform.position - (Vector3)worldPos;
        offsetArraste.z = 0f;

        // ?? CORREÇÃO: Tocar som com volume máximo
        TocarSomRelar();

        transform.rotation = Quaternion.identity;
        SetInterativo(true);
        if (joystick) joystick.SetActive(false);
        if (cameraScript) cameraScript.player = transform;

        if (srVagao != null)
            srVagao.color = new Color(0f, 0f, 0f, 0.59f);

        if (piscarInicioCorrotina != null)
        {
            StopCoroutine(piscarInicioCorrotina);
            piscarInicioCorrotina = null;
        }

        GameObject destinoCorreto = vagoesPretos[pontoCorretoIndex];
        if (destinoCorreto != null && !manager.VagaoPretoJaCorrompido(destinoCorreto))
        {
            destinoCorreto.SetActive(true);
            if (corrotinasPiscar[pontoCorretoIndex] != null)
                StopCoroutine(corrotinasPiscar[pontoCorretoIndex]);
            corrotinasPiscar[pontoCorretoIndex] = StartCoroutine(PiscarPadrao(destinoCorreto.GetComponent<SpriteRenderer>()));
        }

        // Ativa apenas vagões já resolvidos
        for (int i = 0; i < vagoesPretos.Length; i++)
        {
            if (i != pontoCorretoIndex && vagoesPretos[i] != null && manager.VagaoPretoJaCorrompido(vagoesPretos[i]))
            {
                vagoesPretos[i].SetActive(true);
                SpriteRenderer sr = vagoesPretos[i].GetComponent<SpriteRenderer>();
                if (sr) sr.color = Color.white;
            }
        }
    }

    void AtualizarPosicaoAlvo(Vector2 worldPos)
    {
        Vector3 nova = worldPos + (Vector2)offsetArraste;
        nova.z = transform.position.z;
        nova.x = Mathf.Clamp(nova.x, minX, maxX);
        nova.y = Mathf.Clamp(nova.y, minY, maxY);
        posicaoAlvo = nova;
    }

    void SoltarVagao()
    {
        estaArrastando = false;
        GameObject destinoCorreto = vagoesPretos[pontoCorretoIndex];
        float dist = Vector2.Distance(transform.position, destinoCorreto.transform.position);

        if (dist <= distanciaMaximaEncaixe)
        {
            // ?? CORREÇÃO: Tocar som destino com volume máximo
            TocarSomDestino();

            transform.position = destinoCorreto.transform.position;
            SpriteRenderer sr = destinoCorreto.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;

            manager.RegistrarVagaoPretoFixo(destinoCorreto);
            StartCoroutine(DesativarVagaoDepoisDoSom());
        }
        else
        {
            for (int i = 0; i < vagoesPretos.Length; i++)
            {
                if (vagoesPretos[i] != null && !manager.VagaoPretoJaCorrompido(vagoesPretos[i]))
                    vagoesPretos[i].SetActive(false);
            }

            transform.position = posicaoOriginal;
            transform.rotation = rotacaoOriginal;

            if (interativo)
                AtivarPiscarInicio();
        }

        if (corrotinasPiscar[pontoCorretoIndex] != null)
            StopCoroutine(corrotinasPiscar[pontoCorretoIndex]);

        RestaurarEstado();
    }

    IEnumerator DesativarVagaoDepoisDoSom()
    {
        float tempoSom = (destinoTrem != null && destinoTrem.clip != null) ? destinoTrem.clip.length : 0.5f;
        yield return new WaitForSeconds(tempoSom);

        gameObject.SetActive(false);
        manager.VagaoConcluido(gameObject);
    }

    void RestaurarEstado()
    {
        if (joystick) joystick.SetActive(true);
        if (cameraScript && player) cameraScript.player = player;
    }

    IEnumerator PiscarPadrao(SpriteRenderer sr)
    {
        Color cor1 = Color.black;
        Color cor2 = Color.white;

        while (true)
        {
            if (sr == null) yield break;
            sr.color = Color.Lerp(cor1, cor2, Mathf.PingPong(Time.time * velocidadePiscar, 1f));
            yield return null;
        }
    }

    // ?? NOVO MÉTODO: Tocar som relar com volume máximo
    private void TocarSomRelar()
    {
        if (relarTrem != null && relarTrem.clip != null && Time.time - ultimoSomRelar > cooldownRelar)
        {
            // Cria um AudioSource temporário para controle melhor do volume
            GameObject tempAudioObject = new GameObject("TempAudio");
            AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();

            // Configurações para volume máximo
            audioSource.clip = relarTrem.clip;
            audioSource.volume = 1f; // Volume máximo
            audioSource.pitch = relarTrem.pitch;
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.Play();

            // Destroi o objeto após tocar o som
            Destroy(tempAudioObject, relarTrem.clip.length);

            ultimoSomRelar = Time.time;
        }
    }

    // ?? NOVO MÉTODO: Tocar som destino com volume máximo
    private void TocarSomDestino()
    {
        if (destinoTrem != null && destinoTrem.clip != null)
        {
            // Cria um AudioSource temporário para controle melhor do volume
            GameObject tempAudioObject = new GameObject("TempAudio");
            AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();

            // Configurações para volume máximo
            audioSource.clip = destinoTrem.clip;
            audioSource.volume = 1f; // Volume máximo
            audioSource.pitch = destinoTrem.pitch;
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.Play();

            // Destroi o objeto após tocar o som
            Destroy(tempAudioObject, destinoTrem.clip.length);
        }
    }

    // ?? CORREÇÃO: Som de relar usando novo método
    private void OnTriggerStay2D(Collider2D other)
    {
        foreach (var destino in vagoesPretos)
        {
            if (other.gameObject == destino)
            {
                TocarSomRelar();
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 centro = transform.position + (Vector3)offsetAreaInteracao;
        Gizmos.DrawWireCube(centro, new Vector3(tamanhoAreaInteracao.x, tamanhoAreaInteracao.y, 0f));

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(centro, 0.05f);
    }
}