using UnityEngine;

// Onda com o dobro de inimigos, nascendo mais rápido e correndo mais.
public class EventoChuvaDeFastFood : EventoDeOnda
{
    private const float MultiplicadorDeVelocidade = 1.3f;

    public override string Nome => "CHUVA DE FAST FOOD";
    public override string Descricao => "Inimigos em dobro e mais rápidos!";
    public override Color Cor => new Color(1f, 0.45f, 0.35f);

    public override int ModificarQuantidade(int quantidade) => quantidade * 2;
    public override float ModificarIntervalo(float intervalo) => intervalo * 0.6f;

    public override void AoSpawnarInimigo(Inimigo inimigo)
    {
        inimigo.AplicarMultiplicadorDeVelocidade(MultiplicadorDeVelocidade);
    }
}
