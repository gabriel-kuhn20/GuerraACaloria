using UnityEngine;

// Define o chefão de uma fase: nome, modelo 3D, tamanho e características
// próprias (vida, velocidade, dano, ritmo de troca de fraqueza, quais
// munições ele aceita como fraqueza, habilidades e lacaios). Cada fase
// aponta para o seu asset, editável no Inspector sem mexer em código.
[CreateAssetMenu(fileName = "NovoChefao", menuName = "GuerraACaloria/ConfiguracaoDeChefao")]
public class ConfiguracaoDeChefao : ScriptableObject
{
    [Header("Identidade")]
    [SerializeField] private string _nome = "CHEFÃO DE CALORIA";
    [TextArea(2, 3)]
    [SerializeField] private string _descricao;
    [SerializeField] private Color _cor = new Color(1f, 0.5f, 0.45f);

    [Header("Aparência")]
    [Tooltip("Prefab do modelo 3D (vazio = mantém o visual padrão do prefab do chefão).")]
    [SerializeField] private GameObject _modelo;
    [SerializeField] private float _altura = 3f;
    [Tooltip("Graus por segundo que o modelo gira em volta de si (0 = parado).")]
    [SerializeField] private float _giroDoModelo = 30f;

    [Header("Atributos")]
    [SerializeField] private int _vida = 30;
    [SerializeField] private float _velocidade = 5f;
    [SerializeField] private int _danoAoAlcancar = 30;
    [Tooltip("Segundos entre trocas de fraqueza (fica mais rápido com a fúria).")]
    [SerializeField] private float _intervaloDeTrocaDeFraqueza = 4f;
    [SerializeField] private TipoMunicao[] _fraquezas = { TipoMunicao.Agua, TipoMunicao.Fruta, TipoMunicao.EnergiaPositiva };

    [Header("Habilidades")]
    [SerializeField] private HabilidadesDoChefao _habilidades = HabilidadesDoChefao.Investida;
    [SerializeField] private float _intervaloDeLacaios = 9f;
    [Tooltip("Vazio = usa os inimigos permitidos na fase.")]
    [SerializeField] private GameObject[] _lacaios;

    public string Nome => _nome;
    public string Descricao => _descricao;
    public Color Cor => _cor;
    public GameObject Modelo => _modelo;
    public float Altura => _altura;
    public float GiroDoModelo => _giroDoModelo;
    public int Vida => _vida;
    public float Velocidade => _velocidade;
    public int DanoAoAlcancar => _danoAoAlcancar;
    public float IntervaloDeTrocaDeFraqueza => _intervaloDeTrocaDeFraqueza;
    public TipoMunicao[] Fraquezas => _fraquezas;
    public HabilidadesDoChefao Habilidades => _habilidades;
    public float IntervaloDeLacaios => _intervaloDeLacaios;
    public GameObject[] Lacaios => _lacaios;
}
