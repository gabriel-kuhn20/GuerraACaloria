using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Ao atingir certas pontuações, pausa o jogo e oferece 3 melhorias
// sorteadas; o jogador seleciona uma (clique ou teclas 1/2/3) e confirma
// (botão CONFIRMAR ou Enter) — clicar só seleciona. As melhorias
// são acumulativas e valem pelo resto da campanha (guardadas no
// ProgressoDaCampanha ao trocar de fase). Também aplica a regeneração da
// melhoria Hidratação. Singleton, como os demais managers.
public class GerenciadorDeMelhorias : MonoBehaviour
{
    public static GerenciadorDeMelhorias Instancia { get; private set; }

    // Atributos atuais; cenas sem este manager usam os valores padrão.
    public static AtributosDoJogador Atributos => Instancia != null ? Instancia._atributos : AtributosDoJogador.Padrao;

    [SerializeField] private PainelDeMelhorias _painel;
    [Tooltip("Pontos da 1ª oferta; a n-ésima oferta pede este valor × (1 + 2 + ... + n).")]
    [SerializeField] private int _pontosDaPrimeiraOferta = 1500;
    [SerializeField] private int _opcoesPorOferta = 3;
    [Tooltip("Segundos (tempo real) em que cliques/teclas são ignorados ao abrir — o jogador pode estar atirando.")]
    [SerializeField] private float _tempoDeProtecaoAoAbrir = 0.6f;

    private readonly Melhoria[] _todas =
    {
        new MelhoriaTiroMultiplo(),
        new MelhoriaRajadaNatural(),
        new MelhoriaPerfurante(),
        new MelhoriaTenisDeCorrida(),
        new MelhoriaAbracoSaudavel(),
        new MelhoriaHidratacao(),
        new MelhoriaDietaEquilibrada(),
        new MelhoriaForcaDeVontade(),
    };

    private readonly Dictionary<string, int> _niveis = new Dictionary<string, int>();
    private AtributosDoJogador _atributos = new AtributosDoJogador();
    private int _ofertasFeitas;
    private List<Melhoria> _opcoesAtuais;
    private bool _escolhaAberta;
    private float _acumuladoDeRegeneracao;
    private int _frameDoFechamento = -1;
    private int _indiceSelecionado = -1;
    private float _aceitaEntradaAPartirDe;

    public bool EscolhaAberta => _escolhaAberta;
    // A tecla 1/2/3 usada para escolher não deve trocar a munição no mesmo frame.
    public static bool EscolhaRecemFechada => Instancia != null && Instancia._frameDoFechamento == Time.frameCount;
    public int PontosDaProximaOferta => _pontosDaPrimeiraOferta * (_ofertasFeitas + 1) * (_ofertasFeitas + 2) / 2;
    public bool TemMelhoriaDisponivel => ObterDisponiveis().Count > 0;

    public event Action<Melhoria, int> AoEscolherMelhoria;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;

