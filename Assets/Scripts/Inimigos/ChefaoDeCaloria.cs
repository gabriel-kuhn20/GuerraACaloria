using UnityEngine;
using System;
using System.Collections.Generic;

// Chefão do final de cada fase: usa os três tipos de munição, alternando
// qual é a fraqueza ativa a cada intervalo. Tem várias vidas (barra
// própria) em vez de morrer com 1 acerto certo como os inimigos comuns.
// Fica mais perigoso conforme perde vida (fúria): troca de fraqueza e
// anda mais rápido. Cada fase liga habilidades diferentes (investida,
// lacaios, escudo) e os tempos variam um pouco a cada partida.
// Munição errada cura o chefão — atirar sem pensar não funciona.
// Cada fase pode trocar o modelo 3D e os atributos via ConfiguracaoDeChefao.
// Com o modelo padrão toca "Hop"/"Death"; com modelo trocado, gira,
// quica e encolhe ao ser derrotado.
// Ao alcançar um cidadão, causa dano e recua em vez de sumir.
public class ChefaoDeCaloria : Inimigo
{
    [SerializeField] private int _vidaMaxima = 30;
    [SerializeField] private float _intervaloTrocaDeFraqueza = 4f;
    [SerializeField] private float _escalaVisual = 0.1f;
    [SerializeField] private Animator _animator;
    [SerializeField] private string _nomeTriggerMorte = "Morrer";
    [SerializeField] private float _duracaoAnimacaoMorte = 2f;
    [SerializeField] private float _distanciaDeRecuo = 12f;
    [SerializeField] private string _nome = "CHEFÃO DE CALORIA";
    [SerializeField] private int _danoAoAlcancar = 30;
    [SerializeField] private TipoMunicao[] _fraquezasPossiveis = { TipoMunicao.Agua, TipoMunicao.Fruta, TipoMunicao.EnergiaPositiva };

    [Header("Dificuldade")]
    [SerializeField] private float _velocidadeBase = 5f;
    [Tooltip("Vida curada a cada tiro com a munição errada.")]
    [SerializeField] private int _curaPorErro = 1;
    [Tooltip("Por estágio de fúria (0 = calmo, 1 = abaixo de 66%, 2 = abaixo de 33%).")]
    [SerializeField] private float[] _multiplicadorDeVelocidadePorEstagio = { 1f, 1.35f, 1.7f };
    [SerializeField] private float[] _intervaloDeTrocaPorEstagio = { 4f, 2.8f, 1.8f };
    [SerializeField] private float _invulnerabilidadeAoEnfurecer = 1.5f;
    [Tooltip("Fração da vida máxima que o especial do jogador tira.")]
    [SerializeField, Range(0f, 1f)] private float _fracaoDeDanoDoEspecial = 0.2f;
    [Tooltip("Variação aleatória (±) aplicada aos intervalos de cada chefão.")]
    [SerializeField, Range(0f, 0.5f)] private float _variacaoDosTempos = 0.2f;

    [Header("Investida")]
    [SerializeField] private float _intervaloDeInvestida = 7f;
    [SerializeField] private float _duracaoDaInvestida = 1.2f;
    [SerializeField] private float _multiplicadorDaInvestida = 3f;

    [Header("Lacaios (prefabs vêm da fase, via GeradorDeInimigos)")]
    [SerializeField] private float _intervaloDeLacaios = 9f;
    [SerializeField] private int[] _lacaiosPorEstagio = { 1, 2, 3 };
    [SerializeField] private int _maximoDeLacaiosVivos = 6;

    [Header("Arremesso no Agente")]
    [SerializeField] private ProjetilDoChefao _prefabDoArremesso;
    [SerializeField] private float _intervaloDeArremesso = 5f;
    [SerializeField] private int[] _arremessosPorEstagio = { 1, 2, 3 };
    [SerializeField] private float _aberturaDoLeque = 14f;

    [Header("Escudo")]
    [SerializeField] private float _intervaloDeEscudo = 10f;
    [SerializeField] private float _duracaoDoEscudo = 2.5f;

