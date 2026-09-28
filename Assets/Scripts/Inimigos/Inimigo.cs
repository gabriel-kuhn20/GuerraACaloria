using UnityEngine;
using System;

// Classe base de todo inimigo do jogo (nuvens de maus hábitos).
// Persegue o cidadão vivo mais próximo (recalculado periodicamente,
// não todo frame); ao alcançá-lo, causa dano à energia da cidade.
// O cálculo do vetor de movimento é virtual — cada subclasse pode
// sobrescrever para ter um padrão de movimento único.
public abstract class Inimigo : MonoBehaviour, IDanificavel
{
    protected virtual float Velocidade => 3f;
    protected virtual int DanoAoAlcancarAlvo => 10;
    protected virtual float AmplitudeOscilacao => 0.6f;

    [SerializeField] private float _intervaloBuscaAlvo = 0.5f;
    protected int _vida = 1;

    protected Transform _alvo;
    private float _proximaBusca;
    private bool _jaNotificouMorte = false;
    protected float _deslocamentoAleatorio;
    protected float _velocidadeOscilacao;
    private float _alturaFixa;

    public abstract TipoMunicao Fraqueza { get; }
    public event Action AoMorrer;

    protected virtual void Awake()
    {
        _deslocamentoAleatorio = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        _velocidadeOscilacao = UnityEngine.Random.Range(1.5f, 3f);
    }

    protected virtual void Start()
    {
        _alturaFixa = transform.position.y;
    }

    protected virtual void Update()
    {
        AtualizarAlvoPeriodicamente();

        if (_alvo == null) return;

        AplicarMovimento();
        AplicarAlturaFinal();
        transform.position = GerenciadorDeRua.Instancia.RestringirPosicao(transform.position);

        if (DistanciaHorizontalAteAlvo() < 1f)
        {
            GerenciadorDeJogo.Instancia.ReceberDanoNaCidade(DanoAoAlcancarAlvo);
            NotificarMorteEDestruir();
        }
    }

    // Ponto de extensão principal: calcula e aplica o deslocamento deste
    // frame. Implementação padrão: linha reta ao alvo + oscilação lateral.
    protected virtual void AplicarMovimento()
    {
        Vector3 direcaoAoAlvo = DirecaoHorizontalAoAlvo();
        Vector3 lateral = Vector3.Cross(Vector3.up, direcaoAoAlvo);
        float oscilacao = Mathf.Sin(Time.time * _velocidadeOscilacao + _deslocamentoAleatorio) * AmplitudeOscilacao;

        Vector3 movimento = (direcaoAoAlvo * Velocidade) + (lateral * oscilacao);
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

    private void AtualizarAlvoPeriodicamente()
    {
        if (Time.time < _proximaBusca) return;

        _alvo = GerenciadorDeCidadaos.Instancia.ObterCidadaoMaisProximo(transform.position);
        _proximaBusca = Time.time + _intervaloBuscaAlvo;
    }

    public virtual void ReceberDano(TipoMunicao municaoUsada)
    {
        if (municaoUsada == Fraqueza)
        {
            _vida--;
            GerenciadorDeAudio.Instancia.TocarSomAcerto();
        }
        else
        {
            GerenciadorDeAudio.Instancia.TocarSomErro();
        }

        if (_vida <= 0)
        {
            NotificarMorteEDestruir();
        }
    }

    protected void NotificarMorteEDestruir()
    {
        if (_jaNotificouMorte) return;
        _jaNotificouMorte = true;

        AoMorrer?.Invoke();
        Destroy(gameObject);
    }
}