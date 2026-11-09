using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Ferramenta de editor (não vai para o build): prepara os cidadãos.
// 1) Liga o loop dos clipes Idle/Walk/Run nos FBX; 2) remonta o
// ControllerCidadao com um Blend Tree (parado -> andando -> correndo pelo
// parâmetro "Velocidade"), comemoração (trigger "Comemorar") e desmaio
// sentado (bool "Desmaiado"); 3) nas cenas de jogo aplica o controller,
// ajusta as velocidades e coloca a zona dos cidadãos à frente do jogador.
// Menu: GuerraACaloria > Configurar Cidadãos.
public static class ConfiguradorDeAnimacaoDosCidadaos
{
    private const string PastaDosModelos = "Assets/AssetsExternos/Cidadaos";
    private const string CaminhoDoController = "Assets/AssetsExternos/Cidadaos/ControllerCidadao.controller";
    private const string ModeloDeReferencia = "Assets/AssetsExternos/Cidadaos/Man/Male_Casual.fbx";

    private static readonly string[] ClipesEmLoop = { "Man_Idle", "Man_Walk", "Man_Run" };
    private static readonly string[] Cenas =
    {
        "Assets/Scenes/SampleScene.unity",
        "Assets/Scenes/Fase2.unity",
        "Assets/Scenes/Fase3.unity",
        "Assets/Scenes/Fase4.unity",
    };

    // Velocidades (m/s) em que cada animação toca em velocidade normal;
    // casam com _velocidade (1.5) e _velocidadeDeFuga (4) do Cidadao.
    private const float VelocidadeAndando = 1.5f;
    private const float VelocidadeCorrendo = 4f;
    private const float DistanciaDaZonaNaFrenteDoJogador = 10f;

    [MenuItem("GuerraACaloria/Configurar Cidadãos")]
    private static void Configurar()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        LigarLoopDosClipes();
        AnimatorController controller = MontarController();
        AplicarNasCenas(controller);