    private TipoMunicao _fraquezaAtual;
    private float _proximaTroca;
    private bool _estaMorrendo = false;
    private int _estagioDeFuria = 0;
    private float _fimDaInvulnerabilidade;
    private float _proximaInvestida;
    private float _fimDaInvestida;
    private float _proximoLacaio;
    private float _proximoEscudo;
    private float _proximoArremesso;
    private HabilidadesDoChefao _habilidades = HabilidadesDoChefao.Investida;
    private GameObject[] _prefabsDeLacaios;
    private readonly List<Inimigo> _lacaiosVivos = new List<Inimigo>();
    private Transform _modeloTrocado;
    private Vector3 _escalaDoModelo;
    private float _alturaBaseDoModelo;
    private float _giroDoModelo;
    private string _descricao = "";
    private Color _cor = new Color(1f, 0.5f, 0.45f);

    public override TipoMunicao Fraqueza => _fraquezaAtual;
    protected override float Velocidade => _velocidadeBase * ValorDoEstagio(_multiplicadorDeVelocidadePorEstagio, 1f)
        * (EstaInvestindo ? _multiplicadorDaInvestida : 1f);
    protected override int DanoAoAlcancarAlvo => _danoAoAlcancar;
    protected override float AmplitudeOscilacao => 0.3f;
    protected override int PontosAoDerrotar => 1000;
    // O chefão não troca de alvo (mira nos cidadãos), mas encostar nele dói bem mais
    // e ele ataca o agente à distância com arremessos.
    protected override float ChanceDeSerAgressivo => 0f;
    protected override int DanoNoAgente => 20;
    protected override float RaioDeContatoComAgente => 2.6f;
    protected override float ForcaDoEmpurraoNoAgente => 2.5f;
    // O chefão não recua ao encostar: as investidas dele continuam valendo.
    protected override float DuracaoDoRecuoAposAtacar => 0f;

    private bool EstaInvestindo => Time.time < _fimDaInvestida;
    private bool EstaInvulneravel => Time.time < _fimDaInvulnerabilidade;

    public string Nome => _nome;
    public string Descricao => _descricao;
    public Color Cor => _cor;
    public int VidaAtual => _vida;
    public int VidaMaxima => _vidaMaxima;
    public event Action<int, int> AoMudarVida;
    public event Action<TipoMunicao> AoMudarFraqueza;

    protected override void Awake()
    {
        base.Awake();
        transform.localScale = Vector3.one * _escalaVisual;
        _vida = _vidaMaxima;
    }

    // Chamado pelo GeradorDeInimigos logo após o Instantiate, para que
    // cada fase tenha um chefão com vida diferente.
    public void ConfigurarVida(int vidaMaxima)
    {
        _vidaMaxima = Mathf.Max(1, vidaMaxima);
        _vida = _vidaMaxima;
    }

    // Chamado pelo GeradorDeInimigos junto com ConfigurarVida: quais
    // habilidades este chefão usa e quais inimigos ele pode invocar.
    public void ConfigurarHabilidades(HabilidadesDoChefao habilidades, GameObject[] prefabsDeLacaios)
    {
        _habilidades = habilidades;
        _prefabsDeLacaios = prefabsDeLacaios;
    }

    // Configuração completa por fase: atributos, habilidades e aparência.
    // Chamado logo após o Instantiate (antes do Start).
    public void Configurar(ConfiguracaoDeChefao config, GameObject[] lacaiosDaFase)
    {
        _nome = config.Nome;
        _descricao = config.Descricao;
        _cor = config.Cor;
        ConfigurarVida(config.Vida);
        _velocidadeBase = config.Velocidade;
        _danoAoAlcancar = config.DanoAoAlcancar;
        _intervaloDeLacaios = config.IntervaloDeLacaios;

        // A fúria acelera a troca a partir do ritmo próprio deste chefão.
        float troca = Mathf.Max(0.5f, config.IntervaloDeTrocaDeFraqueza);
        _intervaloTrocaDeFraqueza = troca;
        _intervaloDeTrocaPorEstagio = new[] { troca, troca * 0.7f, troca * 0.45f };

        if (config.Fraquezas != null && config.Fraquezas.Length > 0) _fraquezasPossiveis = config.Fraquezas;

        bool temLacaiosProprios = config.Lacaios != null && config.Lacaios.Length > 0;
        ConfigurarHabilidades(config.Habilidades, temLacaiosProprios ? config.Lacaios : lacaiosDaFase);

        if (config.Modelo != null) TrocarModelo(config.Modelo, config.Altura, config.GiroDoModelo);
    }

