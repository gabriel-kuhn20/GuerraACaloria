using UnityEngine;
using UnityEngine.InputSystem;

// Controla o disparo da luva tecnológica do Agente da Vitalidade.
// Fica anexado à Main Camera; instancia um projétil na direção da mira,
// que viaja até colidir com um alvo.
public class WeaponController : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private GameObject _prefabProjetil;
    [SerializeField] private Transform _pontoDeDisparo;
    private TipoMunicao _municaoAtual = TipoMunicao.Agua;

    private void Start()
    {
        _uiManager.AtualizarTextoMunicao(_municaoAtual);
    }

    private void Update()
    {
        TrocarMunicaoPorTeclado();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Atirar();
        }
    }

    private void TrocarMunicaoPorTeclado()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) DefinirMunicao(TipoMunicao.Agua);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) DefinirMunicao(TipoMunicao.Fruta);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) DefinirMunicao(TipoMunicao.EnergiaPositiva);
    }

    private void DefinirMunicao(TipoMunicao municao)
    {
        _municaoAtual = municao;
        _uiManager.AtualizarTextoMunicao(_municaoAtual);
    }

    private void Atirar()
    {
        Transform origem = _pontoDeDisparo != null ? _pontoDeDisparo : transform;
        GameObject projetilCriado = Instantiate(_prefabProjetil, origem.position, origem.rotation);
        projetilCriado.GetComponent<Projetil>().Configurar(_municaoAtual);
        GerenciadorDeAudio.Instancia.TocarSomTiro();
    }
}