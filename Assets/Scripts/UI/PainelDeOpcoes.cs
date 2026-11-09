using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Painel de opções (sensibilidade do mouse e volumes de música e efeitos).
// Lê e grava as preferências em ConfiguracoesDoJogo. Usado tanto no menu
// principal quanto no menu de pausa.
public class PainelDeOpcoes : MonoBehaviour
{
    [SerializeField] private Slider _sliderSensibilidade;
    [SerializeField] private Slider _sliderMusica;
    [SerializeField] private Slider _sliderEfeitos;
    [SerializeField] private TextMeshProUGUI _valorSensibilidade;
    [SerializeField] private TextMeshProUGUI _valorMusica;
    [SerializeField] private TextMeshProUGUI _valorEfeitos;

    private void Awake()
    {
        _sliderSensibilidade.onValueChanged.AddListener(AoMudarSensibilidade);
        _sliderMusica.onValueChanged.AddListener(AoMudarMusica);
        _sliderEfeitos.onValueChanged.AddListener(AoMudarEfeitos);
    }

    private void OnEnable()
    {
        _sliderSensibilidade.SetValueWithoutNotify(ConfiguracoesDoJogo.Sensibilidade);
        _sliderMusica.SetValueWithoutNotify(ConfiguracoesDoJogo.VolumeMusica);
        _sliderEfeitos.SetValueWithoutNotify(ConfiguracoesDoJogo.VolumeEfeitos);
        AtualizarTextos();
    }

    private void OnDisable()
    {
        ConfiguracoesDoJogo.Persistir();
    }

    private void AoMudarSensibilidade(float valor)
    {
        ConfiguracoesDoJogo.DefinirSensibilidade(valor);
        AtualizarTextos();
    }

    private void AoMudarMusica(float valor)
    {
        ConfiguracoesDoJogo.DefinirVolumeMusica(valor);
        AtualizarTextos();
    }

    private void AoMudarEfeitos(float valor)
    {
        ConfiguracoesDoJogo.DefinirVolumeEfeitos(valor);
        AtualizarTextos();
    }

    private void AtualizarTextos()
    {
        _valorSensibilidade.text = $"{ConfiguracoesDoJogo.Sensibilidade:0.0}x";
        _valorMusica.text = $"{ConfiguracoesDoJogo.VolumeMusica * 100f:0}%";
        _valorEfeitos.text = $"{ConfiguracoesDoJogo.VolumeEfeitos * 100f:0}%";
    }
}
