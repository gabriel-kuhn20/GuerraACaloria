using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UtilitariosDeUIEditor;

// Ferramenta de editor (não vai para o build): remonta o Canvas da cena
// MainMenu com fundo em degradê, título com contorno, botões animados,
// ícones flutuantes, fonte Fredoka e os painéis
// "Fases" (ir direto para uma fase), "Como Jogar", Opções e Créditos;
// garante a música tocando no menu.
// Apaga o Canvas antigo e cria um novo, então pode ser rodada de novo.
// Menu: GuerraACaloria > Montar Menu Principal.
public static class CriadorDeMenuPrincipal
{
    private const string CaminhoCena = "Assets/Scenes/MainMenu.unity";
    private const string PastaIcones = "Assets/ithappy/Food_Free/Icons/";
    private const string PastaInimigos = "Assets/Prefabs/Inimigos/";
    private const string PastaAudio = "Assets/AssetsExternos/Audio/";
    private const string PastaFases = "Assets/ScriptableObjects/Fases/";
    private const string CaminhoDaFonte = "Assets/AssetsExternos/Fontes/Fredoka-SemiBold SDF.asset";
    private const string CaminhoMaterialDoTitulo = "Assets/AssetsExternos/Fontes/Fredoka-SemiBold Titulo.mat";
    private const string CaminhoFundoDegrade = "Assets/Sprites/UI/FundoMenuDegrade.png";
    private const string CaminhoBrilho = "Assets/Sprites/UI/BrilhoMenu.png";

    private static readonly Color CorFundoTopo = new Color(0.06f, 0.24f, 0.34f);
    private static readonly Color CorFundoBase = new Color(0.10f, 0.06f, 0.20f);
    private static readonly Color CorLaranja = new Color(1f, 0.55f, 0.2f);
    private static readonly Color CorVidro = new Color(1f, 1f, 1f, 0.13f);

    private static readonly Color CorFundo = new Color(0.07f, 0.16f, 0.24f);
    private static readonly Color CorPainel = new Color(0.04f, 0.09f, 0.14f, 0.92f);
    private static readonly Color CorTitulo = new Color(1f, 0.82f, 0.25f);
    private static readonly Color CorVerde = new Color(0.27f, 0.72f, 0.38f);
    private static readonly Color CorAzul = new Color(0.22f, 0.53f, 0.86f);
    private static readonly Color CorVermelha = new Color(0.82f, 0.3f, 0.3f);

    [MenuItem("GuerraACaloria/Montar Menu Principal")]
    private static void MontarMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene cena = EditorSceneManager.OpenScene(CaminhoCena);

        foreach (Canvas antigo in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(antigo.gameObject);
        }

