using UnityEngine;

// Dieta Equilibrada: power-ups caem mais e duram mais.
public class MelhoriaDietaEquilibrada : Melhoria
{
    private const float DuracaoPorNivel = 0.4f;
    private const float DropPorNivel = 0.07f;

    public override string Id => "dieta_equilibrada";
    public override string Nome => "DIETA EQUILIBRADA";
    public override Color Cor => new Color(1f, 0.82f, 0.25f);

    public override string Descricao(int proximoNivel) =>
        $"Power-ups duram <b>+{Mathf.RoundToInt(DuracaoPorNivel * 100f)}%</b> e caem <b>com mais frequência</b>.";

    public override void Aplicar(AtributosDoJogador atributos, int nivel)
    {
        atributos.MultiplicadorDeDuracaoDePowerUp += DuracaoPorNivel * nivel;
        atributos.ChanceExtraDeDrop += DropPorNivel * nivel;
    }
}
