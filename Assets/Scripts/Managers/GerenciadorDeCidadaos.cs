using System;
using System.Collections.Generic;
using UnityEngine;

// Mantém a lista de cidadãos da praça: soma a saúde deles (que é a saúde
// da cidade), escolhe o alvo de cada inimigo, distribui curas, dá bônus
// no fim da onda para quem está saudável e avisa a UI sobre sequestros e
// perdas. Durante o evento "Proteja o VIP", todos os inimigos perseguem
// o cidadão VIP. Singleton.
public class GerenciadorDeCidadaos : MonoBehaviour
{
    public static GerenciadorDeCidadaos Instancia { get; private set; }

    [SerializeField] private int _bonusPorCidadaoSaudavel = 100;

    private readonly List<Cidadao> _cidadaosVivos = new List<Cidadao>();
    private readonly List<Cidadao> _todosOsCidadaos = new List<Cidadao>();
    private Cidadao _vip;
    private bool _vipFoiAtingido;

    public bool VipFoiAtingido => _vipFoiAtingido;
    public IReadOnlyList<Cidadao> CidadaosVivos => _cidadaosVivos;

    // Cidadãos levados contam como saúde zero, mas continuam no máximo.
    public int SaudeTotal
    {
        get
        {
            int total = 0;
            foreach (Cidadao cidadao in _cidadaosVivos) total += cidadao.Saude;
            return total;
        }
    }

    public int SaudeMaximaTotal
    {
        get
        {
            int total = 0;
            foreach (Cidadao cidadao in _todosOsCidadaos) total += cidadao.SaudeMaxima;
            return total;
        }
    }

    // (cidadãos saudáveis, pontos ganhos)
    public event Action<int, int> AoDarBonusDeOnda;
    public event Action<Cidadao> AoSequestrarCidadao;
    public event Action<Cidadao> AoPerderCidadao;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    public void RegistrarCidadao(Cidadao cidadao)
    {
        if (!_cidadaosVivos.Contains(cidadao)) _cidadaosVivos.Add(cidadao);
        if (!_todosOsCidadaos.Contains(cidadao)) _todosOsCidadaos.Add(cidadao);
    }

    public void RemoverCidadao(Cidadao cidadao)
    {
        _cidadaosVivos.Remove(cidadao);
        if (cidadao == _vip) _vip = null;
    }

    public Cidadao ObterCidadaoMaisProximo(Vector3 posicaoDeReferencia)
    {
        if (_vip != null && _vip.PodeSerAlvo) return _vip;

        Cidadao maisProximo = null;
        float menorDistancia = float.MaxValue;

        foreach (Cidadao cidadao in _cidadaosVivos)
        {
            if (!cidadao.PodeSerAlvo) continue;

            float distancia = Vector3.Distance(posicaoDeReferencia, cidadao.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                maisProximo = cidadao;
            }
        }

        return maisProximo;
    }

    // Cura vinda de itens, quiz e eventos: vai sempre para quem está pior.
    public void CurarMaisNecessitados(int quantidade)
    {
        while (quantidade > 0)
        {
            Cidadao maisFraco = null;
            foreach (Cidadao cidadao in _cidadaosVivos)
            {
                if (cidadao.EstaSendoCarregado || cidadao.Saude >= cidadao.SaudeMaxima) continue;
                if (maisFraco == null || cidadao.Saude < maisFraco.Saude) maisFraco = cidadao;
            }

            if (maisFraco == null) return;

            int cura = Mathf.Min(quantidade, maisFraco.SaudeMaxima - maisFraco.Saude);
            maisFraco.Curar(cura, peloJogador: false);
            quantidade -= cura;
        }
    }

    public void DefinirSaudeProporcional(float fracao)
    {
        foreach (Cidadao cidadao in _cidadaosVivos) cidadao.DefinirSaudeProporcional(fracao);
    }

    // Fim de onda: cidadãos 100% saudáveis comemoram e rendem bônus.
    public void AoTerminarOnda()
    {
        int saudaveis = 0;
        foreach (Cidadao cidadao in _cidadaosVivos)
        {
            if (cidadao.EstaContaminado || cidadao.EstaDesmaiado || cidadao.EstaSendoCarregado) continue;

            saudaveis++;
            // Pequeno atraso aleatório para não parecer coreografado.
            cidadao.Invoke(nameof(Cidadao.Comemorar), UnityEngine.Random.Range(0f, 0.6f));
        }

        if (saudaveis == 0) return;

        int pontos = saudaveis * _bonusPorCidadaoSaudavel;
        GerenciadorDePontuacao.Instancia.AdicionarBonus(pontos);
        AoDarBonusDeOnda?.Invoke(saudaveis, pontos);
    }

    public void NotificarAtaque(Cidadao cidadao)
    {
        if (cidadao == _vip) _vipFoiAtingido = true;
    }

    public void NotificarSequestro(Cidadao cidadao)
    {
        if (cidadao == _vip) _vipFoiAtingido = true;
        AoSequestrarCidadao?.Invoke(cidadao);
    }

    public void NotificarPerda(Cidadao cidadao)
    {
        AoPerderCidadao?.Invoke(cidadao);
    }

    public void SortearVip()
    {
        List<Cidadao> candidatos = _cidadaosVivos.FindAll(c => c.PodeSerAlvo);
        if (candidatos.Count == 0) return;

        _vip = candidatos[UnityEngine.Random.Range(0, candidatos.Count)];
        _vip.DefinirComoVip(true);
        _vipFoiAtingido = false;
    }

    public void EncerrarVip()
    {
        if (_vip != null) _vip.DefinirComoVip(false);
        _vip = null;
    }
}
