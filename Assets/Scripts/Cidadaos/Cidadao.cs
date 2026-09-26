using UnityEngine;

// Representa um cidadão da praça, alvo perseguido pelos inimigos.
// Patrulha dentro de uma zona fixa, definida a partir da posição inicial
// do jogador (atrás dele, no sentido oposto de onde os inimigos nascem).
// Foge de inimigos próximos. Mantém sempre a própria altura.
public class Cidadao : MonoBehaviour
{
    [SerializeField] private float _velocidade = 1.5f;
    [SerializeField] private float _velocidadeDeFuga = 4f;
    [SerializeField] private float _raioDePatrulha = 6f;
    [SerializeField] private float _distanciaParaNovoDestino = 0.5f;
    [SerializeField] private float _velocidadeDeGiro = 4f;
    [SerializeField] private float _raioDeDeteccaoDeInimigo = 5f;
    [SerializeField] private Transform _centroDaZonaDePatrulha;

    private Vector3 _destinoAtual;
    private float _raioIndividual;
    private float _velocidadeIndividual;
    private float _tempoDePausa;
    private Transform _inimigoMaisProximo;
    private float _alturaFixa;

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
        _alturaFixa = transform.position.y;
        _raioIndividual = _raioDePatrulha * Random.Range(0.7f, 1.3f);
        _velocidadeIndividual = _velocidade * Random.Range(0.8f, 1.2f);
        EscolherNovoDestino();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        AtualizarInimigoMaisProximo();

        if (_inimigoMaisProximo != null)
        {
            Fugir();
        }
        else
        {
            Patrulhar();
        }

        ManterAlturaFixa();
        transform.position = GerenciadorDeRua.Instancia.RestringirPosicao(transform.position);
    }

    private void AtualizarInimigoMaisProximo()
    {
        Inimigo[] inimigos = FindObjectsByType<Inimigo>(FindObjectsSortMode.None);
        Transform maisProximo = null;
        float menorDistancia = _raioDeDeteccaoDeInimigo;

        foreach (Inimigo inimigo in inimigos)
        {
            float distancia = Vector3.Distance(transform.position, inimigo.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                maisProximo = inimigo.transform;
            }
        }

        _inimigoMaisProximo = maisProximo;
    }

    private void Fugir()
    {
        Vector3 posicaoInimigoMesmaAltura = new Vector3(_inimigoMaisProximo.position.x, transform.position.y, _inimigoMaisProximo.position.z);
        Vector3 direcaoDeFuga = (transform.position - posicaoInimigoMesmaAltura).normalized;
        transform.position += direcaoDeFuga * _velocidadeDeFuga * Time.deltaTime;
        GirarSuavementePara(direcaoDeFuga);
    }

    private void Patrulhar()
    {
        if (_tempoDePausa > 0f)
        {
            _tempoDePausa -= Time.deltaTime;
            return;
        }

        Vector3 direcao = (_destinoAtual - transform.position).normalized;
        transform.position += direcao * _velocidadeIndividual * Time.deltaTime;
        GirarSuavementePara(direcao);

        if (Vector3.Distance(transform.position, _destinoAtual) < _distanciaParaNovoDestino)
        {
            _tempoDePausa = Random.Range(0.5f, 2f);
            EscolherNovoDestino();
        }
    }

    private void GirarSuavementePara(Vector3 direcao)
    {
        direcao.y = 0f; // evita olhar pro céu/chão ao seguir alvo em altura diferente
        if (direcao.sqrMagnitude < 0.0001f) return;

        Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, _velocidadeDeGiro * Time.deltaTime);
    }
    private void EscolherNovoDestino()
    {
        Vector3 centro = _centroDaZonaDePatrulha != null ? _centroDaZonaDePatrulha.position : transform.position;
        Vector2 pontoAleatorio = Random.insideUnitCircle * _raioIndividual;
        _destinoAtual = centro + new Vector3(pontoAleatorio.x, 0f, pontoAleatorio.y);
    }

    private void ManterAlturaFixa()
    {
        Vector3 posicao = transform.position;
        posicao.y = _alturaFixa;
        transform.position = posicao;
    }
}