    // Esconde o visual padrão e coloca o modelo da fase com a altura pedida,
    // apoiado no chão; o colisor (trigger) passa a cobrir o novo modelo.
    private void TrocarModelo(GameObject prefabDoModelo, float altura, float giro)
    {
        foreach (Renderer antigo in GetComponentsInChildren<Renderer>()) antigo.enabled = false;
        _animator = null;

        GameObject modelo = Instantiate(prefabDoModelo, transform);
        modelo.transform.localPosition = Vector3.zero;
        foreach (Collider colisor in modelo.GetComponentsInChildren<Collider>()) Destroy(colisor);

        Renderer[] renderers = modelo.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds limites = CalcularLimites(renderers);
        if (limites.size.y > 0.0001f) modelo.transform.localScale *= altura / limites.size.y;

        // Base do modelo ~0,6 abaixo do pivô do chefão = nível da calçada/asfalto.
        limites = CalcularLimites(renderers);
        Vector3 ajuste = new Vector3(transform.position.x - limites.center.x, transform.position.y - 0.6f - limites.min.y, transform.position.z - limites.center.z);
        modelo.transform.position += ajuste;

        SphereCollider colisorDoChefao = GetComponent<SphereCollider>();
        if (colisorDoChefao != null)
        {
            limites = CalcularLimites(renderers);
            float escala = Mathf.Max(0.0001f, transform.lossyScale.x);
            colisorDoChefao.center = transform.InverseTransformPoint(limites.center);
            colisorDoChefao.radius = Mathf.Max(limites.extents.x, limites.extents.y, limites.extents.z) / escala;
        }

        _modeloTrocado = modelo.transform;
        _escalaDoModelo = modelo.transform.localScale;
        _alturaBaseDoModelo = modelo.transform.localPosition.y;
        _giroDoModelo = giro;
    }

    private static Bounds CalcularLimites(Renderer[] renderers)
    {
        Bounds limites = renderers[0].bounds;
        foreach (Renderer r in renderers) limites.Encapsulate(r.bounds);
        return limites;
    }

    // Sem animação própria: gira devagar e dá pulinhos; ao morrer, encolhe.
    private void AnimarModeloTrocado()
    {
        if (_modeloTrocado == null) return;

        if (_estaMorrendo)
        {
            _modeloTrocado.localScale = Vector3.MoveTowards(_modeloTrocado.localScale, Vector3.zero,
                _escalaDoModelo.magnitude / Mathf.Max(0.1f, _duracaoAnimacaoMorte) * Time.deltaTime);
            return;
        }

        _modeloTrocado.Rotate(Vector3.up, _giroDoModelo * Time.deltaTime, Space.World);
        float pulo = Mathf.Abs(Mathf.Sin(Time.time * (EstaInvestindo ? 9f : 4f))) * 0.12f / Mathf.Max(0.0001f, transform.lossyScale.y);
        Vector3 posicao = _modeloTrocado.localPosition;
        posicao.y = _alturaBaseDoModelo + pulo;
        _modeloTrocado.localPosition = posicao;
    }

    private bool Tem(HabilidadesDoChefao habilidade) => (_habilidades & habilidade) != 0;

    private float Variar(float tempo)
    {
        return tempo * UnityEngine.Random.Range(1f - _variacaoDosTempos, 1f + _variacaoDosTempos);
    }

    protected override void Start()
    {
        base.Start();
        SortearNovaFraqueza();
        _proximaTroca = Time.time + Variar(_intervaloTrocaDeFraqueza);
        _proximaInvestida = Time.time + Variar(_intervaloDeInvestida);
        _proximoLacaio = Time.time + Variar(_intervaloDeLacaios * 0.5f);
        _proximoEscudo = Time.time + Variar(_intervaloDeEscudo);
        _proximoArremesso = Time.time + Variar(_intervaloDeArremesso * 0.6f);
    }

    protected override void Update()
    {
        AnimarModeloTrocado();
        if (_estaMorrendo) return;

        base.Update();
        AtualizarTrocaDeFraqueza();
        if (Tem(HabilidadesDoChefao.Investida)) AtualizarInvestida();
        if (Tem(HabilidadesDoChefao.Lacaios)) AtualizarLacaios();
        if (Tem(HabilidadesDoChefao.Escudo)) AtualizarEscudo();
        AtualizarArremesso();
    }

    private void AtualizarTrocaDeFraqueza()
    {
        if (Time.time < _proximaTroca) return;

        SortearNovaFraqueza();
        _proximaTroca = Time.time + Variar(ValorDoEstagio(_intervaloDeTrocaPorEstagio, _intervaloTrocaDeFraqueza));
    }

