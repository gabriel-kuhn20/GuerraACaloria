using UnityEngine;
using TMPro;

// Controla a cena de menu principal: iniciar a partida (do começo ou
// direto numa fase), abrir/fechar os painéis "Fases", "Como Jogar",
// Opções e Créditos, mostrar o recorde e sair. Fica na cena MainMenu, separada
// das cenas de jogo (SampleScene, Fase2, Fase3, Fase4).
public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private string _nomeCenaDeJogo = "SampleScene";
    [SerializeField] private GameObject _painelComoJogar;
    [SerializeField] private TextMeshProUGUI _textoRecorde;
    [SerializeField] private GameObject _painelOpcoes;
    [SerializeField] private GameObject _painelCreditos;
    [SerializeField] private GameObject _painelFases;
    [Tooltip("Cena de cada fase, na ordem (índice 0 = Fase 1).")]
    [SerializeField] private string[] _cenasDasFases = { "SampleScene", "Fase2", "Fase3", "Fase4" };

    private void Start()
    {
        // Ao voltar de uma partida o cursor pode estar travado pelo MouseLook.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        FecharPaineis();
        MostrarRecorde();
    }

    private void MostrarRecorde()
    {
        if (_textoRecorde == null) return;

        int recorde = GerenciadorDePontuacao.Recorde;
        _textoRecorde.text = recorde > 0 ? $"RECORDE  {recorde:N0}" : "";
    }

    public void IniciarJogo()
    {
        ProgressoDaCampanha.Reiniciar();
        TransicaoDeTela.Instancia.CarregarCena(_nomeCenaDeJogo);
    }

    // Começa direto na fase escolhida (1 a 4), com a campanha zerada:
    // pontos 0, cidadãos com saúde cheia e sem melhorias.
    public void IniciarFase(int numeroDaFase)
    {
        int indice = numeroDaFase - 1;
        if (indice < 0 || indice >= _cenasDasFases.Length) return;

        TocarClique();
        ProgressoDaCampanha.Reiniciar();
        TransicaoDeTela.Instancia.CarregarCena(_cenasDasFases[indice]);
    }

    public void AbrirFases() => AbrirPainel(_painelFases);
    public void FecharFases() => FecharComClique();
    public void AbrirComoJogar() => AbrirPainel(_painelComoJogar);
    public void AbrirOpcoes() => AbrirPainel(_painelOpcoes);
    public void AbrirCreditos() => AbrirPainel(_painelCreditos);

    public void FecharComoJogar() => FecharComClique();
    public void FecharOpcoes() => FecharComClique();
    public void FecharCreditos() => FecharComClique();

    private void FecharComClique()
    {
        TocarClique();
        FecharPaineis();
    }

    private void AbrirPainel(GameObject painel)
    {
        TocarClique();
        if (painel != null) painel.SetActive(true);
    }

    private void FecharPaineis()
    {
        if (_painelComoJogar != null) _painelComoJogar.SetActive(false);
        if (_painelOpcoes != null) _painelOpcoes.SetActive(false);
        if (_painelCreditos != null) _painelCreditos.SetActive(false);
        if (_painelFases != null) _painelFases.SetActive(false);
    }

    private void TocarClique()
    {
        if (GerenciadorDeAudio.Instancia != null) GerenciadorDeAudio.Instancia.TocarSomClique();
    }

    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
