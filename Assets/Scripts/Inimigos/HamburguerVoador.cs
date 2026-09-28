using UnityEngine;

// Representa o excesso de gordura/ultraprocessados. Fraqueza: Água.
// Começa pesado e lento, mas acelera progressivamente quanto mais
// perto fica do alvo — perigoso justamente no impacto final.
public class HamburguerVoador : Inimigo
{
    [SerializeField] private float _distanciaInicioAceleracao = 15f;
    [SerializeField] private float _multiplicadorVelocidadeMaxima = 2.5f;

    public override TipoMunicao Fraqueza => TipoMunicao.Agua;
    protected override float Velocidade => 10f;
    protected override int DanoAoAlcancarAlvo => 20;
    protected override float AmplitudeOscilacao => 0.4f;

    protected override void AplicarMovimento()
    {
        Vector3 direcaoAoAlvo = DirecaoHorizontalAoAlvo();
        float distancia = DistanciaHorizontalAteAlvo();

        float proporcaoProximidade = 1f - Mathf.Clamp01(distancia / _distanciaInicioAceleracao);
        float multiplicador = Mathf.Lerp(1f, _multiplicadorVelocidadeMaxima, proporcaoProximidade);

        Vector3 movimento = direcaoAoAlvo * Velocidade * multiplicador;
        transform.position += movimento * Time.deltaTime;
    }
}