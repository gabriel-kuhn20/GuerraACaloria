using UnityEditor;
using UnityEngine;

// Ferramenta de editor (não vai para o build): cria um ConfiguracaoDeChefao
// para cada fase (modelo 3D do pacote de comidas + atributos próprios) e
// liga cada um no asset da sua fase. Assets que já existem NÃO são
// sobrescritos — ajustes feitos no Inspector ficam. Também roda junto com
// "Montar HUD e Sistemas das Fases". Menu: GuerraACaloria > Criar Chefões das Fases.
public static class CriadorDeChefoes
{
    private const string PastaChefoes = "Assets/ScriptableObjects/Chefoes";
    private const string PastaFases = "Assets/ScriptableObjects/Fases/";
    private const string PastaComidas = "Assets/ithappy/Food_Free/Prefabs/";
    private const string PastaInimigos = "Assets/Prefabs/Inimigos/";

    private struct Definicao
    {
        public int Fase;
        public string Nome;
        public string Descricao;
        public Color Cor;
        public string Modelo;
        public float Altura;
        public float Giro;
        public int Vida;
        public float Velocidade;
        public int Dano;
        public float Troca;
        public TipoMunicao[] Fraquezas;
        public HabilidadesDoChefao Habilidades;
        public float IntervaloDeLacaios;
        public string[] Lacaios;
    }

    [MenuItem("GuerraACaloria/Criar Chefões das Fases")]
    public static void GarantirChefoes()
    {
        if (!AssetDatabase.IsValidFolder(PastaChefoes)) AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Chefoes");

        Definicao[] chefoes =
        {
            // Fase 1: chefão de apresentação — só 2 fraquezas, lento, investidas.
            new Definicao
            {
                Fase = 1, Nome = "Rei Hambúrguer", Cor = new Color(1f, 0.62f, 0.3f),
                Descricao = "Fraco contra ÁGUA e FRUTA. Cuidado com as investidas!",
                Modelo = "Burger_001", Altura = 3f, Giro = 25f,
                Vida = 30, Velocidade = 4.5f, Dano = 25, Troca = 4.5f,
                Fraquezas = new[] { TipoMunicao.Agua, TipoMunicao.Fruta },
                Habilidades = HabilidadesDoChefao.Investida,
                IntervaloDeLacaios = 9f, Lacaios = new string[0],
            },
            // Fase 2: alto e rápido, invoca Refrigerantes Zumbis.
            new Definicao
            {
                Fase = 2, Nome = "Refri Gigante", Cor = new Color(1f, 0.4f, 0.4f),
                Descricao = "Rápido e cheio de açúcar: chama Refrigerantes Zumbis o tempo todo.",
                Modelo = "Soda_002", Altura = 4f, Giro = 45f,
                Vida = 40, Velocidade = 6f, Dano = 25, Troca = 3.5f,
                Fraquezas = new[] { TipoMunicao.Agua, TipoMunicao.Fruta, TipoMunicao.EnergiaPositiva },
                Habilidades = HabilidadesDoChefao.Lacaios,
                IntervaloDeLacaios = 6f, Lacaios = new[] { "RefrigeranteZumbi" },
            },
            // Fase 3: lento e resistente, escudo + Nuvens de Preguiça.
            new Definicao
            {
                Fase = 3, Nome = "Barão do Salgadinho", Cor = new Color(0.75f, 0.65f, 1f),
                Descricao = "Preguiçoso, mas resistente: levanta escudo e chama Nuvens de Preguiça.",
                Modelo = "Chips_008", Altura = 3.4f, Giro = 15f,
                Vida = 50, Velocidade = 3.5f, Dano = 35, Troca = 3f,
                Fraquezas = new[] { TipoMunicao.Agua, TipoMunicao.Fruta, TipoMunicao.EnergiaPositiva },
                Habilidades = HabilidadesDoChefao.Escudo | HabilidadesDoChefao.Lacaios,
                IntervaloDeLacaios = 8f, Lacaios = new[] { "NuvemDePreguica" },
            },
            // Fase 4: chefão final, enorme, troca rápido e usa tudo.
            new Definicao
            {
                Fase = 4, Nome = "Sorvetão Supremo", Cor = new Color(1f, 0.55f, 0.85f),
                Descricao = "O chefão final: investe, chama todos os inimigos e levanta escudo.",
                Modelo = "Ice_cream_001", Altura = 4.8f, Giro = 35f,
                Vida = 65, Velocidade = 5f, Dano = 40, Troca = 2.6f,
                Fraquezas = new[] { TipoMunicao.Agua, TipoMunicao.Fruta, TipoMunicao.EnergiaPositiva },
                Habilidades = HabilidadesDoChefao.Investida | HabilidadesDoChefao.Lacaios | HabilidadesDoChefao.Escudo,
                IntervaloDeLacaios = 7f, Lacaios = new string[0],
            },
        };

        foreach (Definicao definicao in chefoes) CriarELigar(definicao);
        AssetDatabase.SaveAssets();
    }

