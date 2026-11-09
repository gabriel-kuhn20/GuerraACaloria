using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Ícones de alerta "!" na borda da tela apontando para cidadãos em perigo
// que estão fora da visão do jogador (contaminado, desmaiado ou sendo
// levado). Cor: munição que cura / vermelho (desmaiado) / amarelo
// pulsando (sequestro). Só lê o estado dos cidadãos.
public class IndicadoresDeCidadaos : MonoBehaviour
{
    [SerializeField] private RectTransform[] _indicadores;
    [SerializeField] private Image[] _imagens;
    [SerializeField] private TextMeshProUGUI[] _textos;
    [SerializeField] private float _margem = 70f;

    private Camera _camera;
    private RectTransform _area;

    private void Start()
    {
        _camera = Camera.main;
        _area = (RectTransform)transform;
        foreach (RectTransform indicador in _indicadores) indicador.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_camera == null) return;

        int usados = 0;
        foreach (Cidadao cidadao in GerenciadorDeCidadaos.Instancia.CidadaosVivos)
        {
            if (usados >= _indicadores.Length) break;
            if (!cidadao.PrecisaDeAjuda) continue;
            if (!TentarPosicionarNaBorda(cidadao.transform.position + Vector3.up * 1.5f, _indicadores[usados])) continue;

            Pintar(usados, cidadao);
            usados++;
        }

        for (int i = usados; i < _indicadores.Length; i++)
        {
            if (_indicadores[i].gameObject.activeSelf) _indicadores[i].gameObject.SetActive(false);
        }
    }

    // Retorna false se o cidadão já está visível na tela (não precisa de indicador).
    private bool TentarPosicionarNaBorda(Vector3 posicaoNoMundo, RectTransform indicador)
    {
        Vector3 ponto = _camera.WorldToViewportPoint(posicaoNoMundo);
        bool naFrente = ponto.z > 0f;
        bool dentroDaTela = naFrente && ponto.x > 0f && ponto.x < 1f && ponto.y > 0f && ponto.y < 1f;
        if (dentroDaTela) return false;

        // Atrás da câmera a projeção vem invertida.
        if (!naFrente)
        {
            ponto.x = 1f - ponto.x;
            ponto.y = 1f - ponto.y;
        }

        Vector2 direcao = new Vector2(ponto.x - 0.5f, ponto.y - 0.5f);
        if (direcao.sqrMagnitude < 0.0001f) direcao = Vector2.down;
        direcao /= Mathf.Max(Mathf.Abs(direcao.x), Mathf.Abs(direcao.y)) * 2f;

        Vector2 tamanho = _area.rect.size;
        indicador.anchoredPosition = new Vector2(direcao.x * (tamanho.x - 2f * _margem), direcao.y * (tamanho.y - 2f * _margem));
        if (!indicador.gameObject.activeSelf) indicador.gameObject.SetActive(true);
        return true;
    }

    private void Pintar(int indice, Cidadao cidadao)
    {
        Color cor;
        float escala = 1f;

        if (cidadao.EstaSendoCarregado)
        {
            cor = new Color(1f, 0.82f, 0.25f);
            escala = 1f + Mathf.PingPong(Time.unscaledTime * 3f, 0.35f);
        }
        else if (cidadao.EstaDesmaiado)
        {
            cor = new Color(0.95f, 0.3f, 0.3f);
        }
        else
        {
            cor = CoresDeMunicao.Obter(cidadao.MunicaoIndicada);
        }

        _imagens[indice].color = cor;
        _textos[indice].text = cidadao.EstaSendoCarregado ? "!!" : "!";
        _indicadores[indice].localScale = Vector3.one * escala;
    }
}
