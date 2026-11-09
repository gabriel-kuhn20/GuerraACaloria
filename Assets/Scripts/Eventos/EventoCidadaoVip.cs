using UnityEngine;

// Um cidadão vira VIP e todos os inimigos vão atrás dele. Se nenhum
// inimigo encostar nele até o fim da onda, o jogador ganha bônus.
public class EventoCidadaoVip : EventoDeOnda
{
    private const int BonusDePontos = 1500;
    private const int BonusDeEnergia = 10;

    public override string Nome => "PROTEJA O VIP";
    public override string Descricao => "Todos vão atrás do cidadão dourado. Proteja-o até o fim da onda!";
    public override Color Cor => new Color(1f, 0.82f, 0.2f);

    public override void AoIniciar(GameObject[] prefabsDisponiveis)
    {
        GerenciadorDeCidadaos.Instancia.SortearVip();
    }

    public override string AoTerminar()
    {
        bool vipSalvo = !GerenciadorDeCidadaos.Instancia.VipFoiAtingido;
        GerenciadorDeCidadaos.Instancia.EncerrarVip();

        if (!vipSalvo) return "O VIP foi atingido... sem bônus desta vez.";

        GerenciadorDePontuacao.Instancia.AdicionarBonus(BonusDePontos);
        GerenciadorDeJogo.Instancia.RestaurarEnergia(BonusDeEnergia);
        return $"VIP PROTEGIDO!  +{BonusDePontos} pontos  +{BonusDeEnergia} de energia";
    }
}
