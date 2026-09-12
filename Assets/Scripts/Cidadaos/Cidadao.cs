using UnityEngine;

// Representa um cidadão da praça, alvo perseguido pelos inimigos.
// Anda sozinho por pontos aleatórios dentro de um raio (patrulha simples).
// Não tem vida própria — o dano real é aplicado na energia da cidade
// via GerenciadorDeJogo quando um inimigo o alcança.
public class Cidadao : MonoBehaviour
{
    [SerializeField] private float _velocidade = 1.5f;
    [SerializeField] private float _raioDePatrulha = 8f;
    [SerializeField] private float _distanciaParaNovoDestino = 0.5f;

    private Vector3 _posicaoInicial;
    private Vector3 _destinoAtual;

    private void OnEnable()
    {
        GerenciadorDeCidadaos.Instancia.RegistrarCidadao(this);
    }

    private void OnDisable()
    {
        GerenciadorDeCidadaos.Instancia.RemoverCidadao(this);
    }

    private void Start()
    {
        _posicaoInicial = transform.position;
        EscolherNovoDestino();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        MoverEmDirecaoAoDestino();

        if (Vector3.Distance(transform.position, _destinoAtual) < _distanciaParaNovoDestino)
        {
            EscolherNovoDestino();
        }
    }

    private void MoverEmDirecaoAoDestino()
    {
        Vector3 direcao = (_destinoAtual - transform.position).normalized;
        transform.position += direcao * _velocidade * Time.deltaTime;

        if (direcao != Vector3.zero)
        {
            transform.forward = direcao;
        }
    }

    private void EscolherNovoDestino()
    {
        Vector2 pontoAleatorio = Random.insideUnitCircle * _raioDePatrulha;
        _destinoAtual = _posicaoInicial + new Vector3(pontoAleatorio.x, 0f, pontoAleatorio.y);
    }
}