using System.Collections.Generic;
using UnityEngine;

// Lista de perguntas do quiz entre fases. Editável no Inspector: dá para
// adicionar, remover ou corrigir perguntas sem mexer em código.
[CreateAssetMenu(fileName = "BancoDePerguntas", menuName = "GuerraACaloria/BancoDePerguntas")]
public class BancoDePerguntas : ScriptableObject
{
    [SerializeField] private List<PerguntaDoQuiz> _perguntas = new List<PerguntaDoQuiz>();

    public IReadOnlyList<PerguntaDoQuiz> Perguntas => _perguntas;
}
