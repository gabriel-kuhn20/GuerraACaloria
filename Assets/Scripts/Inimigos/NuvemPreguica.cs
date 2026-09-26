// Representa o sedentarismo. Fraqueza: Energia Positiva.
// Muito lento e constante, movimento quase reto, dano médio.
public class NuvemDePreguica : Inimigo
{
    public override TipoMunicao Fraqueza => TipoMunicao.EnergiaPositiva;
    protected override float Velocidade => 2f;
    protected override int DanoAoAlcancarAlvo => 12;
    protected override float AmplitudeOscilacao => 0.2f;
}