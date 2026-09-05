using UnityEngine;
using UnityEngine.InputSystem;

// Controla o disparo da luva tecnológica do Agente da Vitalidade.
// Fica anexado à Main Camera; dispara um raycast na direção da mira,
// aplicando a munição selecionada a qualquer IDanificavel atingido.
// Usa o novo Input System diretamente (Mouse.current / Keyboard.current).
public class WeaponController : MonoBehaviour
{
    [SerializeField] private float _alcance = 100f;
    private TipoMunicao _municaoAtual = TipoMunicao.Agua;

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
        if (Keyboard.current.digit1Key.wasPressedThisFrame) _municaoAtual = TipoMunicao.Agua;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) _municaoAtual = TipoMunicao.Fruta;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) _municaoAtual = TipoMunicao.EnergiaPositiva;
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