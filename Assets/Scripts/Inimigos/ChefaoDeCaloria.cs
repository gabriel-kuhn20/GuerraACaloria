using UnityEngine;
using System;

// Chefão final da onda 3: usa os três tipos de munição, alternando
// qual é a fraqueza ativa a cada intervalo. Tem várias vidas (barra
// própria) em vez de morrer com 1 acerto certo como os inimigos comuns.
// Toca animação de "Hop" andando e "Death" ao ser derrotado.
public class ChefaoDeCaloria : Inimigo
{
    [SerializeField] private int _vidaMaxima = 30;
    [SerializeField] private float _intervaloTrocaDeFraqueza = 4f;
    [SerializeField] private float _escalaVisual = 0.1f;
    [SerializeField] private Animator _animator;
    [SerializeField] private string _nomeTriggerMorte = "Morrer";
    [SerializeField] private float _duracaoAnimacaoMorte = 2f;

    private TipoMunicao _fraquezaAtual;
    private float _proximaTroca;
    private bool _estaMorrendo = false;

    public override TipoMunicao Fraqueza => _fraquezaAtual;
    protected override float Velocidade => 4f;
    protected override int DanoAoAlcancarAlvo => 30;
    protected override float AmplitudeOscilacao => 0.3f;

    public int VidaAtual => _vida;
    public int VidaMaxima => _vidaMaxima;
    public event Action<int, int> AoMudarVida;
    public event Action<TipoMunicao> AoMudarFraqueza;

    protected override void Awake()
    {
        base.Awake();
        transform.localScale = Vector3.one * _escalaVisual;
        _vida = _vidaMaxima;
    }

    protected override void Start()
    {
        base.Start();
        SortearNovaFraqueza();
        _proximaTroca = Time.time + _intervaloTrocaDeFraqueza;
    }

    protected override void Update()
    {
        if (_estaMorrendo) return;

        base.Update();
        AtualizarTrocaDeFraqueza();

#if UNITY_EDITOR
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"[ChefaoDeCaloria] Alvo: {(_alvo != null ? _alvo.name : "NULL")} | Posicao: {transform.position} | Vida: {_vida}");
        }
#endif
    }

    private void AtualizarTrocaDeFraqueza()
    {
        if (Time.time < _proximaTroca) return;

        SortearNovaFraqueza();
        _proximaTroca = Time.time + _intervaloTrocaDeFraqueza;
    }

    private void SortearNovaFraqueza()
    {
        TipoMunicao[] opcoes = { TipoMunicao.Agua, TipoMunicao.Fruta, TipoMunicao.EnergiaPositiva };
        _fraquezaAtual = opcoes[UnityEngine.Random.Range(0, opcoes.Length)];
        AoMudarFraqueza?.Invoke(_fraquezaAtual);
    }

    public override void ReceberDano(TipoMunicao municaoUsada)
    {
        if (_estaMorrendo) return;

        if (municaoUsada == Fraqueza)
        {
            _vida--;
            GerenciadorDeAudio.Instancia.TocarSomAcerto();
            AoMudarVida?.Invoke(_vida, _vidaMaxima);
        }
        else
        {
            GerenciadorDeAudio.Instancia.TocarSomErro();
        }

        if (_vida <= 0)
        {
            IniciarMorte();
        }
    }

    private void IniciarMorte()
    {
        _estaMorrendo = true;
        _animator.SetTrigger(_nomeTriggerMorte);
        Invoke(nameof(NotificarMorteEDestruir), _duracaoAnimacaoMorte);
    }
}