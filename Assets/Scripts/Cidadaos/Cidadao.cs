using UnityEngine;

// Cidadão da praça: é quem o jogador protege. Tem saúde própria; quando um
// inimigo o alcança ele fica contaminado (mais lento, barrinha de saúde
// com a cor da munição que o cura) e, se a saúde zerar, desmaia e cai
// no chão. O jogador cura atirando a munição certa nele. Hambúrgueres
// podem sequestrá-lo. A saúde da cidade é a soma da saúde dos cidadãos.
// Comportamento por máquina de estados: passeia, para, foge, comemora e,
// passado o perigo, volta ao seu ponto de origem (onde nasceu na cena).
public class Cidadao : MonoBehaviour, IDanificavel, IAlvoDaMira
{
    private enum Estado { Passeando, Parado, Fugindo, Comemorando, Desmaiado, Carregado, Voltando }

    private static readonly int ParametroVelocidade = Animator.StringToHash("Velocidade");
    private static readonly int ParametroComemorar = Animator.StringToHash("Comemorar");
    private static readonly int ParametroDesmaiado = Animator.StringToHash("Desmaiado");

    [Header("Saúde")]
    [SerializeField] private int _saudeMaxima = 60;
    [SerializeField] private int _curaPorTiro = 20;
    [SerializeField, Range(0f, 1f)] private float _fracaoParaLevantar = 0.4f;
    [SerializeField] private float _fatorDeVelocidadeContaminado = 0.6f;
    [SerializeField] private int _pontosPorCura = 50;
    [SerializeField] private int _pontosPorResgate = 200;
    [SerializeField, Range(0f, 1f)] private float _chanceDePresenteAoCurar = 0.3f;

    [Header("Movimento")]
    [SerializeField] private float _velocidade = 1.5f;
    [SerializeField] private float _velocidadeDeFuga = 4f;
    [SerializeField] private float _aceleracao = 6f;
    [SerializeField] private float _velocidadeDeGiro = 4f;
    [SerializeField] private float _raioDeEspacoPessoal = 1.3f;

    [Header("Passeio")]
    [SerializeField] private float _raioDePatrulha = 6f;
    [SerializeField] private float _distanciaParaNovoDestino = 0.5f;
    [SerializeField] private float _pausaMinima = 1f;
    [SerializeField] private float _pausaMaxima = 3.5f;
    [SerializeField] private Transform _centroDaZonaDePatrulha;

    [Header("Volta ao ponto de origem")]
    [Tooltip("Longe mais que isso do ponto de origem (após fugir, levantar ou ser salvo) = volta para lá.")]
    [SerializeField] private float _distanciaParaVoltar = 3f;
    [SerializeField] private float _velocidadeDeVolta = 2.8f;
    [Tooltip("Ao passear, nunca escolhe destino mais longe que isso do ponto de origem.")]
    [SerializeField] private float _raioMaximoDePasseio = 4f;

    [Header("Medo e comemoração")]
    [SerializeField] private float _raioDeDeteccaoDeInimigo = 5f;
    [SerializeField] private float _tempoDePanicoAposFuga = 1.5f;
    [SerializeField, Range(0f, 1f)] private float _chanceDeComemorarAposFuga = 0.5f;
    [SerializeField] private float _duracaoDaComemoracao = 2.2f;
    [SerializeField] private float _intervaloDeVerificacaoDePerigo = 0.2f;

    [Header("VIP")]
    [SerializeField] private float _alturaDoMarcadorVip = 2.4f;
    [SerializeField] private Color _corDoMarcadorVip = new Color(1f, 0.82f, 0.2f);

    private Estado _estado = Estado.Parado;
    private Animator _animator;
    private bool _animacaoConfigurada;
    private bool _temAnimacaoDeDesmaio;
    private BarraDeSaudeDoCidadao _barra;
    private float _alturaDaCabeca = 2f;

    private int _saude;
    private bool _contaminado;
    private TipoMunicao _tipoDaContaminacao;

