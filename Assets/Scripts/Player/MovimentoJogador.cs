using UnityEngine;
using UnityEngine.InputSystem;

// Controla o movimento livre do Agente da Vitalidade pela rua (WASD),
// restrito aos limites definidos pelo GerenciadorDeRua.
[RequireComponent(typeof(CharacterController))]
public class MovimentoJogador : MonoBehaviour
{
    [SerializeField] private float _velocidade = 5f;
    private CharacterController _controller;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        Vector2 entrada = ObterEntradaMovimento();
        Vector3 direcao = (transform.right * entrada.x + transform.forward * entrada.y).normalized;
        _controller.Move(direcao * _velocidade * Time.deltaTime);

        RestringirNaRua();
    }

    private void RestringirNaRua()
    {
        Vector3 posicaoRestringida = GerenciadorDeRua.Instancia.RestringirPosicao(transform.position);

        if (posicaoRestringida != transform.position)
        {
            _controller.enabled = false;
            transform.position = posicaoRestringida;
            _controller.enabled = true;
        }
    }

    private Vector2 ObterEntradaMovimento()
    {
        float x = 0f;
        float y = 0f;

        if (Keyboard.current.wKey.isPressed) y += 1f;
        if (Keyboard.current.sKey.isPressed) y -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.aKey.isPressed) x -= 1f;

        return new Vector2(x, y);
    }
}