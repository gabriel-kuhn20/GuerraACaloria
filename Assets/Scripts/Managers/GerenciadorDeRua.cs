using UnityEngine;
using UnityEngine.Serialization;

// Define os limites retangulares da rua (o "túnel" por onde o jogo
// acontece) e por onde os inimigos entram nela: uma ou as duas pontas
// e, opcionalmente, entradas laterais (ruas transversais). Áreas extras
// somam outros retângulos à área jogável (ex: o braço de um cruzamento). Jogador e
// cidadãos ficam presos ao retângulo; inimigos podem ocupar também uma
// faixa extra atrás das barricadas, onde nascem escondidos.
// Singleton, consultado por qualquer objeto que precise da área jogável.
public class GerenciadorDeRua : MonoBehaviour
{
    public enum OrigemDosInimigos { PontaFinal, PontaInicial, AmbasAsPontas }

    [System.Serializable]
    public struct AreaExtra
    {
        public Vector3 Centro;
        [Tooltip("X = largura, Y = comprimento (em Z).")]
        public Vector2 Tamanho;
    }

    public static GerenciadorDeRua Instancia { get; private set; }

    [SerializeField] private float _largura = 10f;
    [SerializeField] private float _comprimento = 40f;
    [SerializeField] private Vector3 _centro = Vector3.zero;
    [SerializeField] private AreaExtra[] _areasExtras = new AreaExtra[0];

    [Header("Entradas dos inimigos")]
    [Tooltip("PontaFinal = Z maior, PontaInicial = Z menor.")]
    [FormerlySerializedAs("_inimigosVemDoInicio")]
    [SerializeField] private OrigemDosInimigos _origemDosInimigos = OrigemDosInimigos.PontaFinal;
    [Tooltip("Distância atrás da barricada onde o inimigo nasce (fora da vista do jogador).")]
    [SerializeField] private float _recuoDoSpawn = 4f;
    [Tooltip("Pontos na borda lateral da rua (X = borda, Z = centro da transversal).")]
    [SerializeField] private Vector3[] _entradasLaterais = new Vector3[0];
    [SerializeField, Range(0f, 1f)] private float _chanceDeEntradaLateral = 0.3f;

    private float MinX => _centro.x - _largura / 2f;
    private float MaxX => _centro.x + _largura / 2f;
    private float MinZ => _centro.z - _comprimento / 2f;
    private float MaxZ => _centro.z + _comprimento / 2f;

    private bool NascemNoInicio => _origemDosInimigos != OrigemDosInimigos.PontaFinal;
    private bool NascemNoFinal => _origemDosInimigos != OrigemDosInimigos.PontaInicial;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    public Vector3 RestringirPosicao(Vector3 posicao)
    {
        Vector3 resultado = Prender(posicao, MinX, MaxX, MinZ, MaxZ);
        return PrenderNasAreasExtras(posicao, resultado, 0f);
    }

    private static Vector3 Prender(Vector3 posicao, float minX, float maxX, float minZ, float maxZ)
    {
        return new Vector3(Mathf.Clamp(posicao.x, minX, maxX), posicao.y, Mathf.Clamp(posicao.z, minZ, maxZ));
    }

    // Com áreas extras, vale o ponto permitido mais próximo da posição
    // original (dentro de qualquer retângulo = posição não muda).
    private Vector3 PrenderNasAreasExtras(Vector3 original, Vector3 melhor, float margem)
    {
        if (_areasExtras == null || _areasExtras.Length == 0) return melhor;

        float melhorDistancia = DistanciaHorizontal(original, melhor);
        foreach (AreaExtra area in _areasExtras)
        {
            float meiaLargura = area.Tamanho.x / 2f + margem;
            float meioComprimento = area.Tamanho.y / 2f + margem;
            Vector3 candidata = Prender(original, area.Centro.x - meiaLargura, area.Centro.x + meiaLargura,
                area.Centro.z - meioComprimento, area.Centro.z + meioComprimento);

            float distancia = DistanciaHorizontal(original, candidata);
            if (distancia >= melhorDistancia) continue;
            melhorDistancia = distancia;
            melhor = candidata;
        }
        return melhor;
    }

    private static float DistanciaHorizontal(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }

