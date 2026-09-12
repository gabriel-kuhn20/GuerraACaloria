using UnityEngine;

// Representa um cidadão da praça, alvo perseguido pelos inimigos.
// Não tem vida própria — servir de alvo é sua única responsabilidade;
// o dano real é aplicado na energia da cidade via GerenciadorDeJogo.
public class Cidadao : MonoBehaviour
{
    private void OnEnable()
    {
        GerenciadorDeCidadaos.Instancia.RegistrarCidadao(this);
    }

    private void OnDisable()
    {
        GerenciadorDeCidadaos.Instancia.RemoverCidadao(this);
    }
}