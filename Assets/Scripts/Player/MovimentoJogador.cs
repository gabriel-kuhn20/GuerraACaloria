using UnityEngine;
using UnityEngine.InputSystem;

// Controla o movimento livre do Agente da Vitalidade pela rua (WASD),
// restrito aos limites definidos pelo GerenciadorDeRua. Pode ficar
// lento temporariamente (ex: perto da Nuvem de Preguiça).
[RequireComponent(typeof(CharacterController))]
public class MovimentoJogador : MonoBehaviour
{
    [SerializeField] private float _velocidade = 5f;
    private CharacterController _controller;
    private float _fatorDeLentidao = 1f;
    private float _fimDaLentidao;

    public bool EstaLento => Time.time < _fimDaLentidao;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        Vector2 entrada = ObterEntradaMovimento();
        Vector3 direcao = (transform.right * entrada.x + transform.forward * entrada.y).normalized;
        float fator = EstaLento ? _fatorDeLentidao : 1f;
        float velocidade = _velocidade * GerenciadorDeMelhorias.Atributos.MultiplicadorDeMovimento;
        _controller.Move(direcao * velocidade * fator * Time.deltaTime);

        RestringirNaRua();
    }

    // Fatores menores = mais lento. Se vários efeitos se sobrepõem, vale o mais forte.
    public void AplicarLentidao(float fator, float duracao)
    {
        if (!EstaLento) _fatorDeLentidao = 1f;

        _fatorDeLentidao = Mathf.Min(_fatorDeLentidao, Mathf.Clamp01(fator));
        _fimDaLentidao = Mathf.Max(_fimDaLentidao, Time.time + duracao);
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