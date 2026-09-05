using UnityEngine;
using UnityEngine.InputSystem;

// Controla o disparo da luva tecnológica do Agente da Vitalidade.
// Fica anexado à Main Camera; dispara um raycast na direção da mira,
// aplicando a munição selecionada a qualquer IDanificavel atingido.
public class WeaponController : MonoBehaviour
{
    [SerializeField] private float _alcance = 100f;
    [SerializeField] private UIManager _uiManager;
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
        Ray raio = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(raio, out RaycastHit acerto, _alcance))
        {
            IDanificavel alvo = acerto.collider.GetComponent<IDanificavel>();
            alvo?.ReceberDano(_municaoAtual);
        }
    }
}