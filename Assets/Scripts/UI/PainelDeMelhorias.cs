using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Parte visual da escolha de melhoria: até 3 cards com nome, nível
// (atual » próximo) e descrição, mais o botão CONFIRMAR. Clicar num card
// só o seleciona; a escolha vale ao confirmar (evita clique por engano).
// As regras ficam no GerenciadorDeMelhorias.
public class PainelDeMelhorias : MonoBehaviour
{
    [SerializeField] private GameObject _raiz;
    [SerializeField] private Button[] _cards;
    [SerializeField] private Image[] _faixas;
    [SerializeField] private TextMeshProUGUI[] _nomes;
    [SerializeField] private TextMeshProUGUI[] _niveis;
    [SerializeField] private TextMeshProUGUI[] _descricoes;
    [SerializeField] private Button _botaoConfirmar;
    [SerializeField] private TextMeshProUGUI _textoConfirmar;
    [SerializeField] private Color _corDoCard = new Color(0.06f, 0.12f, 0.19f, 0.98f);
    [SerializeField] private float _escalaSelecionado = 1.06f;

    private List<Melhoria> _opcoes;

    private void Awake()
    {
        _raiz.SetActive(false);
    }

    public void Mostrar(List<Melhoria> opcoes, GerenciadorDeMelhorias gerenciador, Action<int> aoSelecionar, Action aoConfirmar)
    {
        _opcoes = opcoes;

        for (int i = 0; i < _cards.Length; i++)
        {
            bool existe = i < opcoes.Count;
            _cards[i].gameObject.SetActive(existe);
            if (!existe) continue;

            Melhoria melhoria = opcoes[i];
            int nivelAtual = gerenciador.Nivel(melhoria);
            string corHex = ColorUtility.ToHtmlStringRGB(melhoria.Cor);

            _faixas[i].color = melhoria.Cor;
            _nomes[i].text = $"<color=#{corHex}>{melhoria.Nome}</color>";
            _niveis[i].text = nivelAtual == 0
                ? $"NOVA  ·  NÍVEL 1/{melhoria.NivelMaximo}"
                : $"NÍVEL {nivelAtual} » {nivelAtual + 1}/{melhoria.NivelMaximo}";
            _descricoes[i].text = melhoria.Descricao(nivelAtual + 1);

            int indice = i;
            _cards[i].onClick.RemoveAllListeners();
            _cards[i].onClick.AddListener(() => aoSelecionar(indice));
        }

        _botaoConfirmar.onClick.RemoveAllListeners();
        _botaoConfirmar.onClick.AddListener(() => aoConfirmar());

        Destacar(-1);
        _raiz.SetActive(true);
    }

    // -1 = nada selecionado (botão de confirmar desabilitado).
    public void Destacar(int indiceSelecionado)
    {
        for (int i = 0; i < _cards.Length; i++)
        {
            bool selecionado = i == indiceSelecionado;
            Color corDaMelhoria = _opcoes != null && i < _opcoes.Count ? _opcoes[i].Cor : Color.white;

            _cards[i].image.color = selecionado ? Color.Lerp(_corDoCard, corDaMelhoria, 0.35f) : _corDoCard;
            _cards[i].transform.localScale = Vector3.one * (selecionado ? _escalaSelecionado : 1f);
        }

        bool temSelecao = indiceSelecionado >= 0 && _opcoes != null && indiceSelecionado < _opcoes.Count;
        _botaoConfirmar.interactable = temSelecao;
        _textoConfirmar.text = temSelecao
            ? $"CONFIRMAR  {_opcoes[indiceSelecionado].Nome}"
            : "SELECIONE UMA MELHORIA";
    }

    public void Esconder() => _raiz.SetActive(false);
}
