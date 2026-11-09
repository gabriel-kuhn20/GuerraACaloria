using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Ferramenta de editor: gera o executável do jogo para Windows na pasta
// Build/ (na raiz do projeto, ao lado de Assets/), usando as cenas
// marcadas no Build Settings. Mostra no Console se deu certo ou não.
// Menu: GuerraACaloria > Gerar Build Windows.
public static class GeradorDeBuild
{
    private const string CaminhoDoExecutavel = "Build/GuerraACaloria.exe";

    [MenuItem("GuerraACaloria/Gerar Build Windows")]
    private static void Gerar()
    {
        string[] cenas = EditorBuildSettings.scenes.Where(c => c.enabled).Select(c => c.path).ToArray();

        BuildPlayerOptions opcoes = new BuildPlayerOptions
        {
            scenes = cenas,
            locationPathName = CaminhoDoExecutavel,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };

        BuildReport relatorio = BuildPipeline.BuildPlayer(opcoes);
        BuildSummary resumo = relatorio.summary;

        if (resumo.result == BuildResult.Succeeded)
        {
            Debug.Log($"[GeradorDeBuild] Build gerado com sucesso em {CaminhoDoExecutavel} " +
                      $"({resumo.totalSize / (1024 * 1024)} MB, {resumo.totalErrors} erros, {resumo.totalWarnings} avisos).");
        }
        else
        {
            Debug.LogError($"[GeradorDeBuild] Build falhou: {resumo.result} ({resumo.totalErrors} erros). Veja o Console.");
        }
    }
}
