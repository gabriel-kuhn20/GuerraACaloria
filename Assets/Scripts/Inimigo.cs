using UnityEngine;

// Representa uma "nuvem de maus hábitos" avançando sobre a cidade.
// Move-se em direção ao GerenciadorDeJogo (centro da praça); se chegar lá,
// causa dano à energia da cidade. Versão simplificada da demo — ainda sem
// herança para os diferentes tipos de inimigo.
public class Inimigo : MonoBehaviour
{
    [SerializeField] private float _velocidade = 2f;
    private Transform _alvo;

    private void Start()
    {
        _alvo = GerenciadorDeJogo.Instancia.transform;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, _alvo.position, _velocidade * Time.deltaTime);

        if (Vector3.Distance(transform.position, _alvo.position) < 1f)
        {
            GerenciadorDeJogo.Instancia.ReceberDanoNaCidade(10);
            Destroy(gameObject);
        }
    }

    public void ReceberDano()
    {
        Destroy(gameObject);
    }
}