using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Controla o HUD da partida: energia da cidade, fase/onda, munição
// selecionada, pontuação/combo, efeitos ativos, avisos de eventos e
// power-ups, mira, painel do chefão, transição de fase e telas de fim.
// Só lê o estado do jogo e atualiza o visual; quem decide vitória/derrota
// é o GerenciadorDeJogo.
public class UIManager : MonoBehaviour
{
    [Header("Energia da cidade")]
    [SerializeField] private Image _barraDeEnergia;
    [SerializeField] private Image _barraDeEnergiaAtraso;
    [SerializeField] private TextMeshProUGUI _textoEnergia;
    [Tooltip("Opcional: \"3 de 4 cidadãos saudáveis\".")]
    [SerializeField] private TextMeshProUGUI _textoCidadaos;
    [SerializeField] private Image _vinhetaDeDano;
    [SerializeField] private Color _corEnergiaCheia = new Color(0.3f, 0.85f, 0.45f);
    [SerializeField] private Color _corEnergiaBaixa = new Color(0.95f, 0.3f, 0.3f);
    [SerializeField] private float _intensidadeVinheta = 0.35f;

    [Header("Munição")]
    [SerializeField] private TextMeshProUGUI _textoMunicao;
    [Tooltip("Um slot por munição, na mesma ordem do enum TipoMunicao.")]
    [SerializeField] private RectTransform[] _slotsMunicao;
    [SerializeField] private float _escalaSlotSelecionado = 1.15f;
    [Tooltip("Ícones das munições, na mesma ordem do enum TipoMunicao (painel do chefão).")]
    [SerializeField] private Sprite[] _iconesDeMunicao;
    [Tooltip("Opcional: texto abaixo da mira dizendo qual munição usar no alvo.")]
    [SerializeField] private TextMeshProUGUI _textoDicaDaMira;

    [Header("Pontuação")]
    [SerializeField] private TextMeshProUGUI _textoPontos;
    [SerializeField] private TextMeshProUGUI _textoCombo;
    [SerializeField] private Image _barraDoCombo;
    [Tooltip("Opcional: \"próxima melhoria em 4.500\".")]
    [SerializeField] private TextMeshProUGUI _textoProximaMelhoria;

    [Header("Especial")]
    [SerializeField] private Image _barraEspecial;
    [SerializeField] private TextMeshProUGUI _textoEspecial;
    [SerializeField] private Color _corEspecialCarregando = new Color(0.4f, 1f, 0.5f);
    [SerializeField] private Color _corEspecialCheio = new Color(1f, 0.82f, 0.25f);

    [Header("Efeitos e avisos")]
    [SerializeField] private TextMeshProUGUI _textoEfeitos;
    [Tooltip("Opcional: fundo da lista de efeitos, escondido quando não há efeito ativo.")]
    [SerializeField] private GameObject _painelEfeitos;
    [SerializeField] private MovimentoJogador _jogador;
    [SerializeField] private GameObject _painelAviso;
    [SerializeField] private TextMeshProUGUI _textoAviso;
    [SerializeField] private float _duracaoAviso = 3.5f;

    [Header("Progresso")]
    [SerializeField] private TextMeshProUGUI _textoOnda;
    [SerializeField] private GeradorDeInimigos _geradorDeInimigos;

    [Header("Telas")]
    [SerializeField] private GameObject _hudDuranteJogo;
    [SerializeField] private GameObject _telaGameOver;
    [SerializeField] private GameObject _telaVitoria;
    [SerializeField] private TextMeshProUGUI _textoResultadoGameOver;
    [SerializeField] private TextMeshProUGUI _textoResultadoVitoria;
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private WeaponController _arma;
    [SerializeField] private string _nomeCenaDoMenu = "MainMenu";
    [SerializeField] private string _nomePrimeiraCena = "SampleScene";

