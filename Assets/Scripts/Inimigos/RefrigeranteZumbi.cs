using UnityEngine;

// Representa o excesso de açúcar. Fraqueza: Fruta.
// Movimento em zigue-zague acentuado e caótico. Habilidade: de tempos
// em tempos dá uma arrancada (pico de açúcar) — corre bem mais rápido
// e quase em linha reta por um instante.
public class RefrigeranteZumbi : Inimigo
{
    [SerializeField] private float _intervaloMinimoArrancada = 2.5f;
    [SerializeField] private float _intervaloMaximoArrancada = 4.5f;
    [SerializeField] private float _duracaoArrancada = 0.5f;
    [SerializeField] private float _multiplicadorArrancada = 2.5f;

    private float _proximaArrancada;
    private float _fimDaArrancada;

    public override TipoMunicao Fraqueza => TipoMunicao.Fruta;
    protected override float Velocidade => 16f;
    protected override int DanoAoAlcancarAlvo => 12;
    protected override float AmplitudeOscilacao => 1.5f;
    protected override int PontosAoDerrotar => 120;

    private bool EmArrancada => Time.time < _fimDaArrancada;

    protected override void Start()
    {
        base.Start();
        AgendarProximaArrancada();
    }

    protected override void AplicarMovimento()
    {
        AtualizarArrancada();

        Vector3 direcaoAoAlvo = DirecaoHorizontalAoAlvo();
        Vector3 lateral = Vector3.Cross(Vector3.up, direcaoAoAlvo);

        float oscilacaoPrincipal = Mathf.Sin(Time.time * _velocidadeOscilacao + _deslocamentoAleatorio) * AmplitudeOscilacao;
        float oscilacaoSecundaria = Mathf.Sin(Time.time * _velocidadeOscilacao * 3.7f + _deslocamentoAleatorio) * (AmplitudeOscilacao * 0.5f);
        float fatorOscilacao = EmArrancada ? 0.15f : 1f;
        float velocidade = VelocidadeAtual * (EmArrancada ? _multiplicadorArrancada : 1f);

        Vector3 movimento = (direcaoAoAlvo * velocidade) + (lateral * (oscilacaoPrincipal + oscilacaoSecundaria) * fatorOscilacao);
        transform.position += movimento * Time.deltaTime;
    }

    private void AtualizarArrancada()
    {
        if (Time.time < _proximaArrancada) return;

        _fimDaArrancada = Time.time + _duracaoArrancada;
        AgendarProximaArrancada();
    }

    private void AgendarProximaArrancada()
    {
        _proximaArrancada = Time.time + Random.Range(_intervaloMinimoArrancada, _intervaloMaximoArrancada);
    }
}
