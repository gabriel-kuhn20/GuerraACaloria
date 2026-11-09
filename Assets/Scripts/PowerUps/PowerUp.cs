using UnityEngine;

// Classe base dos itens saudáveis que caem de inimigos derrotados.
// Gira e flutua no lugar; é coletado quando o jogador encosta nele ou
// quando é atingido por um tiro (por isso implementa IDanificavel).
// Pisca nos últimos segundos e some se não for pego a tempo.
// Cada subclasse define nome, cor e o efeito ao ser coletado.
public abstract class PowerUp : MonoBehaviour, IDanificavel
{
    [SerializeField] private float _raioDeColeta = 3f;
    [SerializeField] private float _tempoDeVida = 15f;
    [SerializeField] private float _velocidadeDeGiro = 90f;
    [SerializeField] private float _amplitudeDeFlutuacao = 0.25f;

    private Vector3 _posicaoInicial;
    private float _fimDaVida;
    private bool _foiColetado;
    private Renderer[] _renderers;

    public bool AceitaTiro => true;
    public abstract string Nome { get; }
    public abstract string Descricao { get; }
    public abstract Color Cor { get; }

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
    }

    private void Start()
    {
        _posicaoInicial = transform.position;
        _fimDaVida = Time.time + _tempoDeVida;
    }

    private void Update()
    {
        Animar();
        VerificarColetaPeloJogador();

        if (Time.time >= _fimDaVida) Destroy(gameObject);
    }

    private void Animar()
    {
        transform.Rotate(Vector3.up, _velocidadeDeGiro * Time.deltaTime, Space.World);
        transform.position = _posicaoInicial + Vector3.up * Mathf.Sin(Time.time * 2.5f) * _amplitudeDeFlutuacao;

        bool ultimosSegundos = _fimDaVida - Time.time < 3f;
        bool visivel = !ultimosSegundos || Mathf.Repeat(Time.time * 6f, 1f) > 0.35f;
        foreach (Renderer r in _renderers) r.enabled = visivel;
    }

    private void VerificarColetaPeloJogador()
    {
        Transform jogador = GerenciadorDePowerUps.Instancia.Jogador;
        if (jogador == null) return;

        Vector3 diferenca = jogador.position - transform.position;
        diferenca.y = 0f;
        if (diferenca.magnitude <= _raioDeColeta) Coletar();
    }

    public void ReceberDano(TipoMunicao municaoUsada)
    {
        Coletar();
    }

    private void Coletar()
    {
        if (_foiColetado) return;
        _foiColetado = true;

        AplicarEfeito();
        GerenciadorDePowerUps.Instancia.NotificarColeta(this);
        Destroy(gameObject);
    }

    protected abstract void AplicarEfeito();
}
