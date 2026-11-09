using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Barra de especial do jogador: enche ganhando pontos e pegando
// power-ups. Cheia, a tecla Q (ou o botão direito do mouse) solta a
// "Onda Saudável": derrota os inimigos comuns em volta do jogador, tira
// parte da vida do chefão e cura um pouco todos os cidadãos.
// Singleton, como os demais managers.
public class GerenciadorDeEspecial : MonoBehaviour
{
    public static GerenciadorDeEspecial Instancia { get; private set; }

    [Header("Carga")]
    [Tooltip("Pontos ganhos necessários para encher a barra do zero.")]
    [SerializeField] private int _pontosParaEncher = 2500;
    [Tooltip("Fração da barra que cada power-up coletado dá.")]
    [SerializeField, Range(0f, 1f)] private float _cargaPorPowerUp = 0.2f;

    [Header("Onda Saudável")]
    [SerializeField] private float _raio = 40f;
    [SerializeField] private int _curaNosCidadaos = 20;
    [SerializeField] private float _forcaDoTremor = 0.35f;

    private float _carga;
    private Transform _jogador;
    private TremorDeCamera _tremor;
    private bool _soltando;

    public float Fracao => _carga;
    public bool EstaCheio => _carga >= 1f;

    public event Action AoUsarEspecial;
    public event Action AoEncher;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    private void Start()
    {
        MovimentoJogador jogador = FindFirstObjectByType<MovimentoJogador>();
        if (jogador != null) _jogador = jogador.transform;
        _tremor = FindFirstObjectByType<TremorDeCamera>();

        GerenciadorDePontuacao.Instancia.AoGanharPontos += AoGanharPontos;
        GerenciadorDePowerUps.Instancia.AoColetarPowerUp += AoColetarPowerUp;
    }

    private void OnDestroy()
    {
        if (GerenciadorDePontuacao.Instancia != null) GerenciadorDePontuacao.Instancia.AoGanharPontos -= AoGanharPontos;
        if (GerenciadorDePowerUps.Instancia != null) GerenciadorDePowerUps.Instancia.AoColetarPowerUp -= AoColetarPowerUp;
    }

    private void Update()
    {
        if (Time.timeScale == 0f || GerenciadorDeJogo.Instancia.JogoAcabou) return;

        bool apertou = Keyboard.current.qKey.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame;
        if (apertou && EstaCheio) Soltar();
    }

    private void AoGanharPontos(int pontos)
    {
        AdicionarCarga((float)pontos / Mathf.Max(1, _pontosParaEncher));
    }

    private void AoColetarPowerUp(PowerUp powerUp)
    {
        AdicionarCarga(_cargaPorPowerUp);
    }

    public void AdicionarCarga(float quantidade)
    {
        // Pontos dos inimigos derrotados pelo próprio especial não recarregam a barra.
        if (_soltando || quantidade <= 0f || EstaCheio) return;

        _carga = Mathf.Min(1f, _carga + quantidade);
        if (!EstaCheio) return;
        GerenciadorDeAudio.Instancia.TocarSomEspecialPronto();
        AoEncher?.Invoke();
    }

    private void Soltar()
    {
        _carga = 0f;
        _soltando = true;
        Vector3 centro = _jogador != null ? _jogador.position : Vector3.zero;

        // FindObjectsByType devolve uma cópia: dá para destruir durante o laço.
        foreach (Inimigo inimigo in FindObjectsByType<Inimigo>(FindObjectsSortMode.None))
        {
            Vector3 diferenca = inimigo.transform.position - centro;
            diferenca.y = 0f;
            if (diferenca.magnitude <= _raio) inimigo.ReceberEspecial();
        }
        _soltando = false;

        foreach (Cidadao cidadao in GerenciadorDeCidadaos.Instancia.CidadaosVivos)
        {
            cidadao.Curar(_curaNosCidadaos, peloJogador: false);
        }

        GerenciadorDeEfeitos.Instancia.CriarExplosao(centro + Vector3.up, new Color(0.4f, 1f, 0.5f), 5f);
        GerenciadorDeEfeitos.Instancia.CriarExplosao(centro + Vector3.up, new Color(1f, 0.82f, 0.25f), 3f);
        if (_tremor != null) _tremor.Tremer(_forcaDoTremor, 0.5f);
        GerenciadorDeAudio.Instancia.TocarSomEspecialSoltar();

        AoUsarEspecial?.Invoke();
    }
}
