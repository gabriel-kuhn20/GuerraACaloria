using UnityEngine;

// Representa o excesso de açúcar. Fraqueza: Fruta.
// Movimento em zigue-zague acentuado e caótico, mudando de amplitude
// e frequência de oscilação de forma irregular ao longo do tempo.
public class RefrigeranteZumbi : Inimigo
{
    public override TipoMunicao Fraqueza => TipoMunicao.Fruta;
    protected override float Velocidade => 16f;
    protected override int DanoAoAlcancarAlvo => 12;
    protected override float AmplitudeOscilacao => 1.5f;

    protected override void AplicarMovimento()
    {
        Vector3 direcaoAoAlvo = DirecaoHorizontalAoAlvo();
        Vector3 lateral = Vector3.Cross(Vector3.up, direcaoAoAlvo);

        float oscilacaoPrincipal = Mathf.Sin(Time.time * _velocidadeOscilacao + _deslocamentoAleatorio) * AmplitudeOscilacao;
        float oscilacaoSecundaria = Mathf.Sin(Time.time * _velocidadeOscilacao * 3.7f + _deslocamentoAleatorio) * (AmplitudeOscilacao * 0.5f);

        Vector3 movimento = (direcaoAoAlvo * Velocidade) + (lateral * (oscilacaoPrincipal + oscilacaoSecundaria));
        transform.position += movimento * Time.deltaTime;
    }
}