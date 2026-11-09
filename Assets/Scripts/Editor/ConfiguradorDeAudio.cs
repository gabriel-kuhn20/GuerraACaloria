using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Ferramenta de editor (não vai para o build): liga os efeitos sonoros
// (Assets/Audio/Efeitos) e as músicas (Assets/Audio/Musicas) gerados para
// o jogo no GerenciadorDeAudio de todas as cenas e nos assets das fases
// (música de cada fase, do chefão e do menu). Também é chamada pelo
// "Montar HUD e Sistemas das Fases". Menu: GuerraACaloria > Configurar Áudio.
public static class ConfiguradorDeAudio
{
    private const string PastaEfeitos = "Assets/Audio/Efeitos/";
    private const string PastaMusicas = "Assets/Audio/Musicas/";
    private const string PastaFases = "Assets/ScriptableObjects/Fases/";

    // Qual arquivo de Assets/Audio/Musicas cada momento usa (troque aqui para mudar a trilha).
    private const string MusicaDoChefao = "MusicaChefaoRock";
    private static readonly string[] MusicasDasFases = { "MusicaFase1Rock", "MusicaFase2Rock", "MusicaFase3Rock", "MusicaFase4Rock" };
    private const string MusicaDoMenu = "MusicaMenuRock";

    private static readonly string[] Cenas =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/SampleScene.unity",
        "Assets/Scenes/Fase2.unity",
        "Assets/Scenes/Fase3.unity",
        "Assets/Scenes/Fase4.unity",
    };

    // campo do GerenciadorDeAudio -> arquivo em Assets/Audio/Efeitos
    private static readonly string[,] Ligacoes =
    {
        { "_somPowerUp", "PowerUp" },
        { "_somComboSubiu", "ComboSubiu" },
        { "_somComboQuebrou", "ComboQuebrou" },
        { "_somEvento", "Alerta" },
        { "_somChefao", "ChefaoChegada" },
        { "_somClique", "Clique" },
        { "_somTiroAgua", "TiroAgua" },
        { "_somTiroFruta", "TiroFruta" },
        { "_somTiroEnergia", "TiroEnergia" },
        { "_somTrocaMunicao", "TrocaMunicao" },
        { "_somEspecialPronto", "EspecialPronto" },
        { "_somEspecialSoltar", "EspecialSoltar" },
        { "_somInimigoDerrotado", "InimigoDerrotado" },
        { "_somCura", "CuraCidadao" },
        { "_somDesmaio", "CidadaoDesmaio" },
        { "_somChefaoFuria", "ChefaoFuria" },
        { "_somChefaoInvestida", "ChefaoInvestida" },
        { "_somChefaoEscudo", "ChefaoEscudo" },
        { "_somLacaio", "LacaioInvocado" },
        { "_somChefaoDerrotado", "ChefaoDerrotado" },
        { "_somOndaInicio", "OndaInicio" },
        { "_somOndaCompleta", "OndaCompleta" },
        { "_somMelhoriaAbrir", "MelhoriaAbrir" },
        { "_somMelhoriaEscolher", "MelhoriaEscolher" },
        { "_somVitoria", "Vitoria" },
        { "_somDerrota", "Derrota" },
        { "_somAgenteAtingido", "AgenteAtingido" },
        { "_somChefaoArremesso", "ChefaoArremesso" },
    };

    [MenuItem("GuerraACaloria/Configurar Áudio")]
    private static void ConfigurarTodasAsCenas()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        ConfigurarMusicasDasFases();

        foreach (string caminho in Cenas)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(caminho) == null) continue;

            Scene cena = EditorSceneManager.OpenScene(caminho);
            GerenciadorDeAudio audio = Object.FindFirstObjectByType<GerenciadorDeAudio>();
            if (audio == null) continue;

            Configurar(audio);
            EditorSceneManager.MarkSceneDirty(cena);
            EditorSceneManager.SaveScene(cena);
        }

        EditorSceneManager.OpenScene(Cenas[1]);
        Debug.Log("[ConfiguradorDeAudio] Efeitos sonoros ligados em todas as cenas.");
    }

    public static void Configurar(GerenciadorDeAudio audio)
    {
        if (audio == null) return;

        SerializedObject dados = new SerializedObject(audio);
        for (int i = 0; i < Ligacoes.GetLength(0); i++)
        {
            AudioClip clipe = AssetDatabase.LoadAssetAtPath<AudioClip>($"{PastaEfeitos}{Ligacoes[i, 1]}.wav");
            SerializedProperty campo = dados.FindProperty(Ligacoes[i, 0]);
            if (clipe == null || campo == null)
            {
                Debug.LogWarning($"[ConfiguradorDeAudio] Não encontrado: {Ligacoes[i, 0]} / {Ligacoes[i, 1]}.wav");
                continue;
            }
            campo.objectReferenceValue = clipe;
        }

        // Música inicial da cena: a da própria fase (ou do menu); a do chefão vale para todas.
        dados.FindProperty("_musicaDoChefao").objectReferenceValue = CarregarMusica(MusicaDoChefao);
        AudioClip musicaDaCena = CarregarMusica(MusicaDaCena(audio.gameObject.scene.path));
        if (musicaDaCena != null) dados.FindProperty("_musicaDeFundo").objectReferenceValue = musicaDaCena;

        dados.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void ConfigurarMusicasDasFases()
    {
        for (int numero = 1; numero <= 4; numero++)
        {
            ConfiguracaoDeFase fase = AssetDatabase.LoadAssetAtPath<ConfiguracaoDeFase>($"{PastaFases}Fase{numero}.asset");
            AudioClip musica = CarregarMusica(MusicasDasFases[numero - 1]);
            if (fase == null || musica == null) continue;

            SerializedObject dados = new SerializedObject(fase);
            dados.FindProperty("_musica").objectReferenceValue = musica;
            dados.ApplyModifiedPropertiesWithoutUndo();
        }
        AssetDatabase.SaveAssets();
    }

    private static string MusicaDaCena(string caminhoDaCena)
    {
        if (caminhoDaCena.EndsWith("MainMenu.unity")) return MusicaDoMenu;
        if (caminhoDaCena.EndsWith("SampleScene.unity")) return MusicasDasFases[0];
        for (int numero = 2; numero <= 4; numero++)
        {
            if (caminhoDaCena.EndsWith($"Fase{numero}.unity")) return MusicasDasFases[numero - 1];
        }
        return null;
    }

    private static AudioClip CarregarMusica(string nome)
    {
        return string.IsNullOrEmpty(nome) ? null : AssetDatabase.LoadAssetAtPath<AudioClip>($"{PastaMusicas}{nome}.ogg");
    }
}
