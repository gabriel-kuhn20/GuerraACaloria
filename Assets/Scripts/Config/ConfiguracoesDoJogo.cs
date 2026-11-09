using System;
using UnityEngine;

// Preferências do jogador (sensibilidade do mouse e volumes), salvas no
// computador com PlayerPrefs. Classe estática: lida tanto pelo menu
// principal quanto pelas cenas de jogo.
public static class ConfiguracoesDoJogo
{
    private const string ChaveSensibilidade = "GuerraACaloria_Sensibilidade";
    private const string ChaveVolumeMusica = "GuerraACaloria_VolumeMusica";
    private const string ChaveVolumeEfeitos = "GuerraACaloria_VolumeEfeitos";

    public static float SensibilidadeMinima => 0.2f;
    public static float SensibilidadeMaxima => 3f;

    public static float Sensibilidade => PlayerPrefs.GetFloat(ChaveSensibilidade, 1f);
    public static float VolumeMusica => PlayerPrefs.GetFloat(ChaveVolumeMusica, 0.5f);
    public static float VolumeEfeitos => PlayerPrefs.GetFloat(ChaveVolumeEfeitos, 1f);

    // Avisa quem aplica os valores (áudio, mouse) para atualizar na hora.
    public static event Action AoMudar;

    public static void DefinirSensibilidade(float valor)
    {
        Definir(ChaveSensibilidade, Mathf.Clamp(valor, SensibilidadeMinima, SensibilidadeMaxima));
    }

    public static void DefinirVolumeMusica(float valor)
    {
        Definir(ChaveVolumeMusica, Mathf.Clamp01(valor));
    }

    public static void DefinirVolumeEfeitos(float valor)
    {
        Definir(ChaveVolumeEfeitos, Mathf.Clamp01(valor));
    }

    // Grava no disco; chamado ao fechar o painel de opções (não a cada
    // movimento do slider, para não escrever no disco o tempo todo).
    public static void Persistir()
    {
        PlayerPrefs.Save();
    }

    private static void Definir(string chave, float valor)
    {
        PlayerPrefs.SetFloat(chave, valor);
        AoMudar?.Invoke();
    }
}
