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
// MainMenu com título, botões estilizados, ícones decorativos e os painéis
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

        RectTransform colunaBotoes = CriarUI("Botoes", canvas);
        Ancorar(colunaBotoes, new Vector2(0.5f, 0.5f), new Vector2(0f, -135f), new Vector2(420f, 500f));
        VerticalLayoutGroup layout = colunaBotoes.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;

        CriarBotao(colunaBotoes, "BotaoJogar", "JOGAR", CorVerde, menu.IniciarJogo);
        CriarBotao(colunaBotoes, "BotaoFases", "FASES", CorVerde, menu.AbrirFases);
        CriarBotao(colunaBotoes, "BotaoComoJogar", "COMO JOGAR", CorAzul, menu.AbrirComoJogar);
        CriarBotao(colunaBotoes, "BotaoOpcoes", "OPÇÕES", CorAzul, menu.AbrirOpcoes);
        CriarBotao(colunaBotoes, "BotaoCreditos", "CRÉDITOS", CorAzul, menu.AbrirCreditos);
        CriarBotao(colunaBotoes, "BotaoSair", "SAIR", CorVermelha, menu.Sair);

        TextMeshProUGUI recorde = CriarTexto(canvas, "TextoRecorde", "RECORDE  0", 34f, CorTitulo, new Vector2(0f, -420f), new Vector2(800f, 50f));
        recorde.fontStyle = FontStyles.Bold;
        recorde.characterSpacing = 3f;

        GameObject painel = CriarPainelComoJogar(canvas, menu);
        GameObject opcoes = CriarPainelDeOpcoes(canvas, menu.FecharOpcoes);
        GameObject creditos = CriarPainelDeCreditos(canvas, menu);
        GameObject fases = CriarPainelDeFases(canvas, menu);
        GarantirMusicaNoMenu();

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
        RectTransform fundo = CriarUI("Fundo", canvas);
        Esticar(fundo);
        fundo.gameObject.AddComponent<Image>().color = CorFundo;

        // Faixa clara atrás do título para dar profundidade.
        RectTransform faixa = CriarUI("FaixaTitulo", canvas);
        Ancorar(faixa, new Vector2(0.5f, 0.5f), new Vector2(0f, 230f), new Vector2(2200f, 260f));
        faixa.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.05f);
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

        CriarTexto(canvas, "LegendaVilas", "VILÕES", 30f, new Color(1f, 0.55f, 0.45f), new Vector2(-690f, 470f), new Vector2(400f, 60f));
        CriarTexto(canvas, "LegendaAliados", "ALIADOS", 30f, new Color(0.55f, 0.95f, 0.6f), new Vector2(690f, 470f), new Vector2(400f, 60f));
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
    }

    private static void CriarTitulo(Transform canvas)
    {
        TextMeshProUGUI titulo = CriarTexto(canvas, "Titulo", "GUERRA À CALORIA", 120f, CorTitulo, new Vector2(0f, 255f), new Vector2(1300f, 170f));
        titulo.fontStyle = FontStyles.Bold;
        titulo.characterSpacing = 4f;
        titulo.textWrappingMode = TextWrappingModes.NoWrap;
        titulo.enableAutoSizing = true;
        titulo.fontSizeMin = 60f;
        titulo.fontSizeMax = 120f;

        CriarTexto(canvas, "Subtitulo", "Proteja a cidade dos maus hábitos com a munição certa!", 38f,
            new Color(0.88f, 0.94f, 1f), new Vector2(0f, 145f), new Vector2(1300f, 60f));
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
            "<color=#FFD240>Música:</color> trilha gerada com Suno\n" +
            "<color=#FFD240>Chefão e nuvem:</color> [preencher autor e licença]\n\n" +
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
}
