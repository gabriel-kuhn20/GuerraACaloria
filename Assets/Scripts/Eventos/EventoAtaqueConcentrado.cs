using UnityEngine;

// Todos os inimigos da onda são do mesmo tipo — e valem o dobro de pontos.
public class EventoAtaqueConcentrado : EventoDeOnda
{
    private GameObject _prefabEscolhido;

    public override string Nome => "ATAQUE CONCENTRADO";
    public override string Descricao => $"Só {NomeDoInimigo()} nesta onda — pontos em dobro!";
    public override Color Cor => new Color(0.55f, 0.75f, 1f);

    public override void AoIniciar(GameObject[] prefabsDisponiveis)
    {
        _prefabEscolhido = prefabsDisponiveis[Random.Range(0, prefabsDisponiveis.Length)];
        GerenciadorDePontuacao.Instancia.DefinirBonusDaOnda(2);
    }

    public override GameObject EscolherPrefab(GameObject[] opcoes) => _prefabEscolhido;

    public override string AoTerminar()
    {
        GerenciadorDePontuacao.Instancia.DefinirBonusDaOnda(1);
        return null;
    }

    // "RefrigeranteZumbi" -> "Refrigerante Zumbi"
    private string NomeDoInimigo()
    {
        if (_prefabEscolhido == null) return "um tipo de inimigo";
        return System.Text.RegularExpressions.Regex.Replace(_prefabEscolhido.name, "(?<=[a-z])(?=[A-Z])", " ");
    }
}
