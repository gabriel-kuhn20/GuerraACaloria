using UnityEngine;

// Define uma fase do jogo: nome, dica saudável exibida na transição,
// sequência de ondas, quais inimigos podem aparecer e o chefão do final.
// Cada fase é um asset separado, editável no Inspector sem mexer em código.
[CreateAssetMenu(fileName = "NovaFase", menuName = "GuerraACaloria/ConfiguracaoDeFase")]
public class ConfiguracaoDeFase : ScriptableObject
{
    [SerializeField] private string _nome = "Nova Fase";
    [TextArea(2, 4)]
    [SerializeField] private string _dicaSaudavel;
    [SerializeField] private ConfiguracaoDeOnda[] _ondas;

    [Tooltip("Vazio = usa a lista padrão de prefabs do GeradorDeInimigos.")]
    [SerializeField] private GameObject[] _prefabsDeInimigo;

    [SerializeField] private bool _temChefao = true;
    [Tooltip("Chefão próprio desta fase (modelo e atributos). Vazio = usa vida/habilidades abaixo com o visual padrão.")]
    [SerializeField] private ConfiguracaoDeChefao _chefao;
    [SerializeField] private int _vidaDoChefao = 30;
    [SerializeField] private HabilidadesDoChefao _habilidadesDoChefao = HabilidadesDoChefao.Investida;
    [SerializeField] private int _energiaRecuperadaAoIniciar = 0;
    [Tooltip("Música de fundo desta fase (vazio = mantém a música da cena).")]
    [SerializeField] private AudioClip _musica;

    public string Nome => _nome;
    public string DicaSaudavel => _dicaSaudavel;
    public ConfiguracaoDeOnda[] Ondas => _ondas;
    public GameObject[] PrefabsDeInimigo => _prefabsDeInimigo;
    public bool TemChefao => _temChefao;
    public ConfiguracaoDeChefao Chefao => _chefao;
    public int VidaDoChefao => _vidaDoChefao;
    public HabilidadesDoChefao HabilidadesDoChefao => _habilidadesDoChefao;
    public int EnergiaRecuperadaAoIniciar => _energiaRecuperadaAoIniciar;
    public AudioClip Musica => _musica;
}
