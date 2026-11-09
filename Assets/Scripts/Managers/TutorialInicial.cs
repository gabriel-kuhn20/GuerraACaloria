using System.Collections;
using UnityEngine;

// Mostra dicas rápidas no começo da Fase 1 para o jogador aprender os
// controles e a regra das fraquezas. Usa a faixa de avisos do UIManager.
// Só existe na primeira cena de jogo (SampleScene).
public class TutorialInicial : MonoBehaviour
{
    [SerializeField] private UIManager _ui;
    [SerializeField] private float _atrasoInicial = 5.5f;
    [SerializeField] private float _duracaoDeCadaDica = 4.5f;

    private static readonly string[] Titulos =
    {
        "BEM-VINDO, AGENTE DA VITALIDADE!",
        "TROQUE DE MUNIÇÃO",
        "ACERTE A FRAQUEZA",
        "COMBOS E ITENS",
        "PRECISA DE UMA PAUSA?",
    };

    private static readonly string[] Detalhes =
    {
        "W A S D para andar e o mouse para mirar. Proteja os cidadãos!",
        "Teclas 1 Água, 2 Fruta e 3 Energia Positiva.",
        "Mire em um inimigo: a cor da mira mostra a munição certa.",
        "Acertos seguidos multiplicam os pontos. Pegue os itens saudáveis!",
        "Aperte Esc para pausar e mudar as opções.",
    };

    private void Start()
    {
        StartCoroutine(MostrarDicas());
    }

    private IEnumerator MostrarDicas()
    {
        yield return new WaitForSeconds(_atrasoInicial);

        for (int i = 0; i < Titulos.Length; i++)
        {
            _ui.MostrarDica(Titulos[i], Detalhes[i], _duracaoDeCadaDica);
            yield return new WaitForSeconds(_duracaoDeCadaDica);
        }
    }
}