    // Nunca repete a fraqueza atual — senão o jogador às vezes nem
    // precisaria trocar de munição.
    private void SortearNovaFraqueza()
    {
        TipoMunicao[] opcoes = _fraquezasPossiveis;
        TipoMunicao nova = opcoes[UnityEngine.Random.Range(0, opcoes.Length)];
        if (nova == _fraquezaAtual) nova = opcoes[(Array.IndexOf(opcoes, nova) + UnityEngine.Random.Range(1, opcoes.Length)) % opcoes.Length];

        _fraquezaAtual = nova;
        AoMudarFraqueza?.Invoke(_fraquezaAtual);
    }

    private void AtualizarInvestida()
    {
        if (Time.time < _proximaInvestida) return;

        _fimDaInvestida = Time.time + _duracaoDaInvestida;
        _proximaInvestida = _fimDaInvestida + Variar(_intervaloDeInvestida / ValorDoEstagio(_multiplicadorDeVelocidadePorEstagio, 1f));
        TextoFlutuante.Criar(transform.position + Vector3.up * 3f, "INVESTIDA!", new Color(1f, 0.4f, 0.3f));
        GerenciadorDeAudio.Instancia.TocarSomChefaoInvestida();
    }

    private void AtualizarLacaios()
    {
        if (Time.time < _proximoLacaio) return;
        _proximoLacaio = Time.time + Variar(_intervaloDeLacaios);

        if (_prefabsDeLacaios == null || _prefabsDeLacaios.Length == 0) return;

        _lacaiosVivos.RemoveAll(lacaio => lacaio == null);
        int quantidade = Mathf.Min(ValorDoEstagio(_lacaiosPorEstagio, 1), _maximoDeLacaiosVivos - _lacaiosVivos.Count);

        for (int i = 0; i < quantidade; i++) InvocarLacaio();
    }

    // Escudo periódico: o jogador precisa parar de atirar e cuidar dos
    // cidadãos/lacaios até ele baixar.
    private void AtualizarEscudo()
    {
        if (Time.time < _proximoEscudo) return;

        _fimDaInvulnerabilidade = Time.time + _duracaoDoEscudo;
        _proximoEscudo = _fimDaInvulnerabilidade + Variar(_intervaloDeEscudo);
        TextoFlutuante.Criar(transform.position + Vector3.up * 3f, "ESCUDO!", new Color(0.6f, 0.8f, 1f));
        GerenciadorDeAudio.Instancia.TocarSomChefaoEscudo();
    }

    // Arremesso: joga comida no agente (mais de uma, em leque, com a fúria).
    private void AtualizarArremesso()
    {
        if (_prefabDoArremesso == null || Time.time < _proximoArremesso) return;
        _proximoArremesso = Time.time + Variar(_intervaloDeArremesso / ValorDoEstagio(_multiplicadorDeVelocidadePorEstagio, 1f));

        VidaDoAgente agente = VidaDoAgente.Instancia;
        if (agente == null || agente.EstaNocauteado) return;

        int quantidade = Mathf.Max(1, ValorDoEstagio(_arremessosPorEstagio, 1));
        Vector3 origem = transform.position + Vector3.up * 2f;
        for (int i = 0; i < quantidade; i++)
        {
            float desvio = (i - (quantidade - 1) / 2f) * _aberturaDoLeque;
            ProjetilDoChefao arremesso = Instantiate(_prefabDoArremesso, origem, Quaternion.identity);
            arremesso.Lancar(agente.transform, desvio);
        }
        GerenciadorDeAudio.Instancia.TocarSomChefaoArremesso();
    }

    private void InvocarLacaio()
    {
        GameObject prefab = _prefabsDeLacaios[UnityEngine.Random.Range(0, _prefabsDeLacaios.Length)];
        Vector2 desvio = UnityEngine.Random.insideUnitCircle * 2.5f;
        Vector3 posicao = new Vector3(transform.position.x + desvio.x, 1f, transform.position.z + desvio.y);

        Inimigo lacaio = Instantiate(prefab, posicao, Quaternion.identity).GetComponent<Inimigo>();
        if (lacaio == null) return;

        _lacaiosVivos.Add(lacaio);
        GerenciadorDeAudio.Instancia.TocarSomLacaio();
        GerenciadorDeEfeitos.Instancia.CriarExplosao(posicao, CoresDeMunicao.Obter(lacaio.Fraqueza), 0.6f);
    }

