using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Ferramenta de editor (não vai para o build): define o level design das
// 4 fases sobre a avenida principal da cidade (X ≈ -0.4) e gera as cenas
// Fase2, Fase3 e Fase4 a partir da SampleScene (Fase 1). Cada fase usa um
// trecho diferente da avenida, com entradas de inimigos, barricadas,
// obstáculos e iluminação próprios. Toda ponta/abertura da área jogável
// recebe uma barreira visível (nada de parede invisível).
// Pode ser rodada de novo sempre que a SampleScene mudar.
// Menu: GuerraACaloria > Gerar Cenas das Fases.
public static class CriadorDeCenasDeFase
{
    private const string CenaBase = "Assets/Scenes/SampleScene.unity";
    private const string PastaCenas = "Assets/Scenes/";
    private const string PastaFases = "Assets/ScriptableObjects/Fases/";
    private const string PastaCarros = "Assets/ithappy/Cartoon_City_Free/Prefabs/Cars/";
    private const string PastaProps = "Assets/ithappy/Cartoon_City_Free/Prefabs/Props/";
    private const string PastaComidas = "Assets/ithappy/Food_Free/Prefabs/";
    private const string NomeDecoracao = "[ CENARIO DA FASE ]";
    private const string CaminhoDaCidade = "[ CENARIO ]/Cidade";

    // Geometria da avenida principal (medida na SampleScene).
    private const float XAvenida = -0.4f;
    private const float LarguraAvenida = 12f;
    // Rua transversal que sai à esquerda da avenida (cruzamento de Z 45 a 60).
    private const float ZTransversal = 52.5f;
    private const float MeiaLarguraTransversal = 6f;
    // Até onde o braço da transversal entra na área jogável (fase do cruzamento).
    private const float XFimDoBraco = -22.4f;

    private const float RecuoDoSpawn = 4.5f;
    private const float DistanciaDaBarreira = 1.3f;
    private const float ComprimentoDoCarro = 4.2f;

    private static readonly string[] CarrosDaBarricada = { "Van", "Car_06", "Car_13", "Car_19" };
    private static readonly string[] CarrosDoFechamento = { "Car_16", "Futuristic_Car_1", "Car_06" };

    private enum Origem { PontaFinal, PontaInicial, AmbasAsPontas }

    private class Obstaculo
    {
        public string Prefab;
        public float X;
        public float Z;
        public float Giro;
        public float AlturaAlvo; // 0 = mantém escala original
        public bool NoAsfalto;   // true = carro na pista; false = objeto de calçada
        public float Raio = 1.8f; // espaço livre necessário em volta (evita sobrepor a cidade)
    }

    private class Fase
    {
        public int Numero;
        public string ProximaCena;
        public float ZInicio;
        public float ZFim;
        public Origem Origem;
        public bool EntradaLateralAtiva;
        // true = a transversal vira parte da arena (cruzamento em T) e os
        // inimigos laterais vêm do fim do braço, não da boca.
        public bool BracoNaTransversal;
        // Se definido, os cidadãos são levados para cá (centro da zona de patrulha).
        public Vector3? CentroDosCidadaos;
        public float ChanceDeEntradaLateral;
        public Vector3 PosicaoJogador;
        public float GiroJogador;
        public bool AlterarIluminacao;
        public Color CorLuz;
        public float IntensidadeLuz;
        public Vector3 RotacaoLuz;
        public bool UsarAmbienteFixo;
        public Color CorAmbiente;
        public bool Neblina;
        public Color CorNeblina;
        public float DensidadeNeblina;
        public bool Noturna;
        public List<Obstaculo> Obstaculos = new List<Obstaculo>();

        public float Comprimento => ZFim - ZInicio;
        public float ZCentro => (ZInicio + ZFim) / 2f;
        public bool InimigosNoInicio => Origem != Origem.PontaFinal;
        public bool InimigosNoFim => Origem != Origem.PontaInicial;
        public bool TemTransversal => ZTransversal - MeiaLarguraTransversal > ZInicio && ZTransversal + MeiaLarguraTransversal < ZFim;
        public float ZDoChefao => InimigosNoFim ? ZFim - 1f : ZInicio + 1f;
    }

