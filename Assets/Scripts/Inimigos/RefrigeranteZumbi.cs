// Representa o excesso de açúcar. Fraqueza: Fruta.
// Rápido e errático (oscilação maior), mas causa pouco dano.
public class RefrigeranteZumbi : Inimigo
{
    public override TipoMunicao Fraqueza => TipoMunicao.Fruta;
    protected override float Velocidade => 4.5f;
    protected override int DanoAoAlcancarAlvo => 8;
    protected override float AmplitudeOscilacao => 1f;
}