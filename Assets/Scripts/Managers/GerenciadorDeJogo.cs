using UnityEngine;
using System;

// Gerencia o estado global do jogo: energia da cidade, e futuramente
// onda atual e condição de vitória/derrota. Singleton acessado por
// outros scripts via GerenciadorDeJogo.Instancia.
public class GerenciadorDeJogo : MonoBehaviour
{
    public static GerenciadorDeJogo Instancia { get; private set; }

    [SerializeField] private int _energiaMaxima = 250;
    private int _energiaAtual;
    private bool _jogoAcabou = false;

    public int EnergiaMaxima => _energiaMaxima;
    public int EnergiaAtual => _energiaAtual;
    public event Action AoPerderJogo;
    public event Action AoReceberDano;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        _energiaAtual = _energiaMaxima;
        Time.timeScale = 1f; // garante que a cena nunca comece pausada,
                              // mesmo se um Game Over anterior deixou timeScale em 0
    }

    public void ReceberDanoNaCidade(int quantidade)
    {
        if (_jogoAcabou) return;

        _energiaAtual = Mathf.Max(0, _energiaAtual - quantidade);
        GerenciadorDeAudio.Instancia.TocarSomAlarme();
        AoReceberDano?.Invoke();

        if (_energiaAtual <= 0)
        {
            _jogoAcabou = true;
            EncerrarJogo();
        }
    }

    private void EncerrarJogo()
    {
        GerenciadorDeAudio.Instancia.PararTodosOsSons();
        DestruirProjeteisRestantes();
        AoPerderJogo?.Invoke();
    }

    private void DestruirProjeteisRestantes()
    {
        Projetil[] projeteis = FindObjectsByType<Projetil>(FindObjectsSortMode.None);
        foreach (Projetil projetil in projeteis)
        {
            Destroy(projetil.gameObject);
        }
    }
}