    public override void ReceberDano(TipoMunicao municaoUsada)
    {
        if (_estaMorrendo) return;

        if (EstaInvulneravel)
        {
            GerenciadorDeAudio.Instancia.TocarSomErro();
            return;
        }

        if (EhFraqueza(municaoUsada))
        {
            _vida = Mathf.Max(0, _vida - 1 - GerenciadorDeMelhorias.Atributos.DanoExtraNoChefao);
            GerenciadorDeAudio.Instancia.TocarSomAcerto();
            GerenciadorDePontuacao.Instancia.RegistrarAcerto();
            GerenciadorDeEfeitos.Instancia.CriarExplosao(transform.position, CoresDeMunicao.Obter(municaoUsada), 0.5f);
        }
        else
        {
            _vida = Mathf.Min(_vidaMaxima, _vida + _curaPorErro);
            GerenciadorDeAudio.Instancia.TocarSomErro();
            GerenciadorDePontuacao.Instancia.RegistrarErro();
        }

        AoMudarVida?.Invoke(_vida, _vidaMaxima);

        if (_vida <= 0)
        {
            IniciarMorte();
            return;
        }

        AtualizarEstagioDeFuria();
    }

    // 66% e 33% da vida: fica mais rápido, troca de fraqueza mais rápido
    // e invoca mais lacaios. Breve invulnerabilidade marca a transição.
    private void AtualizarEstagioDeFuria()
    {
        float fracaoDeVida = (float)_vida / _vidaMaxima;
        int novoEstagio = fracaoDeVida <= 0.33f ? 2 : fracaoDeVida <= 0.66f ? 1 : 0;
        if (novoEstagio <= _estagioDeFuria) return;

        _estagioDeFuria = novoEstagio;
        _fimDaInvulnerabilidade = Time.time + _invulnerabilidadeAoEnfurecer;
        _proximoLacaio = Time.time + _invulnerabilidadeAoEnfurecer;
        SortearNovaFraqueza();
        _proximaTroca = Time.time + Variar(ValorDoEstagio(_intervaloDeTrocaPorEstagio, _intervaloTrocaDeFraqueza));

        GerenciadorDeAudio.Instancia.TocarSomChefaoFuria();
        TextoFlutuante.Criar(transform.position + Vector3.up * 3f, _estagioDeFuria == 2 ? "FÚRIA MÁXIMA!" : "FÚRIA!", new Color(1f, 0.25f, 0.2f));
    }

    protected override void AoAlcancarAlvo()
    {
        CidadaoAlvo.ReceberAtaque(DanoAoAlcancarAlvo, Fraqueza);
        GerenciadorDePontuacao.Instancia.RegistrarErro();

        Vector3 recuo = -DirecaoHorizontalAoAlvo() * _distanciaDeRecuo;
        transform.position = GerenciadorDeRua.Instancia.RestringirPosicao(transform.position + recuo);
        _fimDaInvestida = 0f;
    }

    // Especial: tira uma fração da vida máxima, ignorando fraqueza e escudo.
    public override void ReceberEspecial()
    {
        if (_estaMorrendo) return;

        int dano = Mathf.Max(1, Mathf.CeilToInt(_vidaMaxima * _fracaoDeDanoDoEspecial));
        _vida = Mathf.Max(0, _vida - dano);
        AoMudarVida?.Invoke(_vida, _vidaMaxima);
        GerenciadorDeEfeitos.Instancia.CriarExplosao(transform.position, new Color(0.4f, 1f, 0.5f), 2f);

        if (_vida <= 0) IniciarMorte();
        else AtualizarEstagioDeFuria();
    }

    private void IniciarMorte()
    {
        _estaMorrendo = true;
        ComemorarAbate(3f);
        DestruirLacaios();
        if (_animator != null) _animator.SetTrigger(_nomeTriggerMorte);
        Invoke(nameof(NotificarMorteEDestruir), _duracaoAnimacaoMorte);
    }

    // Lacaios não contam na onda; somem junto com o chefão para não
    // atacarem durante a transição de fase.
    private void DestruirLacaios()
    {
        foreach (Inimigo lacaio in _lacaiosVivos)
        {
            if (lacaio == null) continue;
            GerenciadorDeEfeitos.Instancia.CriarExplosao(lacaio.transform.position, CoresDeMunicao.Obter(lacaio.Fraqueza), 0.6f);
            Destroy(lacaio.gameObject);
        }
        _lacaiosVivos.Clear();
    }

    private T ValorDoEstagio<T>(T[] valores, T padrao)
    {
        if (valores == null || valores.Length == 0) return padrao;
        return valores[Mathf.Min(_estagioDeFuria, valores.Length - 1)];
    }
}
