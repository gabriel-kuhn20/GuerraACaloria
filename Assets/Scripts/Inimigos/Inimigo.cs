using UnityEngine;

// Classe base de todo inimigo do jogo (nuvens de maus hábitos).
// Define movimento em direção ao centro, dano e morte; cada tipo de
// inimigo (Hambúrguer Voador, Refrigerante Zumbi, etc) herda daqui.
public abstract class Inimigo : MonoBehaviour, IDanificavel
{
    [SerializeField] protected float _velocidade = 2f;
    protected int _vida = 1;
    private Transform _alvo;

    public abstract TipoMunicao Fraqueza { get; }

    protected virtual void Start()
    {
        _alvo = GerenciadorDeJogo.Instancia.transform;
    }

    protected virtual void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, _alvo.position, _velocidade * Time.deltaTime);

        if (Vector3.Distance(transform.position, _alvo.position) < 1f)
        {
            GerenciadorDeJogo.Instancia.ReceberDanoNaCidade(10);
            Destroy(gameObject);
        }
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