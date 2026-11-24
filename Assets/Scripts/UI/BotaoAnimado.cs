using UnityEngine;
using UnityEngine.EventSystems;

// Dá vida aos botões dos menus: cresce um pouco quando o mouse passa por
// cima e "afunda" ao clicar. Usa tempo real, então funciona com o jogo pausado.
public class BotaoAnimado : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float _escalaAoPassar = 1.06f;
    [SerializeField] private float _escalaAoApertar = 0.95f;
    [SerializeField] private float _velocidade = 14f;

    private Vector3 _escalaOriginal = Vector3.one;
    private float _escalaAlvo = 1f;
    private bool _mouseEmCima = false;

    private void Awake()
    {
        _escalaOriginal = transform.localScale;
    }

    private void OnDisable()
    {
        _mouseEmCima = false;
        _escalaAlvo = 1f;
        transform.localScale = _escalaOriginal;
    }

    private void Update()
    {
        Vector3 desejada = _escalaOriginal * _escalaAlvo;
        transform.localScale = Vector3.Lerp(transform.localScale, desejada, Time.unscaledDeltaTime * _velocidade);
    }

    public void OnPointerEnter(PointerEventData dados)
    {
        _mouseEmCima = true;
        _escalaAlvo = _escalaAoPassar;
    }

    public void OnPointerExit(PointerEventData dados)
    {
        _mouseEmCima = false;
        _escalaAlvo = 1f;
    }

    public void OnPointerDown(PointerEventData dados)
    {
        _escalaAlvo = _escalaAoApertar;
    }

    public void OnPointerUp(PointerEventData dados)
    {
        _escalaAlvo = _mouseEmCima ? _escalaAoPassar : 1f;
    }
}