    private static List<Fase> DefinirFases()
    {
        float bordaEsquerda = XAvenida - LarguraAvenida / 2f;
        float bordaDireita = XAvenida + LarguraAvenida / 2f;
        // Calçada: a faixa de grama com arbustos fica colada no meio-fio;
        // lixeiras, pontos de ônibus e estátuas vão no passeio, depois dela.
        // Postes de luz ficam no meio-fio, como os da própria cidade.
        float calcadaEsquerda = bordaEsquerda - 5.1f;
        float calcadaDireita = bordaDireita + 4.9f;
        float posteEsquerdo = bordaEsquerda - 1f;
        float posteDireito = bordaDireita + 0.9f;

        // Fase 1 — A Invasão Começa (tutorial): trecho sul, reto e limpo,
        // uma única entrada à frente. Iluminação original da SampleScene.
        Fase fase1 = new Fase
        {
            Numero = 1, ProximaCena = "Fase2",
            ZInicio = -29f, ZFim = 44f, Origem = Origem.PontaFinal,
            PosicaoJogador = new Vector3(0f, 0f, -21.9f), GiroJogador = 0f,
            AlterarIluminacao = false,
        };
        fase1.Obstaculos.Add(new Obstaculo { Prefab = PastaProps + "Bus_Stop_02", X = calcadaDireita, Z = 4f, Giro = -90f, Raio = 3f });
        fase1.Obstaculos.Add(new Obstaculo { Prefab = PastaProps + "Trash_Can_05", X = calcadaEsquerda, Z = -10f });
        fase1.Obstaculos.Add(new Obstaculo { Prefab = PastaProps + "Trash_Can_07", X = calcadaDireita, Z = 18f });

        // Fase 2 — Hora do Lanche: avenida inteira, inimigos vêm da ponta
        // norte E da rua transversal à esquerda (duas frentes).
        Fase fase2 = new Fase
        {
            Numero = 2, ProximaCena = "Fase3",
            ZInicio = -29f, ZFim = 89f, Origem = Origem.PontaFinal,
            EntradaLateralAtiva = true, ChanceDeEntradaLateral = 0.35f,
            PosicaoJogador = new Vector3(XAvenida, 0f, -15f), GiroJogador = 0f,
            AlterarIluminacao = true,
            CorLuz = new Color(1f, 0.96f, 0.86f), IntensidadeLuz = 1.4f, RotacaoLuz = new Vector3(75f, 20f, 0f),
        };
        fase2.Obstaculos.Add(new Obstaculo { Prefab = PastaProps + "Bus_Stop_02", X = calcadaDireita, Z = 52f, Giro = -90f, Raio = 3f });
        fase2.Obstaculos.Add(new Obstaculo { Prefab = PastaProps + "Trash_Can_06", X = calcadaEsquerda, Z = 64f });
        fase2.Obstaculos.Add(new Obstaculo { Prefab = PastaProps + "Trash_Can_08", X = calcadaDireita, Z = 8f });

        // Fase 3 — Tarde no Sofá: avenida invertida (jogador no norte),
        // carros parados em zigue-zague no meio da pista quebram a linha
        // de tiro e obrigam o jogador a se mover. Pôr do sol com neblina.
        Fase fase3 = new Fase
        {
            Numero = 3, ProximaCena = "Fase4",
            ZInicio = 5f, ZFim = 89f, Origem = Origem.PontaInicial,
            PosicaoJogador = new Vector3(XAvenida, 0f, 84f), GiroJogador = 180f,
            AlterarIluminacao = true,
            CorLuz = new Color(1f, 0.58f, 0.32f), IntensidadeLuz = 1.1f, RotacaoLuz = new Vector3(12f, -60f, 0f),
            UsarAmbienteFixo = true, CorAmbiente = new Color(0.55f, 0.4f, 0.35f),
            Neblina = true, CorNeblina = new Color(0.95f, 0.6f, 0.4f), DensidadeNeblina = 0.012f,
        };
        fase3.Obstaculos.Add(new Obstaculo { Prefab = PastaCarros + "Car_19", X = bordaDireita - 2.2f, Z = 18f, Giro = 4f, NoAsfalto = true, Raio = 3f });
        fase3.Obstaculos.Add(new Obstaculo { Prefab = PastaCarros + "Van", X = bordaEsquerda + 2.2f, Z = 28f, Giro = -6f, NoAsfalto = true, Raio = 3f });
        fase3.Obstaculos.Add(new Obstaculo { Prefab = PastaCarros + "Car_13", X = bordaDireita - 2.2f, Z = 38f, Giro = 8f, NoAsfalto = true, Raio = 3f });
        fase3.Obstaculos.Add(new Obstaculo { Prefab = PastaCarros + "Car_16", X = bordaDireita - 2.2f, Z = 62f, Giro = -3f, NoAsfalto = true, Raio = 3f });
        fase3.Obstaculos.Add(new Obstaculo { Prefab = PastaProps + "Trash_Can_05", X = calcadaDireita, Z = 70f });

        // Fase 4 — Festival do Fast Food: arena em T no cruzamento da
        // transversal, à noite. Cidadãos no meio do cruzamento; inimigos
        // pelas DUAS pontas da avenida e pelo fim do braço da transversal.
        Fase fase4 = new Fase
        {
            Numero = 4, ProximaCena = "",
            ZInicio = 22f, ZFim = 80f, Origem = Origem.AmbasAsPontas,
            EntradaLateralAtiva = true, ChanceDeEntradaLateral = 0.3f, BracoNaTransversal = true,
            PosicaoJogador = new Vector3(XAvenida, 0f, 38f), GiroJogador = 0f,
            CentroDosCidadaos = new Vector3(XAvenida, 0f, ZTransversal),
            AlterarIluminacao = true,
            CorLuz = new Color(0.5f, 0.6f, 1f), IntensidadeLuz = 0.35f, RotacaoLuz = new Vector3(40f, 30f, 0f),
            UsarAmbienteFixo = true, CorAmbiente = new Color(0.12f, 0.14f, 0.25f),
            Neblina = true, CorNeblina = new Color(0.08f, 0.1f, 0.2f), DensidadeNeblina = 0.02f,
            Noturna = true,
        };
        // "Estátuas" gigantes de fast food enfeitando as calçadas do festival.
        fase4.Obstaculos.Add(new Obstaculo { Prefab = PastaComidas + "Burger_001", X = calcadaDireita, Z = 55f, AlturaAlvo = 2.2f, Raio = 2.6f });
        fase4.Obstaculos.Add(new Obstaculo { Prefab = PastaComidas + "Soda_002", X = calcadaDireita, Z = 70f, AlturaAlvo = 2.6f, Raio = 2.6f });
        fase4.Obstaculos.Add(new Obstaculo { Prefab = PastaComidas + "Donut_002", X = calcadaEsquerda, Z = 28f, Giro = 90f, AlturaAlvo = 1.8f, Raio = 2.6f });
        fase4.Obstaculos.Add(new Obstaculo { Prefab = PastaComidas + "Ice_cream_001", X = calcadaEsquerda, Z = 66f, AlturaAlvo = 2.4f, Raio = 2.6f });
        // Postes extras (a boca da transversal, Z 45–60 à esquerda, fica livre).
        foreach (float z in new[] { 25f, 38f, 70f })
            fase4.Obstaculos.Add(new Obstaculo { Prefab = PastaProps + "Spotlight_01", X = posteEsquerdo, Z = z, Giro = 90f, Raio = 0.8f });
        foreach (float z in new[] { 30f, 47f, 76f })
            fase4.Obstaculos.Add(new Obstaculo { Prefab = PastaProps + "Spotlight_02", X = posteDireito, Z = z, Giro = -90f, Raio = 0.8f });

        return new List<Fase> { fase1, fase2, fase3, fase4 };
    }

