using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Tela preta que escurece antes de trocar de cena e clareia na cena nova,
// para a passagem entre fases não ser um corte seco. Singleton criado pelo
// próprio código na primeira vez que é usado (não precisa estar na cena)
// e mantido entre as cenas com DontDestroyOnLoad.
public class TransicaoDeTela : MonoBehaviour
{
    private static TransicaoDeTela _instancia;

    [SerializeField] private float _duracaoEscurecer = 0.8f;
    [SerializeField] private float _pausaNoEscuro = 0.3f;
    [SerializeField] private float _duracaoClarear = 0.8f;

    private CanvasGroup _grupo;
    private bool _emTransicao = false;

    public static TransicaoDeTela Instancia
    {
        get
        {
            if (_instancia == null) Criar();
            return _instancia;
        }
    }

    public bool EmTransicao => _emTransicao;

    private static void Criar()
    {
        GameObject objeto = new GameObject("[ TRANSICAO DE TELA ]");
        DontDestroyOnLoad(objeto);
        _instancia = objeto.AddComponent<TransicaoDeTela>();
        _instancia.MontarTela();
    }

    private void MontarTela()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // acima de toda a HUD e dos menus

        _grupo = gameObject.AddComponent<CanvasGroup>();
        _grupo.alpha = 0f;
        _grupo.blocksRaycasts = false;

        GameObject fundo = new GameObject("Fundo", typeof(RectTransform), typeof(Image));
        fundo.transform.SetParent(transform, false);
        RectTransform retangulo = fundo.GetComponent<RectTransform>();
        retangulo.anchorMin = Vector2.zero;
        retangulo.anchorMax = Vector2.one;
        retangulo.offsetMin = Vector2.zero;
        retangulo.offsetMax = Vector2.zero;
        fundo.GetComponent<Image>().color = Color.black;
    }

    public void CarregarCena(string nomeDaCena)
    {
        if (_emTransicao) return;

        StartCoroutine(ExecutarTransicao(nomeDaCena));
    }

    private IEnumerator ExecutarTransicao(string nomeDaCena)
    {
        _emTransicao = true;
        _grupo.blocksRaycasts = true;

        yield return Desvanecer(0f, 1f, _duracaoEscurecer);

        AsyncOperation carregamento = SceneManager.LoadSceneAsync(nomeDaCena);
        while (!carregamento.isDone) yield return null;

        yield return new WaitForSecondsRealtime(_pausaNoEscuro);
        yield return Desvanecer(1f, 0f, _duracaoClarear);

        _grupo.blocksRaycasts = false;
        _emTransicao = false;
    }

    // Tempo real (unscaled): funciona mesmo com o jogo pausado (Time.timeScale = 0).
    private IEnumerator Desvanecer(float de, float para, float duracao)
    {
        float tempo = 0f;
        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;
            _grupo.alpha = Mathf.Lerp(de, para, Mathf.SmoothStep(0f, 1f, tempo / duracao));
            yield return null;
        }
        _grupo.alpha = para;
    }
}