    [Header("HUD do Chefão")]
    [SerializeField] private GameObject _painelChefao;
    [SerializeField] private Image _barraDeVidaChefao;
    [SerializeField] private TextMeshProUGUI _textoNomeChefao;
    [SerializeField] private TextMeshProUGUI _textoVidaChefao;
    [SerializeField] private TextMeshProUGUI _textoFraquezaChefao;
    [SerializeField] private Image _indicadorFraquezaChefao;

    [Header("Transição de Fase")]
    [SerializeField] private GameObject _painelTransicaoFase;
    [SerializeField] private TextMeshProUGUI _textoTransicaoFase;
    [SerializeField] private float _duracaoTransicaoFase = 4f;

    private int _numeroDaFase = 1;
    private string _nomeDaFase = "";
    private TipoMunicao _municaoAtual = TipoMunicao.Agua;
    private float _alphaVinheta;
    private CanvasGroup[] _gruposDosSlots;
    private float _escalaDosPontos = 1f;
    private float _proximaAtualizacaoDeEfeitos;
    private Image _imagemDaMira;

    private void Awake()
    {
        _imagemDaMira = _crosshair.GetComponent<Image>();
        _gruposDosSlots = new CanvasGroup[_slotsMunicao.Length];
        for (int i = 0; i < _slotsMunicao.Length; i++)
        {
            CanvasGroup grupo = _slotsMunicao[i].GetComponent<CanvasGroup>();
            _gruposDosSlots[i] = grupo != null ? grupo : _slotsMunicao[i].gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        GerenciadorDeJogo.Instancia.AoPerderJogo += MostrarTelaGameOver;
        GerenciadorDeJogo.Instancia.AoVencerJogo += MostrarTelaVitoria;
        GerenciadorDeJogo.Instancia.AoReceberDano += ReagirAoDano;
        _geradorDeInimigos.AoMudarDeOnda += AtualizarTextoOnda;
        _geradorDeInimigos.AoSpawnarChefao += ConfigurarUIChefao;
        _geradorDeInimigos.AoIniciarFase += MostrarTransicaoDeFase;
        _geradorDeInimigos.AoIniciarEvento += MostrarEvento;
        _geradorDeInimigos.AoTerminarEvento += MostrarResultadoDeEvento;
        GerenciadorDePontuacao.Instancia.AoGanharPontos += DestacarPontos;
        GerenciadorDePowerUps.Instancia.AoColetarPowerUp += MostrarColeta;
        GerenciadorDeCidadaos.Instancia.AoDarBonusDeOnda += MostrarBonusDeOnda;
        GerenciadorDeCidadaos.Instancia.AoSequestrarCidadao += MostrarSequestro;
        GerenciadorDeCidadaos.Instancia.AoPerderCidadao += MostrarPerdaDeCidadao;
        if (GerenciadorDeMelhorias.Instancia != null) GerenciadorDeMelhorias.Instancia.AoEscolherMelhoria += MostrarMelhoriaEscolhida;
        if (GerenciadorDeEspecial.Instancia != null)
        {
            GerenciadorDeEspecial.Instancia.AoEncher += MostrarEspecialPronto;
            GerenciadorDeEspecial.Instancia.AoUsarEspecial += MostrarEspecialUsado;
        }

        _painelChefao.SetActive(false);
        _telaGameOver.SetActive(false);
        _telaVitoria.SetActive(false);
        EsconderTransicaoDeFase();
        EsconderAviso();

        _barraDeEnergiaAtraso.fillAmount = FracaoDeEnergia();
    }

    private void Update()
    {
        AtualizarBarraDeEnergia();
        AnimarSlotsDeMunicao();
        AnimarVinheta();
        AtualizarPontuacao();
        AtualizarEfeitosAtivos();
        AtualizarMira();
        AtualizarBarraEspecial();
    }

    // Cheia: pisca entre amarelo e branco e mostra a tecla.
    private void AtualizarBarraEspecial()
    {
        GerenciadorDeEspecial especial = GerenciadorDeEspecial.Instancia;
        if (_barraEspecial == null || especial == null) return;

        _barraEspecial.fillAmount = Mathf.MoveTowards(_barraEspecial.fillAmount, especial.Fracao, 2f * Time.unscaledDeltaTime);
        float pulso = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 8f);
        _barraEspecial.color = especial.EstaCheio ? Color.Lerp(_corEspecialCheio, Color.white, pulso * 0.5f) : _corEspecialCarregando;

        if (_textoEspecial == null) return;
        _textoEspecial.color = especial.EstaCheio ? new Color(0.05f, 0.08f, 0.12f) : Color.white;
        _textoEspecial.text = especial.EstaCheio
            ? "ESPECIAL PRONTO!  APERTE Q"
            : $"ESPECIAL  {Mathf.FloorToInt(especial.Fracao * 100f)}%";
    }

