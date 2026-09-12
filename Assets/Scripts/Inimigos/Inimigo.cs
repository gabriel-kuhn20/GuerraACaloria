using UnityEngine;

// Classe base de todo inimigo do jogo (nuvens de maus hábitos).
// Persegue o cidadão vivo mais próximo (recalculado periodicamente,
// não todo frame); ao alcançá-lo, causa dano à energia da cidade.
public abstract class Inimigo : MonoBehaviour, IDanificavel
{
    [SerializeField] protected float _velocidade = 2f;
    [SerializeField] private float _intervaloBuscaAlvo = 0.5f;
    protected int _vida = 1;

    private Transform _alvo;
    private float _proximaBusca;

    public abstract TipoMunicao Fraqueza { get; }

    protected virtual void Update()
    {
        AtualizarAlvoPeriodicamente();

        if (_alvo == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position, _alvo.position, _velocidade * Time.deltaTime);

        if (Vector3.Distance(transform.position, _alvo.position) < 1f)
        {
            GerenciadorDeJogo.Instancia.ReceberDanoNaCidade(10);
            Destroy(gameObject);
        }
    }

    private void AtualizarAlvoPeriodicamente()
    {
        if (Time.time < _proximaBusca) return;

        _alvo = GerenciadorDeCidadaos.Instancia.ObterCidadaoMaisProximo(transform.position);
        _proximaBusca = Time.time + _intervaloBuscaAlvo;
    }

    public virtual void ReceberDano(TipoMunicao municaoUsada)
    {
        if (municaoUsada == Fraqueza)
        {
            _vida--;
            GerenciadorDeAudio.Instancia.TocarSomAcerto();
        }
        else
        {
            GerenciadorDeAudio.Instancia.TocarSomErro();
        }

        if (_vida <= 0)
        {
            Morrer();
        }
    }

    protected virtual void Morrer()
    {
        Destroy(gameObject);
    }
}