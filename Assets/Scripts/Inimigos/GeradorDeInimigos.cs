using UnityEngine;
using System;
using System.Collections;

// Gerencia a progressão de ondas: spawna inimigos em grupos (rajadas),
// sempre à frente do jogador (dentro de um arco), conforme a
// configuração da onda atual.
public class GeradorDeInimigos : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabsDeInimigo;
    [SerializeField] private Transform _jogador;
    [SerializeField] private float _distanciaDeSpawn = 20f;
    [SerializeField] private float _anguloDeVariacao = 30f;
    [SerializeField] private ConfiguracaoDeOnda[] _ondas;
    [SerializeField] private int _inimigosPorRajada = 3;

    private int _ondaAtual = 0;
    private int _inimigosRestantesNaOnda;

    public event Action<int> AoMudarDeOnda;

    private void Start()
    {
        IniciarProximaOnda();
    }

    private void IniciarProximaOnda()
    {
        if (_ondaAtual >= _ondas.Length)
        {
            return;
        }

        AoMudarDeOnda?.Invoke(_ondaAtual + 1);
        _inimigosRestantesNaOnda = _ondas[_ondaAtual].quantidadeDeInimigos;
        StartCoroutine(SpawnarOnda());
    }

    private IEnumerator SpawnarOnda()
    {
        ConfiguracaoDeOnda onda = _ondas[_ondaAtual];
        int inimigosSpawnados = 0;

        while (inimigosSpawnados < onda.quantidadeDeInimigos)
        {
            int quantidadeNestaRajada = Mathf.Min(_inimigosPorRajada, onda.quantidadeDeInimigos - inimigosSpawnados);

            for (int i = 0; i < quantidadeNestaRajada; i++)
            {
                SpawnarInimigo();
                inimigosSpawnados++;
            }

            yield return new WaitForSeconds(onda.intervaloEntreSpawns);
        }
    }

    private void SpawnarInimigo()
    {
        Vector3 posicao = PosicaoAleatoriaAFrenteDoJogador();
        GameObject prefabEscolhido = _prefabsDeInimigo[UnityEngine.Random.Range(0, _prefabsDeInimigo.Length)];
        GameObject inimigoCriado = Instantiate(prefabEscolhido, posicao, Quaternion.identity);

        Inimigo inimigo = inimigoCriado.GetComponent<Inimigo>();
        inimigo.AoMorrer += RegistrarDerrota;
    }

    private Vector3 PosicaoAleatoriaAFrenteDoJogador()
    {
        float anguloAleatorio = UnityEngine.Random.Range(-_anguloDeVariacao, _anguloDeVariacao);
        Quaternion rotacao = Quaternion.Euler(0f, anguloAleatorio, 0f);
        Vector3 direcaoAFrente = rotacao * _jogador.forward;

        Vector3 posicao = _jogador.position + direcaoAFrente * _distanciaDeSpawn;
        posicao.y = 1f;
        return posicao;
    }

    private void RegistrarDerrota()
    {
        _inimigosRestantesNaOnda--;

        if (_inimigosRestantesNaOnda <= 0)
        {
            _ondaAtual++;
            IniciarProximaOnda();
        }
    }
}