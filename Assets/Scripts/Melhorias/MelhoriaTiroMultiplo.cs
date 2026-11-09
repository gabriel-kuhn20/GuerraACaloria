using UnityEngine;

// Tiro Múltiplo: cada disparo sai com projéteis extras em leque.
public class MelhoriaTiroMultiplo : Melhoria
{
    public override string Id => "tiro_multiplo";
    public override string Nome => "TIRO MÚLTIPLO";
    public override Color Cor => new Color(0.3f, 0.68f, 1f);
    public override int NivelMaximo => 2;

    public override string Descricao(int proximoNivel) =>
        $"Cada disparo lança <b>{proximoNivel + 1} projéteis</b> em leque.";

    public override void Aplicar(AtributosDoJogador atributos, int nivel)
    {
        atributos.ProjeteisExtras += nivel;
    }
}
