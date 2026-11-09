using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Parte visual do quiz entre fases: mostra a pergunta, as alternativas
// como botões, o resultado de cada resposta e o resumo final. As regras
// e recompensas ficam no GerenciadorDeQuiz.
public class PainelDoQuiz : MonoBehaviour
{
    [SerializeField] private GameObject _raiz;
    [SerializeField] private TextMeshProUGUI _textoProgresso;
    [SerializeField] private TextMeshProUGUI _textoPergunta;
    [SerializeField] private Button[] _botoes;
    [SerializeField] private TextMeshProUGUI[] _textosDosBotoes;
    [SerializeField] private TextMeshProUGUI _textoFeedback;
    [SerializeField] private Color _corNormal = new Color(0.22f, 0.53f, 0.86f);
    [SerializeField] private Color _corCerta = new Color(0.27f, 0.72f, 0.38f);
    [SerializeField] private Color _corErrada = new Color(0.82f, 0.3f, 0.3f);
    [SerializeField] private Color _corApagada = new Color(0.25f, 0.28f, 0.32f);

    private void Awake()
    {
        _raiz.SetActive(false);
    }

    public void Mostrar() => _raiz.SetActive(true);
    public void Esconder() => _raiz.SetActive(false);

    public void MostrarPergunta(PerguntaDoQuiz pergunta, int numero, int total, Action<int> aoResponder)
    {
        _textoProgresso.text = $"QUIZ SAUDÁVEL  -  PERGUNTA {numero} DE {total}";
        _textoPergunta.text = pergunta.Enunciado;
        _textoFeedback.text = "";

        for (int i = 0; i < _botoes.Length; i++)
        {
            bool existe = i < pergunta.Alternativas.Count;
            _botoes[i].gameObject.SetActive(existe);
            if (!existe) continue;

            int indice = i;
            _textosDosBotoes[i].text = $"{(char)('A' + i)})  {pergunta.Alternativas[i]}";
            _botoes[i].image.color = _corNormal;
            _botoes[i].interactable = true;
            _botoes[i].onClick.RemoveAllListeners();
            _botoes[i].onClick.AddListener(() => aoResponder(indice));
        }
    }

    public void MostrarResultado(bool acertou, int escolhida, PerguntaDoQuiz pergunta)
    {
        for (int i = 0; i < _botoes.Length; i++)
        {
            _botoes[i].interactable = false;

            if (i == pergunta.IndiceCorreto) _botoes[i].image.color = _corCerta;
            else if (i == escolhida) _botoes[i].image.color = _corErrada;
            else _botoes[i].image.color = _corApagada;
        }

        string titulo = acertou ? "<color=#6EE07F>CERTO!</color>" : "<color=#FF6B6B>QUASE!</color>";
        _textoFeedback.text = $"{titulo}\n<size=80%>{pergunta.Explicacao}</size>";
    }

    public void MostrarResumo(int acertos, int total, int energiaGanha, int pontosGanhos)
    {
        _textoProgresso.text = "FIM DO QUIZ";
        _textoPergunta.text = $"Você acertou {acertos} de {total}!";
        foreach (Button botao in _botoes) botao.gameObject.SetActive(false);

        _textoFeedback.text = acertos > 0
            ? $"<color=#6EE07F>+{energiaGanha} de energia</color>   <color=#FFD240>+{pontosGanhos} pontos</color>"
            : "Na próxima você acerta! Bora para a próxima fase.";
    }
}