        Debug.Log("[ConfiguradorDeAnimacaoDosCidadaos] Animações dos cidadãos configuradas.");
    }

    private static void LigarLoopDosClipes()
    {
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { PastaDosModelos });

        foreach (string guid in guids)
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importador = AssetImporter.GetAtPath(caminho) as ModelImporter;
            if (importador == null) continue;

            ModelImporterClipAnimation[] clipes = importador.clipAnimations.Length > 0
                ? importador.clipAnimations
                : importador.defaultClipAnimations;

            foreach (ModelImporterClipAnimation clipe in clipes)
            {
                if (ClipesEmLoop.Any(nome => clipe.name.EndsWith(nome))) clipe.loopTime = true;
            }

            importador.clipAnimations = clipes;
            importador.SaveAndReimport();
        }
    }

    private static AnimationClip BuscarClipe(string sufixo)
    {
        AnimationClip clipe = AssetDatabase.LoadAllAssetsAtPath(ModeloDeReferencia)
            .OfType<AnimationClip>()
            .FirstOrDefault(c => c.name.EndsWith(sufixo) && !c.name.StartsWith("__preview__"));

        if (clipe == null) Debug.LogWarning($"[ConfiguradorDeAnimacaoDosCidadaos] Clipe não encontrado: {sufixo}");
        return clipe;
    }

    private static AnimatorController MontarController()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoDoController);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(CaminhoDoController);

        foreach (AnimatorControllerParameter parametro in controller.parameters) controller.RemoveParameter(parametro);
        controller.AddParameter("Velocidade", AnimatorControllerParameterType.Float);
        controller.AddParameter("Comemorar", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Desmaiado", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine maquina = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState estado in maquina.states) maquina.RemoveState(estado.state);
        foreach (AnimatorStateTransition transicao in maquina.anyStateTransitions) maquina.RemoveAnyStateTransition(transicao);

        AnimatorState locomocao = controller.CreateBlendTreeInController("Locomocao", out BlendTree arvore, 0);
        arvore.blendType = BlendTreeType.Simple1D;
        arvore.blendParameter = "Velocidade";
        arvore.useAutomaticThresholds = false;
        arvore.AddChild(BuscarClipe("Man_Idle"), 0f);
        arvore.AddChild(BuscarClipe("Man_Walk"), VelocidadeAndando);
        arvore.AddChild(BuscarClipe("Man_Run"), VelocidadeCorrendo);
        maquina.defaultState = locomocao;

        AnimatorState comemorando = maquina.AddState("Comemorando");
        comemorando.motion = BuscarClipe("Man_Clapping");

        AnimatorStateTransition entrada = maquina.AddAnyStateTransition(comemorando);
        entrada.AddCondition(AnimatorConditionMode.If, 0f, "Comemorar");
        entrada.duration = 0.15f;
        entrada.canTransitionToSelf = false;

        AnimatorStateTransition saida = comemorando.AddTransition(locomocao);
        saida.hasExitTime = true;
        saida.exitTime = 0.9f;
        saida.duration = 0.25f;

        AnimatorState sentado = maquina.AddState("Desmaiado");
        sentado.motion = BuscarClipe("Man_Sitting");

        AnimatorStateTransition desmaiar = maquina.AddAnyStateTransition(sentado);
        desmaiar.AddCondition(AnimatorConditionMode.If, 0f, "Desmaiado");
        desmaiar.duration = 0.2f;
        desmaiar.canTransitionToSelf = false;

        AnimatorStateTransition levantar = sentado.AddTransition(locomocao);
        levantar.AddCondition(AnimatorConditionMode.IfNot, 0f, "Desmaiado");
        levantar.hasExitTime = false;
        levantar.duration = 0.3f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void AplicarNasCenas(AnimatorController controller)
    {
        foreach (string caminho in Cenas)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(caminho) == null) continue;

            Scene cena = EditorSceneManager.OpenScene(caminho);
            foreach (Cidadao cidadao in Object.FindObjectsByType<Cidadao>(FindObjectsSortMode.None))
            {
                Animator animator = cidadao.GetComponentInChildren<Animator>();
                if (animator == null) animator = cidadao.gameObject.AddComponent<Animator>();

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                EditorUtility.SetDirty(animator);

                SerializedObject dados = new SerializedObject(cidadao);
                dados.FindProperty("_velocidade").floatValue = VelocidadeAndando;
                dados.FindProperty("_velocidadeDeFuga").floatValue = VelocidadeCorrendo;
                dados.ApplyModifiedPropertiesWithoutUndo();
            }
            ColocarZonaNaFrenteDoJogador();
            EditorSceneManager.MarkSceneDirty(cena);
            EditorSceneManager.SaveScene(cena);
        }

        EditorSceneManager.OpenScene(Cenas[0]);
    }

    // Move o centro de patrulha e os cidadãos para a frente do jogador, onde a
    // ação acontece. Calculado a partir do jogador: rodar de novo não acumula.
    private static void ColocarZonaNaFrenteDoJogador()
    {
        MovimentoJogador jogador = Object.FindFirstObjectByType<MovimentoJogador>();
        Cidadao[] cidadaos = Object.FindObjectsByType<Cidadao>(FindObjectsSortMode.None);
        if (jogador == null || cidadaos.Length == 0) return;

        Transform centro = new SerializedObject(cidadaos[0]).FindProperty("_centroDaZonaDePatrulha").objectReferenceValue as Transform;
        if (centro == null) return;

        Vector3 frente = jogador.transform.forward;
        frente.y = 0f;
        frente.Normalize();

        Vector3 novoCentro = jogador.transform.position + frente * DistanciaDaZonaNaFrenteDoJogador;
        Vector3 deslocamento = new Vector3(novoCentro.x - centro.position.x, 0f, novoCentro.z - centro.position.z);

        centro.position += deslocamento;
        foreach (Cidadao cidadao in cidadaos) cidadao.transform.position += deslocamento;
    }
}
