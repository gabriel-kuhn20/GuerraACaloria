using UnityEngine;
using System;

// Controla o estado geral da partida: saúde da cidade (soma da saúde dos
// cidadãos), derrota, vitória e passagem para a cena da próxima fase.
// Singleton.
public class GerenciadorDeJogo : MonoBehaviour
{
    public static GerenciadorDeJogo Instancia { get; private set; }

    private bool _jogoAcabou = false;

    public int EnergiaMaxima => GerenciadorDeCidadaos.Instancia.SaudeMaximaTotal;
    public int EnergiaAtual => GerenciadorDeCidadaos.Instancia.SaudeTotal;
    public bool JogoAcabou => _jogoAcabou;
    public event Action AoPerderJogo;
    public event Action AoVencerJogo;
    public event Action AoReceberDano;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        Time.timeScale = 1f;
    }

    // No Start todos os cidadãos já se registraram: aplica a saúde que
    // veio da fase anterior.
    private void Start()
    {
        if (ProgressoDaCampanha.TemSaudeSalva)
        {
            GerenciadorDeCidadaos.Instancia.DefinirSaudeProporcional(ProgressoDaCampanha.FracaoDeSaudeSalva);
        }
    }

    // Chamado quando um cidadão é atingido ou levado.
    public void RegistrarDanoNaCidade()
    {
        if (_jogoAcabou) return;

        GerenciadorDeAudio.Instancia.TocarSomAlarme();
        AoReceberDano?.Invoke();

        if (EnergiaAtual <= 0)
        {
            _jogoAcabou = true;
            EncerrarJogo(vitoria: false);
        }
    }

    // Cura vinda de itens e eventos (vai para os cidadãos mais fracos).
    public void RestaurarEnergia(int quantidade)
    {
        if (_jogoAcabou || quantidade <= 0) return;

        GerenciadorDeCidadaos.Instancia.CurarMaisNecessitados(quantidade);
    }

    // Leva a saúde da cidade e os pontos para a cena da próxima fase.
    public void AvancarParaCena(string nomeDaCena)
    {
        if (_jogoAcabou) return;

        float fracao = EnergiaMaxima > 0 ? (float)EnergiaAtual / EnergiaMaxima : 1f;
        ProgressoDaCampanha.SalvarFracaoDeSaude(fracao);
        ProgressoDaCampanha.SalvarPontuacao(GerenciadorDePontuacao.Instancia.Pontos);
        if (GerenciadorDeMelhorias.Instancia != null) GerenciadorDeMelhorias.Instancia.SalvarNaCampanha();
        TransicaoDeTela.Instancia.CarregarCena(nomeDaCena);
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
        GerenciadorDePontuacao.Instancia.SalvarRecordeSeMaior();

        if (vitoria) GerenciadorDeAudio.Instancia.TocarSomVitoria();
        else GerenciadorDeAudio.Instancia.TocarSomDerrota();

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
