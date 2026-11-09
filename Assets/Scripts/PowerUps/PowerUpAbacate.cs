using UnityEngine;

// Abacate Coringa: por alguns segundos, qualquer munição acerta qualquer
// inimigo (inclusive o chefão) e os projéteis atravessam vários alvos.
public class PowerUpAbacate : PowerUp
{
    [SerializeField] private float _duracao = 10f;

    public override string Nome => "ABACATE CORINGA";
    public override string Descricao => "Qualquer munição serve e o tiro atravessa inimigos!";
    public override Color Cor => new Color(0.75f, 0.95f, 0.35f);

    protected override void AplicarEfeito()
    {
        GerenciadorDePowerUps.Instancia.AtivarMunicaoCoringa(_duracao);
    }
}
