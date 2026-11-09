using UnityEngine;
using System;

// Classe base de todo inimigo do jogo (nuvens de maus hábitos).
// Persegue o cidadão vivo mais próximo (recalculado periodicamente,
// não todo frame); ao alcançá-lo, contamina o cidadão (tira saúde dele).
// Também interage com o Agente: encostar nele causa dano e empurrão, e
// inimigos "agressivos" trocam de alvo e vão atrás dele quando ele chega perto.
// Ao ser derrotado por tiro, dá pontos e pode soltar um power-up.
// Movimento, recompensa e reação à derrota são virtuais — cada
// subclasse pode ter seu próprio padrão e habilidade.
public abstract class Inimigo : MonoBehaviour, IDanificavel, IAlvoDaMira
{
    protected virtual float Velocidade => 3f;
    protected virtual int DanoAoAlcancarAlvo => 10;
    protected virtual float AmplitudeOscilacao => 0.6f;
    protected virtual int PontosAoDerrotar => 100;
    protected virtual int DanoNoAgente => 8;
    protected virtual float RaioDeContatoComAgente => 1.4f;
    protected virtual float ForcaDoEmpurraoNoAgente => 1f;
    // Chance (no nascimento) de ser agressivo: persegue o agente se ele estiver perto.
    protected virtual float ChanceDeSerAgressivo => 0.25f;
    protected virtual float RaioDeAgressao => 9f;

    [SerializeField] private float _intervaloBuscaAlvo = 0.5f;
    protected int _vida = 1;

    protected Transform _alvo;
    private Cidadao _cidadaoAlvo;
    private float _proximaBusca;
    private bool _jaNotificouMorte = false;
    protected float _deslocamentoAleatorio;
    protected float _velocidadeOscilacao;
    private float _alturaFixa;
    private float _multiplicadorDeVelocidade = 1f;
    private bool _agressivo;
    private bool _perseguindoAgente;
    private float _proximoContatoComAgente;

    public abstract TipoMunicao Fraqueza { get; }
    public bool AceitaTiro => true;
    public bool IndicaMunicao => true;
    public TipoMunicao MunicaoIndicada => Fraqueza;
    public event Action AoMorrer;
    // Disparado quando este inimigo cria outro (ex: hambúrguer que se divide),
    // para o GeradorDeInimigos contar o novo inimigo na onda.
    public event Action<Inimigo> AoGerarFilhote;

    protected float VelocidadeAtual => Velocidade * _multiplicadorDeVelocidade;
    protected float MultiplicadorDeVelocidade => _multiplicadorDeVelocidade;
    protected Cidadao CidadaoAlvo => _cidadaoAlvo;
    protected bool PerseguindoAgente => _perseguindoAgente;
    protected bool EstaMorto => _jaNotificouMorte;

    // Pontos de extensão: o hambúrguer que carrega um cidadão continua se
    // movendo sem alvo e não "alcança" ninguém enquanto foge.
    protected virtual bool PodeSeMover => _alvo != null;
    protected virtual bool PodeAlcancarAlvo => true;

    protected virtual void Awake()
    {
        _deslocamentoAleatorio = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        _velocidadeOscilacao = UnityEngine.Random.Range(1.5f, 3f);
        _agressivo = UnityEngine.Random.value < ChanceDeSerAgressivo;
    }

    protected virtual void Start()
    {
        _alturaFixa = transform.position.y;
    }

    protected virtual void Update()
    {
        if (_jaNotificouMorte) return;

        AtualizarAlvoPeriodicamente();

        if (!PodeSeMover) return;

        AplicarMovimento();
        if (_jaNotificouMorte) return;

        AplicarAlturaFinal();
        transform.position = GerenciadorDeRua.Instancia.RestringirPosicaoDeInimigo(transform.position);
        VerificarContatoComAgente();

        if (_alvo != null && _cidadaoAlvo != null && PodeAlcancarAlvo && DistanciaHorizontalAteAlvo() < 1f)
        {
            AoAlcancarAlvo();
        }
    }

    public void AplicarMultiplicadorDeVelocidade(float multiplicador)
    {
        _multiplicadorDeVelocidade = Mathf.Max(0.1f, multiplicador);
    }

    // Ponto de extensão principal: calcula e aplica o deslocamento deste
    // frame. Implementação padrão: linha reta ao alvo + oscilação lateral.
    protected virtual void AplicarMovimento()
    {
        Vector3 direcaoAoAlvo = DirecaoHorizontalAoAlvo();
        Vector3 lateral = Vector3.Cross(Vector3.up, direcaoAoAlvo);
        float oscilacao = Mathf.Sin(Time.time * _velocidadeOscilacao + _deslocamentoAleatorio) * AmplitudeOscilacao;

        Vector3 movimento = (direcaoAoAlvo * VelocidadeAtual) + (lateral * oscilacao);
        transform.position += movimento * Time.deltaTime;
    }

    // Ponto de extensão secundário: permite alterar a altura antes de
    // travar na altura fixa (ex: oscilação vertical). Padrão: mantém
    // a altura de nascimento, sem variação.
    protected virtual void AplicarAlturaFinal()
    {
        Vector3 posicao = transform.position;
        posicao.y = _alturaFixa;
        transform.position = posicao;
    }

