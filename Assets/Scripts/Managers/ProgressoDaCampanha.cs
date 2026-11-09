using System.Collections.Generic;
using UnityEngine;

// Guarda o que precisa sobreviver à troca de cena entre as fases
// (saúde da cidade, em fração de 0 a 1, pontuação e melhorias). Classe estática
// pura, sem MonoBehaviour, porque não fica anexada a nenhum objeto.
public static class ProgressoDaCampanha
{
    private static bool _temSaudeSalva = false;
    private static float _fracaoDeSaudeSalva = 1f;
    private static int _pontuacaoSalva;
    private static readonly Dictionary<string, int> _niveisDeMelhoria = new Dictionary<string, int>();
    private static int _ofertasDeMelhoriaFeitas;

    // Zera os dados ao entrar em Play Mode, mesmo com "Domain Reload" desligado.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reiniciar()
    {
        _temSaudeSalva = false;
        _fracaoDeSaudeSalva = 1f;
        _pontuacaoSalva = 0;
        _niveisDeMelhoria.Clear();
        _ofertasDeMelhoriaFeitas = 0;
    }

    // Só é salvo ao avançar de fase: reiniciar a fase volta às melhorias
    // (e à pontuação) que o jogador tinha quando ela começou.
    public static void SalvarMelhorias(Dictionary<string, int> niveis, int ofertasFeitas)
    {
        _niveisDeMelhoria.Clear();
        foreach (KeyValuePair<string, int> par in niveis) _niveisDeMelhoria[par.Key] = par.Value;
        _ofertasDeMelhoriaFeitas = ofertasFeitas;
    }

    public static void CarregarMelhorias(Dictionary<string, int> destino, out int ofertasFeitas)
    {
        destino.Clear();
        foreach (KeyValuePair<string, int> par in _niveisDeMelhoria) destino[par.Key] = par.Value;
        ofertasFeitas = _ofertasDeMelhoriaFeitas;
    }

    public static bool TemSaudeSalva => _temSaudeSalva;
    public static float FracaoDeSaudeSalva => _fracaoDeSaudeSalva;

    public static void SalvarPontuacao(int pontos)
    {
        _pontuacaoSalva = pontos;
    }

    public static int ObterPontuacaoInicial()
    {
        return _pontuacaoSalva;
    }

    public static void SalvarFracaoDeSaude(float fracao)
    {
        _fracaoDeSaudeSalva = Mathf.Clamp01(fracao);
        _temSaudeSalva = true;
    }
}