    private void MostrarEspecialPronto()
    {
        MostrarAviso("ESPECIAL PRONTO!", "Aperte Q (ou o botão direito) para soltar a Onda Saudável", _corEspecialCheio, 2.5f);
    }

    private void MostrarEspecialUsado()
    {
        MostrarAviso("ONDA SAUDÁVEL!", "Inimigos em volta derrotados e cidadãos curados", _corEspecialCarregando, 2.5f);
    }

    // A mira toma a cor da fraqueza do inimigo apontado e cresce um pouco.
    private void AtualizarMira()
    {
        if (_arma == null || _imagemDaMira == null) return;

        IAlvoDaMira alvo = _arma.AlvoNaMira;
        // (alvo as Object) != null detecta objetos já destruídos pela Unity.
        bool temAlvo = alvo != null && (alvo as Object) != null && alvo.IndicaMunicao;
        Color corAlvo = temAlvo ? CoresDeMunicao.Obter(alvo.MunicaoIndicada) : new Color(1f, 1f, 1f, 0.9f);
        float suavizacao = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);

        _imagemDaMira.color = Color.Lerp(_imagemDaMira.color, corAlvo, suavizacao);
        _crosshair.transform.localScale = Vector3.Lerp(_crosshair.transform.localScale, Vector3.one * (temAlvo ? 1.35f : 1f), suavizacao);

