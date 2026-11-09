using UnityEngine;

// Tênis de Corrida: o Agente da Vitalidade anda mais rápido.
public class MelhoriaTenisDeCorrida : Melhoria
{
    private const float BonusPorNivel = 0.15f;

    public override string Id => "tenis_de_corrida";
    public override string Nome => "TÊNIS DE CORRIDA";
    public override Color Cor => new Color(0.55f, 0.85f, 1f);

    public override string Descricao(int proximoNivel) =>
        $"Movimento <b>+{Mathf.RoundToInt(BonusPorNivel * 100f)}%</b> (total +{Mathf.RoundToInt(BonusPorNivel * proximoNivel * 100f)}%).";

    public override void Aplicar(AtributosDoJogador atributos, int nivel)
    {
        atributos.MultiplicadorDeMovimento += BonusPorNivel * nivel;
    }
}
