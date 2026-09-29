using UnityEngine;
using UnityEngine.SceneManagement;

// Controla a cena de menu principal: botão de iniciar a partida.
// Fica na cena MainMenu, separada da cena de jogo (SampleScene).
public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private string _nomeCenaDeJogo = "SampleScene";

    public void IniciarJogo()
    {
        SceneManager.LoadScene(_nomeCenaDeJogo);
    }
}