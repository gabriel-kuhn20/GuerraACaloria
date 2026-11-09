using UnityEngine;

// Classe base de uma melhoria escolhida ao atingir uma pontuação.
// Melhorias são acumulativas: cada escolha sobe o nível em 1 (até o
// máximo) e o efeito soma nos AtributosDoJogador. Cada subclasse define
// nome, cor, descrição e como contribui para os atributos.
public abstract class Melhoria
{
    public abstract string Id { get; }
    public abstract string Nome { get; }
    public abstract Color Cor { get; }
    public virtual int NivelMaximo => 3;

    // Texto do card: o que o jogador ganha ao subir para "proximoNivel".
    public abstract string Descricao(int proximoNivel);

    // Soma o efeito deste nível nos atributos (chamado para cada melhoria
    // com nível > 0 sempre que os atributos são recalculados).
    public abstract void Aplicar(AtributosDoJogador atributos, int nivel);
}
