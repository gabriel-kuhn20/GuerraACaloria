using UnityEngine;
using System;

// Sorteia o drop de power-ups quando um inimigo é derrotado e controla
// os efeitos temporários ativos (tiro rápido e munição coringa).
// Singleton, como os demais managers.
public class GerenciadorDePowerUps : MonoBehaviour
{
    public static GerenciadorDePowerUps Instancia { get; private set; }

    [SerializeField] private GameObject[] _prefabsDePowerUp;
    [SerializeField, Range(0f, 1f)] private float _chanceDeDrop = 0.22f;
    [SerializeField] private float _intervaloMinimoEntreDrops = 3f;
    [Tooltip("Projéteis extras de cada disparo enquanto o Suco Turbo está ativo.")]
    [SerializeField] private int _projeteisExtrasDoSuco = 2;
    [Tooltip("Alvos extras que o projétil atravessa enquanto o Abacate Coringa está ativo.")]
    [SerializeField] private int _perfuracaoDoAbacate = 2;
    [SerializeField] private float _alturaDoDrop = 1.2f;

    private float _fimDoTiroRapido;
    private float _fimDaMunicaoCoringa;
    private float _proximoDropPermitido;
    private Transform _jogador;

    public Transform Jogador => _jogador;
    public bool TiroRapidoAtivo => Time.time < _fimDoTiroRapido;
    public bool MunicaoCoringaAtiva => Time.time < _fimDaMunicaoCoringa;
    public float TempoRestanteTiroRapido => Mathf.Max(0f, _fimDoTiroRapido - Time.time);
    public float TempoRestanteMunicaoCoringa => Mathf.Max(0f, _fimDaMunicaoCoringa - Time.time);
    public int ProjeteisExtrasTemporarios => TiroRapidoAtivo ? _projeteisExtrasDoSuco : 0;
    public int PerfuracaoTemporaria => MunicaoCoringaAtiva ? _perfuracaoDoAbacate : 0;

    public event Action<PowerUp> AoColetarPowerUp;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;

        MovimentoJogador jogador = FindFirstObjectByType<MovimentoJogador>();
        if (jogador != null) _jogador = jogador.transform;
    }

    public void TentarDropar(Vector3 posicao)
    {
        if (_prefabsDePowerUp == null || _prefabsDePowerUp.Length == 0) return;
        if (Time.time < _proximoDropPermitido) return;
        if (UnityEngine.Random.value > _chanceDeDrop + GerenciadorDeMelhorias.Atributos.ChanceExtraDeDrop) return;

        GameObject prefab = _prefabsDePowerUp[UnityEngine.Random.Range(0, _prefabsDePowerUp.Length)];
        // Inimigo pode morrer atrás da barricada: traz o item para dentro da rua.
        Vector3 dentroDaRua = GerenciadorDeRua.Instancia.RestringirPosicao(posicao);
        Vector3 posicaoDoDrop = new Vector3(dentroDaRua.x, _alturaDoDrop, dentroDaRua.z);
        Instantiate(prefab, posicaoDoDrop, Quaternion.identity);
        _proximoDropPermitido = Time.time + _intervaloMinimoEntreDrops;
    }

    // Item garantido (sem sorteio nem intervalo): presente de um cidadão curado.
    public void DroparPresente(Vector3 posicao)
    {
        if (_prefabsDePowerUp == null || _prefabsDePowerUp.Length == 0) return;

        GameObject prefab = _prefabsDePowerUp[UnityEngine.Random.Range(0, _prefabsDePowerUp.Length)];
        Instantiate(prefab, new Vector3(posicao.x, _alturaDoDrop, posicao.z), Quaternion.identity);
    }

    public void AtivarTiroRapido(float duracao)
    {
        _fimDoTiroRapido = Mathf.Max(_fimDoTiroRapido, Time.time) + duracao * GerenciadorDeMelhorias.Atributos.MultiplicadorDeDuracaoDePowerUp;
    }

    public void AtivarMunicaoCoringa(float duracao)
    {
        _fimDaMunicaoCoringa = Mathf.Max(_fimDaMunicaoCoringa, Time.time) + duracao * GerenciadorDeMelhorias.Atributos.MultiplicadorDeDuracaoDePowerUp;
    }

    public void NotificarColeta(PowerUp powerUp)
    {
        GerenciadorDeAudio.Instancia.TocarSomPowerUp();
        GerenciadorDeEfeitos.Instancia.CriarExplosao(powerUp.transform.position, powerUp.Cor, 0.7f);
        AoColetarPowerUp?.Invoke(powerUp);
    }
}
