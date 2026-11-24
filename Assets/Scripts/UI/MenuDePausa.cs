using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Menu de pausa da partida (tecla Esc): continuar, reiniciar a fase,
// abrir as opções ou voltar ao menu principal. Pausa congelando o tempo
// (Time.timeScale = 0). Não abre durante a escolha de melhoria nem depois do fim de jogo.
public class MenuDePausa : MonoBehaviour
{
    [SerializeField] private GameObject _painelPausa;
    [SerializeField] private GameObject _painelOpcoes;
    [SerializeField] private string _nomeCenaDoMenu = "MainMenu";

    private bool _pausado;

    public bool Pausado => _pausado;

    private void Start()
    {
        _painelPausa.SetActive(false);
        _painelOpcoes.SetActive(false);
    }

    private void Update()
    {
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (!_pausado)
        {
            if (PodePausar()) Pausar();
            return;
        }

        if (_painelOpcoes.activeSelf) FecharOpcoes();
        else Continuar();
    }

    private bool PodePausar()
    {
        if (GerenciadorDeJogo.Instancia.JogoAcabou) return false;
        if (GerenciadorDeMelhorias.Instancia != null && GerenciadorDeMelhorias.Instancia.EscolhaAberta) return false;
        return true;
    }

    private void Pausar()
    {
        _pausado = true;
        Time.timeScale = 0f;
        _painelPausa.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Continuar()
    {
        GerenciadorDeAudio.Instancia.TocarSomClique();
        _pausado = false;
        _painelPausa.SetActive(false);
        _painelOpcoes.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReiniciarFase()
    {
        TransicaoDeTela.Instancia.CarregarCena(SceneManager.GetActiveScene().name);
    }

    public void VoltarAoMenu()
    {
        TransicaoDeTela.Instancia.CarregarCena(_nomeCenaDoMenu);
    }

    public void AbrirOpcoes()
    {
        GerenciadorDeAudio.Instancia.TocarSomClique();
        _painelPausa.SetActive(false);
        _painelOpcoes.SetActive(true);
    }

    public void FecharOpcoes()
    {
        GerenciadorDeAudio.Instancia.TocarSomClique();
        _painelOpcoes.SetActive(false);
        _painelPausa.SetActive(true);
    }
}
