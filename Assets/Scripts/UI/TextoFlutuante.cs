using TMPro;
using UnityEngine;

// Texto 3D que sobe e desaparece (ex: "+200  x3" quando um inimigo é
// derrotado). Fica sempre virado para a câmera. Criado por código via
// TextoFlutuante.Criar, sem precisar de prefab.
public class TextoFlutuante : MonoBehaviour
{
    private const float Duracao = 1f;
    private const float Subida = 2f;

    private TextMeshPro _texto;
    private Transform _camera;
    private Color _corInicial;
    private float _inicio;

    public static void Criar(Vector3 posicao, string conteudo, Color cor)
    {
        GameObject objeto = new GameObject("TextoFlutuante");
        objeto.transform.position = posicao;

        TextMeshPro texto = objeto.AddComponent<TextMeshPro>();
        texto.text = conteudo;
        texto.fontSize = 14f;
        texto.color = cor;
        texto.fontStyle = FontStyles.Bold;
        texto.alignment = TextAlignmentOptions.Center;
        texto.textWrappingMode = TextWrappingModes.NoWrap;

        objeto.AddComponent<TextoFlutuante>();
    }

    private void Start()
    {
        _texto = GetComponent<TextMeshPro>();
        _corInicial = _texto.color;
        _inicio = Time.time;
        if (Camera.main != null) _camera = Camera.main.transform;
    }

    private void Update()
    {
        float progresso = (Time.time - _inicio) / Duracao;
        if (progresso >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += Vector3.up * (Subida / Duracao) * Time.deltaTime;
        if (_camera != null) transform.rotation = _camera.rotation;

        Color cor = _corInicial;
        cor.a = 1f - progresso * progresso;
        _texto.color = cor;
    }
}
