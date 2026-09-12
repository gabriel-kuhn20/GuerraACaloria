using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Atualiza os elementos visuais do HUD: barra de energia da cidade,
// indicador de munição atual e tela de Game Over. A barra de energia
// só fica visível por alguns segundos após dano recente.
public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider _barraDeEnergia;
    [SerializeField] private TextMeshProUGUI _textoMunicao;
    [SerializeField] private GameObject _telaGameOver;
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private float _duracaoVisivelAposDano = 3f;

    private float _tempoRestanteVisivel;

    private void Start()
    {
        GerenciadorDeJogo.Instancia.AoPerderJogo += MostrarTelaGameOver;
        GerenciadorDeJogo.Instancia.AoReceberDano += MostrarBarraTemporariamente;
    }

    private void Update()
    {
        AtualizarBarraDeEnergia();
        ContarTempoDaBarra();
    }

    private void AtualizarBarraDeEnergia()
    {
        _barraDeEnergia.maxValue = 100f;
        _barraDeEnergia.value = GerenciadorDeJogo.Instancia.EnergiaAtual;
    }

    private void MostrarBarraTemporariamente()
    {   
        if(!_barraDeEnergia.gameObject.activeSelf)
        {
        _barraDeEnergia.gameObject.SetActive(true);
        }
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

    private void MostrarTelaGameOver()
    {
        _telaGameOver.SetActive(true);
        _textoMunicao.gameObject.SetActive(false);
        _crosshair.SetActive(false);
        _barraDeEnergia.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}