using UnityEngine;
using UnityEngine.InputSystem;

// Controla a rotação da câmera em 360° a partir do movimento do mouse.
// Fica anexado à Main Camera, dentro do objeto Player.
// Usa o novo Input System diretamente (Mouse.current), sem depender
// de um asset de Input Actions configurado. A sensibilidade final é
// multiplicada pela opção escolhida pelo jogador (ConfiguracoesDoJogo).
public class MouseLook : MonoBehaviour
{
    [SerializeField] private float _sensibilidade = 0.1f;
    private float _rotacaoX = 0f;
    private float _multiplicadorDoJogador = 1f;

    // Barras de saúde, indicadores e textos flutuantes dependem de Camera.main,
    // que só encontra câmeras com a tag MainCamera.
    private void Awake()
    {
        gameObject.tag = "MainCamera";
    }

    private void OnEnable()
    {
        ConfiguracoesDoJogo.AoMudar += AtualizarSensibilidade;
        AtualizarSensibilidade();
    }

    private void OnDisable()
    {
        ConfiguracoesDoJogo.AoMudar -= AtualizarSensibilidade;
    }

    private void AtualizarSensibilidade()
    {
        _multiplicadorDoJogador = ConfiguracoesDoJogo.Sensibilidade;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        float mouseX = delta.x * _sensibilidade * _multiplicadorDoJogador;
        float mouseY = delta.y * _sensibilidade * _multiplicadorDoJogador;

        _rotacaoX -= mouseY;
        _rotacaoX = Mathf.Clamp(_rotacaoX, -80f, 80f);

        transform.localRotation = Quaternion.Euler(_rotacaoX, 0f, 0f);
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}