using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Atualiza os elementos visuais do HUD: barra de energia da cidade
// e indicador de munição atual selecionada.

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider _barraDeEnergia;
    [SerializeField] private TextMeshProUGUI _textoMunicao;

    private void Update()
    {
        AtualizarBarraDeEnergia();
    }

    private void AtualizarBarraDeEnergia()
    {
        _barraDeEnergia.maxValue = 100f;
        _barraDeEnergia.value = GerenciadorDeJogo.Instancia.EnergiaAtual;
    }

    public void AtualizarTextoMunicao(TipoMunicao tipoMunicao)
    {
        _textoMunicao.text = $"Munição: {tipoMunicao}";
    }

}