    // Igual ao RestringirPosicao, mas libera a faixa atrás das barricadas
    // por onde os inimigos entram — senão eles seriam "teleportados" para
    // dentro da rua no primeiro frame e o spawn ficaria visível.
    public Vector3 RestringirPosicaoDeInimigo(Vector3 posicao)
    {
        bool temLateralEsquerda = false;
        bool temLateralDireita = false;
        foreach (Vector3 entrada in _entradasLaterais)
        {
            if (entrada.x < _centro.x) temLateralEsquerda = true;
            else temLateralDireita = true;
        }

        float minX = MinX - (temLateralEsquerda ? _recuoDoSpawn : 0f);
        float maxX = MaxX + (temLateralDireita ? _recuoDoSpawn : 0f);
        float minZ = MinZ - (NascemNoInicio ? _recuoDoSpawn : 0f);
        float maxZ = MaxZ + (NascemNoFinal ? _recuoDoSpawn : 0f);

        Vector3 resultado = Prender(posicao, minX, maxX, minZ, maxZ);
        return PrenderNasAreasExtras(posicao, resultado, _recuoDoSpawn);
    }

    // Sorteia uma das entradas configuradas e devolve um ponto atrás da
    // barricada correspondente, com variação ao longo da entrada.
    public Vector3 ObterPontoDeSpawn(float variacao)
    {
        bool usarLateral = _entradasLaterais.Length > 0 && Random.value < _chanceDeEntradaLateral;
        if (usarLateral)
        {
            Vector3 entrada = _entradasLaterais[Random.Range(0, _entradasLaterais.Length)];
            float ladoX = entrada.x < _centro.x ? -1f : 1f;
            float zLateral = entrada.z + Random.Range(-variacao, variacao);
            return new Vector3(entrada.x + ladoX * _recuoDoSpawn, 0f, zLateral);
        }

        bool usarInicio = _origemDosInimigos == OrigemDosInimigos.PontaInicial
            || (_origemDosInimigos == OrigemDosInimigos.AmbasAsPontas && Random.value < 0.5f);

        float z = usarInicio ? MinZ - _recuoDoSpawn : MaxZ + _recuoDoSpawn;
        float x = Mathf.Clamp(_centro.x + Random.Range(-variacao, variacao), MinX, MaxX);
        return new Vector3(x, 0f, z);
    }

    // Ponta principal de entrada (usada como referência por quem só
    // precisa de uma — com AmbasAsPontas, devolve a final).
    public Vector3 ObterPontaFinal()
    {
        float z = _origemDosInimigos == OrigemDosInimigos.PontaInicial ? MinZ : MaxZ;
        return new Vector3(_centro.x, 0f, z);
    }

    // Entrada de inimigos mais próxima de uma posição — rota de fuga do
    // hambúrguer que sequestra um cidadão.
    public Vector3 ObterSaidaMaisProximaDe(Vector3 posicao)
    {
        Vector3 melhor = ObterPontaFinal();
        float melhorDistancia = float.MaxValue;

        void Considerar(Vector3 candidata)
        {
            float distancia = Vector2.Distance(new Vector2(posicao.x, posicao.z), new Vector2(candidata.x, candidata.z));
            if (distancia >= melhorDistancia) return;
            melhorDistancia = distancia;
            melhor = candidata;
        }

        if (NascemNoInicio) Considerar(new Vector3(_centro.x, 0f, MinZ));
        if (NascemNoFinal) Considerar(new Vector3(_centro.x, 0f, MaxZ));
        foreach (Vector3 entrada in _entradasLaterais) Considerar(new Vector3(entrada.x, 0f, entrada.z));

        return melhor;
    }

    // Retorna a ponta da rua (das duas extremidades) mais distante da
    // posição de referência — usado pro chefão sempre nascer do lado
    // oposto de onde o jogador está.
    public Vector3 ObterPontaMaisDistanteDe(Vector3 posicaoReferencia)
    {
        Vector3 pontaPositiva = new Vector3(_centro.x, 0f, MaxZ);
        Vector3 pontaNegativa = new Vector3(_centro.x, 0f, MinZ);

        float distanciaPositiva = Mathf.Abs(pontaPositiva.z - posicaoReferencia.z);
        float distanciaNegativa = Mathf.Abs(pontaNegativa.z - posicaoReferencia.z);

        return distanciaPositiva >= distanciaNegativa ? pontaPositiva : pontaNegativa;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_centro, new Vector3(_largura, 0.1f, _comprimento));

        if (_areasExtras != null)
        {
            foreach (AreaExtra area in _areasExtras) Gizmos.DrawWireCube(area.Centro, new Vector3(area.Tamanho.x, 0.1f, area.Tamanho.y));
        }

        Gizmos.color = Color.red;
        if (NascemNoInicio) Gizmos.DrawWireCube(new Vector3(_centro.x, 0f, MinZ - _recuoDoSpawn), new Vector3(_largura, 1f, 0.5f));
        if (NascemNoFinal) Gizmos.DrawWireCube(new Vector3(_centro.x, 0f, MaxZ + _recuoDoSpawn), new Vector3(_largura, 1f, 0.5f));
        if (_entradasLaterais == null) return;
        foreach (Vector3 entrada in _entradasLaterais) Gizmos.DrawWireSphere(entrada, 1f);
    }
}
