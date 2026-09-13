using UnityEngine;
using System;

// Classe base de todo inimigo do jogo (nuvens de maus hábitos).
// Persegue o cidadão vivo mais próximo (recalculado periodicamente,
// não todo frame); ao alcançá-lo, causa dano à energia da cidade.
// Movimento tem uma leve oscilação lateral individual, para parecer
// menos robótico e mais orgânico.
public abstract class Inimigo : MonoBehaviour, IDanificavel
{
    [SerializeField] protected float _velocidade = 2f;
    [SerializeField] private float _intervaloBuscaAlvo = 0.5f;
    [SerializeField] private float _amplitudeOscilacao = 0.6f;
    protected int _vida = 1;

    private Transform _alvo;
    private float _proximaBusca;
    private bool _jaNotificouMorte = false;
    private float _deslocamentoAleatorio;
    private float _velocidadeOscilacao;

    public abstract TipoMunicao Fraqueza { get; }
    public event Action AoMorrer;

    protected virtual void Awake()
    {
        _deslocamentoAleatorio = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        _velocidadeOscilacao = UnityEngine.Random.Range(1.5f, 3f);
    }

    protected virtual void Update()
    {
        AtualizarAlvoPeriodicamente();

        if (_alvo == null) return;

        MoverEmDirecaoAoAlvoComOscilacao();

        if (Vector3.Distance(transform.position, _alvo.position) < 1f)
        {
            GerenciadorDeJogo.Instancia.ReceberDanoNaCidade(10);
            NotificarMorteEDestruir();
        }
    }

    private void MoverEmDirecaoAoAlvoComOscilacao()
    {
        Vector3 direcaoAoAlvo = (_alvo.position - transform.position).normalized;
        Vector3 lateral = Vector3.Cross(Vector3.up, direcaoAoAlvo);

        float oscilacao = Mathf.Sin(Time.time * _velocidadeOscilacao + _deslocamentoAleatorio) * _amplitudeOscilacao;

        Vector3 movimento = (direcaoAoAlvo * _velocidade) + (lateral * oscilacao);
        transform.position += movimento * Time.deltaTime;
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

    private void NotificarMorteEDestruir()
    {
        if (_jaNotificouMorte) return;
        _jaNotificouMorte = true;

        AoMorrer?.Invoke();
        Destroy(gameObject);
    }
}