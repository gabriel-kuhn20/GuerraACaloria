using UnityEngine;

// Define os parâmetros de uma onda: quantidade de inimigos e intervalo
// entre spawns. Cada onda é um asset separado, editável no Inspector
// sem precisar mexer em código.
[CreateAssetMenu(fileName = "NovaOnda", menuName = "GuerraACaloria/ConfiguracaoDeOnda")]
public class ConfiguracaoDeOnda : ScriptableObject
{
    public int quantidadeDeInimigos = 5;
    public float intervaloEntreSpawns = 2f;
}