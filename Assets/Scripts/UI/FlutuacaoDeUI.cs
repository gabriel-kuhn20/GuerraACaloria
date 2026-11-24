using UnityEngine;

// Movimento decorativo suave para elementos de UI do menu (ícones e título):
// sobe e desce, balança e pulsa, cada um com uma fase aleatória para não
// ficarem sincronizados.
public class FlutuacaoDeUI : MonoBehaviour
{
    [SerializeField] private float _alturaDaFlutuacao = 12f;
    [SerializeField] private float _anguloDoBalanco = 4f;
    [SerializeField] private float _pulsoDeEscala = 0f;
    [SerializeField] private float _velocidade = 1.2f;

    private RectTransform _rect;
    private Vector2 _posicaoOriginal;
    private float _giroOriginal;
    private Vector3 _escalaOriginal;
    private float _fase;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _posicaoOriginal = _rect.anchoredPosition;
        _giroOriginal = _rect.localEulerAngles.z;
        _escalaOriginal = _rect.localScale;
        _fase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float t = Time.unscaledTime * _velocidade + _fase;
        _rect.anchoredPosition = _posicaoOriginal + Vector2.up * (Mathf.Sin(t) * _alturaDaFlutuacao);
        _rect.localRotation = Quaternion.Euler(0f, 0f, _giroOriginal + Mathf.Sin(t * 0.8f) * _anguloDoBalanco);
        _rect.localScale = _escalaOriginal * (1f + Mathf.Sin(t * 1.3f) * _pulsoDeEscala);
    }
}
