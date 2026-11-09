using System;
using System.Collections.Generic;
using UnityEngine;

// Uma pergunta do quiz de nutrição: enunciado, alternativas, qual é a
// certa e uma explicação curta mostrada depois da resposta.
[Serializable]
public class PerguntaDoQuiz
{
    [SerializeField] private string _enunciado;
    [SerializeField] private string[] _alternativas = new string[3];
    [SerializeField] private int _indiceCorreto;
    [TextArea(2, 4)]
    [SerializeField] private string _explicacao;

    public string Enunciado => _enunciado;
    public IReadOnlyList<string> Alternativas => _alternativas;
    public int IndiceCorreto => _indiceCorreto;
    public string Explicacao => _explicacao;
}
