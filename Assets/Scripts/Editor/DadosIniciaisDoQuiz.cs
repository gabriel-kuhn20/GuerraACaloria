using UnityEditor;
using UnityEngine;

// Ferramenta de editor: cria o asset BancoDePerguntas com as perguntas
// iniciais do quiz de nutrição (só se ele ainda não existir, para não
// apagar perguntas que você tenha editado no Inspector).
public static class DadosIniciaisDoQuiz
{
    private const string Pasta = "Assets/ScriptableObjects/Quiz";
    private const string Caminho = Pasta + "/BancoDePerguntas.asset";

    // { enunciado, alternativa A, B, C, índice correto, explicação }
    private static readonly string[][] Perguntas =
    {
        new[] { "Qual é a melhor bebida para matar a sede no dia a dia?", "Refrigerante", "Água", "Suco de caixinha", "1",
            "Água hidrata sem açúcar nem calorias. É a melhor escolha para o dia todo." },
        new[] { "Qual destes alimentos é ultraprocessado?", "Maçã", "Arroz com feijão", "Salgadinho de pacote", "2",
            "Ultraprocessados são feitos na indústria com muito sal, açúcar, gordura e aditivos." },
        new[] { "O Guia Alimentar brasileiro recomenda que a base da alimentação seja de:", "Alimentos in natura ou minimamente processados", "Alimentos ultraprocessados", "Suplementos alimentares", "0",
            "Frutas, verduras, legumes, grãos, ovos e carnes frescas devem ser a base do prato." },
        new[] { "Uma lata de refrigerante comum (350 ml) tem açúcar equivalente a cerca de:", "1 colher de chá", "Nenhuma colher", "7 colheres de chá", "2",
            "São cerca de 35 g de açúcar, mais ou menos 7 colheres de chá em uma única lata!" },
        new[] { "Quanto tempo de atividade física por dia é recomendado para crianças e adolescentes?", "60 minutos", "10 minutos", "Nenhum", "0",
            "A OMS recomenda, em média, 60 minutos por dia de atividade física moderada a intensa." },
        new[] { "Frutas, verduras e grãos integrais são ricos em qual nutriente que ajuda o intestino?", "Gordura trans", "Fibras", "Sódio", "1",
            "As fibras ajudam o intestino a funcionar e dão mais saciedade." },
        new[] { "O excesso de sal (sódio) na alimentação está ligado a qual problema?", "Pressão alta", "Miopia", "Unhas fracas", "0",
            "Comer muito sal ao longo do tempo aumenta o risco de pressão alta." },
        new[] { "Qual é uma troca mais saudável para o lanche da tarde?", "Biscoito recheado", "Bolinho industrializado", "Fruta com iogurte natural", "2",
            "Fruta com iogurte natural tem menos açúcar adicionado e mais nutrientes." },
        new[] { "Passar muitas horas parado em frente a telas, sem se mexer, é chamado de:", "Hidratação", "Sedentarismo", "Metabolismo", "1",
            "Sedentarismo é a falta de movimento. Levantar e se mexer faz bem ao corpo e à mente." },
        new[] { "Por que é bom ler o rótulo dos alimentos?", "Para ver quanto açúcar, sal e gordura ele tem", "Para conhecer a cor da embalagem", "Não serve para nada", "0",
            "O rótulo mostra ingredientes e nutrientes, ajudando a fazer escolhas melhores." },
        new[] { "Qual destas bebidas costuma ter MAIS açúcar?", "Água com gás", "Chá sem açúcar", "Refrigerante", "2",
            "Refrigerantes têm muito açúcar adicionado e quase nenhum nutriente." },
        new[] { "Dormir bem ajuda o corpo a:", "Ficar mais cansado", "Recuperar a energia e se desenvolver", "Precisar de menos água", "1",
            "Durante o sono o corpo descansa, se recupera e a memória se organiza." },
        new[] { "Comer devagar e com atenção na refeição ajuda a:", "Perceber quando já está satisfeito", "Sentir mais fome", "Comer mais rápido", "0",
            "Comer com calma dá tempo para o corpo sinalizar a saciedade." },
        new[] { "Arroz com feijão é uma combinação que oferece:", "Só gordura", "Apenas açúcar", "Boas fontes de proteína e fibras", "2",
            "Juntos, arroz e feijão formam uma combinação nutritiva e tradicional do prato brasileiro." },
        new[] { "Qual opção tem mais fibras?", "Pão integral", "Pão branco", "Bolo de chocolate", "0",
            "Alimentos integrais mantêm a casca do grão, onde ficam mais fibras." },
    };

    public static BancoDePerguntas CriarSeNaoExistir()
    {
        BancoDePerguntas existente = AssetDatabase.LoadAssetAtPath<BancoDePerguntas>(Caminho);
        if (existente != null) return existente;

        if (!AssetDatabase.IsValidFolder(Pasta)) AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Quiz");

        BancoDePerguntas banco = ScriptableObject.CreateInstance<BancoDePerguntas>();
        AssetDatabase.CreateAsset(banco, Caminho);

        SerializedObject dados = new SerializedObject(banco);
        SerializedProperty lista = dados.FindProperty("_perguntas");
        lista.arraySize = Perguntas.Length;

        for (int i = 0; i < Perguntas.Length; i++)
        {
            string[] linha = Perguntas[i];
            SerializedProperty item = lista.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("_enunciado").stringValue = linha[0];

            SerializedProperty alternativas = item.FindPropertyRelative("_alternativas");
            alternativas.arraySize = 3;
            for (int a = 0; a < 3; a++) alternativas.GetArrayElementAtIndex(a).stringValue = linha[1 + a];

            item.FindPropertyRelative("_indiceCorreto").intValue = int.Parse(linha[4]);
            item.FindPropertyRelative("_explicacao").stringValue = linha[5];
        }

        dados.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        return banco;
    }
}
