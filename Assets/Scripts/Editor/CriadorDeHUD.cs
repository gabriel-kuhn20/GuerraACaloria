using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using static UtilitariosDeUIEditor;

// Ferramenta de editor (não vai para o build): nas cenas de jogo
// (SampleScene e Fase2–4) remonta o Canvas do HUD, liga todas as
// referências do UIManager, monta menu de pausa e opções, garante
// os managers (pontuação, power-ups, efeitos), o tutorial da Fase 1,
// (re)cria os prefabs de power-up, gera os ícones das munições a partir
// dos modelos reais do projétil e aplica a fonte Fredoka em todo o HUD.
// Pode ser rodada de novo sem duplicar.
// Menu: GuerraACaloria > Montar HUD e Sistemas das Fases.
public static class CriadorDeHUD
{
    private static readonly string[] Cenas =
    {
        "Assets/Scenes/SampleScene.unity",
        "Assets/Scenes/Fase2.unity",
        "Assets/Scenes/Fase3.unity",
        "Assets/Scenes/Fase4.unity",
    };

    private const string PastaIcones = "Assets/ithappy/Food_Free/Icons/";
    private const string MiraPadrao = "Assets/AssetsExternos/Crosshairs/crosshair-020.png";
    private const string PastaPowerUps = "Assets/Prefabs/PowerUps/";
    private const string PastaComidas = "Assets/ithappy/Food_Free/Prefabs/";
    private const string CaminhoDoProjetil = "Assets/Prefabs/Projeteis/Projetil.prefab";
    private const string PastaIconesDeMunicao = "Assets/Sprites/UI";
    private const string CaminhoDaFonte = "Assets/AssetsExternos/Fontes/Fredoka-SemiBold.ttf";
    private const string CaminhoDoAssetDaFonte = "Assets/AssetsExternos/Fontes/Fredoka-SemiBold SDF.asset";
    private const string CaracteresDoJogo =
        " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
        "ÀÁÂÃÇÉÊÍÓÔÕÚÜàáâãçéêíóôõúü·•×ºª»";
    private const string MaterialDeParticulas = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat";

    private static readonly Color CorPainel = new Color(0.03f, 0.07f, 0.12f, 0.78f);
    private static readonly Color CorTrilho = new Color(1f, 1f, 1f, 0.14f);
    private static readonly Color CorTextoSecundario = new Color(1f, 1f, 1f, 0.72f);
    private static readonly Color CorDestaque = new Color(1f, 0.82f, 0.25f);
    private static readonly Color CorVerde = new Color(0.27f, 0.72f, 0.38f);
    private static readonly Color CorAzul = new Color(0.22f, 0.53f, 0.86f);

    [MenuItem("GuerraACaloria/Montar HUD e Sistemas das Fases")]
    private static void MontarHUD()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorSceneManager.OpenScene(Cenas[0]);
        GameObject[] prefabsDePowerUp = CriarPrefabsDePowerUp();
        CriarArremessoDoChefao();
        CriadorDeChefoes.GarantirChefoes();
        ConfiguradorDeAudio.ConfigurarMusicasDasFases();
        Sprite[] iconesDeMunicao = GerarIconesDeMunicao();

