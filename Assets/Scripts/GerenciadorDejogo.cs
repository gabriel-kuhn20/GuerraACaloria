using UnityEngine;

// Controla o disparo da luva tecnológica do Agente da Vitalidade.
// Fica anexado à Main Camera; dispara um raycast na direção da mira
// e aplica dano a qualquer Inimigo atingido.
namespace GuerraACaloria
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private float _alcance = 100f;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Verifica se o botão esquerdo do mouse foi pressionado
            {
                Atirar();
            }
        }

        private void Atirar()
        {
            Ray raio = new Ray(transform.position, transform.forward); // Cria um raio a partir da posição da câmera na direção que ela está olhando

            if(Physics.Raycast(raio, out RaycastHit acerto, _alcance)) // Verifica se o raio colidiu com algum objeto dentro do alcance
            {
                Inimigo inimigoAtingido = acerto.collider.GetComponent<Inimigo>(); // Tenta obter o componente Inimigo do objeto atingido
                if (inimigoAtingido != null) // Se o objeto atingido for um inimigo
                {
                    inimigoAtingido.ReceberDano(); // Aplica dano ao inimigo
                }
            }
        }

    }
}