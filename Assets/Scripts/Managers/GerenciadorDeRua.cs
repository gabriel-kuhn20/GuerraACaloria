using UnityEngine;

// Define os limites retangulares da rua (o "túnel" por onde o jogo
// acontece): jogador, cidadãos e inimigos ficam contidos dentro dela.
// Singleton, consultado por qualquer objeto que precise restringir
// seu movimento à área jogável.
public class GerenciadorDeRua : MonoBehaviour
{
    public static GerenciadorDeRua Instancia { get; private set; }

    [SerializeField] private float _largura = 10f;
    [SerializeField] private float _comprimento = 40f;
    [SerializeField] private Vector3 _centro = Vector3.zero;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    public Vector3 RestringirPosicao(Vector3 posicao)
    {
        float metadeLargura = _largura / 2f;
        float metadeComprimento = _comprimento / 2f;

        float x = Mathf.Clamp(posicao.x, _centro.x - metadeLargura, _centro.x + metadeLargura);
        float z = Mathf.Clamp(posicao.z, _centro.z - metadeComprimento, _centro.z + metadeComprimento);

        return new Vector3(x, posicao.y, z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_centro, new Vector3(_largura, 0.1f, _comprimento));
    }

    public Vector3 ObterPontaFinal()
    {
        float metadeComprimento = _comprimento / 2f;
        return new Vector3(_centro.x, 0f, _centro.z + metadeComprimento);
    }

    // Retorna a ponta da rua (das duas extremidades) mais distante da
    // posição de referência — usado pro chefão sempre nascer do lado
    // oposto de onde o jogador está, sem depender de qual ponta é
    // "início" ou "fim" na configuração da cena.
    public Vector3 ObterPontaMaisDistanteDe(Vector3 posicaoReferencia)
    {
        float metadeComprimento = _comprimento / 2f;
        Vector3 pontaPositiva = new Vector3(_centro.x, 0f, _centro.z + metadeComprimento);
        Vector3 pontaNegativa = new Vector3(_centro.x, 0f, _centro.z - metadeComprimento);

        float distanciaPositiva = Mathf.Abs(pontaPositiva.z - posicaoReferencia.z);
        float distanciaNegativa = Mathf.Abs(pontaNegativa.z - posicaoReferencia.z);

        return distanciaPositiva >= distanciaNegativa ? pontaPositiva : pontaNegativa;
    }
}