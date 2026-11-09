using UnityEngine;

// Salada: cura na hora metade da saúde de TODOS os cidadãos (quem
// ficar 100% sai da contaminação).
public class PowerUpSalada : PowerUp
{
    [SerializeField, Range(0f, 1f)] private float _fracaoDeCura = 0.5f;

    public override string Nome => "SALADA";
    public override string Descricao => $"Todos os cidadãos recuperam {Mathf.RoundToInt(_fracaoDeCura * 100f)}% da saúde!";
    public override Color Cor => new Color(0.4f, 0.9f, 0.45f);

    protected override void AplicarEfeito()
    {
        foreach (Cidadao cidadao in GerenciadorDeCidadaos.Instancia.CidadaosVivos)
        {
            cidadao.Curar(Mathf.CeilToInt(cidadao.SaudeMaxima * _fracaoDeCura), peloJogador: false);
        }
    }
}
