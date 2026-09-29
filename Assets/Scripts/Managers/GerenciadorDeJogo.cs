using UnityEngine;
using System;

public class GerenciadorDeJogo : MonoBehaviour
{
    public static GerenciadorDeJogo Instancia { get; private set; }

    [SerializeField] private int _energiaMaxima = 250;
    private int _energiaAtual;
    private bool _jogoAcabou = false;

    public int EnergiaMaxima => _energiaMaxima;
    public int EnergiaAtual => _energiaAtual;
    public event Action AoPerderJogo;
    public event Action AoVencerJogo;
    public event Action AoReceberDano;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        _energiaAtual = _energiaMaxima;
        Time.timeScale = 1f;
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
            EncerrarJogo(vitoria: false);
        }
    }

    public void RegistrarVitoria()
    {
        if (_jogoAcabou) return;

        _jogoAcabou = true;
        EncerrarJogo(vitoria: true);
    }

    private void EncerrarJogo(bool vitoria)
    {
        GerenciadorDeAudio.Instancia.PararTodosOsSons();
        DestruirProjeteisRestantes();

        if (vitoria) AoVencerJogo?.Invoke();
        else AoPerderJogo?.Invoke();
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