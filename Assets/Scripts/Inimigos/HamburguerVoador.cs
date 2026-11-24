using UnityEngine;

// Representa o excesso de gordura/ultraprocessados. Fraqueza: Fruta.
// Começa pesado e lento, mas acelera progressivamente quanto mais
// perto fica do alvo. Habilidades: ao ser derrotado por tiro, se divide
// em dois mini-hambúrgueres (que não se dividem de novo); e às vezes, em
// vez de contaminar, agarra o cidadão e voa com ele para o fim da rua.
// Derrubá-lo no caminho salva o cidadão; se ele chegar, o cidadão é levado.
public class HamburguerVoador : Inimigo
{
    [SerializeField] private float _distanciaInicioAceleracao = 15f;
    [SerializeField] private float _multiplicadorVelocidadeMaxima = 2.5f;
    [SerializeField] private int _quantidadeDeFilhotes = 2;
    [SerializeField] private float _escalaDoFilhote = 0.55f;
    [SerializeField] private float _afastamentoDosFilhotes = 1.5f;

    [Header("Sequestro")]
    [SerializeField, Range(0f, 1f)] private float _chanceDeSequestro = 0.4f;
    [SerializeField] private float _multiplicadorDeVelocidadeCarregando = 0.35f;
    [SerializeField] private float _distanciaDoCidadaoAbaixo = 1.6f;
    [SerializeField] private float _alturaExtraCarregando = 2.5f;

    private bool _ehFilhote = false;
    private Cidadao _cidadaoCarregado;

    protected override bool PodeSeMover => _cidadaoCarregado != null || base.PodeSeMover;
    protected override bool PodeAlcancarAlvo => _cidadaoCarregado == null;

    public override TipoMunicao Fraqueza => TipoMunicao.Fruta;
    protected override float Velocidade => 14f;
    protected override int DanoAoAlcancarAlvo => _ehFilhote ? 8 : 20;
    protected override float AmplitudeOscilacao => 0.4f;
    protected override int PontosAoDerrotar => _ehFilhote ? 50 : 100;

    protected override void AplicarMovimento()
    {
        if (_cidadaoCarregado != null)
        {
            FugirComCidadao();
            return;
        }

        Vector3 direcaoAoAlvo = DirecaoHorizontalAoAlvo();
        float distancia = DistanciaHorizontalAteAlvo();

        float proporcaoProximidade = 1f - Mathf.Clamp01(distancia / _distanciaInicioAceleracao);
        float multiplicador = Mathf.Lerp(1f, _multiplicadorVelocidadeMaxima, proporcaoProximidade);

        Vector3 movimento = direcaoAoAlvo * VelocidadeAtual * multiplicador;
        transform.position += movimento * Time.deltaTime;
    }

    protected override void AoAlcancarAlvo()
    {
        Cidadao vitima = CidadaoAlvo;
        bool vaiSequestrar = !_ehFilhote && vitima.PodeSerCarregado && Random.value < _chanceDeSequestro;

        if (!vaiSequestrar)
        {
            base.AoAlcancarAlvo();
            return;
        }

        _cidadaoCarregado = vitima;
        vitima.IniciarCarregamento();
        GerenciadorDeCidadaos.Instancia.NotificarSequestro(vitima);
        GerenciadorDePontuacao.Instancia.RegistrarErro();
        GerenciadorDeAudio.Instancia.TocarSomEvento();
    }

    private void FugirComCidadao()
    {
        Vector3 saida = GerenciadorDeRua.Instancia.ObterSaidaMaisProximaDe(transform.position);
        Vector3 caminho = saida - transform.position;
        caminho.y = 0f;

        if (caminho.magnitude < 1.5f)
        {
            ConcluirSequestro();
            return;
        }

        transform.position += caminho.normalized * VelocidadeAtual * _multiplicadorDeVelocidadeCarregando * Time.deltaTime;
    }

    // Sobe enquanto carrega, para o cidadão não ser arrastado pelo chão.
    protected override void AplicarAlturaFinal()
    {
        if (_cidadaoCarregado == null)
        {
            base.AplicarAlturaFinal();
            return;
        }

        Vector3 posicao = transform.position;
        posicao.y = Mathf.MoveTowards(posicao.y, AlturaFixa + _alturaExtraCarregando, 3f * Time.deltaTime);
        transform.position = posicao;
    }

    // Depois de mover, o cidadão vai "pendurado" logo abaixo do hambúrguer.
    private void LateUpdate()
    {
        if (_cidadaoCarregado == null || EstaMorto) return;
        _cidadaoCarregado.AcompanharCarregador(transform.position + Vector3.down * _distanciaDoCidadaoAbaixo);
    }

    private void ConcluirSequestro()
    {
        Cidadao levado = _cidadaoCarregado;
        _cidadaoCarregado = null;
        levado.SerLevado();
        NotificarMorteEDestruir();
    }

    protected override void SerDerrotadoPorTiro()
    {
        if (_cidadaoCarregado != null)
        {
            _cidadaoCarregado.SerResgatado();
            _cidadaoCarregado = null;
        }

        if (!_ehFilhote) GerarFilhotes();
        base.SerDerrotadoPorTiro();
    }

    // Segurança: se for destruído por outro motivo carregando alguém, solta o cidadão.
    private void OnDestroy()
    {
        // Ao fechar a cena tudo é destruído junto; não há o que soltar.
        if (_cidadaoCarregado != null && gameObject.scene.isLoaded) _cidadaoCarregado.SerResgatado();
    }

    private void GerarFilhotes()
    {
        for (int i = 0; i < _quantidadeDeFilhotes; i++)
        {
            float lado = i % 2 == 0 ? -1f : 1f;
            Vector3 posicao = transform.position + transform.right * lado * _afastamentoDosFilhotes;

            // Clona este próprio objeto (mesmo modelo/componentes) e o reduz.
            GameObject copia = Instantiate(gameObject, posicao, transform.rotation);
            HamburguerVoador filhote = copia.GetComponent<HamburguerVoador>();
            filhote.TornarFilhote(_escalaDoFilhote, MultiplicadorDeVelocidade);
            NotificarFilhote(filhote);
        }
    }

    private void TornarFilhote(float escala, float multiplicadorDeVelocidade)
    {
        _ehFilhote = true;
        transform.localScale *= escala;
        AplicarMultiplicadorDeVelocidade(multiplicadorDeVelocidade * 1.2f);
    }
}