    private Vector3 _velocidadeAtual;
    private Vector3 _destinoAtual;
    private Vector3 _direcaoDoOlhar;
    private Vector3 _direcaoDeFuga;
    private float _fimDoEstado;
    private float _fimDoPanico;
    private float _proximaVerificacaoDePerigo;
    private float _raioIndividual;
    private float _velocidadeIndividual;
    private Transform _inimigoMaisProximo;
    private float _alturaFixa;
    private GameObject _marcadorVip;
    private Vector3 _pontoDeOrigem;

    public int Saude => _saude;
    public int SaudeMaxima => _saudeMaxima;
    public bool EstaContaminado => _contaminado;
    public bool EstaDesmaiado => _estado == Estado.Desmaiado;
    public bool EstaSendoCarregado => _estado == Estado.Carregado;
    public bool PodeSerAlvo => isActiveAndEnabled && !EstaDesmaiado && !EstaSendoCarregado;
    public bool PodeSerCarregado => PodeSerAlvo;
    public bool PrecisaDeAjuda => _contaminado || EstaDesmaiado || EstaSendoCarregado;

    public bool AceitaTiro => _contaminado && !EstaSendoCarregado;
    public bool IndicaMunicao => _contaminado;
    public TipoMunicao MunicaoIndicada => _tipoDaContaminacao;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _saude = _saudeMaxima;
        GarantirColisorEMedirAltura();
    }

    private void OnEnable()
    {
        GerenciadorDeCidadaos.Instancia.RegistrarCidadao(this);
    }

    private void OnDisable()
    {
        GerenciadorDeCidadaos.Instancia.RemoverCidadao(this);
    }

    private void Start()
    {
        // Verificado no Start (e não no Awake) porque o Animator só termina de
        // inicializar depois do Awake de todos os scripts do objeto.
        _animacaoConfigurada = _animator != null && TemParametro(_animator, ParametroVelocidade);
        _temAnimacaoDeDesmaio = _animator != null && TemParametro(_animator, ParametroDesmaiado);
        if (_animator != null && !_animacaoConfigurada)
        {
            Debug.LogWarning($"[Cidadao] {name}: o Animator não tem o parâmetro 'Velocidade'. " +
                             "Rode GuerraACaloria > Configurar Cidadãos.", this);
        }

        _alturaFixa = transform.position.y;
        _pontoDeOrigem = transform.position;
        _raioIndividual = _raioDePatrulha * Random.Range(0.7f, 1.3f);
        _velocidadeIndividual = _velocidade * Random.Range(0.85f, 1.15f);
        _direcaoDoOlhar = transform.forward;
        _barra = BarraDeSaudeDoCidadao.Criar(transform, _alturaDaCabeca);

        // Cada um começa num momento diferente, para não andarem todos sincronizados.
        EntrarEmParado(Random.Range(0f, 2f));
    }

    // Os modelos não têm colisor: cria um (trigger) do tamanho do corpo,
    // para o projétil e a mira conseguirem detectar o cidadão.
    private void GarantirColisorEMedirAltura()
    {
        Renderer[] renderizadores = GetComponentsInChildren<Renderer>();
        if (renderizadores.Length == 0) return;

        Bounds limites = renderizadores[0].bounds;
        foreach (Renderer r in renderizadores) limites.Encapsulate(r.bounds);
        _alturaDaCabeca = limites.max.y - transform.position.y + 0.35f;

        if (GetComponentInChildren<Collider>() != null) return;

        CapsuleCollider colisor = gameObject.AddComponent<CapsuleCollider>();
        colisor.isTrigger = true;
        colisor.center = transform.InverseTransformPoint(limites.center);
        colisor.height = limites.size.y / Mathf.Max(0.01f, transform.lossyScale.y);
        colisor.radius = 0.4f / Mathf.Max(0.01f, transform.lossyScale.x);
    }

    private static bool TemParametro(Animator animator, int hashDoParametro)
    {
        foreach (AnimatorControllerParameter parametro in animator.parameters)
        {
            if (parametro.nameHash == hashDoParametro) return true;
        }
        return false;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        AtualizarBarra();

        if (EstaSendoCarregado)
        {
            DefinirVelocidadeDaAnimacao(_velocidadeDeFuga); // "pedala" no ar
            return;
        }

        if (EstaDesmaiado)
        {
            _velocidadeAtual = Vector3.zero;
            DefinirVelocidadeDaAnimacao(0f);
            return;
        }

        VerificarPerigoPeriodicamente();
        AtualizarEstado();

        Vector3 velocidadeDesejada = CalcularVelocidadeDesejada() + CalcularSeparacao();
        if (_contaminado) velocidadeDesejada *= _fatorDeVelocidadeContaminado;
        _velocidadeAtual = Vector3.MoveTowards(_velocidadeAtual, velocidadeDesejada, _aceleracao * Time.deltaTime);
        transform.position += _velocidadeAtual * Time.deltaTime;

        GirarSuavemente();
        ManterAlturaFixa();
        transform.position = GerenciadorDeRua.Instancia.RestringirPosicao(transform.position);
        DefinirVelocidadeDaAnimacao(new Vector3(_velocidadeAtual.x, 0f, _velocidadeAtual.z).magnitude);
    }

    // ---------- Saúde ----------

    // Chamado por um inimigo que alcançou este cidadão.
    public void ReceberAtaque(int dano, TipoMunicao tipoDoAtacante)
    {
        if (!PodeSerAlvo) return;

        _saude = Mathf.Max(0, _saude - dano);
        _contaminado = true;
        _tipoDaContaminacao = tipoDoAtacante;

        GerenciadorDeEfeitos.Instancia.CriarExplosao(transform.position + Vector3.up, CoresDeMunicao.Obter(tipoDoAtacante), 0.6f);
        GerenciadorDeCidadaos.Instancia.NotificarAtaque(this);
        if (_saude == 0) Desmaiar();

        GerenciadorDeJogo.Instancia.RegistrarDanoNaCidade();
    }

    // Tiro do jogador: só a munição certa cura a contaminação.
    public void ReceberDano(TipoMunicao municaoUsada)
    {
        if (!AceitaTiro) return;

        if (municaoUsada != _tipoDaContaminacao)
        {
            GerenciadorDeAudio.Instancia.TocarSomErro();
            GerenciadorDePontuacao.Instancia.RegistrarErro();
            TextoFlutuante.Criar(transform.position + Vector3.up * _alturaDaCabeca,
                $"USE {CoresDeMunicao.Nome(_tipoDaContaminacao).ToUpper()}", CoresDeMunicao.Obter(_tipoDaContaminacao));
            return;
        }

        GerenciadorDeAudio.Instancia.TocarSomAcerto();
        GerenciadorDePontuacao.Instancia.RegistrarAcerto();
        GerenciadorDePontuacao.Instancia.AdicionarBonus(_pontosPorCura);
        GerenciadorDeEfeitos.Instancia.CriarExplosao(transform.position + Vector3.up, new Color(0.4f, 1f, 0.5f), 0.5f);
        Curar(Mathf.RoundToInt(_curaPorTiro * GerenciadorDeMelhorias.Atributos.MultiplicadorDeCura), peloJogador: true);
    }

    // Cura vinda do jogador (tiro) ou de itens/eventos (peloJogador = false).
    public void Curar(int quantidade, bool peloJogador)
    {
        if (EstaSendoCarregado || quantidade <= 0) return;

        _saude = Mathf.Min(_saudeMaxima, _saude + quantidade);

        if (EstaDesmaiado && _saude >= _saudeMaxima * _fracaoParaLevantar) Levantar();
        if (_saude < _saudeMaxima) return;

        _contaminado = false;
        if (!peloJogador) return;

        TextoFlutuante.Criar(transform.position + Vector3.up * _alturaDaCabeca, "CURADO!", new Color(0.4f, 1f, 0.5f));
        GerenciadorDeAudio.Instancia.TocarSomCura();
        Comemorar();
        if (Random.value < _chanceDePresenteAoCurar) GerenciadorDePowerUps.Instancia.DroparPresente(transform.position);
    }

    // Usado ao trocar de cena: a saúde da cidade é dividida entre os cidadãos.
    public void DefinirSaudeProporcional(float fracao)
    {
        _saude = Mathf.Clamp(Mathf.RoundToInt(_saudeMaxima * fracao), 1, _saudeMaxima);
        _contaminado = _saude < _saudeMaxima;
        if (_contaminado) _tipoDaContaminacao = (TipoMunicao)Random.Range(0, 3);
    }

    private void Desmaiar()
    {
        _estado = Estado.Desmaiado;
        _velocidadeAtual = Vector3.zero;
        GerenciadorDeAudio.Instancia.TocarSomDesmaio();
        if (_temAnimacaoDeDesmaio) _animator.SetBool(ParametroDesmaiado, true);
    }

    private void Levantar()
    {
        if (_temAnimacaoDeDesmaio) _animator.SetBool(ParametroDesmaiado, false);
        if (EstaLongeDaOrigem()) EntrarEmVolta();
        else EntrarEmParado(0.5f);
    }

    private void AtualizarBarra()
    {
        if (_barra == null) return;

        bool visivel = _saude < _saudeMaxima || EstaSendoCarregado;
        Color corDaMoldura = _contaminado ? CoresDeMunicao.Obter(_tipoDaContaminacao) : Color.white;
        _barra.Atualizar((float)_saude / _saudeMaxima, visivel, corDaMoldura);
    }

    // ---------- Sequestro ----------

    public void IniciarCarregamento()
    {
        _estado = Estado.Carregado;
        _velocidadeAtual = Vector3.zero;
        if (_temAnimacaoDeDesmaio) _animator.SetBool(ParametroDesmaiado, false);
    }

    // Chamado todo frame pelo inimigo que está carregando este cidadão.
    public void AcompanharCarregador(Vector3 posicao)
    {
        transform.position = posicao;
    }

    // O inimigo foi derrotado antes de fugir: o cidadão cai e é salvo.
    public void SerResgatado()
    {
        if (!EstaSendoCarregado) return;

        Vector3 posicao = transform.position;
        posicao.y = _alturaFixa;
        transform.position = GerenciadorDeRua.Instancia.RestringirPosicao(posicao);

        _estado = Estado.Fugindo;
        _fimDoPanico = Time.time + _tempoDePanicoAposFuga;
        _direcaoDeFuga = -transform.forward;

        GerenciadorDeAudio.Instancia.TocarSomPowerUp();
        GerenciadorDePontuacao.Instancia.AdicionarBonus(_pontosPorResgate);
        TextoFlutuante.Criar(transform.position + Vector3.up * _alturaDaCabeca, $"SALVO!  +{_pontosPorResgate}", new Color(1f, 0.82f, 0.25f));
    }

    // O inimigo conseguiu fugir com ele: o cidadão sai do jogo.
    public void SerLevado()
    {
        _saude = 0;
        DefinirComoVip(false);
        GerenciadorDeCidadaos.Instancia.NotificarPerda(this);
        gameObject.SetActive(false);
        GerenciadorDeJogo.Instancia.RegistrarDanoNaCidade();
    }

    // ---------- Estados ----------

    private void AtualizarEstado()
    {
        if (_inimigoMaisProximo != null)
        {
            if (_estado != Estado.Fugindo) _estado = Estado.Fugindo;
            _fimDoPanico = Time.time + _tempoDePanicoAposFuga;
            return;
        }

        switch (_estado)
        {
            case Estado.Fugindo:
                if (Time.time >= _fimDoPanico) TerminarFuga();
                break;

            case Estado.Parado:
            case Estado.Comemorando:
                if (Time.time >= _fimDoEstado) EntrarEmPasseio();
                break;

            case Estado.Passeando:
                if (DistanciaHorizontal(transform.position, _destinoAtual) < _distanciaParaNovoDestino)
                {
                    EntrarEmParado(Random.Range(_pausaMinima, _pausaMaxima));
                }
                break;

            case Estado.Voltando:
                if (DistanciaHorizontal(transform.position, _pontoDeOrigem) < _distanciaParaNovoDestino)
                {
                    EntrarEmParado(Random.Range(_pausaMinima, _pausaMaxima));
                }
                break;
        }
    }

    // Passado o susto: se fugiu para longe, volta para casa; senão, segue a vida.
    private void TerminarFuga()
    {
        if (EstaLongeDaOrigem()) EntrarEmVolta();
        else if (Random.value < _chanceDeComemorarAposFuga) Comemorar();
        else EntrarEmParado(Random.Range(_pausaMinima, _pausaMaxima));
    }

    private bool EstaLongeDaOrigem()
    {
        return DistanciaHorizontal(transform.position, _pontoDeOrigem) > _distanciaParaVoltar;
    }

    private void EntrarEmVolta()
    {
        _estado = Estado.Voltando;
        _destinoAtual = _pontoDeOrigem;
    }

    private void EntrarEmPasseio()
    {
        _estado = Estado.Passeando;
        EscolherNovoDestino();
    }

    private void EntrarEmParado(float duracao)
    {
        _estado = Estado.Parado;
        _fimDoEstado = Time.time + duracao;

        // Às vezes olha para outro lado enquanto espera.
        if (Random.value < 0.6f)
        {
            Vector2 aleatorio = Random.insideUnitCircle.normalized;
            _direcaoDoOlhar = new Vector3(aleatorio.x, 0f, aleatorio.y);
        }
    }

    // Chamado também pelo GerenciadorDeCidadaos quando uma onda termina.
    public void Comemorar()
    {
        if (_estado == Estado.Fugindo || _estado == Estado.Comemorando) return;
        if (EstaDesmaiado || EstaSendoCarregado) return;

        _estado = Estado.Comemorando;
        _fimDoEstado = Time.time + _duracaoDaComemoracao;
        if (_animacaoConfigurada) _animator.SetTrigger(ParametroComemorar);
    }

    // ---------- Movimento ----------

    private Vector3 CalcularVelocidadeDesejada()
    {
        switch (_estado)
        {
            case Estado.Fugindo:
                return DirecaoDeFuga() * _velocidadeDeFuga;

            case Estado.Passeando:
            case Estado.Voltando:
                Vector3 paraDestino = _destinoAtual - transform.position;
                paraDestino.y = 0f;
                // Desacelera ao chegar, em vez de parar de uma vez.
                float fatorDeChegada = Mathf.Clamp01(paraDestino.magnitude / 1.5f);
                float velocidade = _estado == Estado.Voltando ? _velocidadeDeVolta : _velocidadeIndividual;
                return paraDestino.normalized * velocidade * Mathf.Max(0.35f, fatorDeChegada);

            default:
                return Vector3.zero;
        }
    }

    private Vector3 DirecaoDeFuga()
    {
        if (_inimigoMaisProximo != null)
        {
            Vector3 longeDoInimigo = transform.position - _inimigoMaisProximo.position;
            longeDoInimigo.y = 0f;
            // Pequeno desvio lateral para a fuga não ser uma linha reta perfeita.
            Vector3 lateral = Vector3.Cross(Vector3.up, longeDoInimigo).normalized * Mathf.Sin(Time.time * 3f + GetInstanceID()) * 0.4f;
            _direcaoDeFuga = (longeDoInimigo.normalized + lateral).normalized;
        }
        return _direcaoDeFuga;
    }

    // Afasta-se suavemente de outros cidadãos muito próximos.
    private Vector3 CalcularSeparacao()
    {
        Vector3 empurrao = Vector3.zero;

        foreach (Cidadao outro in GerenciadorDeCidadaos.Instancia.CidadaosVivos)
        {
            if (outro == this) continue;

            Vector3 diferenca = transform.position - outro.transform.position;
            diferenca.y = 0f;
            float distancia = diferenca.magnitude;
            if (distancia < 0.001f || distancia > _raioDeEspacoPessoal) continue;

            empurrao += diferenca.normalized * (1f - distancia / _raioDeEspacoPessoal);
        }

        return empurrao * _velocidadeIndividual;
    }

    private void GirarSuavemente()
    {
        Vector3 direcao = _velocidadeAtual.sqrMagnitude > 0.04f ? _velocidadeAtual : _direcaoDoOlhar;
        direcao.y = 0f; // evita olhar pro céu/chão
        if (direcao.sqrMagnitude < 0.0001f) return;

        float velocidadeDeGiro = _estado == Estado.Fugindo ? _velocidadeDeGiro * 2.5f : _velocidadeDeGiro;
        Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, velocidadeDeGiro * Time.deltaTime);
    }

    private void DefinirVelocidadeDaAnimacao(float velocidade)
    {
        if (!_animacaoConfigurada) return;
        _animator.SetFloat(ParametroVelocidade, velocidade, 0.1f, Time.deltaTime);
    }

    // Passeia perto de onde nasceu: ponto aleatório da zona de patrulha,
    // mas sem se afastar mais que _raioMaximoDePasseio da origem.
    private void EscolherNovoDestino()
    {
        Vector3 centro = _centroDaZonaDePatrulha != null ? _centroDaZonaDePatrulha.position : _pontoDeOrigem;
        Vector2 pontoAleatorio = Random.insideUnitCircle * _raioIndividual;
        Vector3 destino = centro + new Vector3(pontoAleatorio.x, 0f, pontoAleatorio.y);

        Vector3 desdeOrigem = destino - _pontoDeOrigem;
        desdeOrigem.y = 0f;
        destino = _pontoDeOrigem + Vector3.ClampMagnitude(desdeOrigem, _raioMaximoDePasseio);
        _destinoAtual = GerenciadorDeRua.Instancia.RestringirPosicao(destino);
    }

    // ---------- Percepção ----------

    // Busca inimigos só algumas vezes por segundo (FindObjectsByType é caro).
    private void VerificarPerigoPeriodicamente()
    {
        if (Time.time < _proximaVerificacaoDePerigo) return;
        _proximaVerificacaoDePerigo = Time.time + _intervaloDeVerificacaoDePerigo;

        Transform maisProximo = null;
        float menorDistancia = _raioDeDeteccaoDeInimigo;

        foreach (Inimigo inimigo in FindObjectsByType<Inimigo>(FindObjectsSortMode.None))
        {
            float distancia = Vector3.Distance(transform.position, inimigo.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                maisProximo = inimigo.transform;
            }
        }

        _inimigoMaisProximo = maisProximo;
    }

    private static float DistanciaHorizontal(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void ManterAlturaFixa()
    {
        Vector3 posicao = transform.position;
        posicao.y = _alturaFixa;
        transform.position = posicao;
    }

    // ---------- VIP ----------

    public void DefinirComoVip(bool ehVip)
    {
        if (!ehVip)
        {
            if (_marcadorVip != null) Destroy(_marcadorVip);
            return;
        }

        if (_marcadorVip != null) return;
        _marcadorVip = CriarMarcadorVip();
    }

    private GameObject CriarMarcadorVip()
    {
        GameObject marcador = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marcador.name = "MarcadorVip";
        Destroy(marcador.GetComponent<Collider>());

        marcador.transform.position = transform.position + Vector3.up * _alturaDoMarcadorVip;
        marcador.transform.SetParent(transform, true);
        marcador.transform.localScale = Vector3.one * 0.5f / Mathf.Max(0.01f, transform.lossyScale.x);

        // Usa o material padrão da esfera (sempre incluído no build) em vez de
        // Shader.Find, que pode vir rosa no jogo exportado.
        Material material = marcador.GetComponent<Renderer>().material;
        material.color = _corDoMarcadorVip;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", _corDoMarcadorVip * 2f);

        Light brilho = marcador.AddComponent<Light>();
        brilho.type = LightType.Point;
        brilho.color = _corDoMarcadorVip;
        brilho.range = 4f;
        brilho.intensity = 3f;
        return marcador;
    }
}
