// Representa o excesso de gordura/ultraprocessados. Fraqueza: Água.
// Lento e pesado, mas causa dano alto se alcançar um cidadão.
public class HamburguerVoador : Inimigo
{
    public override TipoMunicao Fraqueza => TipoMunicao.Agua;
    protected override float Velocidade => 3f;
    protected override int DanoAoAlcancarAlvo => 15;
    protected override float AmplitudeOscilacao => 0.4f;
}