    // Padrão: contamina o cidadão, quebra o combo do jogador e some.
    // O chefão sobrescreve para não "morrer" ao encostar num cidadão.
    protected virtual void AoAlcancarAlvo()
    {
        _cidadaoAlvo.ReceberAtaque(DanoAoAlcancarAlvo, Fraqueza);
        GerenciadorDePontuacao.Instancia.RegistrarErro();
        NotificarMorteEDestruir();
    }

    protected float AlturaFixa => _alturaFixa;

    protected Vector3 DirecaoHorizontalAoAlvo()
    {
        Vector3 posicaoAlvoMesmaAltura = new Vector3(_alvo.position.x, transform.position.y, _alvo.position.z);
        return (posicaoAlvoMesmaAltura - transform.position).normalized;
    }

    protected float DistanciaHorizontalAteAlvo()
    {
        Vector3 posicaoAtualSemAltura = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 posicaoAlvoSemAltura = new Vector3(_alvo.position.x, 0f, _alvo.position.z);
        return Vector3.Distance(posicaoAtualSemAltura, posicaoAlvoSemAltura);
    }

    // Busca de novo antes da hora se o alvo atual desmaiou ou foi levado.
    // Inimigo agressivo com o agente por perto vai atrás do agente.
    private void AtualizarAlvoPeriodicamente()
    {
        bool alvoInvalido = !_perseguindoAgente && (_cidadaoAlvo == null || !_cidadaoAlvo.PodeSerAlvo);
        if (Time.time < _proximaBusca && !alvoInvalido) return;
        _proximaBusca = Time.time + _intervaloBuscaAlvo;

        if (DevePerseguirAgente())
        {
            _perseguindoAgente = true;
            _cidadaoAlvo = null;
            _alvo = VidaDoAgente.Instancia.transform;
            return;
        }

        _perseguindoAgente = false;
        _cidadaoAlvo = GerenciadorDeCidadaos.Instancia.ObterCidadaoMaisProximo(transform.position);
        _alvo = _cidadaoAlvo != null ? _cidadaoAlvo.transform : null;
    }

    private bool DevePerseguirAgente()
    {
        VidaDoAgente agente = VidaDoAgente.Instancia;
        if (!_agressivo || agente == null || agente.EstaNocauteado) return false;
        return DistanciaHorizontalAte(agente.transform.position) < RaioDeAgressao;
    }

    // Encostou no agente: dano + empurrão (com intervalo, para não "grudar").
    private void VerificarContatoComAgente()
    {
        VidaDoAgente agente = VidaDoAgente.Instancia;
        if (agente == null || Time.time < _proximoContatoComAgente) return;
        if (DistanciaHorizontalAte(agente.transform.position) > RaioDeContatoComAgente) return;

        agente.ReceberDano(DanoNoAgente, transform.position, ForcaDoEmpurraoNoAgente);
        _proximoContatoComAgente = Time.time + 1.2f;
    }

    protected float DistanciaHorizontalAte(Vector3 posicao)
    {
        return Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(posicao.x, posicao.z));
    }

    // A munição "acerta" se for a fraqueza deste inimigo ou se o
    // power-up de munição coringa estiver ativo.
    protected bool EhFraqueza(TipoMunicao municaoUsada)
    {
        return municaoUsada == Fraqueza || GerenciadorDePowerUps.Instancia.MunicaoCoringaAtiva;
    }

    public virtual void ReceberDano(TipoMunicao municaoUsada)
    {
        if (!EhFraqueza(municaoUsada))
        {
            GerenciadorDeAudio.Instancia.TocarSomErro();
            GerenciadorDePontuacao.Instancia.RegistrarErro();
            return;
        }

        _vida--;
        GerenciadorDeAudio.Instancia.TocarSomAcerto();
        GerenciadorDePontuacao.Instancia.RegistrarAcerto();

        if (_vida <= 0)
        {
            SerDerrotadoPorTiro();
        }
    }

    // Atingido pelo especial (Onda Saudável): ignora fraqueza e some na
    // hora, sem se dividir nem soltar item. O chefão sobrescreve (só perde vida).
    public virtual void ReceberEspecial()
    {
        if (_jaNotificouMorte) return;
        ComemorarAbate(1f);
        NotificarMorteEDestruir();
    }

    protected virtual void SerDerrotadoPorTiro()
    {
        ComemorarAbate(1f);
        GerenciadorDePowerUps.Instancia.TentarDropar(transform.position);
        NotificarMorteEDestruir();
    }

    // Pontos + explosão na cor da fraqueza + "+pontos" flutuando no ar.
    protected void ComemorarAbate(float escalaDaExplosao)
    {
        int pontos = GerenciadorDePontuacao.Instancia.RegistrarAbate(PontosAoDerrotar);
        GerenciadorDeEfeitos.Instancia.CriarExplosao(transform.position, CoresDeMunicao.Obter(Fraqueza), escalaDaExplosao);
        GerenciadorDeEfeitos.Instancia.MostrarPontos(transform.position, pontos, GerenciadorDePontuacao.Instancia.Multiplicador);

        // Explosão grande = chefão.
        if (escalaDaExplosao >= 2f) GerenciadorDeAudio.Instancia.TocarSomChefaoDerrotado();
        else GerenciadorDeAudio.Instancia.TocarSomInimigoDerrotado();
    }

    protected void NotificarFilhote(Inimigo filhote)
    {
        AoGerarFilhote?.Invoke(filhote);
    }

    protected void NotificarMorteEDestruir()
    {
        if (_jaNotificouMorte) return;
        _jaNotificouMorte = true;

        AoMorrer?.Invoke();
        Destroy(gameObject);
    }
}
