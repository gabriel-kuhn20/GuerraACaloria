using UnityEngine;
using UnityEngine.InputSystem;

// Controla o disparo da luva tecnológica do Agente da Vitalidade.
// Fica anexado à Main Camera; instancia um projétil na direção da mira,
// que viaja até colidir com um alvo. Com o power-up Suco Natural ativo,
// segurar o botão dispara em rajada. Também detecta qual inimigo está
// na mira (inimigo ou cidadão contaminado), para a UI colorir a mira.
public class WeaponController : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private GameObject _prefabProjetil;
    [SerializeField] private Transform _pontoDeDisparo;
    [SerializeField] private float _intervaloTiroRapido = 0.09f;
    [SerializeField] private float _alcanceDaMira = 150f;
    [SerializeField] private float _intervaloDaMira = 0.05f;
    [SerializeField] private float _anguloEntreProjeteis = 6f;
    private TipoMunicao _municaoAtual = TipoMunicao.Agua;
    private float _proximoTiroAutomatico;
    private float _proximaVerificacaoDaMira;
    private IAlvoDaMira _alvoNaMira;

    public IAlvoDaMira AlvoNaMira => _alvoNaMira;

    private void Start()
    {
        _uiManager.AtualizarTextoMunicao(_municaoAtual);
    }

    private void Update()
    {
        if (Time.timeScale == 0f || GerenciadorDeMelhorias.EscolhaRecemFechada) return;

        TrocarMunicaoPorTeclado();
        AtualizarMira();

        if (Mouse.current.leftButton.wasPressedThisFrame || PodeDispararEmRajada())
        {
            Atirar();
        }
    }

    // Verifica 20x por segundo (não todo frame) o que está no centro da tela.
    private void AtualizarMira()
    {
        if (Time.time < _proximaVerificacaoDaMira) return;
        _proximaVerificacaoDaMira = Time.time + _intervaloDaMira;

        _alvoNaMira = null;
        bool encontrou = Physics.SphereCast(transform.position, 0.4f, transform.forward, out RaycastHit impacto,
            _alcanceDaMira, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        if (encontrou) _alvoNaMira = impacto.collider.GetComponentInParent<IAlvoDaMira>();
    }

    // Rajada vem do power-up Suco (temporário) ou da melhoria Rajada Natural (permanente).
    private bool PodeDispararEmRajada()
    {
        bool temRajada = GerenciadorDePowerUps.Instancia.TiroRapidoAtivo || GerenciadorDeMelhorias.Atributos.TemTiroAutomatico;
        return temRajada
            && Mouse.current.leftButton.isPressed
            && Time.time >= _proximoTiroAutomatico;
    }

    private float IntervaloDaRajada()
    {
        AtributosDoJogador atributos = GerenciadorDeMelhorias.Atributos;
        float intervalo = atributos.TemTiroAutomatico ? atributos.IntervaloDeTiroAutomatico : float.MaxValue;
        if (GerenciadorDePowerUps.Instancia.TiroRapidoAtivo) intervalo = Mathf.Min(intervalo, _intervaloTiroRapido);
        return intervalo;
    }

    private void TrocarMunicaoPorTeclado()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) DefinirMunicao(TipoMunicao.Agua);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) DefinirMunicao(TipoMunicao.Fruta);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) DefinirMunicao(TipoMunicao.EnergiaPositiva);
    }

    private void DefinirMunicao(TipoMunicao municao)
    {
        if (municao != _municaoAtual) GerenciadorDeAudio.Instancia.TocarSomTrocaMunicao(municao);
        _municaoAtual = municao;
        _uiManager.AtualizarTextoMunicao(_municaoAtual);
    }

    private void Atirar()
    {
        Transform origem = _pontoDeDisparo != null ? _pontoDeDisparo : transform;
        AtributosDoJogador atributos = GerenciadorDeMelhorias.Atributos;
        GerenciadorDePowerUps powerUps = GerenciadorDePowerUps.Instancia;
        int quantidade = 1 + atributos.ProjeteisExtras + powerUps.ProjeteisExtrasTemporarios;
        int perfuracao = atributos.Perfuracao + powerUps.PerfuracaoTemporaria;

        // Projéteis extras (melhoria Tiro Múltiplo) saem em leque, centrados na mira.
        for (int i = 0; i < quantidade; i++)
        {
            float angulo = (i - (quantidade - 1) / 2f) * _anguloEntreProjeteis;
            Quaternion rotacao = origem.rotation * Quaternion.Euler(0f, angulo, 0f);
            GameObject projetilCriado = Instantiate(_prefabProjetil, origem.position, rotacao);
            projetilCriado.GetComponent<Projetil>().Configurar(_municaoAtual, perfuracao);
        }

        GerenciadorDeAudio.Instancia.TocarSomTiro(_municaoAtual);
        _proximoTiroAutomatico = Time.time + IntervaloDaRajada();
    }
}