using System;
using UnityEngine;

// Vida do Agente da Vitalidade (o jogador). Inimigos que encostam nele e
// os arremessos do chefão tiram vida, empurram e quebram o combo. Se a
// vida zera, o agente fica nocauteado por alguns segundos (não anda nem
// atira) e volta com a vida cheia — não é game over: o objetivo continua
// sendo proteger os cidadãos. Fora de combate, regenera aos poucos.
// Fica no objeto do jogador, junto do MovimentoJogador.
[RequireComponent(typeof(MovimentoJogador))]
public class VidaDoAgente : MonoBehaviour
{
    public static VidaDoAgente Instancia { get; private set; }

    [SerializeField] private int _vidaMaxima = 100;
    [SerializeField] private float _invulnerabilidadeAposDano = 0.6f;
    [SerializeField] private float _duracaoDoNocaute = 3f;
    [Tooltip("Segundos sem levar dano até começar a regenerar.")]
    [SerializeField] private float _esperaParaRegenerar = 4f;
    [SerializeField] private float _regeneracaoPorSegundo = 8f;
    [SerializeField] private float _forcaDoEmpurrao = 7f;
    [SerializeField] private float _tremorAoLevarDano = 0.25f;

    private float _vida;
    private float _fimDaInvulnerabilidade;
    private float _fimDoNocaute;
    private float _ultimoDano = -999f;
    private MovimentoJogador _movimento;
    private TremorDeCamera _tremor;

    public float Vida => _vida;
    public int VidaMaxima => _vidaMaxima;
    public float Fracao => _vida / _vidaMaxima;
    public bool EstaNocauteado => Time.time < _fimDoNocaute;

    public event Action AoReceberDano;
    public event Action AoNocautear;
    public event Action AoRecuperar;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(this); return; }
        Instancia = this;
        _vida = _vidaMaxima;
        _movimento = GetComponent<MovimentoJogador>();
    }

    private void Update()
    {
        if (EstaNocauteado || _vida >= _vidaMaxima) return;
        if (Time.time - _ultimoDano < _esperaParaRegenerar) return;

        _vida = Mathf.Min(_vidaMaxima, _vida + _regeneracaoPorSegundo * Time.deltaTime);
    }

    // origem = de onde veio o golpe (o empurrão vai na direção oposta).
    public void ReceberDano(int dano, Vector3 origem, float multiplicadorDoEmpurrao = 1f)
    {
        if (dano <= 0 || EstaNocauteado || Time.time < _fimDaInvulnerabilidade) return;
        if (Time.timeScale == 0f || GerenciadorDeJogo.Instancia.JogoAcabou) return;

        _vida = Mathf.Max(0f, _vida - dano);
        _ultimoDano = Time.time;
        _fimDaInvulnerabilidade = Time.time + _invulnerabilidadeAposDano;

        Vector3 direcao = transform.position - origem;
        direcao.y = 0f;
        if (direcao.sqrMagnitude < 0.001f) direcao = -transform.forward;
        _movimento.Empurrar(direcao.normalized * _forcaDoEmpurrao * multiplicadorDoEmpurrao);

        Tremer();
        GerenciadorDeAudio.Instancia.TocarSomAgenteAtingido();
        GerenciadorDePontuacao.Instancia.RegistrarErro();
        AoReceberDano?.Invoke();

        if (_vida <= 0f) Nocautear();
    }

    private void Nocautear()
    {
        _fimDoNocaute = Time.time + _duracaoDoNocaute;
        AoNocautear?.Invoke();
        Invoke(nameof(Recuperar), _duracaoDoNocaute);
    }

    private void Recuperar()
    {
        _vida = _vidaMaxima;
        _fimDaInvulnerabilidade = Time.time + 1.5f;
        AoRecuperar?.Invoke();
    }

    // O TremorDeCamera é criado pelo GerenciadorDeEfeitos no Start: busca na hora.
    private void Tremer()
    {
        if (_tremor == null && Camera.main != null) _tremor = Camera.main.GetComponent<TremorDeCamera>();
        if (_tremor != null) _tremor.Tremer(_tremorAoLevarDano, 0.3f);
    }
}