        foreach (string caminho in Cenas)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(caminho) == null) continue;

            Scene cena = EditorSceneManager.OpenScene(caminho);
            GarantirManagers(prefabsDePowerUp);
            MontarNaCenaAberta(ehPrimeiraCena: caminho == Cenas[0], iconesDeMunicao);
            EditorSceneManager.MarkSceneDirty(cena);
            EditorSceneManager.SaveScene(cena);
        }

        EditorSceneManager.OpenScene(Cenas[0]);
        Debug.Log("[CriadorDeHUD] HUD e sistemas montados nas cenas de jogo.");
    }

    private static GameObject[] CriarPrefabsDePowerUp()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/PowerUps")) AssetDatabase.CreateFolder("Assets/Prefabs", "PowerUps");

        return new[]
        {
            CriarPrefabDePowerUp<PowerUpSalada>("PowerUpSalada", "Bowl_001", new Color(0.4f, 0.9f, 0.45f)),
            CriarPrefabDePowerUp<PowerUpSuco>("PowerUpSuco", "Glass_005", new Color(1f, 0.62f, 0.2f)),
            CriarPrefabDePowerUp<PowerUpAbacate>("PowerUpAbacate", "Avocado_001", new Color(0.75f, 0.95f, 0.35f)),
        };
    }

    // Comida que o chefão arremessa no agente: modelo do pacote de comidas +
    // ProjetilDoChefao + colisor (para o tiro do jogador conseguir destruir).
    // Já liga o prefab no ChefaoDeCaloria.prefab.
    private static void CriarArremessoDoChefao()
    {
        GameObject raiz = new GameObject("ProjetilDoChefao");
        raiz.AddComponent<ProjetilDoChefao>();
        SphereCollider colisor = raiz.AddComponent<SphereCollider>();
        colisor.isTrigger = true;
        colisor.radius = 0.7f;

        GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>($"{PastaComidas}Cookie_006.prefab");
        if (modelo != null)
        {
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(modelo);
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            visual.transform.SetParent(raiz.transform, false);
            foreach (Collider c in visual.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            AjustarTamanhoECentralizar(visual, raiz.transform.position, 1.1f);
        }

        Light brilho = new GameObject("Brilho").AddComponent<Light>();
        brilho.transform.SetParent(raiz.transform, false);
        brilho.type = LightType.Point;
        brilho.color = new Color(1f, 0.55f, 0.2f);
        brilho.range = 4f;
        brilho.intensity = 3f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raiz, "Assets/Prefabs/Projeteis/ProjetilDoChefao.prefab");
        Object.DestroyImmediate(raiz);

        GameObject prefabDoChefao = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Inimigos/ChefaoDeCaloria.prefab");
        ChefaoDeCaloria chefao = prefabDoChefao != null ? prefabDoChefao.GetComponent<ChefaoDeCaloria>() : null;
        if (chefao == null)
        {
            Debug.LogWarning("[CriadorDeHUD] ChefaoDeCaloria.prefab não encontrado; arremesso não foi ligado.");
            return;
        }

        SerializedObject dados = new SerializedObject(chefao);
        dados.FindProperty("_prefabDoArremesso").objectReferenceValue = prefab.GetComponent<ProjetilDoChefao>();
        dados.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(prefabDoChefao);
        AssetDatabase.SaveAssets();
    }

    // Logo acima do painel de saúde dos cidadãos: barra de vida do agente.
    private static void CriarPainelDoAgente(Transform hud, out Image barra, out TextMeshProUGUI rotulo)
    {
        Image painel = CriarImagem(hud, "PainelAgente", CorPainel, SpriteArredondado);
        Ancorar(painel.rectTransform, Vector2.zero, Vector2.zero, new Vector2(32f, 166f), new Vector2(520f, 54f));

        rotulo = CriarTexto(painel.transform, "Rotulo", "AGENTE", 22f, CorTextoSecundario, Vector2.zero, Vector2.zero);
        Ancorar(rotulo.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(170f, 32f));
        rotulo.alignment = TextAlignmentOptions.MidlineLeft;
        rotulo.characterSpacing = 3f;

        Image trilho = CriarImagem(painel.transform, "Trilho", CorTrilho, SpriteArredondado);
        Ancorar(trilho.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(300f, 18f));
        barra = CriarBarraPreenchida(trilho.transform, "Vida", new Color(0.35f, 0.75f, 1f));
    }

    private static GameObject CriarPrefabDePowerUp<T>(string nome, string modelo, Color cor) where T : PowerUp
    {
        GameObject raiz = new GameObject(nome);
        raiz.AddComponent<T>();
        raiz.AddComponent<SphereCollider>().radius = 0.8f;

        GameObject prefabDoModelo = AssetDatabase.LoadAssetAtPath<GameObject>($"{PastaComidas}{modelo}.prefab");
        if (prefabDoModelo != null)
        {
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(prefabDoModelo);
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            visual.transform.SetParent(raiz.transform, false);
            foreach (Collider colisor in visual.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(colisor);
            AjustarTamanhoECentralizar(visual, raiz.transform.position, 1.1f);
        }
        else
        {
            Debug.LogWarning($"[CriadorDeHUD] Modelo não encontrado: {modelo}");
        }

        Light brilho = new GameObject("Brilho").AddComponent<Light>();
        brilho.transform.SetParent(raiz.transform, false);
        brilho.type = LightType.Point;
        brilho.color = cor;
        brilho.range = 3.5f;
        brilho.intensity = 2.5f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raiz, $"{PastaPowerUps}{nome}.prefab");
        Object.DestroyImmediate(raiz);
        return prefab;
    }

    // Os modelos do pacote têm tamanhos variados; normaliza para ~1 unidade.
    private static void AjustarTamanhoECentralizar(GameObject visual, Vector3 centroDesejado, float tamanhoAlvo)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        float maiorLado = Mathf.Max(CalcularLimites(renderers).size.x, CalcularLimites(renderers).size.y, CalcularLimites(renderers).size.z);
        if (maiorLado > 0.0001f) visual.transform.localScale *= tamanhoAlvo / maiorLado;

        visual.transform.position += centroDesejado - CalcularLimites(renderers).center;
    }

    private static Bounds CalcularLimites(Renderer[] renderers)
    {
        Bounds limites = renderers[0].bounds;
        foreach (Renderer r in renderers) limites.Encapsulate(r.bounds);
        return limites;
    }

    private static void GarantirManagers(GameObject[] prefabsDePowerUp)
    {
        // Vida do Agente fica no próprio objeto do jogador.
        MovimentoJogador jogador = Object.FindFirstObjectByType<MovimentoJogador>();
        if (jogador != null && jogador.GetComponent<VidaDoAgente>() == null) jogador.gameObject.AddComponent<VidaDoAgente>();

        GerenciadorDeJogo jogo = Object.FindFirstObjectByType<GerenciadorDeJogo>();
        Transform pastaDosManagers = jogo.transform.parent;

        GerenciadorDeEfeitos efeitos = Object.FindFirstObjectByType<GerenciadorDeEfeitos>();
        if (efeitos == null) efeitos = CriarManager<GerenciadorDeEfeitos>("GerenciadorDeEfeitos", pastaDosManagers);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialDeParticulas);
        if (material == null) Debug.LogWarning("[CriadorDeHUD] Material de partículas do URP não encontrado; usando Shader.Find.");
        SerializedObject dadosDosEfeitos = new SerializedObject(efeitos);
        dadosDosEfeitos.FindProperty("_materialDasParticulas").objectReferenceValue = material;
        dadosDosEfeitos.ApplyModifiedPropertiesWithoutUndo();

        ConfiguradorDeAudio.Configurar(Object.FindFirstObjectByType<GerenciadorDeAudio>());

        // O quiz entre fases foi removido: apaga o manager antigo das cenas
        // (busca por nome porque o script não existe mais).
        Transform quizAntigo = pastaDosManagers.Find("GerenciadorDeQuiz");
        if (quizAntigo != null) Object.DestroyImmediate(quizAntigo.gameObject);

        if (Object.FindFirstObjectByType<GerenciadorDeEspecial>() == null)
        {
            CriarManager<GerenciadorDeEspecial>("GerenciadorDeEspecial", pastaDosManagers);
        }

        if (Object.FindFirstObjectByType<GerenciadorDeMelhorias>() == null)
        {
            CriarManager<GerenciadorDeMelhorias>("GerenciadorDeMelhorias", pastaDosManagers);
        }

        if (Object.FindFirstObjectByType<GerenciadorDePontuacao>() == null)
        {
            CriarManager<GerenciadorDePontuacao>("GerenciadorDePontuacao", pastaDosManagers);
        }

        GerenciadorDePowerUps powerUps = Object.FindFirstObjectByType<GerenciadorDePowerUps>();
        if (powerUps == null) powerUps = CriarManager<GerenciadorDePowerUps>("GerenciadorDePowerUps", pastaDosManagers);

        SerializedObject dados = new SerializedObject(powerUps);
        // Valores de balanceamento ficam salvos na cena: atualiza aqui.
        dados.FindProperty("_chanceDeDrop").floatValue = 0.22f;
        dados.FindProperty("_intervaloMinimoEntreDrops").floatValue = 3f;
        SerializedProperty lista = dados.FindProperty("_prefabsDePowerUp");
        lista.arraySize = prefabsDePowerUp.Length;
        for (int i = 0; i < prefabsDePowerUp.Length; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = prefabsDePowerUp[i];
        dados.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T CriarManager<T>(string nome, Transform pai) where T : Component
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(pai, false);
        return objeto.AddComponent<T>();
    }

    // Barras de saúde, indicadores e textos flutuantes usam Camera.main,
    // que só encontra a câmera com a tag MainCamera.
    private static void MarcarCameraDoJogadorComoPrincipal()
    {
        MouseLook olhar = Object.FindFirstObjectByType<MouseLook>();
        Camera camera = olhar != null ? olhar.GetComponentInChildren<Camera>() : Object.FindFirstObjectByType<Camera>();
        if (camera != null) camera.gameObject.tag = "MainCamera";
    }

    private static void MontarNaCenaAberta(bool ehPrimeiraCena, Sprite[] iconesDeMunicao)
    {
        MarcarCameraDoJogadorComoPrincipal();

        UIManager ui = Object.FindFirstObjectByType<UIManager>();
        SerializedObject dados = new SerializedObject(ui);
        Sprite spriteDaMira = ObterSpriteDaMiraAtual(dados);

        foreach (Canvas antigo in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (antigo != null && antigo.transform.parent == null) Object.DestroyImmediate(antigo.gameObject);
        }

        Transform canvas = CriarCanvas();

        Image vinheta = CriarImagem(canvas, "VinhetaDeDano", new Color(0.9f, 0.1f, 0.1f, 0f));
        Esticar(vinheta.rectTransform);

        RectTransform hud = CriarUI("HUD", canvas);
        Esticar(hud);

        GameObject mira = CriarMira(hud, spriteDaMira, out TextMeshProUGUI dicaDaMira);
        TextMeshProUGUI textoOnda = CriarPainelDeProgresso(hud);
        CriarPainelDeEnergia(hud, out Image barra, out Image barraAtraso, out TextMeshProUGUI textoEnergia, out TextMeshProUGUI textoCidadaos);
        RectTransform[] slots = CriarSeletorDeMunicao(hud, iconesDeMunicao, out TextMeshProUGUI textoMunicao);
        GameObject painelChefao = CriarPainelDoChefao(canvas, out Image vidaChefao, out TextMeshProUGUI textoVidaChefao,
            out TextMeshProUGUI textoFraqueza, out Image indicador, out TextMeshProUGUI nomeChefao);
        GameObject painelTransicao = CriarPainelDeTransicao(canvas, out TextMeshProUGUI textoTransicao);
        CriarPainelDePontuacao(hud, out TextMeshProUGUI textoPontos, out TextMeshProUGUI textoCombo, out Image barraCombo);
        TextMeshProUGUI textoEfeitos = CriarTextoDeEfeitos(hud, out GameObject painelEfeitos);
        GameObject painelAviso = CriarPainelDeAviso(canvas, out TextMeshProUGUI textoAviso);

        GameObject telaGameOver = CriarTelaFinal(canvas, ui, "TelaGameOver", "GAME OVER",
            "Os maus hábitos venceram desta vez.", new Color(0.95f, 0.35f, 0.35f), new Color(0.15f, 0.02f, 0.03f, 0.88f),
            "TENTAR DE NOVO", ui.Reiniciar, out TextMeshProUGUI resultadoGameOver);
        GameObject telaVitoria = CriarTelaFinal(canvas, ui, "TelaVitoria", "VITÓRIA!",
            "A cidade está cheia de energia de novo!", CorDestaque, new Color(0.02f, 0.12f, 0.06f, 0.88f),
            "JOGAR DE NOVO", ui.ReiniciarCampanha, out TextMeshProUGUI resultadoVitoria);

        dados.FindProperty("_barraDeEnergia").objectReferenceValue = barra;
        dados.FindProperty("_barraDeEnergiaAtraso").objectReferenceValue = barraAtraso;
        dados.FindProperty("_textoEnergia").objectReferenceValue = textoEnergia;
        dados.FindProperty("_textoCidadaos").objectReferenceValue = textoCidadaos;
        CriarPainelDoAgente(hud, out Image barraAgente, out TextMeshProUGUI textoAgente);
        dados.FindProperty("_barraVidaAgente").objectReferenceValue = barraAgente;
        dados.FindProperty("_textoVidaAgente").objectReferenceValue = textoAgente;
        CriarBarraEspecial(hud, out Image barraEspecial, out TextMeshProUGUI textoEspecial);
        dados.FindProperty("_barraEspecial").objectReferenceValue = barraEspecial;
        dados.FindProperty("_textoEspecial").objectReferenceValue = textoEspecial;
        dados.FindProperty("_textoProximaMelhoria").objectReferenceValue = CriarTextoDaProximaMelhoria(hud);
        dados.FindProperty("_textoDicaDaMira").objectReferenceValue = dicaDaMira;
        dados.FindProperty("_painelEfeitos").objectReferenceValue = painelEfeitos;
        dados.FindProperty("_textoVidaChefao").objectReferenceValue = textoVidaChefao;
        dados.FindProperty("_textoNomeChefao").objectReferenceValue = nomeChefao;
        SerializedProperty listaIcones = dados.FindProperty("_iconesDeMunicao");
        listaIcones.arraySize = iconesDeMunicao.Length;
        for (int i = 0; i < iconesDeMunicao.Length; i++) listaIcones.GetArrayElementAtIndex(i).objectReferenceValue = iconesDeMunicao[i];
        dados.FindProperty("_vinhetaDeDano").objectReferenceValue = vinheta;
        dados.FindProperty("_textoMunicao").objectReferenceValue = textoMunicao;
        SerializedProperty listaSlots = dados.FindProperty("_slotsMunicao");
        listaSlots.arraySize = slots.Length;
        for (int i = 0; i < slots.Length; i++) listaSlots.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
        dados.FindProperty("_textoOnda").objectReferenceValue = textoOnda;
        dados.FindProperty("_hudDuranteJogo").objectReferenceValue = hud.gameObject;
        dados.FindProperty("_telaGameOver").objectReferenceValue = telaGameOver;
        dados.FindProperty("_telaVitoria").objectReferenceValue = telaVitoria;
        dados.FindProperty("_crosshair").objectReferenceValue = mira;
        dados.FindProperty("_painelChefao").objectReferenceValue = painelChefao;
        dados.FindProperty("_barraDeVidaChefao").objectReferenceValue = vidaChefao;
        dados.FindProperty("_textoFraquezaChefao").objectReferenceValue = textoFraqueza;
        dados.FindProperty("_indicadorFraquezaChefao").objectReferenceValue = indicador;
        dados.FindProperty("_painelTransicaoFase").objectReferenceValue = painelTransicao;
        dados.FindProperty("_textoTransicaoFase").objectReferenceValue = textoTransicao;
        dados.FindProperty("_textoPontos").objectReferenceValue = textoPontos;
        dados.FindProperty("_textoCombo").objectReferenceValue = textoCombo;
        dados.FindProperty("_barraDoCombo").objectReferenceValue = barraCombo;
        dados.FindProperty("_textoEfeitos").objectReferenceValue = textoEfeitos;
        dados.FindProperty("_jogador").objectReferenceValue = Object.FindFirstObjectByType<MovimentoJogador>();
        dados.FindProperty("_painelAviso").objectReferenceValue = painelAviso;
        dados.FindProperty("_textoAviso").objectReferenceValue = textoAviso;
        dados.FindProperty("_textoResultadoGameOver").objectReferenceValue = resultadoGameOver;
        dados.FindProperty("_textoResultadoVitoria").objectReferenceValue = resultadoVitoria;
        WeaponController arma = Object.FindFirstObjectByType<WeaponController>();
        dados.FindProperty("_arma").objectReferenceValue = arma;
        if (arma != null)
        {
            SerializedObject dadosDaArma = new SerializedObject(arma);
            dadosDaArma.FindProperty("_intervaloTiroRapido").floatValue = 0.09f;
            dadosDaArma.ApplyModifiedPropertiesWithoutUndo();
        }
        dados.ApplyModifiedPropertiesWithoutUndo();

        // Painéis que só aparecem durante o jogo começam escondidos, para a
        // Game View no modo de edição mostrar só o HUD normal.
        if (iconesDeMunicao.Length > 0 && iconesDeMunicao[0] != null) indicador.sprite = iconesDeMunicao[0];
        painelChefao.SetActive(false);
        painelAviso.SetActive(false);
        painelTransicao.SetActive(false);

        CriarIndicadoresDeCidadaos(hud);
        CriarMenuDePausa(canvas);
        LigarMelhorias(CriarPainelDeMelhorias(canvas));
        AplicarFonte(canvas);
        ConfigurarTutorial(ui, ehPrimeiraCena);
    }

    // Ícones "!" na borda da tela para cidadãos em perigo fora da visão.
    private static void CriarIndicadoresDeCidadaos(Transform hud)
    {
        const int quantidade = 6;
        RectTransform area = CriarUI("IndicadoresDeCidadaos", hud);
        Esticar(area);
        IndicadoresDeCidadaos indicadores = area.gameObject.AddComponent<IndicadoresDeCidadaos>();

        SerializedObject dados = new SerializedObject(indicadores);
        SerializedProperty listaIndicadores = dados.FindProperty("_indicadores");
        SerializedProperty listaImagens = dados.FindProperty("_imagens");
        SerializedProperty listaTextos = dados.FindProperty("_textos");
        listaIndicadores.arraySize = quantidade;
        listaImagens.arraySize = quantidade;
        listaTextos.arraySize = quantidade;

        for (int i = 0; i < quantidade; i++)
        {
            Image circulo = CriarImagem(area, $"Indicador{i}", Color.white, SpriteCircular, fatiada: false);
            Ancorar(circulo.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64f, 64f));

            TextMeshProUGUI texto = CriarTexto(circulo.transform, "Texto", "!", 44f, new Color(0.05f, 0.08f, 0.12f), Vector2.zero, Vector2.zero);
            Esticar(texto.rectTransform);
            texto.fontStyle = FontStyles.Bold;

            listaIndicadores.GetArrayElementAtIndex(i).objectReferenceValue = circulo.rectTransform;
            listaImagens.GetArrayElementAtIndex(i).objectReferenceValue = circulo;
            listaTextos.GetArrayElementAtIndex(i).objectReferenceValue = texto;
        }

        dados.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CriarMenuDePausa(Transform canvas)
    {
        RectTransform controle = CriarUI("MenuDePausa", canvas);
        Esticar(controle);
        MenuDePausa menu = controle.gameObject.AddComponent<MenuDePausa>();

        RectTransform janela = CriarJanela(canvas, "PainelPausa", "PAUSADO", new Vector2(620f, 640f), out RectTransform caixa);
        Vector2 tamanho = new Vector2(440f, 80f);
        CriarBotaoNaPosicao(caixa, "BotaoContinuar", "CONTINUAR", CorVerdePadrao, menu.Continuar, new Vector2(0f, 140f), tamanho);
        CriarBotaoNaPosicao(caixa, "BotaoReiniciar", "REINICIAR FASE", CorAzulPadrao, menu.ReiniciarFase, new Vector2(0f, 40f), tamanho);
        CriarBotaoNaPosicao(caixa, "BotaoOpcoes", "OPÇÕES", CorAzulPadrao, menu.AbrirOpcoes, new Vector2(0f, -60f), tamanho);
        CriarBotaoNaPosicao(caixa, "BotaoMenu", "MENU PRINCIPAL", CorVermelhaPadrao, menu.VoltarAoMenu, new Vector2(0f, -160f), tamanho);
        janela.gameObject.SetActive(false);

        GameObject opcoes = CriarPainelDeOpcoes(canvas, menu.FecharOpcoes);

        SerializedObject dados = new SerializedObject(menu);
        dados.FindProperty("_painelPausa").objectReferenceValue = janela.gameObject;
        dados.FindProperty("_painelOpcoes").objectReferenceValue = opcoes;
        dados.ApplyModifiedPropertiesWithoutUndo();
    }

    // Centro inferior, acima das munições: barra do especial com o texto por cima.
    private static void CriarBarraEspecial(Transform hud, out Image barra, out TextMeshProUGUI texto)
    {
        Image trilho = CriarImagem(hud, "BarraEspecial", CorPainel, SpriteArredondado);
        Ancorar(trilho.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 222f), new Vector2(440f, 34f));

        Image interior = CriarImagem(trilho.transform, "Trilho", CorTrilho, SpriteArredondado);
        Esticar(interior.rectTransform);
        interior.rectTransform.offsetMin = new Vector2(4f, 4f);
        interior.rectTransform.offsetMax = new Vector2(-4f, -4f);

        barra = CriarBarraPreenchida(interior.transform, "Carga", new Color(0.4f, 1f, 0.5f));
        barra.fillAmount = 0f;

        texto = CriarTexto(trilho.transform, "Texto", "ESPECIAL  0%", 20f, Color.white, Vector2.zero, Vector2.zero);
        Esticar(texto.rectTransform);
        texto.characterSpacing = 2f;
    }

    // Logo abaixo do painel de pontos: quanto falta para a próxima escolha.
    private static TextMeshProUGUI CriarTextoDaProximaMelhoria(Transform hud)
    {
        TextMeshProUGUI texto = CriarTexto(hud, "ProximaMelhoria", "", 20f, CorTextoSecundario, Vector2.zero, Vector2.zero);
        Ancorar(texto.rectTransform, Vector2.one, Vector2.one, new Vector2(-40f, -176f), new Vector2(360f, 28f));
        texto.alignment = TextAlignmentOptions.MidlineRight;
        texto.characterSpacing = 2f;
        return texto;
    }

    // Tela de escolha de melhoria: fundo escurecido, título e 3 cards
    // clicáveis (também dá para escolher com 1, 2 e 3).
    private static PainelDeMelhorias CriarPainelDeMelhorias(Transform canvas)
    {
        const int quantidade = 3;
        RectTransform controle = CriarUI("Melhorias", canvas);
        Esticar(controle);
        PainelDeMelhorias painel = controle.gameObject.AddComponent<PainelDeMelhorias>();

        Image fundo = CriarImagem(controle, "PainelMelhorias", new Color(0.01f, 0.03f, 0.06f, 0.82f));
        Esticar(fundo.rectTransform);
        fundo.raycastTarget = true;

        TextMeshProUGUI titulo = CriarTexto(fundo.transform, "Titulo", "ESCOLHA UMA MELHORIA", 64f, CorDestaque, new Vector2(0f, 330f), new Vector2(1600f, 90f));
        titulo.characterSpacing = 4f;
        CriarTexto(fundo.transform, "Subtitulo", "Selecione um card (clique ou 1, 2, 3) e confirme  ·  as melhorias se acumulam até o fim do jogo",
            26f, CorTextoSecundario, new Vector2(0f, 268f), new Vector2(1600f, 40f));

        Button[] cards = new Button[quantidade];
        Image[] faixas = new Image[quantidade];
        TextMeshProUGUI[] nomes = new TextMeshProUGUI[quantidade];
        TextMeshProUGUI[] niveis = new TextMeshProUGUI[quantidade];
        TextMeshProUGUI[] descricoes = new TextMeshProUGUI[quantidade];

        for (int i = 0; i < quantidade; i++)
        {
            Image fundoDoCard = CriarImagem(fundo.transform, $"Card{i}", new Color(0.06f, 0.12f, 0.19f, 0.98f), SpriteArredondado);
            Ancorar(fundoDoCard.rectTransform, new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 470f, -10f), new Vector2(430f, 440f));
            fundoDoCard.raycastTarget = true;

            Button card = fundoDoCard.gameObject.AddComponent<Button>();
            card.targetGraphic = fundoDoCard;
            ColorBlock cores = card.colors;
            cores.highlightedColor = new Color(1.35f, 1.35f, 1.35f, 1f);
            cores.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cores.fadeDuration = 0.08f;
            card.colors = cores;

            faixas[i] = CriarImagem(fundoDoCard.transform, "Faixa", Color.white, SpriteArredondado);
            Ancorar(faixas[i].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(380f, 10f));

            Image selo = CriarImagem(fundoDoCard.transform, "SeloTecla", CorDestaque, SpriteCircular, fatiada: false);
            Ancorar(selo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(56f, 56f));
            TextMeshProUGUI tecla = CriarTexto(selo.transform, "Tecla", (i + 1).ToString(), 32f, new Color(0.05f, 0.08f, 0.12f), Vector2.zero, Vector2.zero);
            Esticar(tecla.rectTransform);

            nomes[i] = CriarTexto(fundoDoCard.transform, "Nome", "", 36f, Color.white, new Vector2(0f, 80f), new Vector2(390f, 90f));
            niveis[i] = CriarTexto(fundoDoCard.transform, "Nivel", "", 22f, CorTextoSecundario, new Vector2(0f, 22f), new Vector2(390f, 32f));
            niveis[i].characterSpacing = 2f;
            descricoes[i] = CriarTexto(fundoDoCard.transform, "Descricao", "", 28f, Color.white, new Vector2(0f, -100f), new Vector2(370f, 180f));
            descricoes[i].lineSpacing = 6f;

            cards[i] = card;
        }

        Button confirmar = CriarBotaoNaPosicao(fundo.transform, "BotaoConfirmar", "SELECIONE UMA MELHORIA", CorVerdePadrao, null,
            new Vector2(0f, -330f), new Vector2(620f, 88f), 34f);
        ColorBlock coresDoBotao = confirmar.colors;
        coresDoBotao.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
        confirmar.colors = coresDoBotao;
        CriarTexto(fundo.transform, "DicaConfirmar", "ou aperte ENTER", 20f, CorTextoSecundario, new Vector2(0f, -394f), new Vector2(600f, 30f));

        SerializedObject dados = new SerializedObject(painel);
        dados.FindProperty("_raiz").objectReferenceValue = fundo.gameObject;
        dados.FindProperty("_botaoConfirmar").objectReferenceValue = confirmar;
        dados.FindProperty("_textoConfirmar").objectReferenceValue = confirmar.GetComponentInChildren<TextMeshProUGUI>();
        PreencherLista(dados.FindProperty("_cards"), cards);
        PreencherLista(dados.FindProperty("_faixas"), faixas);
        PreencherLista(dados.FindProperty("_nomes"), nomes);
        PreencherLista(dados.FindProperty("_niveis"), niveis);
        PreencherLista(dados.FindProperty("_descricoes"), descricoes);
        dados.ApplyModifiedPropertiesWithoutUndo();

        fundo.gameObject.SetActive(false);
        return painel;
    }

    private static void PreencherLista(SerializedProperty lista, Object[] itens)
    {
        lista.arraySize = itens.Length;
        for (int i = 0; i < itens.Length; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = itens[i];
    }

    private static void LigarMelhorias(PainelDeMelhorias painel)
    {
        GerenciadorDeMelhorias melhorias = Object.FindFirstObjectByType<GerenciadorDeMelhorias>();
        SerializedObject dados = new SerializedObject(melhorias);
        dados.FindProperty("_painel").objectReferenceValue = painel;
        dados.ApplyModifiedPropertiesWithoutUndo();
    }

    // O tutorial só existe na primeira cena (Fase 1).
    private static void ConfigurarTutorial(UIManager ui, bool ehPrimeiraCena)
    {
        TutorialInicial tutorial = Object.FindFirstObjectByType<TutorialInicial>();

        if (!ehPrimeiraCena)
        {
            if (tutorial != null) Object.DestroyImmediate(tutorial.gameObject);
            return;
        }

        if (tutorial == null) tutorial = CriarManager<TutorialInicial>("TutorialInicial", ui.transform.parent);
        SerializedObject dados = new SerializedObject(tutorial);
        dados.FindProperty("_ui").objectReferenceValue = ui;
        dados.ApplyModifiedPropertiesWithoutUndo();
    }

    // Canto superior direito: pontos e, abaixo, o combo com a barrinha de tempo.
    private static void CriarPainelDePontuacao(Transform hud, out TextMeshProUGUI pontos, out TextMeshProUGUI combo, out Image barraCombo)
    {
        Image painel = CriarImagem(hud, "PainelPontuacao", CorPainel, SpriteArredondado);
        Ancorar(painel.rectTransform, Vector2.one, Vector2.one, new Vector2(-32f, -32f), new Vector2(320f, 136f));

        TextMeshProUGUI rotulo = CriarTexto(painel.transform, "Rotulo", "PONTOS", 22f, CorTextoSecundario, Vector2.zero, Vector2.zero);
        Ancorar(rotulo.rectTransform, Vector2.one, Vector2.one, new Vector2(-24f, -14f), new Vector2(280f, 28f));
        rotulo.alignment = TextAlignmentOptions.MidlineRight;
        rotulo.characterSpacing = 4f;

        pontos = CriarTexto(painel.transform, "Pontos", "0", 56f, Color.white, Vector2.zero, Vector2.zero);
        Ancorar(pontos.rectTransform, Vector2.one, new Vector2(1f, 1f), new Vector2(-24f, -40f), new Vector2(280f, 58f));
        pontos.alignment = TextAlignmentOptions.MidlineRight;

        combo = CriarTexto(painel.transform, "Combo", "", 26f, Color.white, Vector2.zero, Vector2.zero);
        Ancorar(combo.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 16f), new Vector2(280f, 30f));
        combo.alignment = TextAlignmentOptions.MidlineRight;

        Image trilho = CriarImagem(painel.transform, "TrilhoCombo", CorTrilho, SpriteArredondado);
        Ancorar(trilho.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(272f, 5f));
        barraCombo = CriarBarraPreenchida(trilho.transform, "TempoCombo", CorDestaque);
        barraCombo.fillAmount = 0f;
    }

    // Acima do painel de saúde: efeitos ativos (tiro rápido, coringa, lento),
    // com fundo próprio que cresce para cima e some quando está vazio.
    private static TextMeshProUGUI CriarTextoDeEfeitos(Transform hud, out GameObject painelEfeitos)
    {
        Image painel = CriarImagem(hud, "PainelEfeitos", CorPainel, SpriteArredondado);
        Ancorar(painel.rectTransform, Vector2.zero, Vector2.zero, new Vector2(32f, 232f), new Vector2(0f, 0f));

        VerticalLayoutGroup layout = painel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 12, 12);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter ajuste = painel.gameObject.AddComponent<ContentSizeFitter>();
        ajuste.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI efeitos = CriarTexto(painel.transform, "EfeitosAtivos", "", 24f, Color.white, Vector2.zero, Vector2.zero);
        efeitos.alignment = TextAlignmentOptions.Left;
        efeitos.lineSpacing = 8f;

        painelEfeitos = painel.gameObject;
        painelEfeitos.SetActive(false);
        return efeitos;
    }

    // Faixa abaixo do painel do chefão para avisos rápidos (eventos e power-ups).
    private static GameObject CriarPainelDeAviso(Transform canvas, out TextMeshProUGUI texto)
    {
        Image painel = CriarImagem(canvas, "PainelAviso", new Color(0.02f, 0.05f, 0.09f, 0.78f), SpriteArredondado);
        Ancorar(painel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -196f), new Vector2(1000f, 112f));

        texto = CriarTexto(painel.transform, "Texto", "", 40f, Color.white, Vector2.zero, Vector2.zero);
        Esticar(texto.rectTransform);
        texto.margin = new Vector4(24f, 8f, 24f, 8f);
        return painel.gameObject;
    }

    private static Sprite ObterSpriteDaMiraAtual(SerializedObject dados)
    {
        GameObject miraAntiga = dados.FindProperty("_crosshair").objectReferenceValue as GameObject;
        Image imagem = miraAntiga != null ? miraAntiga.GetComponent<Image>() : null;
        if (imagem != null && imagem.sprite != null) return imagem.sprite;
        return CarregarComoSprite(MiraPadrao);
    }

    // Mira no centro e, logo abaixo, a dica "1  ÁGUA" quando há um alvo.
    private static GameObject CriarMira(Transform hud, Sprite sprite, out TextMeshProUGUI dica)
    {
        Image mira = CriarImagem(hud, "Mira", new Color(1f, 1f, 1f, 0.9f), sprite, fatiada: false);
        Ancorar(mira.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44f, 44f));
        mira.preserveAspect = true;

        dica = CriarTexto(hud, "DicaDaMira", "", 24f, Color.white, new Vector2(0f, -52f), new Vector2(400f, 34f));
        dica.characterSpacing = 2f;
        return mira.gameObject;
    }

    // Canto superior esquerdo: "ONDA 1 DE 3" e, menor, "FASE 2 DE 4 · HORA DO LANCHE".
    private static TextMeshProUGUI CriarPainelDeProgresso(Transform hud)
    {
        Image painel = CriarImagem(hud, "PainelProgresso", CorPainel, SpriteArredondado);
        Ancorar(painel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -32f), new Vector2(540f, 96f));

        Image destaque = CriarImagem(painel.transform, "Destaque", CorDestaque, SpriteArredondado);
        Ancorar(destaque.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(7f, 60f));

        TextMeshProUGUI texto = CriarTexto(painel.transform, "TextoOnda", "PREPARE-SE", 36f, Color.white, Vector2.zero, Vector2.zero);
        Esticar(texto.rectTransform);
        texto.margin = new Vector4(40f, 0f, 18f, 0f);
        texto.alignment = TextAlignmentOptions.MidlineLeft;
        texto.lineSpacing = -6f;
        texto.textWrappingMode = TextWrappingModes.NoWrap;
        texto.overflowMode = TextOverflowModes.Ellipsis;
        return texto;
    }

    // Canto inferior esquerdo: saúde somada dos cidadãos (em %), barra com
    // rastro de dano e quantos cidadãos estão bem.
    private static void CriarPainelDeEnergia(Transform hud, out Image barra, out Image barraAtraso,
        out TextMeshProUGUI valor, out TextMeshProUGUI cidadaos)
    {
        Image painel = CriarImagem(hud, "PainelEnergia", CorPainel, SpriteArredondado);
        Ancorar(painel.rectTransform, Vector2.zero, Vector2.zero, new Vector2(32f, 32f), new Vector2(520f, 124f));

        TextMeshProUGUI rotulo = CriarTexto(painel.transform, "Rotulo", "SAÚDE DOS CIDADÃOS", 22f, CorTextoSecundario, Vector2.zero, Vector2.zero);
        Ancorar(rotulo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -14f), new Vector2(360f, 32f));
        rotulo.alignment = TextAlignmentOptions.MidlineLeft;
        rotulo.characterSpacing = 3f;

        valor = CriarTexto(painel.transform, "Valor", "100%", 32f, Color.white, Vector2.zero, Vector2.zero);
        Ancorar(valor.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -10f), new Vector2(140f, 40f));
        valor.alignment = TextAlignmentOptions.MidlineRight;

        Image trilho = CriarImagem(painel.transform, "Trilho", CorTrilho, SpriteArredondado);
        Ancorar(trilho.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 44f), new Vector2(472f, 24f));
        barraAtraso = CriarBarraPreenchida(trilho.transform, "Rastro", new Color(1f, 1f, 1f, 0.55f));
        barra = CriarBarraPreenchida(trilho.transform, "Preenchimento", new Color(0.3f, 0.85f, 0.45f));

        cidadaos = CriarTexto(painel.transform, "Cidadaos", "", 20f, CorTextoSecundario, Vector2.zero, Vector2.zero);
        Ancorar(cidadaos.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 10f), new Vector2(472f, 28f));
        cidadaos.alignment = TextAlignmentOptions.MidlineLeft;
    }

    // Centro inferior: nome da munição atual e três slots com o ícone do
    // próprio projétil, a tecla (1, 2, 3) num selo colorido e uma faixa da cor.
    private static RectTransform[] CriarSeletorDeMunicao(Transform hud, Sprite[] icones, out TextMeshProUGUI nomeDaMunicao)
    {
        const float tamanhoSlot = 118f;
        const float espaco = 18f;
        float largura = tamanhoSlot * 3f + espaco * 2f;

        RectTransform area = CriarUI("SeletorDeMunicao", hud);
        Ancorar(area, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(largura + 40f, 180f));

        nomeDaMunicao = CriarTexto(area, "NomeDaMunicao", "ÁGUA", 30f, Color.white, Vector2.zero, Vector2.zero);
        Ancorar(nomeDaMunicao.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(largura + 40f, 40f));
        nomeDaMunicao.characterSpacing = 3f;

        TipoMunicao[] tipos = { TipoMunicao.Agua, TipoMunicao.Fruta, TipoMunicao.EnergiaPositiva };
        RectTransform[] slots = new RectTransform[tipos.Length];

        for (int i = 0; i < tipos.Length; i++)
        {
            Color cor = CoresDeMunicao.Obter(tipos[i]);
            float x = (i - 1) * (tamanhoSlot + espaco);

            Image fundo = CriarImagem(area, $"Slot_{tipos[i]}", CorPainel, SpriteArredondado);
            Ancorar(fundo.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(x, 0f), new Vector2(tamanhoSlot, tamanhoSlot));
            fundo.gameObject.AddComponent<CanvasGroup>();

            Image faixa = CriarImagem(fundo.transform, "Faixa", cor, SpriteArredondado);
            Ancorar(faixa.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(tamanhoSlot - 28f, 6f));

            Sprite sprite = icones != null && i < icones.Length ? icones[i] : null;
            if (sprite != null)
            {
                Image icone = CriarImagem(fundo.transform, "Icone", Color.white, sprite, fatiada: false);
                Ancorar(icone.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(80f, 80f));
                icone.preserveAspect = true;
            }

            Image selo = CriarImagem(fundo.transform, "SeloTecla", cor, SpriteCircular, fatiada: false);
            Ancorar(selo.rectTransform, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(10f, -10f), new Vector2(36f, 36f));
            TextMeshProUGUI tecla = CriarTexto(selo.transform, "Tecla", (i + 1).ToString(), 22f, new Color(0.05f, 0.08f, 0.12f), Vector2.zero, Vector2.zero);
            Esticar(tecla.rectTransform);

            slots[i] = fundo.rectTransform;
        }

        return slots;
    }

    // Topo central: nome, barra de vida com número e a fraqueza atual
    // (ícone da munição + tecla + nome), centralizada.
    private static GameObject CriarPainelDoChefao(Transform canvas, out Image vida, out TextMeshProUGUI textoVida,
        out TextMeshProUGUI fraqueza, out Image indicador, out TextMeshProUGUI nome)
    {
        Image painel = CriarImagem(canvas, "PainelChefao", CorPainel, SpriteArredondado);
        Ancorar(painel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(900f, 156f));

        nome = CriarTexto(painel.transform, "Nome", "CHEFÃO DE CALORIA", 28f, new Color(1f, 0.5f, 0.45f), new Vector2(0f, 52f), new Vector2(840f, 36f));
        nome.characterSpacing = 5f;

        Image trilho = CriarImagem(painel.transform, "Trilho", CorTrilho, SpriteArredondado);
        Ancorar(trilho.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(840f, 30f));
        vida = CriarBarraPreenchida(trilho.transform, "Vida", new Color(0.92f, 0.28f, 0.3f));

        textoVida = CriarTexto(trilho.transform, "TextoVida", "", 22f, Color.white, Vector2.zero, Vector2.zero);
        Esticar(textoVida.rectTransform);

        RectTransform linha = CriarUI("LinhaFraqueza", painel.transform);
        Ancorar(linha, new Vector2(0.5f, 0.5f), new Vector2(0f, -42f), new Vector2(0f, 48f));
        HorizontalLayoutGroup layout = linha.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        linha.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        indicador = CriarImagem(linha, "IndicadorFraqueza", Color.white, SpriteCircular, fatiada: false);
        indicador.preserveAspect = true;
        LayoutElement tamanhoIcone = indicador.gameObject.AddComponent<LayoutElement>();
        tamanhoIcone.preferredWidth = 44f;
        tamanhoIcone.preferredHeight = 44f;

        fraqueza = CriarTexto(linha, "Fraqueza", "FRACO CONTRA ÁGUA", 28f, Color.white, Vector2.zero, Vector2.zero);
        fraqueza.textWrappingMode = TextWrappingModes.NoWrap;

        return painel.gameObject;
    }

    // ----- Fonte do HUD -----

    // Cria (uma vez) o font asset dinâmico da Fredoka e o aplica a todos os
    // textos do Canvas. Negrito "falso" é removido: a fonte já é SemiBold.
    private static void AplicarFonte(Transform canvas)
    {
        TMP_FontAsset fonte = ObterFonte();
        if (fonte == null) return;

        foreach (TextMeshProUGUI texto in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            texto.font = fonte;
            texto.fontStyle &= ~FontStyles.Bold;
        }
    }

    private static TMP_FontAsset ObterFonte()
    {
        TMP_FontAsset existente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CaminhoDoAssetDaFonte);
        if (existente != null) return existente;

        Font fonte = AssetDatabase.LoadAssetAtPath<Font>(CaminhoDaFonte);
        if (fonte == null)
        {
            Debug.LogWarning($"[CriadorDeHUD] Fonte não encontrada em {CaminhoDaFonte}; mantendo a fonte padrão.");
            return null;
        }

        TMP_FontAsset ativo = TMP_FontAsset.CreateFontAsset(fonte, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
        AssetDatabase.CreateAsset(ativo, CaminhoDoAssetDaFonte);
        ativo.atlasTextures[0].name = "Fredoka Atlas";
        AssetDatabase.AddObjectToAsset(ativo.atlasTextures[0], ativo);
        ativo.material.name = "Fredoka Material";
        AssetDatabase.AddObjectToAsset(ativo.material, ativo);

        // Pré-carrega os caracteres do jogo (acentos do português incluídos).
        ativo.TryAddCharacters(CaracteresDoJogo);
        EditorUtility.SetDirty(ativo);
        AssetDatabase.SaveAssets();
        return ativo;
    }

    // ----- Ícones das munições -----

    // Fotografa os modelos 3D reais do Projetil.prefab (água, fruta,
    // energia) numa cena de preview e salva como sprites PNG — assim o HUD
    // mostra exatamente o que o jogador atira. Se algo falhar, usa os
    // ícones antigos do pacote de comidas.
    private static Sprite[] GerarIconesDeMunicao()
    {
        string[] campos = { "_modeloAgua", "_modeloFruta", "_modeloEnergia" };
        string[] nomes = { "Agua", "Fruta", "EnergiaPositiva" };
        string[] iconesReserva = { "glass_001", "watermelon_001", "bar_001" };
        Sprite[] sprites = new Sprite[campos.Length];

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoDoProjetil);
        Projetil projetil = prefab != null ? prefab.GetComponent<Projetil>() : null;
        SerializedObject dados = projetil != null ? new SerializedObject(projetil) : null;

        if (!AssetDatabase.IsValidFolder("Assets/Sprites")) AssetDatabase.CreateFolder("Assets", "Sprites");
        if (!AssetDatabase.IsValidFolder(PastaIconesDeMunicao)) AssetDatabase.CreateFolder("Assets/Sprites", "UI");

        for (int i = 0; i < campos.Length; i++)
        {
            GameObject modelo = dados != null ? dados.FindProperty(campos[i]).objectReferenceValue as GameObject : null;
            string caminho = $"{PastaIconesDeMunicao}/IconeMunicao_{nomes[i]}.png";

            if (modelo != null && FotografarModelo(modelo, caminho)) sprites[i] = ImportarComoSprite(caminho);
            if (sprites[i] != null) continue;

            Debug.LogWarning($"[CriadorDeHUD] Não foi possível gerar o ícone de {nomes[i]}; usando ícone reserva.");
            sprites[i] = CarregarComoSprite($"{PastaIcones}{iconesReserva[i]}.png");
        }

        return sprites;
    }

    private static bool FotografarModelo(GameObject modelo, string caminhoPng)
    {
        const int resolucao = 256;
        Scene cena = EditorSceneManager.NewPreviewScene();
        RenderTexture alvo = RenderTexture.GetTemporary(resolucao, resolucao, 24, RenderTextureFormat.ARGB32);

        try
        {
            GameObject copia = Object.Instantiate(modelo);
            copia.SetActive(true);
            SceneManager.MoveGameObjectToScene(copia, cena);
            copia.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(-12f, 35f, 8f) * copia.transform.rotation);

            Renderer[] renderers = copia.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return false;
            Bounds limites = CalcularLimites(renderers);
            float maiorLado = Mathf.Max(limites.size.x, limites.size.y, limites.size.z);
            if (maiorLado < 0.0001f) return false;
            copia.transform.localScale *= 1f / maiorLado;
            copia.transform.position -= CalcularLimites(renderers).center;

            CriarLuz(cena, Quaternion.Euler(40f, -30f, 0f), 1.3f);
            CriarLuz(cena, Quaternion.Euler(-20f, 150f, 0f), 0.5f);

            Camera camera = new GameObject("CameraDoIcone").AddComponent<Camera>();
            SceneManager.MoveGameObjectToScene(camera.gameObject, cena);
            camera.scene = cena;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.fieldOfView = 25f;
            camera.nearClipPlane = 0.01f;
            camera.transform.position = new Vector3(0f, 0f, -2.6f);
            camera.transform.LookAt(Vector3.zero);
            camera.targetTexture = alvo;
            camera.Render();

            RenderTexture anterior = RenderTexture.active;
            RenderTexture.active = alvo;
            Texture2D imagem = new Texture2D(resolucao, resolucao, TextureFormat.RGBA32, false);
            imagem.ReadPixels(new Rect(0, 0, resolucao, resolucao), 0, 0);
            imagem.Apply();
            RenderTexture.active = anterior;
            camera.targetTexture = null;

            if (!TemConteudo(imagem)) return false;

            System.IO.File.WriteAllBytes(caminhoPng, imagem.EncodeToPNG());
            Object.DestroyImmediate(imagem);
            return true;
        }
        catch (System.Exception erro)
        {
            Debug.LogWarning($"[CriadorDeHUD] Erro ao fotografar {modelo.name}: {erro.Message}");
            return false;
        }
        finally
        {
            RenderTexture.ReleaseTemporary(alvo);
            EditorSceneManager.ClosePreviewScene(cena);
        }
    }

    private static void CriarLuz(Scene cena, Quaternion rotacao, float intensidade)
    {
        Light luz = new GameObject("LuzDoIcone").AddComponent<Light>();
        SceneManager.MoveGameObjectToScene(luz.gameObject, cena);
        luz.type = LightType.Directional;
        luz.intensity = intensidade;
        luz.transform.rotation = rotacao;
    }

    // Imagem toda transparente = a câmera não renderizou nada (ex: pipeline
    // não suportou a cena de preview).
    private static bool TemConteudo(Texture2D imagem)
    {
        foreach (Color32 pixel in imagem.GetPixels32())
        {
            if (pixel.a > 10) return true;
        }
        return false;
    }

    private static Sprite ImportarComoSprite(string caminho)
    {
        AssetDatabase.ImportAsset(caminho, ImportAssetOptions.ForceUpdate);
        TextureImporter importador = AssetImporter.GetAtPath(caminho) as TextureImporter;
        if (importador == null) return null;

        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.alphaIsTransparency = true;
        importador.mipmapEnabled = false;
        importador.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
    }

    private static GameObject CriarPainelDeTransicao(Transform canvas, out TextMeshProUGUI texto)
    {
        Image faixa = CriarImagem(canvas, "PainelTransicaoFase", new Color(0.02f, 0.05f, 0.09f, 0.78f));
        faixa.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        faixa.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        faixa.rectTransform.sizeDelta = new Vector2(0f, 300f);
        faixa.rectTransform.anchoredPosition = new Vector2(0f, 120f);

        texto = CriarTexto(faixa.transform, "Texto", "FASE 1", 84f, Color.white, Vector2.zero, Vector2.zero);
        Esticar(texto.rectTransform);
        texto.fontStyle = FontStyles.Bold;
        texto.lineSpacing = -10f;
        return faixa.gameObject;
    }

    private static GameObject CriarTelaFinal(Transform canvas, UIManager ui, string nome, string titulo, string subtitulo,
        Color corTitulo, Color corFundo, string rotuloPrincipal, UnityEngine.Events.UnityAction acaoPrincipal,
        out TextMeshProUGUI resultado)
    {
        RectTransform tela = CriarUI(nome, canvas);
        Esticar(tela);
        Image fundo = tela.gameObject.AddComponent<Image>();
        fundo.color = corFundo;

        TextMeshProUGUI textoTitulo = CriarTexto(tela, "Titulo", titulo, 130f, corTitulo, new Vector2(0f, 220f), new Vector2(1600f, 180f));
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.characterSpacing = 6f;

        CriarTexto(tela, "Subtitulo", subtitulo, 40f, new Color(1f, 1f, 1f, 0.85f), new Vector2(0f, 110f), new Vector2(1400f, 60f));

        resultado = CriarTexto(tela, "Resultado", "PONTOS 0", 56f, Color.white, new Vector2(0f, -10f), new Vector2(1200f, 140f));
        resultado.fontStyle = FontStyles.Bold;

        RectTransform botoes = CriarUI("Botoes", tela);
        Ancorar(botoes, new Vector2(0.5f, 0.5f), new Vector2(0f, -170f), new Vector2(760f, 96f));
        HorizontalLayoutGroup layout = botoes.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 30f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;

        CriarBotao(botoes, "BotaoPrincipal", rotuloPrincipal, CorVerde, acaoPrincipal, 36f);
        CriarBotao(botoes, "BotaoMenu", "MENU", CorAzul, ui.VoltarAoMenu, 36f);

        tela.gameObject.SetActive(false);
        return tela.gameObject;
    }
}
