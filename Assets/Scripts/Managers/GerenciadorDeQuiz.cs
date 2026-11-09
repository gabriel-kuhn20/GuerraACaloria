using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Quiz rápido de nutrição entre as fases: pausa o jogo, faz algumas
// perguntas sorteadas do BancoDePerguntas e recompensa cada acerto com
// energia para a cidade e pontos. A parte visual fica no PainelDoQuiz.
// Singleton, como os demais managers.
public class GerenciadorDeQuiz : MonoBehaviour
{
    public static GerenciadorDeQuiz Instancia { get; private set; }

    [SerializeField] private BancoDePerguntas _banco;
    [SerializeField] private PainelDoQuiz _painel;
    [SerializeField] private int _perguntasPorQuiz = 3;
    [SerializeField] private int _energiaPorAcerto = 10;
    [SerializeField] private int _pontosPorAcerto = 300;
    [SerializeField] private float _tempoParaLerResposta = 3f;

    private bool _quizAberto;
    private int _respostaEscolhida = -1;

    public bool QuizAberto => _quizAberto;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    // Executa o quiz e chama aoTerminar no final (ex: carregar a próxima fase).
    public void IniciarQuiz(Action aoTerminar)
    {
        if (_banco == null || _painel == null || _banco.Perguntas.Count == 0)
        {
            aoTerminar?.Invoke();
            return;
        }

        StartCoroutine(ExecutarQuiz(aoTerminar));
    }

    private IEnumerator ExecutarQuiz(Action aoTerminar)
    {
        _quizAberto = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        List<PerguntaDoQuiz> perguntas = SortearPerguntas();
        int acertos = 0;
        _painel.Mostrar();

        for (int i = 0; i < perguntas.Count; i++)
        {
            _respostaEscolhida = -1;
            _painel.MostrarPergunta(perguntas[i], i + 1, perguntas.Count, RegistrarResposta);

            // WaitUntil e WaitForSecondsRealtime funcionam com o tempo pausado.
            yield return new WaitUntil(() => _respostaEscolhida >= 0);

            bool acertou = _respostaEscolhida == perguntas[i].IndiceCorreto;
            if (acertou)
            {
                acertos++;
                GerenciadorDeJogo.Instancia.RestaurarEnergia(_energiaPorAcerto);
                GerenciadorDePontuacao.Instancia.AdicionarBonus(_pontosPorAcerto);
                GerenciadorDeAudio.Instancia.TocarSomPowerUp();
            }
            else
            {
                GerenciadorDeAudio.Instancia.TocarSomErro();
            }

            _painel.MostrarResultado(acertou, _respostaEscolhida, perguntas[i]);
            yield return new WaitForSecondsRealtime(_tempoParaLerResposta);
        }

        _painel.MostrarResumo(acertos, perguntas.Count, acertos * _energiaPorAcerto, acertos * _pontosPorAcerto);
        yield return new WaitForSecondsRealtime(_tempoParaLerResposta);

        _painel.Esconder();
        _quizAberto = false;
        Time.timeScale = 1f;
        aoTerminar?.Invoke();
    }

    private void RegistrarResposta(int indice)
    {
        if (_respostaEscolhida < 0) _respostaEscolhida = indice;
    }

    // Embaralha (Fisher-Yates) e pega as primeiras, sem repetir pergunta.
    private List<PerguntaDoQuiz> SortearPerguntas()
    {
        List<PerguntaDoQuiz> todas = new List<PerguntaDoQuiz>(_banco.Perguntas);
        for (int i = todas.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (todas[i], todas[j]) = (todas[j], todas[i]);
        }

        int quantidade = Mathf.Min(_perguntasPorQuiz, todas.Count);
        return todas.GetRange(0, quantidade);
    }
}