        ProgressoDaCampanha.CarregarMelhorias(_niveis, out _ofertasFeitas);
        RecalcularAtributos();
    }

    private void Update()
    {
        if (_escolhaAberta)
        {
            VerificarTeclasDeEscolha();
            return;
        }

        AplicarRegeneracao();
        if (PodeOferecer()) AbrirOferta();
    }

    public int Nivel(Melhoria melhoria) => _niveis.TryGetValue(melhoria.Id, out int nivel) ? nivel : 0;

    // Só oferece com o jogo rodando (não durante pausa, quiz ou fim de jogo).
    private bool PodeOferecer()
    {
        if (_painel == null || Time.timeScale == 0f) return false;
        if (GerenciadorDeJogo.Instancia.JogoAcabou) return false;
        if (GerenciadorDePontuacao.Instancia.Pontos < PontosDaProximaOferta) return false;
        return TemMelhoriaDisponivel;
    }

    private void AbrirOferta()
    {
        _opcoesAtuais = SortearOpcoes();
        _escolhaAberta = true;
        _indiceSelecionado = -1;
        _aceitaEntradaAPartirDe = Time.unscaledTime + _tempoDeProtecaoAoAbrir;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GerenciadorDeAudio.Instancia.TocarSomMelhoriaAbrir();
        _painel.Mostrar(_opcoesAtuais, this, Selecionar, Confirmar);
    }

    private bool AceitaEntrada => _escolhaAberta && Time.unscaledTime >= _aceitaEntradaAPartirDe;

    // 1/2/3 selecionam; Enter (ou Enter do teclado numérico) confirma.
    private void VerificarTeclasDeEscolha()
    {
        Key[] teclas = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };
        for (int i = 0; i < _opcoesAtuais.Count && i < teclas.Length; i++)
        {
            if (Keyboard.current[teclas[i]].wasPressedThisFrame) { Selecionar(i); return; }
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame) Confirmar();
    }

    private void Selecionar(int indice)
    {
        if (!AceitaEntrada || indice < 0 || indice >= _opcoesAtuais.Count) return;

        _indiceSelecionado = indice;
        _painel.Destacar(indice);
        GerenciadorDeAudio.Instancia.TocarSomClique();
    }

    private void Confirmar()
    {
        if (!AceitaEntrada || _indiceSelecionado < 0) return;
        Escolher(_indiceSelecionado);
    }

    private void Escolher(int indice)
    {
        if (!_escolhaAberta || indice < 0 || indice >= _opcoesAtuais.Count) return;

        Melhoria escolhida = _opcoesAtuais[indice];
        int novoNivel = Nivel(escolhida) + 1;
        _niveis[escolhida.Id] = novoNivel;
        _ofertasFeitas++;
        RecalcularAtributos();

        _escolhaAberta = false;
        _frameDoFechamento = Time.frameCount;
        _painel.Esconder();
        GerenciadorDeAudio.Instancia.TocarSomMelhoriaEscolher();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        AoEscolherMelhoria?.Invoke(escolhida, novoNivel);
    }

    // Chamado pelo GerenciadorDeJogo ao avançar de fase.
    public void SalvarNaCampanha()
    {
        ProgressoDaCampanha.SalvarMelhorias(_niveis, _ofertasFeitas);
    }

    private List<Melhoria> ObterDisponiveis()
    {
        List<Melhoria> disponiveis = new List<Melhoria>();
        foreach (Melhoria melhoria in _todas)
        {
            if (Nivel(melhoria) < melhoria.NivelMaximo) disponiveis.Add(melhoria);
        }
        return disponiveis;
    }

    // Embaralha (Fisher-Yates) as disponíveis e pega as primeiras.
    private List<Melhoria> SortearOpcoes()
    {
        List<Melhoria> disponiveis = ObterDisponiveis();
        for (int i = disponiveis.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (disponiveis[i], disponiveis[j]) = (disponiveis[j], disponiveis[i]);
        }
        return disponiveis.GetRange(0, Mathf.Min(_opcoesPorOferta, disponiveis.Count));
    }

    private void RecalcularAtributos()
    {
        _atributos = new AtributosDoJogador();
        foreach (Melhoria melhoria in _todas)
        {
            int nivel = Nivel(melhoria);
            if (nivel > 0) melhoria.Aplicar(_atributos, nivel);
        }
    }

    private void AplicarRegeneracao()
    {
        if (_atributos.RegeneracaoPorSegundo <= 0 || GerenciadorDeJogo.Instancia.JogoAcabou) return;

        _acumuladoDeRegeneracao += _atributos.RegeneracaoPorSegundo * Time.deltaTime;
        if (_acumuladoDeRegeneracao < 1f) return;

        int cura = Mathf.FloorToInt(_acumuladoDeRegeneracao);
        _acumuladoDeRegeneracao -= cura;
        GerenciadorDeCidadaos.Instancia.CurarMaisNecessitados(cura);
    }
}
