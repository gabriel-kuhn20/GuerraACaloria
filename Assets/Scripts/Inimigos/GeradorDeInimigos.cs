using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

// Gerencia a progressão de ondas: spawna inimigos em grupos (rajadas),
// sempre na ponta fixa "de trás" da rua. Após as ondas configuradas
// terminarem, repete o ciclo aumentando a dificuldade indefinidamente.
// Ao final da Onda3 do primeiro ciclo, spawna o ChefaoDeCaloria depois
// que todos os inimigos comuns dela forem derrotados — a onda só avança
// de verdade quando o chefão também for derrotado. O chefão nasce sempre
// num ponto fixo definido manualmente na cena (_pontoDeSpawnChefao).
public class GeradorDeInimigos : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabsDeInimigo;
    [SerializeField] private GameObject _prefabDoChefao;
    [SerializeField] private Transform _pontoDeSpawnChefao;
    [SerializeField] private ConfiguracaoDeOnda[] _ondas;
    [SerializeField] private int _inimigosPorRajada = 8;
    [SerializeField] private float _variacaoLateralDeSpawn = 4f;
    [SerializeField] private float _multiplicadorDificuldadePorCiclo = 1.8f;

    private int _ondaAtual = 0;
    private int _ciclosCompletos = 0;
    private int _inimigosRestantesNaOnda;
    private bool _chefaoJaApareceu = false;

    public event Action<int> AoMudarDeOnda;
    public event Action<ChefaoDeCaloria> AoSpawnarChefao;

    private void Start()
    {
        IniciarProximaOnda();
    }

#if UNITY_EDITOR
    // Tecla de debug: pressione B pra spawnar o chefão na hora, sem
    // precisar zerar as 2 ondas anteriores. Só compila no Editor.
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            _chefaoJaApareceu = false;
            SpawnarChefao();
        }
    }
#endif

    private void IniciarProximaOnda()
    {
        AoMudarDeOnda?.Invoke(NumeroDaOndaExibido());

        ConfiguracaoDeOnda onda = _ondas[_ondaAtual];
        float multiplicador = Mathf.Pow(_multiplicadorDificuldadePorCiclo, _ciclosCompletos);

        int quantidade = Mathf.RoundToInt(onda.quantidadeDeInimigos * multiplicador);
        float intervalo = onda.intervaloEntreSpawns / multiplicador;

        _inimigosRestantesNaOnda = quantidade;
        StartCoroutine(SpawnarOnda(quantidade, intervalo));
    }

    private int NumeroDaOndaExibido()
    {
        return (_ciclosCompletos * _ondas.Length) + _ondaAtual + 1;
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

    private bool VerificarSpawnDoChefao()
    {
        bool eUltimaOndaDoCiclo = _ondaAtual == _ondas.Length - 1;
        bool ePrimeiroCiclo = _ciclosCompletos == 0;

        if (eUltimaOndaDoCiclo && ePrimeiroCiclo && !_chefaoJaApareceu)
        {
            _chefaoJaApareceu = true;
            SpawnarChefao();
            return true;
        }

        return false;
    }

    private void SpawnarChefao()
    {
        Debug.Log($"[GeradorDeInimigos] Spawnando chefao em: {_pontoDeSpawnChefao.position}");
        GameObject chefaoCriado = Instantiate(_prefabDoChefao, _pontoDeSpawnChefao.position, Quaternion.identity);
        Debug.Log($"[GeradorDeInimigos] Posicao real apos Instantiate: {chefaoCriado.transform.position}");

        ChefaoDeCaloria chefao = chefaoCriado.GetComponent<ChefaoDeCaloria>();
        chefao.AoMorrer += RegistrarVitoriaSobreChefao;
        _inimigosRestantesNaOnda++;

        AoSpawnarChefao?.Invoke(chefao);
    }

    private void RegistrarVitoriaSobreChefao()
    {
        GerenciadorDeJogo.Instancia.RegistrarVitoria();
    }

    private void SpawnarInimigo()
    {
        Vector3 posicao = PosicaoNaPontaDaRua();
        GameObject prefabEscolhido = _prefabsDeInimigo[UnityEngine.Random.Range(0, _prefabsDeInimigo.Length)];
        GameObject inimigoCriado = Instantiate(prefabEscolhido, posicao, Quaternion.identity);

        Inimigo inimigo = inimigoCriado.GetComponent<Inimigo>();
        inimigo.AoMorrer += RegistrarDerrota;
    }

    private Vector3 PosicaoNaPontaDaRua()
    {
        Vector3 pontaDaRua = GerenciadorDeRua.Instancia.ObterPontaFinal();
        float variacaoX = UnityEngine.Random.Range(-_variacaoLateralDeSpawn, _variacaoLateralDeSpawn);

        Vector3 posicao = pontaDaRua;
        posicao.x += variacaoX;
        posicao.y = 1f;
        return posicao;
    }

    private void RegistrarDerrota()
    {
        _inimigosRestantesNaOnda--;

        if (_inimigosRestantesNaOnda > 0) return;

        if (VerificarSpawnDoChefao()) return;

        AvancarOnda();
    }

    private void AvancarOnda()
    {
        _ondaAtual++;

        if (_ondaAtual >= _ondas.Length)
        {
            _ondaAtual = 0;
            _ciclosCompletos++;
        }

        IniciarProximaOnda();
    }
}