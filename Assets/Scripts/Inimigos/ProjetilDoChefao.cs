using UnityEngine;

// Comida gordurosa arremessada pelo chefão no Agente da Vitalidade. Voa
// na direção dele corrigindo um pouco a rota; se encostar, tira vida,
// empurra e deixa o agente lento. Um tiro de qualquer munição destrói o
// arremesso no ar e vale alguns pontos.
public class ProjetilDoChefao : MonoBehaviour, IDanificavel, IAlvoDaMira
{
    [SerializeField] private float _velocidade = 11f;
    [Tooltip("Radianos por segundo que ele consegue virar em direção ao agente.")]
    [SerializeField] private float _correcaoDeRota = 1.6f;
    [SerializeField] private float _tempoDeVida = 6f;
    [SerializeField] private int _dano = 15;
    [SerializeField] private float _raioDeAcerto = 1.2f;
    [SerializeField] private float _fatorDeLentidao = 0.6f;
    [SerializeField] private float _duracaoDaLentidao = 1.5f;
    [SerializeField] private int _pontosAoDestruir = 30;
    [SerializeField] private float _giroVisual = 360f;
    [SerializeField] private Color _cor = new Color(1f, 0.6f, 0.25f);

    private Vector3 _direcao;
    private Transform _alvo;
    private float _fimDaVida;
    private bool _terminou;

    public bool AceitaTiro => true;
    public bool IndicaMunicao => false;
    public TipoMunicao MunicaoIndicada => TipoMunicao.Agua;

    // desvioEmGraus abre o leque quando o chefão arremessa vários de uma vez.
    public void Lancar(Transform alvo, float desvioEmGraus)
    {
        _alvo = alvo;
        Vector3 paraAlvo = alvo.position + Vector3.down * 0.5f - transform.position;
        _direcao = Quaternion.Euler(0f, desvioEmGraus, 0f) * paraAlvo.normalized;
        _fimDaVida = Time.time + _tempoDeVida;
    }

    private void Update()
    {
        if (_terminou || Time.timeScale == 0f) return;

        if (_alvo != null)
        {
            Vector3 desejada = (_alvo.position + Vector3.down * 0.5f - transform.position).normalized;
            _direcao = Vector3.RotateTowards(_direcao, desejada, _correcaoDeRota * Time.deltaTime, 0f);
        }

        transform.position += _direcao * _velocidade * Time.deltaTime;
        transform.Rotate(Vector3.up, _giroVisual * Time.deltaTime, Space.World);

        if (_alvo != null && EncostouNoAlvo()) { Acertar(); return; }
        if (Time.time >= _fimDaVida) Destroy(gameObject);
    }

    private bool EncostouNoAlvo()
    {
        Vector3 diferenca = _alvo.position - transform.position;
        float vertical = Mathf.Abs(diferenca.y);
        diferenca.y = 0f;
        return diferenca.magnitude < _raioDeAcerto && vertical < 2.5f;
    }

    private void Acertar()
    {
        _terminou = true;
        VidaDoAgente agente = VidaDoAgente.Instancia;
        if (agente != null)
        {
            agente.ReceberDano(_dano, transform.position, 1.5f);
            agente.GetComponent<MovimentoJogador>().AplicarLentidao(_fatorDeLentidao, _duracaoDaLentidao);
        }
        GerenciadorDeEfeitos.Instancia.CriarExplosao(transform.position, _cor, 1.2f);
        Destroy(gameObject);
    }

    public void ReceberDano(TipoMunicao municaoUsada)
    {
        if (_terminou) return;
        _terminou = true;

        GerenciadorDeAudio.Instancia.TocarSomAcerto();
        GerenciadorDePontuacao.Instancia.AdicionarBonus(_pontosAoDestruir);
        GerenciadorDeEfeitos.Instancia.CriarExplosao(transform.position, _cor, 0.8f);
        GerenciadorDeEfeitos.Instancia.MostrarPontos(transform.position, _pontosAoDestruir, 1);
        Destroy(gameObject);
    }
}
