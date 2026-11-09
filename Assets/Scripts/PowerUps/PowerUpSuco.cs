using UnityEngine;

// Suco Natural (Turbo): por alguns segundos, segurar o botão dispara em
// rajada bem rápida e cada disparo sai triplo.
public class PowerUpSuco : PowerUp
{
    [SerializeField] private float _duracao = 10f;

    public override string Nome => "SUCO TURBO";
    public override string Descricao => "Segure o botão: rajada rápida e tiro triplo!";
    public override Color Cor => new Color(1f, 0.62f, 0.2f);

    protected override void AplicarEfeito()
    {
        GerenciadorDePowerUps.Instancia.AtivarTiroRapido(_duracao);
    }
}