    private static void CriarELigar(Definicao definicao)
    {
        string caminho = $"{PastaChefoes}/Chefao_Fase{definicao.Fase}.asset";
        ConfiguracaoDeChefao chefao = AssetDatabase.LoadAssetAtPath<ConfiguracaoDeChefao>(caminho);

        if (chefao == null)
        {
            chefao = ScriptableObject.CreateInstance<ConfiguracaoDeChefao>();
            AssetDatabase.CreateAsset(chefao, caminho);
            Preencher(chefao, definicao);
        }

        ConfiguracaoDeFase fase = AssetDatabase.LoadAssetAtPath<ConfiguracaoDeFase>($"{PastaFases}Fase{definicao.Fase}.asset");
        if (fase == null) return;

        SerializedObject dadosDaFase = new SerializedObject(fase);
        dadosDaFase.FindProperty("_chefao").objectReferenceValue = chefao;
        dadosDaFase.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Preencher(ConfiguracaoDeChefao chefao, Definicao definicao)
    {
        SerializedObject dados = new SerializedObject(chefao);
        dados.FindProperty("_nome").stringValue = definicao.Nome;
        dados.FindProperty("_descricao").stringValue = definicao.Descricao;
        dados.FindProperty("_cor").colorValue = definicao.Cor;
        dados.FindProperty("_modelo").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PastaComidas}{definicao.Modelo}.prefab");
        dados.FindProperty("_altura").floatValue = definicao.Altura;
        dados.FindProperty("_giroDoModelo").floatValue = definicao.Giro;
        dados.FindProperty("_vida").intValue = definicao.Vida;
        dados.FindProperty("_velocidade").floatValue = definicao.Velocidade;
        dados.FindProperty("_danoAoAlcancar").intValue = definicao.Dano;
        dados.FindProperty("_intervaloDeTrocaDeFraqueza").floatValue = definicao.Troca;
        dados.FindProperty("_habilidades").intValue = (int)definicao.Habilidades;
        dados.FindProperty("_intervaloDeLacaios").floatValue = definicao.IntervaloDeLacaios;

        SerializedProperty fraquezas = dados.FindProperty("_fraquezas");
        fraquezas.arraySize = definicao.Fraquezas.Length;
        for (int i = 0; i < definicao.Fraquezas.Length; i++) fraquezas.GetArrayElementAtIndex(i).enumValueIndex = (int)definicao.Fraquezas[i];

        SerializedProperty lacaios = dados.FindProperty("_lacaios");
        lacaios.arraySize = definicao.Lacaios.Length;
        for (int i = 0; i < definicao.Lacaios.Length; i++)
        {
            lacaios.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PastaInimigos}{definicao.Lacaios[i]}.prefab");
        }

        dados.ApplyModifiedPropertiesWithoutUndo();
    }
}