        if (_textoDicaDaMira == null) return;
        _textoDicaDaMira.text = temAlvo ? $"{(int)alvo.MunicaoIndicada + 1}  {CoresDeMunicao.Nome(alvo.MunicaoIndicada).ToUpper()}" : "";
        _textoDicaDaMira.color = corAlvo;
    }

    // Usado pelo TutorialInicial para mostrar dicas na faixa de avisos.
    public void MostrarDica(string titulo, string detalhe, float duracao)
    {
        MostrarAviso(titulo, detalhe, new Color(1f, 0.82f, 0.25f), duracao);
    }

    private void AtualizarPontuacao()
    {
        GerenciadorDePontuacao pontuacao = GerenciadorDePontuacao.Instancia;
        _textoPontos.text = pontuacao.Pontos.ToString("N0");

        _escalaDosPontos = Mathf.Lerp(_escalaDosPontos, 1f, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
        _textoPontos.transform.localScale = Vector3.one * _escalaDosPontos;

        bool temCombo = pontuacao.Combo >= 2;
        _textoCombo.text = temCombo
            ? $"COMBO <color=#{CorDoMultiplicador(pontuacao.Multiplicador)}>x{pontuacao.Multiplicador}</color>"
            : "";
        _barraDoCombo.fillAmount = temCombo ? pontuacao.FracaoDoTempoDeCombo : 0f;

        if (_textoProximaMelhoria == null) return;
        GerenciadorDeMelhorias melhorias = GerenciadorDeMelhorias.Instancia;
        bool mostrar = melhorias != null && melhorias.TemMelhoriaDisponivel;
        _textoProximaMelhoria.text = mostrar ? $"PRÓXIMA MELHORIA  {melhorias.PontosDaProximaOferta:N0}" : "";
    }

    private string CorDoMultiplicador(int multiplicador)
    {
        switch (multiplicador)
        {
            case 1: return "FFFFFF";
            case 2: return "6EE07F";
            case 3: return "FFD240";
            case 4: return "FF9E33";
            default: return "FF5A5A";
        }
    }

    private void DestacarPontos(int pontosGanhos)
    {
        _escalaDosPontos = 1.35f;
    }

    // Texto montado só 10x por segundo para não gerar lixo de memória a cada frame.
    private void AtualizarEfeitosAtivos()
    {
        if (Time.unscaledTime < _proximaAtualizacaoDeEfeitos) return;
        _proximaAtualizacaoDeEfeitos = Time.unscaledTime + 0.1f;

        GerenciadorDePowerUps powerUps = GerenciadorDePowerUps.Instancia;
        string texto = "";
        if (powerUps.TiroRapidoAtivo) texto += $"<color=#FF9E33>SUCO TURBO  {powerUps.TempoRestanteTiroRapido:0}s</color>\n";
        if (powerUps.MunicaoCoringaAtiva) texto += $"<color=#BFF259>ABACATE CORINGA  {powerUps.TempoRestanteMunicaoCoringa:0}s</color>\n";
        if (_jogador != null && _jogador.EstaLento) texto += "<color=#9FA8DA>VOCÊ ESTÁ LENTO</color>\n";
        texto = texto.TrimEnd('\n');
        _textoEfeitos.text = texto;
        if (_painelEfeitos != null) _painelEfeitos.SetActive(texto.Length > 0);

        AtualizarTextoCidadaos();
    }

    private void AtualizarTextoCidadaos()
    {
        if (_textoCidadaos == null) return;

        int saudaveis = 0;
        var vivos = GerenciadorDeCidadaos.Instancia.CidadaosVivos;
        foreach (Cidadao cidadao in vivos)
        {
            if (!cidadao.PrecisaDeAjuda) saudaveis++;
        }
        _textoCidadaos.text = $"{saudaveis} de {vivos.Count} cidadãos saudáveis";
    }

    private void MostrarBonusDeOnda(int saudaveis, int pontos)
    {
        string plural = saudaveis == 1 ? "CIDADÃO SAUDÁVEL" : "CIDADÃOS SAUDÁVEIS";
        MostrarAviso($"{saudaveis} {plural}!", $"+{pontos} pontos de bônus", new Color(0.4f, 1f, 0.5f));
    }

    private void MostrarSequestro(Cidadao cidadao)
    {
        MostrarAviso("SOCORRO!", "Um hambúrguer está levando um cidadão. Derrube-o!", new Color(1f, 0.82f, 0.25f));
    }

    private void MostrarPerdaDeCidadao(Cidadao cidadao)
    {
        MostrarAviso("UM CIDADÃO FOI LEVADO!", "Proteja os que restaram.", new Color(1f, 0.4f, 0.4f));
    }

    private void MostrarMelhoriaEscolhida(Melhoria melhoria, int nivel)
    {
        MostrarAviso($"{melhoria.Nome}  NÍVEL {nivel}", melhoria.Descricao(nivel), melhoria.Cor);
    }

    private void MostrarColeta(PowerUp powerUp)
    {
        MostrarAviso($"+ {powerUp.Nome}", powerUp.Descricao, powerUp.Cor);
    }

    private void MostrarEvento(EventoDeOnda evento)
    {
        MostrarAviso($"EVENTO: {evento.Nome}", evento.Descricao, evento.Cor);
    }

    private void MostrarResultadoDeEvento(string mensagem, Color cor)
    {
        MostrarAviso(mensagem, "", cor);
    }

    private void MostrarAviso(string titulo, string detalhe, Color cor, float duracao = -1f)
    {
        string corHex = ColorUtility.ToHtmlStringRGB(cor);
        _textoAviso.text = string.IsNullOrEmpty(detalhe)
            ? $"<color=#{corHex}>{titulo}</color>"
            : $"<color=#{corHex}>{titulo}</color>\n<size=65%>{detalhe}</size>";

        _painelAviso.SetActive(true);
        CancelInvoke(nameof(EsconderAviso));
        Invoke(nameof(EsconderAviso), duracao > 0f ? duracao : _duracaoAviso);
    }

    private void EsconderAviso()
    {
        _painelAviso.SetActive(false);
    }

    private float FracaoDeEnergia()
    {
        GerenciadorDeJogo jogo = GerenciadorDeJogo.Instancia;
        return jogo.EnergiaMaxima > 0 ? (float)jogo.EnergiaAtual / jogo.EnergiaMaxima : 1f;
    }

    private void AtualizarBarraDeEnergia()
    {
        float fracao = FracaoDeEnergia();
        _barraDeEnergia.fillAmount = fracao;
        _barraDeEnergia.color = Color.Lerp(_corEnergiaBaixa, _corEnergiaCheia, fracao);
        // Percentual: a soma bruta da saúde dos cidadãos (ex: 240/240) não diz nada ao jogador.
        _textoEnergia.text = $"{Mathf.RoundToInt(fracao * 100f)}%";

        // Barra "rastro": desce devagar até o valor real, mostrando o dano levado.
        if (_barraDeEnergiaAtraso.fillAmount < fracao) _barraDeEnergiaAtraso.fillAmount = fracao;
        _barraDeEnergiaAtraso.fillAmount = Mathf.MoveTowards(_barraDeEnergiaAtraso.fillAmount, fracao, 0.35f * Time.unscaledDeltaTime);
    }

    private void ReagirAoDano()
    {
        _alphaVinheta = _intensidadeVinheta;
    }

    private void AnimarVinheta()
    {
        _alphaVinheta = Mathf.MoveTowards(_alphaVinheta, 0f, 0.9f * Time.unscaledDeltaTime);
        Color cor = _vinhetaDeDano.color;
        cor.a = _alphaVinheta;
        _vinhetaDeDano.color = cor;
    }

    public void AtualizarTextoMunicao(TipoMunicao municaoAtual)
    {
        _municaoAtual = municaoAtual;
        _textoMunicao.text = CoresDeMunicao.Nome(municaoAtual).ToUpper();
        _textoMunicao.color = CoresDeMunicao.Obter(municaoAtual);
    }

    private void AnimarSlotsDeMunicao()
    {
        float suavizacao = 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime);

        for (int i = 0; i < _slotsMunicao.Length; i++)
        {
            bool selecionado = i == (int)_municaoAtual;
            float escalaAlvo = selecionado ? _escalaSlotSelecionado : 1f;

            _slotsMunicao[i].localScale = Vector3.Lerp(_slotsMunicao[i].localScale, Vector3.one * escalaAlvo, suavizacao);
            _gruposDosSlots[i].alpha = Mathf.Lerp(_gruposDosSlots[i].alpha, selecionado ? 1f : 0.45f, suavizacao);
        }
    }

    private void AtualizarTextoOnda(int numeroDaOnda, int totalDeOndas)
    {
        _textoOnda.text = $"ONDA {numeroDaOnda} DE {totalDeOndas}\n<size=58%><color=#FFFFFFB0>{DescricaoDaFase()}</color></size>";
    }

    private void MostrarTransicaoDeFase(int numeroDaFase, ConfiguracaoDeFase fase)
    {
        _numeroDaFase = numeroDaFase;
        _nomeDaFase = fase.Nome;
        _painelChefao.SetActive(false);
        _textoOnda.text = $"PREPARE-SE\n<size=58%><color=#FFFFFFB0>{DescricaoDaFase()}</color></size>";

        _textoTransicaoFase.text =
            $"<size=55%><color=#FFD240>FASE {numeroDaFase}</color></size>\n" +
            $"{fase.Nome.ToUpper()}\n" +
            $"<size=42%><color=#FFFFFFCC>{fase.DicaSaudavel}</color></size>";
        _painelTransicaoFase.SetActive(true);
        CancelInvoke(nameof(EsconderTransicaoDeFase));
        Invoke(nameof(EsconderTransicaoDeFase), _duracaoTransicaoFase);
    }

    private string DescricaoDaFase()
    {
        return $"FASE {_numeroDaFase} DE {_geradorDeInimigos.TotalDeFases} · {_nomeDaFase.ToUpper()}";
    }

    private void EsconderTransicaoDeFase()
    {
        _painelTransicaoFase.SetActive(false);
    }

    private void MostrarTelaGameOver()
    {
        FinalizarPartida(_telaGameOver);
    }

    private void MostrarTelaVitoria()
    {
        FinalizarPartida(_telaVitoria);
    }

    private void FinalizarPartida(GameObject telaParaMostrar)
    {
        _painelChefao.SetActive(false);
        _hudDuranteJogo.SetActive(false);
        _crosshair.SetActive(false);
        EsconderTransicaoDeFase();
        EsconderAviso();

        string resultado = MontarTextoDeResultado();
        _textoResultadoGameOver.text = resultado;
        _textoResultadoVitoria.text = resultado;
        telaParaMostrar.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    private string MontarTextoDeResultado()
    {
        GerenciadorDePontuacao pontuacao = GerenciadorDePontuacao.Instancia;
        string recorde = pontuacao.BateuRecorde
            ? "<color=#FFD240>NOVO RECORDE!</color>"
            : $"RECORDE  {GerenciadorDePontuacao.Recorde:N0}";
        return $"PONTOS  {pontuacao.Pontos:N0}\n<size=70%>{recorde}</size>";
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReiniciarCampanha()
    {
        Time.timeScale = 1f;
        ProgressoDaCampanha.Reiniciar();
        SceneManager.LoadScene(_nomePrimeiraCena);
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_nomeCenaDoMenu);
    }

    private void ConfigurarUIChefao(ChefaoDeCaloria chefao)
    {
        _painelChefao.SetActive(true);
        if (_textoNomeChefao != null)
        {
            _textoNomeChefao.text = chefao.Nome.ToUpper();
            _textoNomeChefao.color = chefao.Cor;
        }
        MostrarAviso($"{chefao.Nome.ToUpper()} CHEGOU!", chefao.Descricao, chefao.Cor, 4f);
        AtualizarVidaChefao(chefao.VidaAtual, chefao.VidaMaxima);
        AtualizarFraquezaChefao(chefao.Fraqueza);

        chefao.AoMudarVida += AtualizarVidaChefao;
        chefao.AoMudarFraqueza += AtualizarFraquezaChefao;
    }

    private void AtualizarVidaChefao(int vidaAtual, int vidaMaxima)
    {
        _barraDeVidaChefao.fillAmount = (float)vidaAtual / vidaMaxima;
        if (_textoVidaChefao != null) _textoVidaChefao.text = $"{vidaAtual} / {vidaMaxima}";
    }

    private void AtualizarFraquezaChefao(TipoMunicao fraqueza)
    {
        Color cor = CoresDeMunicao.Obter(fraqueza);
        string corHex = ColorUtility.ToHtmlStringRGB(cor);
        _textoFraquezaChefao.text = $"FRACO CONTRA  <color=#{corHex}>{(int)fraqueza + 1} · {CoresDeMunicao.Nome(fraqueza).ToUpper()}</color>";

        // Com ícone: mostra a própria munição; sem ícone: bolinha na cor dela.
        bool temIcone = _iconesDeMunicao != null && (int)fraqueza < _iconesDeMunicao.Length && _iconesDeMunicao[(int)fraqueza] != null;
        if (temIcone) _indicadorFraquezaChefao.sprite = _iconesDeMunicao[(int)fraqueza];
        _indicadorFraquezaChefao.color = temIcone ? Color.white : cor;
    }
}
