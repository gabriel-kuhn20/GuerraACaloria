using UnityEngine;
using System.Collections.Generic;

// Mantém a lista de cidadãos vivos na praça e permite consultar
// qual está mais próximo de uma posição (usado pelos inimigos para
// escolher quem perseguir). Singleton, como os demais managers.
public class GerenciadorDeCidadaos : MonoBehaviour
{
    public static GerenciadorDeCidadaos Instancia { get; private set; }

    private readonly List<Cidadao> _cidadaosVivos = new List<Cidadao>();

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    public void RegistrarCidadao(Cidadao cidadao)
    {
        if (!_cidadaosVivos.Contains(cidadao))
        {
            _cidadaosVivos.Add(cidadao);
        }
    }

    public void RemoverCidadao(Cidadao cidadao)
    {
        _cidadaosVivos.Remove(cidadao);
    }

    public Transform ObterCidadaoMaisProximo(Vector3 posicaoDeReferencia)
    {
        Transform maisProximo = null;
        float menorDistancia = float.MaxValue;

        foreach (Cidadao cidadao in _cidadaosVivos)
        {
            float distancia = Vector3.Distance(posicaoDeReferencia, cidadao.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                maisProximo = cidadao.transform;
            }
        }

        return maisProximo;
    }
}