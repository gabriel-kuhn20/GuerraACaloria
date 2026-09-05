using UnityEngine;

// Representa uma "nuvem de maus hábitos" avançando sobre a cidade.
// Move-se em direção ao GerenciadorDeJogo (centro da praça); se chegar lá,
// causa dano à energia da cidade.
public class Inimigo : MonoBehaviour, IDanificavel
{
    [SerializeField] private float _velocidade = 2f;
    private Transform _alvo;

private void Start()
{
    if (GerenciadorDeJogo.Instancia != null)
    {
        _alvo = GerenciadorDeJogo.Instancia.transform;
    }
    else
    {
        Debug.LogError("GerenciadorDeJogo não foi encontrado na cena!");
    }
}

private void Update()
{
    // Evita rodar o código se o alvo for nulo
    if (_alvo == null) return; 

    transform.position = Vector3.MoveTowards(
        transform.position, _alvo.position, _velocidade * Time.deltaTime);

    if (Vector3.Distance(transform.position, _alvo.position) < 1f)
    {
        GerenciadorDeJogo.Instancia.ReceberDanoNaCidade(10);
        Destroy(gameObject);
    }
}

    public void ReceberDano(TipoMunicao municaoUsada)
    {
        Destroy(gameObject);
    }
}