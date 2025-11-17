using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PoderEsdras_Museu : MonoBehaviour
{
    //=========================//
    //    [ UI & Elementos ]   //
    //=========================//

    [Header("Botões e Cores")]
    public List<Button> botoes;
    public List<Image> coresDosBotoes;
    public List<Image> bordasDosBotoes;
    public Button botaoFechar;
    public Button botaoTentarNovamente;


    [Header("Código na Tela")]
    public List<Text> textosDoCodigo;

    [Header("UI de Feedback")]
    public Text textoErros;
    public Text textoRelogio;
    public Text textoCliques;

    [Header("Configuração de Rodadas")]
    [SerializeField] private int cliquesParaProximaRodada;
    [SerializeField] private int errosRodada;

    [Header("Áudio")]
    public AudioSource somAcerto;
    public AudioSource somErro;

    [Header("Painel de Cores")]
    public List<Image> imagensDasCores; // Imagens quadradas das cores no painel
    public List<Image> bordasBrilhantes; // Bordas brilhantes para destacar a cor atual
    public float intervaloPiscar = 0.5f; // Intervalo para piscar a borda

    [Header("Painel de Jogo")]
    [SerializeField] private GameObject painelDoJogo;

    [Header("Telas Finais")]
    public Image telaVitoria;   // atribua no Inspector
    public Image telaDerrota;    // atribua no Inspector


    //=========================//
    //     [ Configuração ]    //
    //=========================//

    [Header("Configuração de Tempo")]
    public float tempoTotal = 60f;
    public float intervaloEmbaralhamento = 2f;

    [Header("Configurações de Animação")]
    public float duracaoPiscada = 0.3f;
    public int numeroPiscadas = 3;

    [Header("Configurações de Feedback")]
    public Color corAcerto = Color.green;
    public Color corErro = Color.red;
    public float duracao = 0.3f;

    private float tempoRestante;
    private bool relogioAtivo = false;
    private bool desafioFinalizado = false;

    private int errosCometidos = 0;
    private bool podeClicar = false;
    private bool rodadaEmAndamento = false;

    private int rodadaAtual = 0;
    private int acertosNaRodada = 0;
    private int indiceTextoAtual = 0;

    private bool jaFinalizou = false;

    private Dictionary<string, int> corParaPosicao = new(); // Mapeia cor → posição no array
    private Dictionary<int, string> posicaoParaCor = new(); // Mapeia posição → cor
    private int[] cliquesPorCor = new int[8]; // Armazena número de cliques por posição

    // Lista original de cores (será embaralhada no início)
    private List<string> coresBase = new List<string>()
    {
        "Roxo", "Marrom", "Azul", "Laranja", "Verde", "Vermelho", "Branco", "Amarelo"
    };

    private List<string> sequenciaCores = new List<string>(); // Será preenchida com a ordem aleatória

    private readonly List<string> codigoCorreto = new()
    {
        "                       INICIAR    SISTEMA",    // posição 0
        "            DIGITAR    senha    de    acesso",         // posição 1
        "           SE    senha    é    correta,    ENTÃO",                       // posição 2
        "                      {      ''Acesso    Confirmado''",       // posição 3
        "                                   DESTRAVAR     porta     }  ",// posição 4
        "          SENÃO",                   // posição 5
        "                       {      ''Acesso    Negado''", // posição 6
        "                                   MANTER   porta   fechada    }  "                        // posição 7
    };

    private List<string> textosEmbaralhados = new();
    private List<bool> textosFixos = new(); // Indica quais textos já estão na posição correta

    private string CorDaRodada => sequenciaCores[rodadaAtual];
    private Coroutine piscarBordaCoroutine;

    //=========================//
    //        [ Start ]        //
    //=========================//

    private void Start()
    {

        botaoTentarNovamente.gameObject.SetActive(false);
        textoErros.text = $"Erros:  {errosCometidos} / {errosRodada}";
        // Ativa todos os botões no início do jogo
        foreach (var botao in botoes)
        {
            botao.gameObject.SetActive(true);
        }

        EmbaralharCoresInicialmente();

        for (int i = 0; i < imagensDasCores.Count; i++)
            if (i < sequenciaCores.Count)
                imagensDasCores[i].color = NomeParaCor(sequenciaCores[i]);

    }

    public void IniciarMinigame()
    {
        InicializarJogo();
        relogioAtivo = true;
        tempoRestante = tempoTotal;
        StartCoroutine(RelogioContagem());
    }
    private void EmbaralharCoresInicialmente()
    {
        sequenciaCores = new List<string>(coresBase);

        // Embaralha usando o algoritmo Fisher-Yates
        System.Random rng = new System.Random();
        int n = sequenciaCores.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            string value = sequenciaCores[k];
            sequenciaCores[k] = sequenciaCores[n];
            sequenciaCores[n] = value;
        }

        // Atualiza os dicionários de mapeamento
        corParaPosicao.Clear();
        posicaoParaCor.Clear();
        for (int i = 0; i < sequenciaCores.Count; i++)
        {
            corParaPosicao[sequenciaCores[i]] = i;
            posicaoParaCor[i] = sequenciaCores[i];
        }
    }

    public void InicializarJogo()
    {
        textosFixos = new List<bool>(new bool[codigoCorreto.Count]);
        textosEmbaralhados = new List<string>(codigoCorreto);

        EmbaralharTextos();

        indiceTextoAtual = 0;
        rodadaAtual = 0;
        acertosNaRodada = 0;

        StartCoroutine(IniciarRodada());
    }

    //=========================//
    //   [ Lógica de Textos ]  //
    //=========================//

    private void EmbaralharTextos()
    {
        List<string> textosPara = new();
        List<int> indices = new();
        for (int i = 0; i < codigoCorreto.Count; i++)
            if (!textosFixos[i])
            {
                textosPara.Add(codigoCorreto[i]);
                indices.Add(i);
            }

        Derangement(textosPara, indices);
        for (int i = 0; i < indices.Count; i++)
            textosEmbaralhados[indices[i]] = textosPara[i];

        AtualizarTextosUI();
    }

    private void Derangement(List<string> lista, List<int> indices)
    {
        int tent = 0;
        bool valido = false;
        while (!valido && tent < 100)
        {
            for (int i = 0; i < lista.Count; i++)
            {
                int r = Random.Range(i, lista.Count);
                (lista[i], lista[r]) = (lista[r], lista[i]);
            }

            valido = true;
            for (int i = 0; i < lista.Count; i++)
                if (lista[i] == codigoCorreto[indices[i]])
                {
                    valido = false;
                    break;
                }

            tent++;
        }
    }

    private void AtualizarTextosUI()
    {
        for (int i = 0; i < textosDoCodigo.Count; i++)
        {
            if (i < textosEmbaralhados.Count)
            {
                textosDoCodigo[i].text = textosEmbaralhados[i];
                textosDoCodigo[i].color = textosFixos[i] ? Color.green : Color.white;
            }
        }
    }

    private void VerificarEfixarTextos()
    {
        for (int i = 0; i < codigoCorreto.Count; i++)
        {
            if (textosEmbaralhados[i] == codigoCorreto[i] && !textosFixos[i])
            {
                textosFixos[i] = true;
                textosDoCodigo[i].color = Color.green;
                if (i == indiceTextoAtual) indiceTextoAtual++;
            }
        }
        AtualizarTextosUI();
    }

    //=========================//
    //      [ Botões ]         //
    //=========================//

    public void BotaoClicado(Button botao)
    {
        if (!podeClicar || !botao.gameObject.activeSelf || desafioFinalizado || jaFinalizou) return;


        botao.interactable = false;
        bool acerto = botao.name == CorDaRodada;
        int i = botoes.IndexOf(botao);

        if (acerto)
        {
            somAcerto.Play();
            acertosNaRodada++;
            AtualizarTextoCliques();

            // Atualiza contagem de cliques por cor
            if (corParaPosicao.TryGetValue(CorDaRodada, out int pos))
            {
                cliquesPorCor[pos]++;
                if (cliquesPorCor[pos] == cliquesParaProximaRodada)
                {
                    textosEmbaralhados[pos] = codigoCorreto[pos];
                    VerificarEfixarTextos();
                }
            }

            CancelInvoke(nameof(EmbaralharBotoes));

            // Verifica se é o último clique necessário para vencer
            bool ultimoCliqueParaVencer = (acertosNaRodada >= cliquesParaProximaRodada) &&
                                         (rodadaAtual == sequenciaCores.Count - 1);

            if (ultimoCliqueParaVencer)
            {
                // Vitória - desativa cliques imediatamente
                podeClicar = false;
                jaFinalizou = true;
                StartCoroutine(CompletarDesafio());
            }

            else if (acertosNaRodada >= cliquesParaProximaRodada)
            {
                podeClicar = false;
                StartCoroutine(EsperarAntesDeAvancarRodada());
            }
            else
            {
                StartCoroutine(FadeOutEEsconder(botao.image, bordasDosBotoes[i]));
                StartCoroutine(ReiniciarEmbaralhamento());
            }
        }
        else
        {
            // Lógica de erro
            errosCometidos++;
            textoErros.text = $"Erros:  {errosCometidos} / {errosRodada}";
            somErro.Play();

            bool ultimoErro = errosCometidos >= errosRodada;

            if (!ultimoErro)
            {
                StartCoroutine(FadeOutEEsconder(botao.image, bordasDosBotoes[i]));
                CancelInvoke(nameof(EmbaralharBotoes));
                StartCoroutine(ReiniciarEmbaralhamento());
            }
            else
            {
                // Derrota - desativa cliques imediatamente
                podeClicar = false;
                jaFinalizou = true;
                StartCoroutine(Derrota());
            }

        }
    }

    private IEnumerator EsperarAntesDeAvancarRodada()
    {
        yield return new WaitForSeconds(0.2f);
        rodadaAtual++;
        StartCoroutine(IniciarRodada());
    }


    private void EmbaralharBotoes()
    {
        if (!podeClicar) return;

        // Desativa apenas as bordas (os botões permanecem ativos)
        foreach (var borda in bordasDosBotoes)
            borda.gameObject.SetActive(false);

        int qtd = Random.Range(2, Mathf.Min(6, botoes.Count + 1));
        HashSet<int> idxs = new();
        while (idxs.Count < qtd)
        {
            int i = Random.Range(0, botoes.Count);
            if (idxs.Add(i))
            {
                // 2) Reative explicitamente o botão escolhido
                botoes[i].gameObject.SetActive(true);
                botoes[i].interactable = true;

                string cor = Random.Range(0, 100) < 35
                    ? CorDaRodada
                    : sequenciaCores[Random.Range(0, sequenciaCores.Count)];

                botoes[i].name = cor;
                coresDosBotoes[i].color = NomeParaCor(cor);

                // Ativa apenas a borda correspondente
                bordasDosBotoes[i].gameObject.SetActive(true);
                var cb = bordasDosBotoes[i].color;
                cb.a = 214f / 255f;
                bordasDosBotoes[i].color = cb;
            }
        }
    }

    public void ReiniciarTudo()
    {
        StopAllCoroutines();
        desafioFinalizado = false;
        errosCometidos = 0;
        foreach (var t in textosDoCodigo) t.color = Color.white;
        textoErros.text = $"Erros: 0 / {errosRodada}";
        tempoRestante = tempoTotal;
        relogioAtivo = true;
        InicializarJogo();
        StartCoroutine(RelogioContagem());
    }

    //=========================//
    //   [ Lógica de Rodada ]  //
    //=========================//

    private IEnumerator IniciarRodada()
    {
        if (rodadaEmAndamento) yield break;
        rodadaEmAndamento = true;
        podeClicar = false;
        acertosNaRodada = 0;
        AtualizarTextoCliques();

        // 1) Reative todos os botões no início da rodada
        foreach (var botao in botoes)
        {
            botao.gameObject.SetActive(true);
            botao.interactable = true;
        }

        EmbaralharTextos();

        if (piscarBordaCoroutine != null)
            StopCoroutine(piscarBordaCoroutine);

        foreach (var borda in bordasBrilhantes)
            borda.gameObject.SetActive(false);

        piscarBordaCoroutine = StartCoroutine(PiscarBordaCorDaRodada());

        podeClicar = true;
        CancelInvoke(nameof(EmbaralharBotoes));
        EmbaralharBotoes();
        InvokeRepeating(nameof(EmbaralharBotoes), intervaloEmbaralhamento, intervaloEmbaralhamento);

        rodadaEmAndamento = false;
    }


    private void AtualizarTextoCliques()
    {
        textoCliques.text = $"Cliques:  {acertosNaRodada} / {cliquesParaProximaRodada}";
    }

    private IEnumerator PiscarBordaCorDaRodada()
    {
        // Encontra o índice da cor atual no painel
        int indiceCorAtual = -1;
        for (int i = 0; i < sequenciaCores.Count; i++)
        {
            if (sequenciaCores[i] == CorDaRodada)
            {
                indiceCorAtual = i;
                break;
            }
        }

        if (indiceCorAtual < 0 || indiceCorAtual >= bordasBrilhantes.Count)
            yield break;

        while (true)
        {
            bordasBrilhantes[indiceCorAtual].gameObject.SetActive(true);
            yield return new WaitForSeconds(intervaloPiscar);
            bordasBrilhantes[indiceCorAtual].gameObject.SetActive(false);
            yield return new WaitForSeconds(intervaloPiscar);
        }
    }

    private IEnumerator AguardarEIniciarNovaRodada()
    {
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(IniciarRodada());
    }

    private IEnumerator ReiniciarEmbaralhamento()
    {
        yield return new WaitForSeconds(intervaloEmbaralhamento / 2);
        botoes.ForEach(b => b.gameObject.SetActive(false));
        bordasDosBotoes.ForEach(b => b.gameObject.SetActive(false));
        CancelInvoke(nameof(EmbaralharBotoes));
        EmbaralharBotoes();
        InvokeRepeating(nameof(EmbaralharBotoes), intervaloEmbaralhamento, intervaloEmbaralhamento);
    }


    private IEnumerator SuavizarDesaparecimento()
    {
        float dur = 0.4f;
        foreach (var b in botoes)
            if (b.gameObject.activeSelf)
                StartCoroutine(FadeOutEEsconder(b.image, bordasDosBotoes[botoes.IndexOf(b)], dur));

        yield return new WaitForSeconds(dur);
        botoes.ForEach(b => b.gameObject.SetActive(false));
        foreach (var b in botoes)
        {
            b.image.color = Color.white;
            bordasDosBotoes[botoes.IndexOf(b)].color = Color.white;
        }
    }

    private IEnumerator CompletarDesafio()
    {

        desafioFinalizado = true;
        podeClicar = false;
        CancelInvoke(nameof(EmbaralharBotoes));
        if (piscarBordaCoroutine != null)
            StopCoroutine(piscarBordaCoroutine);
        CancelInvoke(nameof(EmbaralharBotoes));


        // Fixa textos corretos...
        for (int i = 0; i < codigoCorreto.Count; i++)
        {
            textosEmbaralhados[i] = codigoCorreto[i];
            textosFixos[i] = true;
        }
        AtualizarTextosUI();

        // 1) Ativa todas as bordas e botões
        for (int i = 0; i < bordasDosBotoes.Count; i++)
        {
            bordasDosBotoes[i].gameObject.SetActive(true);
            bordasDosBotoes[i].color = Color.green;      // define cor final já no início
            botoes[i].gameObject.SetActive(true);
            botoes[i].interactable = false;
        }

        // 2) Pisca verde por 4 segundos
        yield return StartCoroutine(PiscarBotoes(Color.green, 4f));
        telaVitoria.gameObject.SetActive(true);

        botaoFechar.gameObject.SetActive(true);

        // Adiciona ação ao botão
        botaoFechar.onClick.RemoveAllListeners(); // limpa eventos anteriores
        botaoFechar.onClick.AddListener(() =>
        {
            painelDoJogo.SetActive(false);
            botaoFechar.gameObject.SetActive(false); // opcional: esconder o botão depois
        });

        yield return null;
    }

    private IEnumerator Derrota()
    {
        desafioFinalizado = true;
        podeClicar = false;
        CancelInvoke(nameof(EmbaralharBotoes));
        if (piscarBordaCoroutine != null)
            StopCoroutine(piscarBordaCoroutine);

        // 1) Ativa todas as bordas e botões
        for (int i = 0; i < bordasDosBotoes.Count; i++)
        {
            bordasDosBotoes[i].gameObject.SetActive(true);
            bordasDosBotoes[i].color = Color.red;
            botoes[i].gameObject.SetActive(true);
            botoes[i].interactable = false;
        }

        // 2) Pisca vermelho por 4 segundos
        yield return StartCoroutine(PiscarBotoes(Color.red, 4f));
        telaDerrota.gameObject.SetActive(true);

        // Ativa o botão de Tentar Novamente e desativa o de Fechar
        botaoTentarNovamente.gameObject.SetActive(true);
        botaoFechar.gameObject.SetActive(false);

        // Configura o botão Tentar Novamente
        botaoTentarNovamente.onClick.RemoveAllListeners();
        botaoTentarNovamente.onClick.AddListener(ReiniciarMinigame);
    }

    // Crie esta nova função para reiniciar completamente o minigame:
    public void ReiniciarMinigame()
    {
        // Desativa telas finais
        jaFinalizou = false;
        telaDerrota.gameObject.SetActive(false);
        telaVitoria.gameObject.SetActive(false);
        botaoTentarNovamente.gameObject.SetActive(false);
        botaoFechar.gameObject.SetActive(false);

        // Reseta todas as variáveis do jogo
        desafioFinalizado = false;
        errosCometidos = 0;
        rodadaAtual = 0;
        acertosNaRodada = 0;
        tempoRestante = tempoTotal;
        relogioAtivo = true;

        // Reseta a UI
        textoErros.text = $"Erros:  {errosCometidos} / {errosRodada}";
        AtualizarTextoCliques();

        // Reseta os textos
        foreach (var t in textosDoCodigo)
        {
            t.color = Color.white;
        }

        // Reseta os botões
        foreach (var botao in botoes)
        {
            botao.gameObject.SetActive(true);
            botao.interactable = true;
            botao.image.color = Color.white;
        }

        foreach (var borda in bordasDosBotoes)
        {
            borda.gameObject.SetActive(false);
            borda.color = Color.white;
        }

        // Reseta as cores brilhantes
        foreach (var borda in bordasBrilhantes)
        {
            borda.gameObject.SetActive(false);
        }

        // Reembaralha tudo
        EmbaralharCoresInicialmente();
        InicializarJogo();

        // Reinicia a contagem do relógio
        StopCoroutine(RelogioContagem());
        StartCoroutine(RelogioContagem());

        // Atualiza as cores no painel
        for (int i = 0; i < imagensDasCores.Count; i++)
        {
            if (i < sequenciaCores.Count)
                imagensDasCores[i].color = NomeParaCor(sequenciaCores[i]);
        }
    }



    //=========================//
    //        [ Extras ]       //
    //=========================//

    private IEnumerator RelogioContagem()
    {
        while (tempoRestante > 0 && !desafioFinalizado)
        {
            // Calcula minutos e segundos
            int minutos = Mathf.FloorToInt(tempoRestante / 60f);
            int segundos = Mathf.FloorToInt(tempoRestante % 60f);

            // Formata com dois dígitos para minutos e segundos
            textoRelogio.text = $"{minutos:00}:{segundos:00}";

            yield return new WaitForSeconds(1f);
            tempoRestante -= 0.5f;
        }

        if (!desafioFinalizado)
        {
            textoRelogio.text = "00:00";
            StartCoroutine(Derrota());
        }

    }

    private IEnumerator FadeOutEEsconder(Graphic principal, Graphic borda = null, float duracao = 0.5f)
    {
        Color ci = principal.color;
        Color cb = borda ? borda.color : Color.clear;
        float t = 0f;
        while (t < duracao)
        {
            float a = Mathf.Lerp(1f, 0f, t / duracao);
            principal.color = new Color(ci.r, ci.g, ci.b, a);
            if (borda)
                borda.color = new Color(cb.r, cb.g, cb.b, a);
            t += Time.deltaTime;
            yield return null;
        }
        principal.color = new Color(ci.r, ci.g, ci.b, 0f);
        principal.gameObject.SetActive(false);
        if (borda)
        {
            borda.color = new Color(cb.r, cb.g, cb.b, 0f);
            borda.gameObject.SetActive(false);
        }
    }

    private IEnumerator PiscarBotoes(Color cor, float duracaoTotal)
    {
        float intervalo = 0.5f;
        float tempoDecorrido = 0f;

        // 1. Ativa TODOS os botões
        foreach (var botao in botoes)
        {
            botao.gameObject.SetActive(true);
            botao.interactable = false;
            coresDosBotoes[botoes.IndexOf(botao)].color = Color.clear;
        }

        // 2. Animação de piscar
        while (tempoDecorrido < duracaoTotal)
        {
            // Liga - coloca todos na cor especificada
            foreach (var botao in botoes)
            {
                int idx = botoes.IndexOf(botao);
                botao.image.color = cor;
                bordasDosBotoes[idx].color = cor;
            }
            yield return new WaitForSeconds(intervalo / 2);

            // Desliga - deixa todos transparentes
            foreach (var botao in botoes)
            {
                int idx = botoes.IndexOf(botao);
                botao.image.color = Color.clear;
                bordasDosBotoes[idx].color = Color.clear;
            }
            yield return new WaitForSeconds(intervalo / 2);

            tempoDecorrido += intervalo;
        }

        // 3. Mantém estado final
        foreach (var botao in botoes)
        {
            int idx = botoes.IndexOf(botao);
            botao.image.color = cor;
            bordasDosBotoes[idx].color = cor;
        }
    }

    private readonly Dictionary<string, string> corHexadecimal = new()
    {
        { "Azul", "#2545EC" },
        { "Laranja", "#E78A17" },
        { "Roxo", "#9C26E5" },
        { "Amarelo", "#E5E51D" },
        { "Verde", "#29C146" },
        { "Vermelho", "#EA4444" },
        { "Branco", "#FFFFFF" },
        { "Marrom", "#A64A25" },
    };

    private Color NomeParaCor(string nome)
    {
        if (corHexadecimal.TryGetValue(nome, out string hex) &&
            ColorUtility.TryParseHtmlString(hex, out Color cor))
            return cor;
        return Color.yellow;
    }
}