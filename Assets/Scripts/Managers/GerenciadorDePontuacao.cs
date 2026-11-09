using UnityEngine;
using System;

// Controla pontuação, combo e recorde. Acertos seguidos com a munição
// certa aumentam o multiplicador (x2, x3...); errar a munição ou ficar
// muito tempo sem acertar zera o combo. O recorde fica salvo no
// computador (PlayerPrefs). Singleton, como os demais managers.
public class GerenciadorDePontuacao : MonoBehaviour
{
    public static GerenciadorDePontuacao Instancia { get; private set; }

    private const string ChaveDoRecorde = "GuerraACaloria_Recorde";

    [SerializeField] private int _acertosPorNivelDeCombo = 5;
    [SerializeField] private int _multiplicadorMaximo = 5;
    [SerializeField] private float _tempoParaPerderCombo = 4f;

    private int _pontos;
    private int _combo;
    private float _tempoRestanteDoCombo;
    private int _bonusDaOnda = 1;
    private bool _bateuRecorde;

    public int Pontos => _pontos;
    public int Combo => _combo;
    public int Multiplicador => Mathf.Min(1 + _combo / _acertosPorNivelDeCombo, _multiplicadorMaximo);
    public float FracaoDoTempoDeCombo => _combo == 0 ? 0f : _tempoRestanteDoCombo / _tempoParaPerderCombo;
    public bool BateuRecorde => _bateuRecorde;
    public static int Recorde => PlayerPrefs.GetInt(ChaveDoRecorde, 0);

    // (pontos ganhos agora) — usado pela UI para dar destaque visual.
    public event Action<int> AoGanharPontos;
    public event Action AoPerderCombo;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        _pontos = ProgressoDaCampanha.ObterPontuacaoInicial();
    }

    private void Update()
    {
        if (_combo == 0) return;

        _tempoRestanteDoCombo -= Time.deltaTime;
        if (_tempoRestanteDoCombo <= 0f) ZerarCombo();
    }

    public void RegistrarAcerto()
    {
        int multiplicadorAnterior = Multiplicador;
        _combo++;
        _tempoRestanteDoCombo = _tempoParaPerderCombo;

        if (Multiplicador > multiplicadorAnterior) GerenciadorDeAudio.Instancia.TocarSomComboSubiu(Multiplicador);
    }

    public void RegistrarErro()
    {
        ZerarCombo();
    }

    // Retorna quantos pontos foram ganhos de fato (já com multiplicadores).
    public int RegistrarAbate(int pontosBase)
    {
        int pontos = pontosBase * Multiplicador * _bonusDaOnda;
        AdicionarPontos(pontos);
        return pontos;
    }

    public void AdicionarBonus(int pontos)
    {
        AdicionarPontos(pontos);
    }

    public void DefinirBonusDaOnda(int multiplicador)
    {
        _bonusDaOnda = Mathf.Max(1, multiplicador);
    }

    public void SalvarRecordeSeMaior()
    {
        if (_pontos <= Recorde) return;

        _bateuRecorde = true;
        PlayerPrefs.SetInt(ChaveDoRecorde, _pontos);
        PlayerPrefs.Save();
    }

    private void AdicionarPontos(int pontos)
    {
        if (pontos <= 0) return;

        _pontos += pontos;
        AoGanharPontos?.Invoke(pontos);
    }

    private void ZerarCombo()
    {
        if (_combo == 0) return;

        // Só faz o som de "combo perdido" se já tinha multiplicador.
        if (Multiplicador > 1) GerenciadorDeAudio.Instancia.TocarSomComboQuebrou();
        _combo = 0;
        _tempoRestanteDoCombo = 0f;
        AoPerderCombo?.Invoke();
    }
}
