using UnityEngine;
using System;
using System.Collections;

// Gerencia a progressão do jogo em fases: cada fase (ConfiguracaoDeFase)
// tem sua sequência de ondas e, opcionalmente, um chefão no final.
// Inimigos nascem em rajadas atrás das barricadas (pontas e entradas
// laterais definidas no GerenciadorDeRua); o chefão nasce
// num ponto fixo da cena (_pontoDeSpawnChefao). Derrotar o chefão da
// última fase da cena carrega a próxima cena (_proximaCena) ou, se
// não houver, dispara a vitória. Algumas ondas sorteiam um EventoDeOnda
// (chuva de fast food, ataque concentrado, proteja o VIP).
public class GeradorDeInimigos : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabsDeInimigo;
    [SerializeField] private GameObject _prefabDoChefao;
    [SerializeField] private Transform _pontoDeSpawnChefao;
    [SerializeField] private ConfiguracaoDeFase[] _fases;
    [SerializeField] private int _inimigosPorRajada = 8;
    [SerializeField] private float _variacaoLateralDeSpawn = 4f;
    [SerializeField] private float _pausaEntreFases = 5f;
    [SerializeField] private float _pausaEntreOndas = 2.5f;

    [Header("Campanha em várias cenas")]
    [SerializeField] private string _proximaCena = "";
    [SerializeField] private int _numeroDaPrimeiraFase = 1;
    [SerializeField] private int _totalDeFasesDoJogo = 4;
    [SerializeField] private float _esperaAntesDeTrocarCena = 2f;

    [Header("Eventos de onda")]
    [SerializeField, Range(0f, 1f)] private float _chanceDeEvento = 0.35f;

    private int _faseAtual = 0;
    private int _ondaAtual = 0;
    private int _inimigosRestantesNaOnda;
    private bool _chefaoDaFaseJaApareceu = false;
    private EventoDeOnda _eventoAtual;

    public int TotalDeFases => _totalDeFasesDoJogo;

    // (número da onda na fase, total de ondas da fase)
    public event Action<int, int> AoMudarDeOnda;
    // (número da fase, configuração da fase)
    public event Action<int, ConfiguracaoDeFase> AoIniciarFase;
    public event Action<ChefaoDeCaloria> AoSpawnarChefao;
    public event Action<EventoDeOnda> AoIniciarEvento;
    // (mensagem de resultado do evento ao fim da onda)
    public event Action<string, Color> AoTerminarEvento;

    private ConfiguracaoDeFase Fase => _fases[_faseAtual];

    private void Start()
    {
        StartCoroutine(IniciarFase());
    }

    private IEnumerator IniciarFase()
    {
        // Espera 1 frame para garantir que a UI já se inscreveu nos eventos
        // (a ordem dos Start() entre objetos não é garantida).
        yield return null;

        _ondaAtual = 0;
        _chefaoDaFaseJaApareceu = false;
        GerenciadorDeJogo.Instancia.RestaurarEnergia(Fase.EnergiaRecuperadaAoIniciar);
        AoIniciarFase?.Invoke(_numeroDaPrimeiraFase + _faseAtual, Fase);
        GerenciadorDeAudio.Instancia.TrocarMusica(Fase.Musica);

        yield return new WaitForSeconds(_pausaEntreFases);

        IniciarOnda();
    }

    private void IniciarOnda()
    {
        ConfiguracaoDeOnda onda = Fase.Ondas[_ondaAtual];
        AoMudarDeOnda?.Invoke(_ondaAtual + 1, Fase.Ondas.Length);

        int quantidade = Mathf.Max(1, onda.quantidadeDeInimigos);
        float intervalo = onda.intervaloEntreSpawns;

        _eventoAtual = SortearEvento();
        if (_eventoAtual != null)
        {
            quantidade = _eventoAtual.ModificarQuantidade(quantidade);
            intervalo = _eventoAtual.ModificarIntervalo(intervalo);
            _eventoAtual.AoIniciar(PrefabsPermitidosNaFase());
            GerenciadorDeAudio.Instancia.TocarSomEvento();
            AoIniciarEvento?.Invoke(_eventoAtual);
        }
        else
        {
            GerenciadorDeAudio.Instancia.TocarSomOndaInicio();
        }

        _inimigosRestantesNaOnda = quantidade;
        StartCoroutine(SpawnarOnda(quantidade, intervalo));
    }

    // A primeira onda do jogo nunca tem evento, para o jogador aprender o básico.
    private EventoDeOnda SortearEvento()
    {
        bool primeiraOndaDoJogo = _numeroDaPrimeiraFase + _faseAtual == 1 && _ondaAtual == 0;
        if (primeiraOndaDoJogo || UnityEngine.Random.value > _chanceDeEvento) return null;

        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0: return new EventoChuvaDeFastFood();
            case 1: return new EventoAtaqueConcentrado();
            default: return new EventoCidadaoVip();
        }
    }

    private void EncerrarEventoDaOnda()
    {
        if (_eventoAtual == null) return;

        Color cor = _eventoAtual.Cor;
        string resultado = _eventoAtual.AoTerminar();
        _eventoAtual = null;

        if (!string.IsNullOrEmpty(resultado)) AoTerminarEvento?.Invoke(resultado, cor);
    }

    private IEnumerator SpawnarOnda(int quantidade, float intervalo)
    {
        int inimigosSpawnados = 0;

        while (inimigosSpawnados < quantidade)
        {
            int quantidadeNestaRajada = Mathf.Min(_inimigosPorRajada, quantidade - inimigosSpawnados);

            for (int i = 0; i < quantidadeNestaRajada; i++)
            {
                SpawnarInimigo();
                inimigosSpawnados++;
            }

            yield return new WaitForSeconds(intervalo);
        }
    }

    private void SpawnarInimigo()
    {
        GameObject[] opcoes = PrefabsPermitidosNaFase();
        GameObject prefabEscolhido = _eventoAtual != null
            ? _eventoAtual.EscolherPrefab(opcoes)
            : opcoes[UnityEngine.Random.Range(0, opcoes.Length)];
        GameObject inimigoCriado = Instantiate(prefabEscolhido, PosicaoNaPontaDaRua(), Quaternion.identity);

        Inimigo inimigo = inimigoCriado.GetComponent<Inimigo>();
        AcompanharInimigo(inimigo);
        _eventoAtual?.AoSpawnarInimigo(inimigo);
    }

    private void AcompanharInimigo(Inimigo inimigo)
    {
        inimigo.AoMorrer += RegistrarDerrota;
        inimigo.AoGerarFilhote += RegistrarFilhote;
    }

    // Inimigos criados no meio da onda (ex: mini-hambúrgueres) também
    // precisam ser derrotados para a onda acabar.
    private void RegistrarFilhote(Inimigo filhote)
    {
        _inimigosRestantesNaOnda++;
        AcompanharInimigo(filhote);
    }

    private GameObject[] PrefabsPermitidosNaFase()
    {
        GameObject[] daFase = Fase.PrefabsDeInimigo;
        bool faseDefiniuInimigos = daFase != null && daFase.Length > 0;
        return faseDefiniuInimigos ? daFase : _prefabsDeInimigo;
    }

    private Vector3 PosicaoNaPontaDaRua()
    {
        Vector3 posicao = GerenciadorDeRua.Instancia.ObterPontoDeSpawn(_variacaoLateralDeSpawn);
        posicao.y = 1f;
        return posicao;
    }

    private void RegistrarDerrota()
    {
        _inimigosRestantesNaOnda--;

        if (_inimigosRestantesNaOnda > 0) return;

        AvancarOnda();
    }

    private void AvancarOnda()
    {
        EncerrarEventoDaOnda();
        GerenciadorDeCidadaos.Instancia.AoTerminarOnda();
        GerenciadorDeAudio.Instancia.TocarSomOndaCompleta();

        bool eUltimaOnda = _ondaAtual >= Fase.Ondas.Length - 1;

        if (!eUltimaOnda)
        {
            _ondaAtual++;
            StartCoroutine(ExecutarAposPausa(IniciarOnda));
            return;
        }

        if (Fase.TemChefao && !_chefaoDaFaseJaApareceu)
        {
            _chefaoDaFaseJaApareceu = true;
            StartCoroutine(ExecutarAposPausa(SpawnarChefao));
            return;
        }

        ConcluirFase();
    }

    // Dá um respiro ao jogador entre uma onda e a próxima (ou o chefão).
    private IEnumerator ExecutarAposPausa(Action acao)
    {
        yield return new WaitForSeconds(_pausaEntreOndas);
        acao();
    }

    private void SpawnarChefao()
    {
        GameObject chefaoCriado = Instantiate(_prefabDoChefao, _pontoDeSpawnChefao.position, Quaternion.identity);

        ChefaoDeCaloria chefao = chefaoCriado.GetComponent<ChefaoDeCaloria>();
        if (Fase.Chefao != null)
        {
            chefao.Configurar(Fase.Chefao, PrefabsPermitidosNaFase());
        }
        else
        {
            chefao.ConfigurarVida(Fase.VidaDoChefao);
            chefao.ConfigurarHabilidades(Fase.HabilidadesDoChefao, PrefabsPermitidosNaFase());
        }
        chefao.AoMorrer += ConcluirFase;
        GerenciadorDeAudio.Instancia.TocarSomChefao();

        GerenciadorDeAudio.Instancia.TocarMusicaDoChefao();
        AoSpawnarChefao?.Invoke(chefao);
    }

    private void ConcluirFase()
    {
        bool eUltimaFase = _faseAtual >= _fases.Length - 1;

        if (!eUltimaFase)
        {
            _faseAtual++;
            StartCoroutine(IniciarFase());
            return;
        }

        if (string.IsNullOrEmpty(_proximaCena))
        {
            GerenciadorDeJogo.Instancia.RegistrarVitoria();
            return;
        }

        StartCoroutine(TrocarDeCena());
    }

    // Entre uma cena e outra roda o quiz de nutrição (se houver na cena).
    private IEnumerator TrocarDeCena()
    {
        yield return new WaitForSeconds(_esperaAntesDeTrocarCena);

        if (GerenciadorDeQuiz.Instancia == null)
        {
            GerenciadorDeJogo.Instancia.AvancarParaCena(_proximaCena);
            yield break;
        }

        GerenciadorDeQuiz.Instancia.IniciarQuiz(() => GerenciadorDeJogo.Instancia.AvancarParaCena(_proximaCena));
    }
}
