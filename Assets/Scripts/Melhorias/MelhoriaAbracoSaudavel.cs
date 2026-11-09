using UnityEngine;

// Abraço Saudável: cada tiro certo cura mais o cidadão contaminado.
public class MelhoriaAbracoSaudavel : Melhoria
{
    private const float BonusPorNivel = 0.5f;

    public override string Id => "abraco_saudavel";
    public override string Nome => "ABRAÇO SAUDÁVEL";
    public override Color Cor => new Color(0.4f, 1f, 0.5f);

    public override string Descricao(int proximoNivel) =>
        $"Curar cidadãos com o tiro rende <b>+{Mathf.RoundToInt(BonusPorNivel * 100f)}% de saúde</b>.";

    public override void Aplicar(AtributosDoJogador atributos, int nivel)
    {
        atributos.MultiplicadorDeCura += BonusPorNivel * nivel;
    }
}
