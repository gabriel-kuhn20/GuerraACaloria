// Valores finais que os sistemas do jogo consultam (arma, projétil,
// movimento, cura, power-ups, chefão), já somando todas as melhorias
// escolhidas. Recalculado pelo GerenciadorDeMelhorias a cada escolha.
public class AtributosDoJogador
{
    public static readonly AtributosDoJogador Padrao = new AtributosDoJogador();

    public int ProjeteisExtras { get; set; }
    // 0 = sem tiro automático; caso contrário, segundos entre tiros segurando o botão.
    public float IntervaloDeTiroAutomatico { get; set; }
    public int Perfuracao { get; set; }
    public float MultiplicadorDeMovimento { get; set; } = 1f;
    public float MultiplicadorDeCura { get; set; } = 1f;
    public int RegeneracaoPorSegundo { get; set; }
    public float MultiplicadorDeDuracaoDePowerUp { get; set; } = 1f;
    public float ChanceExtraDeDrop { get; set; }
    public int DanoExtraNoChefao { get; set; }

    public bool TemTiroAutomatico => IntervaloDeTiroAutomatico > 0f;
}
