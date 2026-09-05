using UnityEngine;

// Controla a rotação da câmera em 360° a partir do movimento do mouse.
// Fica anexado à Main Camera, dentro do objeto Player.

public class MouseLook : MonoBehaviour   
{
    [SerializeField] private float mouseSensitivity = 100f; // [SerializeField] permite que a variável seja editável no Inspector do Unity
    private float _rotacaoX = 0f; 

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Bloqueia o cursor no centro da tela
    }

    private void Update() 
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; // Captura o movimento do mouse no eixo X
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; // Captura o movimento do mouse no eixo Y

        _rotacaoX -= mouseY; // Inverte a rotação para que o movimento do mouse para cima faça a câmera olhar para cima
        _rotacaoX = Mathf.Clamp(_rotacaoX, -80f, 80f); // Limita a rotação vertical entre -80 e 80 graus

        transform.localRotation = Quaternion.Euler(_rotacaoX, 0f, 0f); // Aplica a rotação vertical à câmera
        transform.parent.Rotate(Vector3.up * mouseX); // Aplica a rotação horizontal ao objeto pai (geralmente o jogador)
    }
    


}
