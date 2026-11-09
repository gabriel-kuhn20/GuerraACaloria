using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Funções de apoio para montar UI por código no editor (criar elementos,
// ancorar, textos, botões, sprites). Usado pelas ferramentas
// CriadorDeMenuPrincipal e CriadorDeHUD para não duplicar código.
public static class UtilitariosDeUIEditor
{
    public static Sprite SpriteArredondado => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    public static Sprite SpriteCircular => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

    public static Transform CriarCanvas(string nome = "Canvas")
    {
        GameObject objeto = new GameObject(nome, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        objeto.layer = LayerMask.NameToLayer("UI");
        objeto.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler escala = objeto.GetComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 0.5f;
        return objeto.transform;
    }

    public static RectTransform CriarUI(string nome, Transform pai)
    {
        GameObject objeto = new GameObject(nome, typeof(RectTransform));
        objeto.layer = LayerMask.NameToLayer("UI");
        objeto.transform.SetParent(pai, false);
        return (RectTransform)objeto.transform;
    }

    public static void Ancorar(RectTransform rect, Vector2 ancora, Vector2 posicao, Vector2 dimensao)
    {
        Ancorar(rect, ancora, ancora, posicao, dimensao);
    }

    // Âncora e pivô iguais: facilita prender elementos nos cantos da tela.
    public static void Ancorar(RectTransform rect, Vector2 ancora, Vector2 pivo, Vector2 posicao, Vector2 dimensao)
    {
        rect.anchorMin = ancora;
        rect.anchorMax = ancora;
        rect.pivot = pivo;
        rect.anchoredPosition = posicao;
        rect.sizeDelta = dimensao;
    }

    public static void Esticar(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public static Image CriarImagem(Transform pai, string nome, Color cor, Sprite sprite = null, bool fatiada = true)
    {
        RectTransform rect = CriarUI(nome, pai);
        Image imagem = rect.gameObject.AddComponent<Image>();
        imagem.color = cor;
        imagem.raycastTarget = false;
        if (sprite != null)
        {
            imagem.sprite = sprite;
            imagem.type = fatiada ? Image.Type.Sliced : Image.Type.Simple;
        }
        return imagem;
    }

    // Image do tipo Filled (horizontal), usada como barra de progresso.
    public static Image CriarBarraPreenchida(Transform pai, string nome, Color cor)
    {
        Image barra = CriarImagem(pai, nome, cor, SpriteArredondado, fatiada: false);
        barra.type = Image.Type.Filled;
        barra.fillMethod = Image.FillMethod.Horizontal;
        barra.fillOrigin = (int)Image.OriginHorizontal.Left;
        barra.fillAmount = 1f;
        Esticar(barra.rectTransform);
        return barra;
    }

    public static TextMeshProUGUI CriarTexto(Transform pai, string nome, string conteudo, float tamanho, Color cor, Vector2 posicao, Vector2 dimensao)
    {
        RectTransform rect = CriarUI(nome, pai);
        Ancorar(rect, new Vector2(0.5f, 0.5f), posicao, dimensao);

        TextMeshProUGUI texto = rect.gameObject.AddComponent<TextMeshProUGUI>();
        texto.text = conteudo;
        texto.fontSize = tamanho;
        texto.color = cor;
        texto.alignment = TextAlignmentOptions.Center;
        texto.raycastTarget = false;
        return texto;
    }

    public static Button CriarBotao(Transform pai, string nome, string rotulo, Color cor, UnityAction acao, float tamanhoFonte = 44f)
    {
        RectTransform rect = CriarUI(nome, pai);
        Image fundo = rect.gameObject.AddComponent<Image>();
        fundo.sprite = SpriteArredondado;
        fundo.type = Image.Type.Sliced;
        fundo.color = cor;

        Button botao = rect.gameObject.AddComponent<Button>();
        ColorBlock cores = botao.colors;
        cores.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        cores.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        cores.fadeDuration = 0.08f;
        botao.colors = cores;
        if (acao != null) UnityEventTools.AddPersistentListener(botao.onClick, acao);

        TextMeshProUGUI texto = CriarTexto(rect, "Texto", rotulo, tamanhoFonte, Color.white, Vector2.zero, Vector2.zero);
        Esticar(texto.rectTransform);
        texto.fontStyle = FontStyles.Bold;
        return botao;
    }

    // Alguns pacotes importam PNG como Textura; a UI precisa de Sprite.
    public static Sprite CarregarComoSprite(string caminho)
    {
        TextureImporter importador = AssetImporter.GetAtPath(caminho) as TextureImporter;
        if (importador == null) return null;

        if (importador.textureType != TextureImporterType.Sprite)
        {
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
    }

    public static Color CorPainelPadrao => new Color(0.04f, 0.09f, 0.14f, 0.95f);
    public static Color CorTituloPadrao => new Color(1f, 0.82f, 0.25f);
    public static Color CorVerdePadrao => new Color(0.27f, 0.72f, 0.38f);
    public static Color CorAzulPadrao => new Color(0.22f, 0.53f, 0.86f);
    public static Color CorVermelhaPadrao => new Color(0.82f, 0.3f, 0.3f);

    // Fundo escuro em tela cheia (bloqueia cliques atrás) + caixa central.
    public static RectTransform CriarJanela(Transform pai, string nome, string titulo, Vector2 tamanho, out RectTransform caixa)
    {
        RectTransform bloqueio = CriarUI(nome, pai);
        Esticar(bloqueio);
        bloqueio.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

        Image fundoDaCaixa = CriarImagem(bloqueio, "Caixa", CorPainelPadrao, SpriteArredondado);
        fundoDaCaixa.raycastTarget = true;
        caixa = fundoDaCaixa.rectTransform;
        Ancorar(caixa, new Vector2(0.5f, 0.5f), Vector2.zero, tamanho);

        TextMeshProUGUI textoTitulo = CriarTexto(caixa, "Titulo", titulo, 60f, CorTituloPadrao,
            new Vector2(0f, tamanho.y / 2f - 70f), new Vector2(tamanho.x - 80f, 80f));
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.characterSpacing = 4f;
        return bloqueio;
    }

    public static Button CriarBotaoNaPosicao(Transform pai, string nome, string rotulo, Color cor, UnityAction acao,
        Vector2 posicao, Vector2 tamanho, float tamanhoFonte = 36f)
    {
        Button botao = CriarBotao(pai, nome, rotulo, cor, acao, tamanhoFonte);
        Ancorar((RectTransform)botao.transform, new Vector2(0.5f, 0.5f), posicao, tamanho);
        return botao;
    }

    public static Slider CriarSlider(Transform pai, string nome, float minimo, float maximo, Color cor)
    {
        DefaultControls.Resources recursos = new DefaultControls.Resources
        {
            standard = SpriteArredondado,
            background = SpriteArredondado,
            knob = SpriteCircular,
        };

        GameObject objeto = DefaultControls.CreateSlider(recursos);
        objeto.name = nome;
        objeto.transform.SetParent(pai, false);
        foreach (Transform filho in objeto.GetComponentsInChildren<Transform>(true)) filho.gameObject.layer = LayerMask.NameToLayer("UI");

        Slider slider = objeto.GetComponent<Slider>();
        slider.minValue = minimo;
        slider.maxValue = maximo;
        slider.fillRect.GetComponent<Image>().color = cor;
        objeto.transform.Find("Background").GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
        return slider;
    }

    // Painel de opções (sensibilidade e volumes) já ligado ao componente PainelDeOpcoes.
    public static GameObject CriarPainelDeOpcoes(Transform pai, UnityAction aoFechar)
    {
        RectTransform janela = CriarJanela(pai, "PainelOpcoes", "OPÇÕES", new Vector2(980f, 640f), out RectTransform caixa);

        string[] rotulos = { "Sensibilidade do mouse", "Volume da música", "Volume dos efeitos" };
        Vector2[] limites = { new Vector2(ConfiguracoesDoJogo.SensibilidadeMinima, ConfiguracoesDoJogo.SensibilidadeMaxima), new Vector2(0f, 1f), new Vector2(0f, 1f) };
        Slider[] sliders = new Slider[3];
        TextMeshProUGUI[] valores = new TextMeshProUGUI[3];

        for (int i = 0; i < 3; i++)
        {
            float y = 120f - i * 110f;

            TextMeshProUGUI rotulo = CriarTexto(caixa, $"Rotulo{i}", rotulos[i], 34f, Color.white, new Vector2(-230f, y), new Vector2(420f, 60f));
            rotulo.alignment = TextAlignmentOptions.MidlineLeft;

            sliders[i] = CriarSlider(caixa, $"Slider{i}", limites[i].x, limites[i].y, CorAzulPadrao);
            Ancorar((RectTransform)sliders[i].transform, new Vector2(0.5f, 0.5f), new Vector2(170f, y), new Vector2(340f, 30f));

            valores[i] = CriarTexto(caixa, $"Valor{i}", "100%", 32f, CorTituloPadrao, new Vector2(400f, y), new Vector2(120f, 60f));
            valores[i].fontStyle = FontStyles.Bold;
        }

        CriarBotaoNaPosicao(caixa, "BotaoVoltar", "VOLTAR", CorVerdePadrao, aoFechar, new Vector2(0f, -230f), new Vector2(360f, 90f));

        PainelDeOpcoes painel = janela.gameObject.AddComponent<PainelDeOpcoes>();
        SerializedObject dados = new SerializedObject(painel);
        dados.FindProperty("_sliderSensibilidade").objectReferenceValue = sliders[0];
        dados.FindProperty("_sliderMusica").objectReferenceValue = sliders[1];
        dados.FindProperty("_sliderEfeitos").objectReferenceValue = sliders[2];
        dados.FindProperty("_valorSensibilidade").objectReferenceValue = valores[0];
        dados.FindProperty("_valorMusica").objectReferenceValue = valores[1];
        dados.FindProperty("_valorEfeitos").objectReferenceValue = valores[2];
        dados.ApplyModifiedPropertiesWithoutUndo();

        janela.gameObject.SetActive(false);
        return janela.gameObject;
    }
}
