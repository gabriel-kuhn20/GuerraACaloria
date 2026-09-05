using UnityEngine;

// Gerencia o estado global do jogo: energia da cidade, e futuramente
// onda atual e condição de vitória/derrota. Singleton acessado por
// outros scripts via GerenciadorDeJogo.Instancia. Também serve como
// ponto-alvo (centro da praça) para os inimigos, nesta versão da demo.
public class GerenciadorDeJogo : MonoBehaviour
{
    public static GerenciadorDeJogo Instancia {get; private set;} // Singleton

    [SerializeField] private int _energiaMaxima = 100;  
    private int _energiaAtual;

    public int EnergiaAtual => _energiaAtual; // Propriedade de leitura para a energia atual da cidade

 private void Awake()
{
    // Se ainda NÃO existe uma instância registrada, registra esta
    if (Instancia == null)
    {
        Instancia = this;
        _energiaAtual = _energiaMaxima;
    }
    else if (Instancia != this)
    {
        Destroy(gameObject); // Garante que não haja duplicatas do Gerenciador
    }
}

    public void ReceberDanoNaCidade(int quantidade)
    {
        _energiaAtual = Mathf.Max(0, _energiaAtual - quantidade); // Reduz a energia atual, garantindo que não fique abaixo de 0
        Debug.Log($"Energia da cidade: {_energiaAtual}/{_energiaMaxima}"); // Loga a energia atual no console

                if (_energiaAtual <= 0)
        {
            Debug.Log("GAME OVER");
        }
    }
}