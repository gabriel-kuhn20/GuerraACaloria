using UnityEngine;

// Barrinha de saúde que flutua sobre a cabeça do cidadão, sempre virada
// para a câmera. Só aparece quando ele não está com saúde cheia. A moldura
// mostra a cor da munição que cura a contaminação dele.
// Criada por código (BarraDeSaudeDoCidadao.Criar), sem prefab.
public class BarraDeSaudeDoCidadao : MonoBehaviour
{
    private const float Largura = 1.1f;
    private const float Altura = 0.14f;
    private const float Borda = 0.05f;

    private static Sprite _spriteBranco;

    private SpriteRenderer _moldura;
    private SpriteRenderer _fundo;
    private SpriteRenderer _preenchimento;
    private Transform _camera;

    public static BarraDeSaudeDoCidadao Criar(Transform dono, float alturaNoMundo)
    {
        GameObject objeto = new GameObject("BarraDeSaude");
        objeto.transform.SetParent(dono, false);
        objeto.transform.position = dono.position + Vector3.up * alturaNoMundo;

        // Compensa a escala do cidadão para a barra ter sempre o mesmo tamanho.
        Vector3 escalaDoDono = dono.lossyScale;
        objeto.transform.localScale = new Vector3(1f / escalaDoDono.x, 1f / escalaDoDono.y, 1f / escalaDoDono.z);

        BarraDeSaudeDoCidadao barra = objeto.AddComponent<BarraDeSaudeDoCidadao>();
        barra.Montar();
        return barra;
    }

    private void Montar()
    {
        _moldura = CriarParte("Moldura", Largura + Borda * 2f, Altura + Borda * 2f, -Borda, 0);
        _fundo = CriarParte("Fundo", Largura, Altura, 0f, 1);
        _preenchimento = CriarParte("Preenchimento", Largura, Altura, 0f, 2);
        _fundo.color = new Color(0f, 0f, 0f, 0.7f);
        if (Camera.main != null) _camera = Camera.main.transform;
        gameObject.SetActive(false);
    }

    private SpriteRenderer CriarParte(string nome, float largura, float altura, float deslocamentoX, int ordem)
    {
        GameObject parte = new GameObject(nome);
        parte.transform.SetParent(transform, false);
        parte.transform.localPosition = new Vector3(-Largura / 2f + deslocamentoX, 0f, 0f);
        parte.transform.localScale = new Vector3(largura, altura, 1f);

        SpriteRenderer renderizador = parte.AddComponent<SpriteRenderer>();
        renderizador.sprite = SpriteBranco();
        renderizador.sortingOrder = ordem;
        return renderizador;
    }

    // Sprite branco 1x1 unidade com pivô na esquerda (para a barra "encher" da esquerda).
    private static Sprite SpriteBranco()
    {
        if (_spriteBranco == null)
        {
            Texture2D textura = Texture2D.whiteTexture;
            _spriteBranco = Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height), new Vector2(0f, 0.5f), textura.width);
        }
        return _spriteBranco;
    }

    public void Atualizar(float fracao, bool visivel, Color corDaMoldura)
    {
        if (gameObject.activeSelf != visivel) gameObject.SetActive(visivel);
        if (!visivel) return;

        fracao = Mathf.Clamp01(fracao);
        _preenchimento.transform.localScale = new Vector3(Largura * fracao, Altura, 1f);
        _preenchimento.color = Color.Lerp(new Color(0.95f, 0.3f, 0.3f), new Color(0.3f, 0.85f, 0.45f), fracao);
        _moldura.color = corDaMoldura;
    }

    private void LateUpdate()
    {
        if (_camera != null) transform.rotation = _camera.rotation;
    }
}