    [MenuItem("GuerraACaloria/Gerar Cenas das Fases")]
    private static void GerarCenas()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        List<Fase> fases = DefinirFases();

        // A Fase 1 é a própria cena base: aplica o layout nela antes de copiar.
        Scene cenaBase = EditorSceneManager.OpenScene(CenaBase);
        AplicarLayout(fases[0], reposicionarJogador: false);
        EditorSceneManager.MarkSceneDirty(cenaBase);
        EditorSceneManager.SaveScene(cenaBase);

        for (int i = 1; i < fases.Count; i++) GerarCena(fases[i]);

        AtualizarBuildSettings();
        EditorSceneManager.OpenScene(CenaBase);
        Debug.Log("[CriadorDeCenasDeFase] Fase 1 atualizada e cenas Fase2, Fase3 e Fase4 geradas com sucesso.");
    }

    private static void GerarCena(Fase fase)
    {
        string caminho = $"{PastaCenas}Fase{fase.Numero}.unity";
        AssetDatabase.DeleteAsset(caminho);
        AssetDatabase.CopyAsset(CenaBase, caminho);

        Scene cena = EditorSceneManager.OpenScene(caminho);
        AplicarLayout(fase, reposicionarJogador: true);
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
    }

    private static void AplicarLayout(Fase fase, bool reposicionarJogador)
    {
        // Seed fixa: gerar de novo produz exatamente o mesmo cenário.
        Random.InitState(fase.Numero * 1000);

        ConfigurarGerador(fase.Numero, fase.ProximaCena);
        ConfigurarRua(fase);
        if (reposicionarJogador) ReposicionarJogadorECidadaos(fase.PosicaoJogador, fase.GiroJogador);
        if (fase.CentroDosCidadaos.HasValue) MoverCidadaosPara(fase.CentroDosCidadaos.Value);
        MoverPontoDoChefao(new Vector3(XAvenida, 0.1f, fase.ZDoChefao));
        if (fase.AlterarIluminacao) ConfigurarIluminacao(fase);
        LiberarAreaJogavel(fase);
        CriarDecoracao(fase);
    }

    private static void ConfigurarGerador(int numeroDaFase, string proximaCena)
    {
        GeradorDeInimigos gerador = Object.FindFirstObjectByType<GeradorDeInimigos>();
        ConfiguracaoDeFase fase = AssetDatabase.LoadAssetAtPath<ConfiguracaoDeFase>($"{PastaFases}Fase{numeroDaFase}.asset");

        SerializedObject dados = new SerializedObject(gerador);
        SerializedProperty fases = dados.FindProperty("_fases");
        fases.arraySize = 1;
        fases.GetArrayElementAtIndex(0).objectReferenceValue = fase;
        dados.FindProperty("_proximaCena").stringValue = proximaCena;
        dados.FindProperty("_numeroDaPrimeiraFase").intValue = numeroDaFase;
        dados.FindProperty("_totalDeFasesDoJogo").intValue = 4;
        dados.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigurarRua(Fase fase)
    {
        GerenciadorDeRua rua = Object.FindFirstObjectByType<GerenciadorDeRua>();

        SerializedObject dados = new SerializedObject(rua);
        dados.FindProperty("_largura").floatValue = LarguraAvenida;
        dados.FindProperty("_centro").vector3Value = new Vector3(XAvenida, 0f, fase.ZCentro);
        dados.FindProperty("_comprimento").floatValue = fase.Comprimento;
        dados.FindProperty("_origemDosInimigos").enumValueIndex = (int)fase.Origem;
        dados.FindProperty("_recuoDoSpawn").floatValue = RecuoDoSpawn;
        dados.FindProperty("_chanceDeEntradaLateral").floatValue = fase.ChanceDeEntradaLateral;

        float bordaEsquerda = XAvenida - LarguraAvenida / 2f;
        bool temBraco = fase.BracoNaTransversal && fase.TemTransversal;

        SerializedProperty entradas = dados.FindProperty("_entradasLaterais");
        bool usarLateral = fase.EntradaLateralAtiva && fase.TemTransversal;
        entradas.arraySize = usarLateral ? 1 : 0;
        if (usarLateral)
        {
            float xDaEntrada = temBraco ? XFimDoBraco : bordaEsquerda;
            entradas.GetArrayElementAtIndex(0).vector3Value = new Vector3(xDaEntrada, 0f, ZTransversal);
        }

        SerializedProperty areas = dados.FindProperty("_areasExtras");
        areas.arraySize = temBraco ? 1 : 0;
        if (temBraco)
        {
            SerializedProperty braco = areas.GetArrayElementAtIndex(0);
            braco.FindPropertyRelative("Centro").vector3Value = new Vector3((bordaEsquerda + XFimDoBraco) / 2f, 0f, ZTransversal);
            braco.FindPropertyRelative("Tamanho").vector2Value = new Vector2(bordaEsquerda - XFimDoBraco, MeiaLarguraTransversal * 2f);
        }

        dados.ApplyModifiedPropertiesWithoutUndo();
    }

    // Move o jogador e leva junto os cidadãos (e seus centros de patrulha),
    // mantendo a mesma posição relativa — se o jogador girar 180°, os
    // cidadãos continuam "à frente" dele.
    private static void ReposicionarJogadorECidadaos(Vector3 novaPosicao, float giro)
    {
        Transform jogador = Object.FindFirstObjectByType<MovimentoJogador>().transform;
        Vector3 posicaoAntiga = jogador.position;
        Quaternion rotacao = Quaternion.Euler(0f, giro, 0f);
        Vector3 destino = new Vector3(novaPosicao.x, posicaoAntiga.y, novaPosicao.z);

        HashSet<Transform> raizesDosCidadaos = new HashSet<Transform>();
        foreach (Cidadao cidadao in Object.FindObjectsByType<Cidadao>(FindObjectsSortMode.None))
        {
            raizesDosCidadaos.Add(cidadao.transform.root);

            Transform centroPatrulha = new SerializedObject(cidadao).FindProperty("_centroDaZonaDePatrulha").objectReferenceValue as Transform;
            if (centroPatrulha != null) raizesDosCidadaos.Add(centroPatrulha.root);
        }
        raizesDosCidadaos.Remove(jogador.root);

        foreach (Transform raiz in raizesDosCidadaos)
        {
            Vector3 relativo = raiz.position - posicaoAntiga;
            relativo.y = 0f;
            raiz.position = new Vector3(destino.x, raiz.position.y, destino.z) + rotacao * relativo;
            raiz.rotation = rotacao * raiz.rotation;
        }

        jogador.position = destino;
        jogador.rotation = rotacao;
    }

    // Leva o grupo de cidadãos (e o centro da patrulha) para que o centro
    // da patrulha fique exatamente no ponto pedido.
    private static void MoverCidadaosPara(Vector3 centroDesejado)
    {
        Cidadao primeiro = Object.FindFirstObjectByType<Cidadao>();
        if (primeiro == null) return;

        Transform centroPatrulha = new SerializedObject(primeiro).FindProperty("_centroDaZonaDePatrulha").objectReferenceValue as Transform;
        Vector3 referencia = centroPatrulha != null ? centroPatrulha.position : primeiro.transform.position;
        Vector3 deslocamento = new Vector3(centroDesejado.x - referencia.x, 0f, centroDesejado.z - referencia.z);

        Transform jogador = Object.FindFirstObjectByType<MovimentoJogador>().transform.root;
        HashSet<Transform> raizes = new HashSet<Transform>();
        foreach (Cidadao cidadao in Object.FindObjectsByType<Cidadao>(FindObjectsSortMode.None)) raizes.Add(cidadao.transform.root);
        if (centroPatrulha != null) raizes.Add(centroPatrulha.root);
        raizes.Remove(jogador);

        foreach (Transform raiz in raizes) raiz.position += deslocamento;
    }

    private static void MoverPontoDoChefao(Vector3 posicao)
    {
        GeradorDeInimigos gerador = Object.FindFirstObjectByType<GeradorDeInimigos>();
        Transform ponto = new SerializedObject(gerador).FindProperty("_pontoDeSpawnChefao").objectReferenceValue as Transform;
        if (ponto != null) ponto.position = posicao;
    }

    private static void ConfigurarIluminacao(Fase fase)
    {
        foreach (Light luz in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (luz.type != LightType.Directional) continue;

            luz.color = fase.CorLuz;
            luz.intensity = fase.IntensidadeLuz;
            luz.transform.rotation = Quaternion.Euler(fase.RotacaoLuz);
        }

        if (fase.UsarAmbienteFixo)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = fase.CorAmbiente;
        }

        RenderSettings.fog = fase.Neblina;
        if (fase.Neblina)
        {
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = fase.CorNeblina;
            RenderSettings.fogDensity = fase.DensidadeNeblina;
        }
    }

    // Desativa (não apaga) objetos da cidade — carros, postes, lixeiras —
    // que ficaram dentro da área jogável desta fase. Asfalto e calçada ficam.
    private static void LiberarAreaJogavel(Fase fase)
    {
        GameObject cidade = GameObject.Find(CaminhoDaCidade);
        if (cidade == null)
        {
            Debug.LogWarning($"[CriadorDeCenasDeFase] '{CaminhoDaCidade}' não encontrado; área jogável não foi liberada.");
            return;
        }

        const float margem = 0.3f;
        float minX = XAvenida - LarguraAvenida / 2f - margem;
        float maxX = XAvenida + LarguraAvenida / 2f + margem;

        foreach (Transform objeto in cidade.transform)
        {
            string nome = objeto.name;
            if (nome.StartsWith("road") || nome.StartsWith("Set_B")) continue;

            Vector3 p = objeto.position;
            bool dentro = p.x > minX && p.x < maxX && p.z > fase.ZInicio - margem && p.z < fase.ZFim + margem;
            bool noBraco = fase.BracoNaTransversal && fase.TemTransversal
                && p.x > XFimDoBraco - margem && p.x < maxX
                && Mathf.Abs(p.z - ZTransversal) < MeiaLarguraTransversal + margem;
            dentro |= noBraco;
            if (!dentro || !objeto.gameObject.activeSelf) continue;

            objeto.gameObject.SetActive(false);
            Debug.Log($"[CriadorDeCenasDeFase] Fase {fase.Numero}: '{nome}' estava dentro da rua e foi desativado.");
        }
    }

    // Altura real do chão, medida nos objetos da própria cidade (a cidade
    // fica abaixo de Y = 0). Carros no asfalto; lixeiras/postes na calçada.
    private static float _alturaDoAsfalto = -0.6f;
    private static float _alturaDaCalcada = -0.5f;
    private static readonly List<Vector3> _ocupados = new List<Vector3>();
    private static readonly List<float> _raiosOcupados = new List<float>();
    private static readonly List<Transform> _arbustos = new List<Transform>();

    private static void MedirCidade()
    {
        _ocupados.Clear();
        _raiosOcupados.Clear();
        _arbustos.Clear();

        GameObject cidade = GameObject.Find(CaminhoDaCidade);
        if (cidade == null) return;

        bool achouCarro = false;
        bool achouCalcada = false;
        foreach (Transform objeto in cidade.transform)
        {
            string nome = objeto.name;
            if (!objeto.gameObject.activeSelf || nome.StartsWith("road") || nome.StartsWith("Set_B")) continue;

            if (!achouCarro && (nome.StartsWith("Car_") || nome.StartsWith("Van")))
            {
                _alturaDoAsfalto = objeto.position.y;
                achouCarro = true;
            }
            if (!achouCalcada && (nome.StartsWith("Trash") || nome.StartsWith("Bush") || nome.StartsWith("Spotlight")))
            {
                _alturaDaCalcada = objeto.position.y;
                achouCalcada = true;
            }

            // Arbustos não bloqueiam: os que ficarem embaixo da decoração são escondidos.
            if (nome.StartsWith("Bush"))
            {
                _arbustos.Add(objeto);
                continue;
            }

            // Placas e grafites ficam no alto/na parede: não ocupam o chão.
            if (nome.StartsWith("Billboard") || nome.StartsWith("Signboard") || nome.StartsWith("Graffiti")) continue;
            Ocupar(objeto.position, RaioDoObjetoDaCidade(nome));
        }
    }

    private static float RaioDoObjetoDaCidade(string nome)
    {
        if (nome.StartsWith("Eco") || nome.StartsWith("Regular")) return 9f;
        if (nome.StartsWith("Fountain")) return 4f;
        if (nome.StartsWith("Car_") || nome.StartsWith("Van") || nome.StartsWith("Futuristic") || nome.StartsWith("Bus")) return 2.4f;
        return 0.9f;
    }

    private static void Ocupar(Vector3 posicao, float raio)
    {
        _ocupados.Add(new Vector3(posicao.x, 0f, posicao.z));
        _raiosOcupados.Add(raio);
    }

    // Procura, deslizando ao longo da rua (Z), o ponto mais próximo do
    // desejado que não encosta em nada da cidade nem da decoração já criada.
    private static Vector3 EncontrarPosicaoLivre(Vector3 desejada, float raio)
    {
        for (int passo = 0; passo <= 24; passo++)
        {
            float deslocamento = (passo % 2 == 0 ? 1f : -1f) * ((passo + 1) / 2) * 0.75f;
            Vector3 candidata = desejada + new Vector3(0f, 0f, deslocamento);
            if (EstaLivre(candidata, raio)) return candidata;
        }

        Debug.LogWarning($"[CriadorDeCenasDeFase] Sem espaço livre perto de {desejada}; objeto mantido na posição original.");
        return desejada;
    }

    private static void EsconderArbustosEmVolta(Vector3 posicao, float raio)
    {
        foreach (Transform arbusto in _arbustos)
        {
            if (!arbusto.gameObject.activeSelf) continue;
            float distancia = Vector2.Distance(new Vector2(posicao.x, posicao.z), new Vector2(arbusto.position.x, arbusto.position.z));
            if (distancia < raio) arbusto.gameObject.SetActive(false);
        }
    }

    private static bool EstaLivre(Vector3 posicao, float raio)
    {
        for (int i = 0; i < _ocupados.Count; i++)
        {
            float distancia = Vector2.Distance(new Vector2(posicao.x, posicao.z), new Vector2(_ocupados[i].x, _ocupados[i].z));
            if (distancia < raio + _raiosOcupados[i]) return false;
        }
        return true;
    }

    private static void CriarDecoracao(Fase fase)
    {
        GameObject antiga = GameObject.Find(NomeDecoracao);
        if (antiga != null) Object.DestroyImmediate(antiga);

        MedirCidade();
        Transform pai = new GameObject(NomeDecoracao).transform;

        // Pontas: barricada onde inimigos entram, fechamento onde não entram.
        CriarBarreiraTransversal(fase.ZInicio - DistanciaDaBarreira, fase.InimigosNoInicio ? CarrosDaBarricada : CarrosDoFechamento, pai);
        CriarBarreiraTransversal(fase.ZFim + DistanciaDaBarreira, fase.InimigosNoFim ? CarrosDaBarricada : CarrosDoFechamento, pai);

        // Boca da transversal: sempre fechada; se for entrada, os inimigos
        // nascem atrás desses carros (o vão no meio é por onde eles passam).
        if (fase.TemTransversal)
        {
            bool temBraco = fase.BracoNaTransversal;
            CriarBarreiraLateral(temBraco ? XFimDoBraco - DistanciaDaBarreira : XAvenida - LarguraAvenida / 2f - DistanciaDaBarreira, pai);
        }

        foreach (Obstaculo obstaculo in fase.Obstaculos)
        {
            float altura = obstaculo.NoAsfalto ? _alturaDoAsfalto : _alturaDaCalcada;
            Vector3 posicao = EncontrarPosicaoLivre(new Vector3(obstaculo.X, altura, obstaculo.Z), obstaculo.Raio);
            Ocupar(posicao, obstaculo.Raio);
            EsconderArbustosEmVolta(posicao, obstaculo.Raio);

            GameObject criado = Instanciar($"{obstaculo.Prefab}.prefab", posicao, obstaculo.Giro, pai);
            if (criado != null && obstaculo.AlturaAlvo > 0f) AjustarAltura(criado, obstaculo.AlturaAlvo, altura);
        }
    }

    // 3 carros atravessados (comprimento ao longo de X) cobrindo a largura da rua.
    private static void CriarBarreiraTransversal(float z, string[] modelos, Transform pai)
    {
        for (int i = 0; i < 3; i++)
        {
            float x = XAvenida + (i - 1) * ComprimentoDoCarro;
            float giro = 90f + Random.Range(-8f, 8f);
            string modelo = modelos[Random.Range(0, modelos.Length)];
            Vector3 posicao = new Vector3(x, _alturaDoAsfalto, z + Random.Range(-0.3f, 0.3f));
            Instanciar($"{PastaCarros}{modelo}.prefab", posicao, giro, pai);
            Ocupar(posicao, ComprimentoDoCarro / 2f);
        }
    }

    // 2 carros alinhados com a avenida atravessando a rua transversal
    // (na boca dela ou no fim do braço), com um vão no meio.
    private static void CriarBarreiraLateral(float x, Transform pai)
    {
        float deslocamento = MeiaLarguraTransversal - ComprimentoDoCarro / 2f - 0.5f;
        Vector3 primeiro = new Vector3(x, _alturaDoAsfalto, ZTransversal - deslocamento);
        Vector3 segundo = new Vector3(x, _alturaDoAsfalto, ZTransversal + deslocamento);
        Instanciar($"{PastaCarros}Van.prefab", primeiro, Random.Range(-6f, 6f), pai);
        Instanciar($"{PastaCarros}Car_13.prefab", segundo, 180f + Random.Range(-6f, 6f), pai);
        Ocupar(primeiro, ComprimentoDoCarro / 2f);
        Ocupar(segundo, ComprimentoDoCarro / 2f);
    }

    // Escala uniforme para que o objeto fique com a altura desejada
    // (os prefabs de comida vêm em tamanho real, minúsculos na rua).
    private static void AjustarAltura(GameObject objeto, float alturaAlvo, float alturaDoChao)
    {
        Renderer[] renderers = objeto.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds limites = renderers[0].bounds;
        foreach (Renderer r in renderers) limites.Encapsulate(r.bounds);
        if (limites.size.y < 0.001f) return;

        objeto.transform.localScale *= alturaAlvo / limites.size.y;

        // Apoia no chão depois de escalar.
        limites = renderers[0].bounds;
        foreach (Renderer r in renderers) limites.Encapsulate(r.bounds);
        objeto.transform.position += Vector3.up * (alturaDoChao - limites.min.y);
    }

    private static GameObject Instanciar(string caminhoPrefab, Vector3 posicao, float giro, Transform pai)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(caminhoPrefab);
        if (prefab == null)
        {
            Debug.LogWarning($"[CriadorDeCenasDeFase] Prefab não encontrado: {caminhoPrefab}");
            return null;
        }

        GameObject objeto = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pai);
        objeto.transform.SetPositionAndRotation(posicao, Quaternion.Euler(0f, giro, 0f));
        return objeto;
    }

    private static void AtualizarBuildSettings()
    {
        List<EditorBuildSettingsScene> cenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int numero = 2; numero <= 4; numero++)
        {
            string caminho = $"{PastaCenas}Fase{numero}.unity";
            if (cenas.Exists(c => c.path == caminho)) continue;
            cenas.Add(new EditorBuildSettingsScene(caminho, true));
        }
        EditorBuildSettings.scenes = cenas.ToArray();
    }
}
