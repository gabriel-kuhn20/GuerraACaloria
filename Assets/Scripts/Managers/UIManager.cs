using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Atualiza os elementos visuais do HUD: barra de energia da cidade,
// indicador de munição atual e tela de Game Over.
public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider _barraDeEnergia;
    [SerializeField] private TextMeshProUGUI _textoMunicao;
    [SerializeField] private GameObject _telaGameOver;

    private void Start()
    {
        GerenciadorDeJogo.Instancia.AoPerderJogo += MostrarTelaGameOver;
    }

    private void Update()
    {
        AtualizarBarraDeEnergia();
    }

    private void AtualizarBarraDeEnergia()
    {
        _barraDeEnergia.maxValue = 100f;
        _barraDeEnergia.value = GerenciadorDeJogo.Instancia.EnergiaAtual;
    }

    public void AtualizarTextoMunicao(TipoMunicao municaoAtual)
    {
        _textoMunicao.text = $"Munição: {municaoAtual}";
    }

    private void MostrarTelaGameOver()
    {
        _telaGameOver.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }
}