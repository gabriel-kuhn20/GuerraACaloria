using UnityEngine;

// Responsável por spawnar inimigos periodicamente em pontos aleatórios
// na borda da praça, alternando entre os 3 tipos disponíveis.
// Versão inicial da demo: spawn contínuo simples, ainda sem sistema
// de ondas com dificuldade progressiva.
public class GeradorDeInimigos : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabsDeInimigo;
    [SerializeField] private float _intervaloEntreSpawns = 2f;
    [SerializeField] private float _raioDeSpawn = 15f;

    private float _proximoSpawn;

    private void Update()
    {
        if (Time.time >= _proximoSpawn)
        {
            SpawnarInimigo();
            _proximoSpawn = Time.time + _intervaloEntreSpawns;
        }
    }

    private void SpawnarInimigo()
    {
        Vector3 posicao = PosicaoAleatoriaNaBorda();
        GameObject prefabEscolhido = _prefabsDeInimigo[Random.Range(0, _prefabsDeInimigo.Length)];
        Instantiate(prefabEscolhido, posicao, Quaternion.identity);
    }

    private Vector3 PosicaoAleatoriaNaBorda()
    {
        float angulo = Random.Range(0f, Mathf.PI * 2f);
        float x = Mathf.Cos(angulo) * _raioDeSpawn;
        float z = Mathf.Sin(angulo) * _raioDeSpawn;
        return new Vector3(x, 1f, z);
    }
}
