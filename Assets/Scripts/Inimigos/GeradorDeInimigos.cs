using UnityEngine;
using System;
using System.Collections;

// Gerencia a progressão de ondas: spawna inimigos conforme a configuração
// da onda atual e avança para a próxima quando todos os inimigos daquela
// onda são derrotados ou passam do tempo de spawn.
public class GeradorDeInimigos : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabsDeInimigo;
    [SerializeField] private float _raioDeSpawn = 15f;
    [SerializeField] private ConfiguracaoDeOnda[] _ondas;

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

        for (int i = 0; i < onda.quantidadeDeInimigos; i++)
        {
            SpawnarInimigo();
            yield return new WaitForSeconds(onda.intervaloEntreSpawns);
        }
    }

    private void SpawnarInimigo()
    {
        Vector3 posicao = PosicaoAleatoriaNaBorda();
        GameObject prefabEscolhido = _prefabsDeInimigo[UnityEngine.Random.Range(0, _prefabsDeInimigo.Length)];
        GameObject inimigoCriado = Instantiate(prefabEscolhido, posicao, Quaternion.identity);

        Inimigo inimigo = inimigoCriado.GetComponent<Inimigo>();
        inimigo.AoMorrer += RegistrarDerrota;
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

    private Vector3 PosicaoAleatoriaNaBorda()
    {
        float angulo = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float raioVariado = _raioDeSpawn + UnityEngine.Random.Range(-3f, 3f);
        float x = Mathf.Cos(angulo) * raioVariado;
        float z = Mathf.Sin(angulo) * raioVariado;
        return new Vector3(x, 1f, z);
    }
}