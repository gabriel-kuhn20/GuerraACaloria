using UnityEngine;

// Fibra Perfurante: o projétil atravessa inimigos e segue em frente.
public class MelhoriaPerfurante : Melhoria
{
    public override string Id => "perfurante";
    public override string Nome => "FIBRA PERFURANTE";
    public override Color Cor => new Color(0.75f, 0.95f, 0.35f);

    public override string Descricao(int proximoNivel) =>
        $"Projéteis atravessam <b>{proximoNivel} {(proximoNivel == 1 ? "alvo" : "alvos")}</b> antes de sumir.";

    public override void Aplicar(AtributosDoJogador atributos, int nivel)
    {
        atributos.Perfuracao += nivel;
    }
}
