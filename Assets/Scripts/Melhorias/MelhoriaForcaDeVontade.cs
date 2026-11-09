using UnityEngine;

// Força de Vontade: cada acerto certo tira mais vida do chefão.
public class MelhoriaForcaDeVontade : Melhoria
{
    public override string Id => "forca_de_vontade";
    public override string Nome => "FORÇA DE VONTADE";
    public override Color Cor => new Color(1f, 0.45f, 0.38f);

    public override string Descricao(int proximoNivel) =>
        $"Acertos no chefão tiram <b>{proximoNivel + 1} de vida</b> em vez de {proximoNivel}.";

    public override void Aplicar(AtributosDoJogador atributos, int nivel)
    {
        atributos.DanoExtraNoChefao += nivel;
    }
}
