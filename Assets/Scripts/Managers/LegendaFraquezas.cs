using UnityEngine;
using TMPro;

// Preenche o texto da legenda de fraquezas no menu principal a partir dos
// dados reais dos prefabs de inimigo, evitando que o texto fique
// desatualizado se as fraquezas mudarem no código.
public class LegendaFraquezas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textoLegenda;
    [SerializeField] private GameObject[] _prefabsDeInimigo;

    private void Start()
    {
        MontarTexto();
    }

    private void MontarTexto()
    {
        _textoLegenda.text = "";

        foreach (GameObject prefab in _prefabsDeInimigo)
        {
            Inimigo inimigo = prefab.GetComponent<Inimigo>();
            _textoLegenda.text += $"{prefab.name} → {inimigo.Fraqueza}\n";
        }
    }
}