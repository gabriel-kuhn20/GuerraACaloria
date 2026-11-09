using UnityEngine;

// Evento especial sorteado em algumas ondas para quebrar a rotina:
// muda quantidade/ritmo dos inimigos, quais aparecem ou dá um objetivo
// extra. Classe C# comum (não é MonoBehaviour): o GeradorDeInimigos cria
// uma instância por onda e chama os pontos de extensão abaixo.
public abstract class EventoDeOnda
{
    public abstract string Nome { get; }
    public abstract string Descricao { get; }
    public virtual Color Cor => new Color(1f, 0.82f, 0.25f);

    public virtual int ModificarQuantidade(int quantidade) => quantidade;
    public virtual float ModificarIntervalo(float intervalo) => intervalo;
    public virtual void AoIniciar(GameObject[] prefabsDisponiveis) { }
    public virtual void AoSpawnarInimigo(Inimigo inimigo) { }

    public virtual GameObject EscolherPrefab(GameObject[] opcoes)
    {
        return opcoes[Random.Range(0, opcoes.Length)];
    }

    // Chamado quando a onda termina. Retorna uma mensagem de resultado
    // para a UI, ou null se não houver nada a mostrar.
    public virtual string AoTerminar() => null;
}
