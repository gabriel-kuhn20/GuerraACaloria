using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider _barraDeEnergia;
    [SerializeField] private TextMeshProUGUI _textoMunicao;
    [SerializeField] private TextMeshProUGUI _textoOnda;
    [SerializeField] private GameObject _telaGameOver;
    [SerializeField] private GameObject _telaVitoria;
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private GeradorDeInimigos _geradorDeInimigos;
    [SerializeField] private float _duracaoVisivelAposDano = 3f;

    [Header("HUD do Chefão")]
    [SerializeField] private GameObject _painelChefao;
    [SerializeField] private Slider _barraDeVidaChefao;
    [SerializeField] private TextMeshProUGUI _textoFraquezaChefao;

    private float _tempoRestanteVisivel;
    private ChefaoDeCaloria _chefaoAtual;

    private void Start()
    {
        GerenciadorDeJogo.Instancia.AoPerderJogo += MostrarTelaGameOver;
        GerenciadorDeJogo.Instancia.AoVencerJogo += MostrarTelaVitoria;
        GerenciadorDeJogo.Instancia.AoReceberDano += MostrarBarraTemporariamente;
        _geradorDeInimigos.AoMudarDeOnda += AtualizarTextoOnda;
        _geradorDeInimigos.AoSpawnarChefao += ConfigurarUIChefao;
        _barraDeEnergia.gameObject.SetActive(false);
        _painelChefao.SetActive(false);
    }

    private void Update()
    {
        AtualizarBarraDeEnergia();
        ContarTempoDaBarra();
    }

    private void AtualizarBarraDeEnergia()
    {
        _barraDeEnergia.maxValue = GerenciadorDeJogo.Instancia.EnergiaMaxima;
        _barraDeEnergia.value = GerenciadorDeJogo.Instancia.EnergiaAtual;
    }

    private void MostrarBarraTemporariamente()
    {
        _barraDeEnergia.gameObject.SetActive(true);
        _tempoRestanteVisivel = _duracaoVisivelAposDano;
    }

    private void ContarTempoDaBarra()
    {
        if (_tempoRestanteVisivel <= 0) return;

        _tempoRestanteVisivel -= Time.deltaTime;
        if (_tempoRestanteVisivel <= 0)
        {
            _barraDeEnergia.gameObject.SetActive(false);
        }
    }

    public void AtualizarTextoMunicao(TipoMunicao municaoAtual)
    {
        _textoMunicao.text = $"Munição: {municaoAtual}";
    }

    private void AtualizarTextoOnda(int numeroDaOnda)
    {
        _textoOnda.text = $"Onda: {numeroDaOnda}";
    }

    private void MostrarTelaGameOver()
    {
        FinalizarPartida(_telaGameOver);
    }

    private void MostrarTelaVitoria()
    {
        _painelChefao.SetActive(false);
        FinalizarPartida(_telaVitoria);
    }

    private void FinalizarPartida(GameObject telaParaMostrar)
    {
        telaParaMostrar.SetActive(true);
        _textoMunicao.gameObject.SetActive(false);
        _crosshair.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ConfigurarUIChefao(ChefaoDeCaloria chefao)
    {
        _chefaoAtual = chefao;

        _painelChefao.SetActive(true);
        _barraDeVidaChefao.maxValue = chefao.VidaMaxima;
        _barraDeVidaChefao.value = chefao.VidaAtual;
        AtualizarFraquezaChefao(chefao.Fraqueza);

        chefao.AoMudarVida += AtualizarVidaChefao;
        chefao.AoMudarFraqueza += AtualizarFraquezaChefao;
    }

    private void AtualizarVidaChefao(int vidaAtual, int vidaMaxima)
    {
        _barraDeVidaChefao.maxValue = vidaMaxima;
        _barraDeVidaChefao.value = vidaAtual;
    }

    private void AtualizarFraquezaChefao(TipoMunicao fraqueza)
    {
        _textoFraquezaChefao.text = $"Fraqueza do chefão: {fraqueza}";
    }
}