        MenuPrincipal menu = Object.FindFirstObjectByType<MenuPrincipal>();
        Camera camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = CorFundo;
        }

        Transform canvas = CriarCanvas();
        CriarFundo(canvas);
        CriarIconesDecorativos(canvas);
        CriarTitulo(canvas);

        // Cartão translúcido atrás dos botões.
        Image cartao = CriarImagem(canvas, "CartaoBotoes", new Color(0f, 0f, 0f, 0.28f), SpriteArredondado);
        Ancorar(cartao.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -165f), new Vector2(520f, 580f));

        RectTransform colunaBotoes = CriarUI("Botoes", canvas);
        Ancorar(colunaBotoes, new Vector2(0.5f, 0.5f), new Vector2(0f, -165f), new Vector2(440f, 540f));
        VerticalLayoutGroup layout = colunaBotoes.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleCenter;

        CriarBotaoDoMenu(colunaBotoes, "BotaoJogar", "JOGAR", CorVerde, menu.IniciarJogo, 118f, 58f);
        CriarBotaoDoMenu(colunaBotoes, "BotaoFases", "FASES", new Color(0.16f, 0.6f, 0.62f), menu.AbrirFases, 78f, 38f);
        CriarBotaoDoMenu(colunaBotoes, "BotaoComoJogar", "COMO JOGAR", CorVidro, menu.AbrirComoJogar, 70f, 34f);
        CriarBotaoDoMenu(colunaBotoes, "BotaoOpcoes", "OPÇÕES", CorVidro, menu.AbrirOpcoes, 70f, 34f);
        CriarBotaoDoMenu(colunaBotoes, "BotaoCreditos", "CRÉDITOS", CorVidro, menu.AbrirCreditos, 70f, 34f);
        CriarBotaoDoMenu(colunaBotoes, "BotaoSair", "SAIR", new Color(0.82f, 0.3f, 0.3f, 0.85f), menu.Sair, 70f, 34f);

        // Selo do recorde (pílula escura com texto dourado).
        Image selo = CriarImagem(canvas, "SeloRecorde", new Color(0f, 0f, 0f, 0.35f), SpriteArredondado);
        Ancorar(selo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -492f), new Vector2(460f, 62f));
        TextMeshProUGUI recorde = CriarTexto(selo.rectTransform, "TextoRecorde", "RECORDE  0", 34f, CorTitulo, Vector2.zero, Vector2.zero);
        Esticar(recorde.rectTransform);
        recorde.characterSpacing = 3f;

        TextMeshProUGUI rodape = CriarTexto(canvas, "Rodape", "Gabriel Kuhn  ·  Engenharia de Software  ·  Univates", 24f,
            new Color(1f, 1f, 1f, 0.45f), Vector2.zero, Vector2.zero);
        Ancorar(rodape.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 20f), new Vector2(900f, 40f));
        rodape.alignment = TextAlignmentOptions.BottomRight;

        GameObject painel = CriarPainelComoJogar(canvas, menu);
        GameObject opcoes = CriarPainelDeOpcoes(canvas, menu.FecharOpcoes);
        GameObject creditos = CriarPainelDeCreditos(canvas, menu);
        GameObject fases = CriarPainelDeFases(canvas, menu);
        GarantirMusicaNoMenu();
        AplicarFonte(canvas);
        AnimarBotoes(canvas);

        SerializedObject dadosMenu = new SerializedObject(menu);
        dadosMenu.FindProperty("_painelComoJogar").objectReferenceValue = painel;
        dadosMenu.FindProperty("_textoRecorde").objectReferenceValue = recorde;
        dadosMenu.FindProperty("_painelOpcoes").objectReferenceValue = opcoes;
        dadosMenu.FindProperty("_painelCreditos").objectReferenceValue = creditos;
        dadosMenu.FindProperty("_painelFases").objectReferenceValue = fases;
        dadosMenu.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log("[CriadorDeMenuPrincipal] Menu principal montado com sucesso.");
    }

    private static void CriarFundo(Transform canvas)
    {
        Image fundo = CriarImagem(canvas, "Fundo", Color.white, CriarTexturaDeDegrade(), fatiada: false);
        Esticar(fundo.rectTransform);

        // Brilho suave atrás do título e outro, mais fraco, na base.
        Sprite brilho = CriarTexturaDeBrilho();
        Image brilhoTitulo = CriarImagem(canvas, "BrilhoTitulo", new Color(1f, 0.82f, 0.35f, 0.22f), brilho, fatiada: false);
        Ancorar(brilhoTitulo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(1700f, 620f));
        Image brilhoBase = CriarImagem(canvas, "BrilhoBase", new Color(0.4f, 1f, 0.6f, 0.08f), brilho, fatiada: false);
        Ancorar(brilhoBase.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -560f), new Vector2(2400f, 700f));
    }

    // Comidas "vilãs" à esquerda e saudáveis à direita, reforçando o tema.
    private static void CriarIconesDecorativos(Transform canvas)
    {
        string[] vilas = { "burger_001", "soda_002", "donut_002", "chips_008" };
        string[] saudaveis = { "watermelon_001", "avocado_001", "pepper_003", "fish_004" };
        Vector2[] posicoes = { new Vector2(-760f, 300f), new Vector2(-600f, 60f), new Vector2(-790f, -180f), new Vector2(-600f, -390f) };

        for (int i = 0; i < vilas.Length; i++)
        {
            CriarIcone(canvas, vilas[i], posicoes[i], -12f + i * 8f);
            Vector2 espelhada = new Vector2(-posicoes[i].x, posicoes[i].y);
            CriarIcone(canvas, saudaveis[i], espelhada, 12f - i * 8f);
        }

        CriarPilula(canvas, "LegendaVilas", "VILÕES", new Color(1f, 0.45f, 0.4f), new Vector2(-690f, 470f));
        CriarPilula(canvas, "LegendaAliados", "ALIADOS", new Color(0.45f, 0.95f, 0.55f), new Vector2(690f, 470f));
    }

    private static void CriarIcone(Transform canvas, string nome, Vector2 posicao, float giro)
    {
        Sprite sprite = CarregarComoSprite($"{PastaIcones}{nome}.png");
        if (sprite == null) return;

        RectTransform icone = CriarUI($"Icone_{nome}", canvas);
        Ancorar(icone, new Vector2(0.5f, 0.5f), posicao, new Vector2(190f, 190f));
        icone.localRotation = Quaternion.Euler(0f, 0f, giro);

        Image imagem = icone.gameObject.AddComponent<Image>();
        imagem.sprite = sprite;
        imagem.preserveAspect = true;
        imagem.raycastTarget = false;

        // Sombra no "chão" e flutuação lenta.
        Shadow sombra = icone.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.35f);
        sombra.effectDistance = new Vector2(6f, -10f);
        AdicionarFlutuacao(icone.gameObject, 14f, 5f, 0f, Random.Range(0.8f, 1.3f));
    }

    private static void CriarPilula(Transform canvas, string nome, string rotulo, Color cor, Vector2 posicao)
    {
        Image fundo = CriarImagem(canvas, nome, new Color(cor.r, cor.g, cor.b, 0.18f), SpriteArredondado);
        Ancorar(fundo.rectTransform, new Vector2(0.5f, 0.5f), posicao, new Vector2(240f, 56f));
        TextMeshProUGUI texto = CriarTexto(fundo.rectTransform, "Texto", rotulo, 30f, cor, Vector2.zero, Vector2.zero);
        Esticar(texto.rectTransform);
        texto.characterSpacing = 4f;
    }

    private static void CriarTitulo(Transform canvas)
    {
        TextMeshProUGUI titulo = CriarTexto(canvas, "Titulo", "GUERRA À CALORIA", 136f, Color.white, new Vector2(0f, 285f), new Vector2(1400f, 190f));
        titulo.characterSpacing = 4f;
        titulo.textWrappingMode = TextWrappingModes.NoWrap;
        titulo.enableAutoSizing = true;
        titulo.fontSizeMin = 60f;
        titulo.fontSizeMax = 136f;
        // Degradê amarelo -> laranja nas letras.
        titulo.enableVertexGradient = true;
        titulo.colorGradient = new VertexGradient(new Color(1f, 0.92f, 0.4f), new Color(1f, 0.92f, 0.4f), CorLaranja, CorLaranja);
        AdicionarFlutuacao(titulo.gameObject, 6f, 0.8f, 0.012f, 0.9f);

        Image fundoSubtitulo = CriarImagem(canvas, "FundoSubtitulo", new Color(0f, 0f, 0f, 0.3f), SpriteArredondado);
        Ancorar(fundoSubtitulo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(1060f, 64f));
        TextMeshProUGUI subtitulo = CriarTexto(fundoSubtitulo.rectTransform, "Subtitulo", "Proteja a cidade dos maus hábitos com a munição certa!", 34f,
            new Color(0.9f, 0.96f, 1f), Vector2.zero, Vector2.zero);
        Esticar(subtitulo.rectTransform);
    }

    private static GameObject CriarPainelComoJogar(Transform canvas, MenuPrincipal menu)
    {
        RectTransform bloqueio = CriarUI("PainelComoJogar", canvas);
        Esticar(bloqueio);
        bloqueio.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        RectTransform caixa = CriarUI("Caixa", bloqueio);
        Ancorar(caixa, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1250f, 780f));
        Image fundoCaixa = caixa.gameObject.AddComponent<Image>();
        fundoCaixa.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        fundoCaixa.type = Image.Type.Sliced;
        fundoCaixa.color = CorPainel;

        TextMeshProUGUI titulo = CriarTexto(caixa, "Titulo", "COMO JOGAR", 64f, CorTitulo, new Vector2(0f, 310f), new Vector2(1000f, 90f));
        titulo.fontStyle = FontStyles.Bold;

        TextMeshProUGUI controles = CriarTexto(caixa, "Controles",
            "<b>CONTROLES</b>\n\n" +
            "<color=#FFD240>W A S D</color>  mover\n" +
            "<color=#FFD240>Mouse</color>  mirar\n" +
            "<color=#FFD240>Botão esquerdo</color>  atirar\n" +
            "<color=#FFD240>1</color>  Água    <color=#FFD240>2</color>  Fruta\n" +
            "<color=#FFD240>3</color>  Energia Positiva",
            34f, Color.white, new Vector2(-300f, 20f), new Vector2(560f, 460f));
        controles.alignment = TextAlignmentOptions.TopLeft;

        TextMeshProUGUI fraquezas = CriarTexto(caixa, "Fraquezas", "", 34f, Color.white, new Vector2(300f, -45f), new Vector2(560f, 380f));
        fraquezas.alignment = TextAlignmentOptions.TopLeft;
        TextMeshProUGUI tituloFraquezas = CriarTexto(caixa, "TituloFraquezas", "<b>FRAQUEZAS DOS INIMIGOS</b>", 34f, Color.white, new Vector2(300f, 215f), new Vector2(560f, 60f));
        tituloFraquezas.alignment = TextAlignmentOptions.TopLeft;
        ConfigurarLegenda(caixa.gameObject, fraquezas);

        RectTransform areaBotao = CriarUI("AreaBotao", caixa);
        Ancorar(areaBotao, new Vector2(0.5f, 0.5f), new Vector2(0f, -300f), new Vector2(360f, 90f));
        CriarBotao(areaBotao, "BotaoVoltar", "VOLTAR", CorVerde, menu.FecharComoJogar);
        Esticar((RectTransform)areaBotao.GetChild(0));

        bloqueio.gameObject.SetActive(false);
        return bloqueio.gameObject;
    }

    // Um botão por fase (com o nome vindo do asset da fase) que leva direto
    // para a cena dela, mais o VOLTAR.
    private static GameObject CriarPainelDeFases(Transform canvas, MenuPrincipal menu)
    {
        const int totalDeFases = 4;
        RectTransform janela = CriarJanela(canvas, "PainelFases", "ESCOLHER FASE", new Vector2(960f, 800f), out RectTransform caixa);

        for (int numero = 1; numero <= totalDeFases; numero++)
        {
            ConfiguracaoDeFase fase = AssetDatabase.LoadAssetAtPath<ConfiguracaoDeFase>($"{PastaFases}Fase{numero}.asset");
            string nome = fase != null ? fase.Nome.ToUpper() : "";
            string rotulo = string.IsNullOrEmpty(nome) ? $"FASE {numero}" : $"FASE {numero}  ·  {nome}";

            Button botao = CriarBotaoNaPosicao(caixa, $"BotaoFase{numero}", rotulo, CorAzul, null,
                new Vector2(0f, 215f - (numero - 1) * 110f), new Vector2(800f, 92f), 34f);
            UnityEventTools.AddIntPersistentListener(botao.onClick, menu.IniciarFase, numero);
        }

        CriarBotaoNaPosicao(caixa, "BotaoVoltar", "VOLTAR", CorVerde, menu.FecharFases, new Vector2(0f, -310f), new Vector2(360f, 90f));

        janela.gameObject.SetActive(false);
        return janela.gameObject;
    }

    // Créditos dos assets de terceiros usados no jogo (alguns exigem atribuição).
    private static GameObject CriarPainelDeCreditos(Transform canvas, MenuPrincipal menu)
    {
        RectTransform janela = CriarJanela(canvas, "PainelCreditos", "CRÉDITOS", new Vector2(1300f, 820f), out RectTransform caixa);

        TextMeshProUGUI texto = CriarTexto(caixa, "Texto",
            "<b>GUERRA À CALORIA</b>\nDesenvolvido por Gabriel Kuhn - Engenharia de Software, Univates\n\n" +
            "<color=#FFD240>Cenário e comidas:</color> Cartoon City Free e Food Free - ithappy (Unity Asset Store)\n" +
            "<color=#FFD240>Cidadãos:</color> modelos low poly de Quaternius (CC0)\n" +
            "<color=#FFD240>Efeitos sonoros:</color> Kenney (CC0)\n" +
            "<color=#FFD240>Raio de energia:</color> \"Lightning bolt\" por Poly by Google (CC-BY)\n" +
            "<color=#FFD240>Música:</color> trilha gerada com Suno\n\n" +
            "<size=80%>Feito com Unity 6 (URP)</size>",
            32f, Color.white, new Vector2(0f, 10f), new Vector2(1180f, 560f));
        texto.alignment = TextAlignmentOptions.Top;

        CriarBotaoNaPosicao(caixa, "BotaoVoltar", "VOLTAR", CorVerde, menu.FecharCreditos, new Vector2(0f, -330f), new Vector2(360f, 90f));

        janela.gameObject.SetActive(false);
        return janela.gameObject;
    }

    // O menu também toca a música de fundo (mesmo GerenciadorDeAudio do jogo).
    private static void GarantirMusicaNoMenu()
    {
        GerenciadorDeAudio audio = Object.FindFirstObjectByType<GerenciadorDeAudio>();
        if (audio == null)
        {
            GameObject objeto = new GameObject("GerenciadorDeAudio", typeof(AudioSource));
            audio = objeto.AddComponent<GerenciadorDeAudio>();
        }

        SerializedObject dados = new SerializedObject(audio);
        dados.FindProperty("_somAcerto").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{PastaAudio}Impacto.ogg");
        dados.FindProperty("_somErro").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{PastaAudio}Erro.ogg");
        dados.FindProperty("_musicaDeFundo").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{PastaAudio}Riff.mp3");
        dados.ApplyModifiedPropertiesWithoutUndo();
        ConfiguradorDeAudio.Configurar(audio);
    }

    private static void ConfigurarLegenda(GameObject alvo, TextMeshProUGUI texto)
    {
        LegendaFraquezas legenda = alvo.AddComponent<LegendaFraquezas>();
        string[] nomes = { "HamburguerVoador", "RefrigeranteZumbi", "NuvemDePreguica" };

        SerializedObject dados = new SerializedObject(legenda);
        dados.FindProperty("_textoLegenda").objectReferenceValue = texto;
        SerializedProperty prefabs = dados.FindProperty("_prefabsDeInimigo");
        prefabs.arraySize = nomes.Length;
        for (int i = 0; i < nomes.Length; i++)
        {
            prefabs.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PastaInimigos}{nomes[i]}.prefab");
        }
        dados.ApplyModifiedPropertiesWithoutUndo();
    }

    // ----- Estilo -----

    private static void CriarBotaoDoMenu(Transform coluna, string nome, string rotulo, Color cor, UnityAction acao, float altura, float tamanhoFonte)
    {
        Button botao = CriarBotao(coluna, nome, rotulo, cor, acao, tamanhoFonte);
        LayoutElement elemento = botao.gameObject.AddComponent<LayoutElement>();
        elemento.preferredHeight = altura;
        elemento.minHeight = altura;
    }

    // Sombra + animação de hover em todos os botões do Canvas (menu e painéis).
    private static void AnimarBotoes(Transform canvas)
    {
        foreach (Button botao in canvas.GetComponentsInChildren<Button>(true))
        {
            if (botao.GetComponent<BotaoAnimado>() == null) botao.gameObject.AddComponent<BotaoAnimado>();
            if (botao.GetComponent<Shadow>() != null) continue;

            Shadow sombra = botao.gameObject.AddComponent<Shadow>();
            sombra.effectColor = new Color(0f, 0f, 0f, 0.4f);
            sombra.effectDistance = new Vector2(0f, -6f);
        }
    }

    private static void AdicionarFlutuacao(GameObject alvo, float altura, float angulo, float pulso, float velocidade)
    {
        FlutuacaoDeUI flutuacao = alvo.AddComponent<FlutuacaoDeUI>();
        SerializedObject dados = new SerializedObject(flutuacao);
        dados.FindProperty("_alturaDaFlutuacao").floatValue = altura;
        dados.FindProperty("_anguloDoBalanco").floatValue = angulo;
        dados.FindProperty("_pulsoDeEscala").floatValue = pulso;
        dados.FindProperty("_velocidade").floatValue = velocidade;
        dados.ApplyModifiedPropertiesWithoutUndo();
    }

    // Fredoka em todos os textos (a mesma da HUD); o título ganha um
    // material com contorno escuro e sombra.
    private static void AplicarFonte(Transform canvas)
    {
        TMP_FontAsset fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CaminhoDaFonte);
        if (fonte == null)
        {
            Debug.LogWarning("[CriadorDeMenuPrincipal] Fonte Fredoka não encontrada; rode antes GuerraACaloria > Montar HUD.");
            return;
        }

        foreach (TextMeshProUGUI texto in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            texto.font = fonte;
            texto.fontStyle &= ~FontStyles.Bold;
        }

        Transform titulo = canvas.Find("Titulo");
        if (titulo != null) titulo.GetComponent<TextMeshProUGUI>().fontSharedMaterial = ObterMaterialDoTitulo(fonte);
    }

    private static Material ObterMaterialDoTitulo(TMP_FontAsset fonte)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(CaminhoMaterialDoTitulo);
        if (material == null)
        {
            material = new Material(fonte.material);
            AssetDatabase.CreateAsset(material, CaminhoMaterialDoTitulo);
        }

        material.EnableKeyword(ShaderUtilities.Keyword_Outline);
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.22f);
        material.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.25f, 0.08f, 0.02f));
        material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.55f));
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.9f);
        material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.25f);
        EditorUtility.SetDirty(material);
        return material;
    }

    // Texturas geradas uma vez e salvas em Assets/Sprites/UI.
    private static Sprite CriarTexturaDeDegrade()
    {
        const int altura = 256;
        Texture2D textura = new Texture2D(1, altura, TextureFormat.RGBA32, false);
        for (int y = 0; y < altura; y++)
        {
            textura.SetPixel(0, y, Color.Lerp(CorFundoBase, CorFundoTopo, y / (altura - 1f)));
        }
        return SalvarComoSprite(textura, CaminhoFundoDegrade);
    }

    private static Sprite CriarTexturaDeBrilho()
    {
        const int tamanho = 128;
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
        Vector2 centro = new Vector2((tamanho - 1) / 2f, (tamanho - 1) / 2f);
        for (int x = 0; x < tamanho; x++)
        {
            for (int y = 0; y < tamanho; y++)
            {
                float distancia = Vector2.Distance(new Vector2(x, y), centro) / (tamanho / 2f);
                float alfa = Mathf.Pow(Mathf.Clamp01(1f - distancia), 2f);
                textura.SetPixel(x, y, new Color(1f, 1f, 1f, alfa));
            }
        }
        return SalvarComoSprite(textura, CaminhoBrilho);
    }

    private static Sprite SalvarComoSprite(Texture2D textura, string caminho)
    {
        textura.Apply();
        System.IO.File.WriteAllBytes(caminho, textura.EncodeToPNG());
        Object.DestroyImmediate(textura);
        AssetDatabase.ImportAsset(caminho);

        TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(caminho);
        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.wrapMode = TextureWrapMode.Clamp;
        importador.mipmapEnabled = false;
        importador.alphaIsTransparency = true;
        importador.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
    }
}
