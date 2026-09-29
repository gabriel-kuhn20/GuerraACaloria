using UnityEngine;

// Representa o sedentarismo. Fraqueza: Energia Positiva.
// Movimento quase reto, mas flutua suavemente para cima e para baixo
// enquanto avança, como uma nuvem no céu.
public class NuvemDePreguica : Inimigo
{
    [SerializeField] private float _amplitudeFlutuacao = 0.5f;
    [SerializeField] private float _velocidadeFlutuacao = 1.2f;

    public override TipoMunicao Fraqueza => TipoMunicao.EnergiaPositiva;
    protected override float Velocidade => 12f;
    protected override int DanoAoAlcancarAlvo => 16;
    protected override float AmplitudeOscilacao => 0.2f;

    protected override void AplicarAlturaFinal()
    {
        float flutuacao = Mathf.Sin(Time.time * _velocidadeFlutuacao + _deslocamentoAleatorio) * _amplitudeFlutuacao;

        Vector3 posicao = transform.position;
        posicao.y = AlturaFixa + flutuacao;
        transform.position = posicao;
    }
}