using UnityEngine;

// Representa o sedentarismo. Fraqueza: Energia Positiva.
// Movimento quase reto, mas flutua suavemente para cima e para baixo
// enquanto avança, como uma nuvem no céu. Habilidade: deixa o jogador
// lento enquanto ele estiver perto dela (a preguiça "contagia").
public class NuvemDePreguica : Inimigo
{
    [SerializeField] private float _amplitudeFlutuacao = 0.5f;
    [SerializeField] private float _velocidadeFlutuacao = 1.2f;
    [SerializeField] private float _raioDeLentidao = 7f;
    [SerializeField] private float _fatorDeLentidao = 0.45f;

    private MovimentoJogador _jogador;

    public override TipoMunicao Fraqueza => TipoMunicao.EnergiaPositiva;
    protected override float Velocidade => 12f;
    protected override int DanoAoAlcancarAlvo => 16;
    protected override float AmplitudeOscilacao => 0.2f;
    protected override int PontosAoDerrotar => 110;

    protected override void Start()
    {
        base.Start();
        _jogador = FindFirstObjectByType<MovimentoJogador>();
    }

    protected override void Update()
    {
        base.Update();
        AplicarLentidaoNoJogador();
    }

    private void AplicarLentidaoNoJogador()
    {
        if (_jogador == null) return;

        float distancia = Vector3.Distance(transform.position, _jogador.transform.position);
        if (distancia > _raioDeLentidao) return;

        // Duração curta e renovada a cada frame: o efeito some logo que o jogador se afasta.
        _jogador.AplicarLentidao(_fatorDeLentidao, 0.3f);
    }

    protected override void AplicarAlturaFinal()
    {
        float flutuacao = Mathf.Sin(Time.time * _velocidadeFlutuacao + _deslocamentoAleatorio) * _amplitudeFlutuacao;

        Vector3 posicao = transform.position;
        posicao.y = AlturaFixa + flutuacao;
        transform.position = posicao;
    }
}
