using UnityEngine;

// Faz a câmera tremer por um instante (ex: quando a cidade leva dano),
// deslocando a posição local e voltando ao lugar no final. Fica na
// Main Camera; adicionado automaticamente pelo GerenciadorDeEfeitos.
public class TremorDeCamera : MonoBehaviour
{
    private Vector3 _posicaoOriginal;
    private float _intensidade;
    private float _duracao;
    private float _fim;

    private void Awake()
    {
        _posicaoOriginal = transform.localPosition;
    }

    public void Tremer(float intensidade, float duracao)
    {
        _intensidade = intensidade;
        _duracao = Mathf.Max(0.01f, duracao);
        _fim = Time.time + _duracao;
    }

    private void LateUpdate()
    {
        if (Time.time >= _fim)
        {
            transform.localPosition = _posicaoOriginal;
            return;
        }

        float forca = _intensidade * ((_fim - Time.time) / _duracao);
        transform.localPosition = _posicaoOriginal + Random.insideUnitSphere * forca;
    }
}
