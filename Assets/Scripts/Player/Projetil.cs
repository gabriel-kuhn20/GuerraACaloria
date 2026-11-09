using System.Collections.Generic;
using UnityEngine;

// Projétil saudável disparado pela luva tecnológica (água, fruta ou
// energia positiva). Viaja em linha reta, mostra o modelo 3D certo
// conforme a munição, e aplica dano ao colidir com qualquer IDanificavel.
// Antes de andar a cada frame, "varre" o trecho com um SphereCast para
// não atravessar alvos pequenos quando o FPS cai. Acerta um alvo, ou
// mais se tiver perfuração (cada alvo só uma vez).
public class Projetil : MonoBehaviour
{
    [SerializeField] private float _velocidade = 30f;
    [SerializeField] private float _tempoDeVida = 3f;
    [SerializeField] private float _raioDeAcerto = 0.3f;
    [SerializeField] private GameObject _modeloAgua;
    [SerializeField] private GameObject _modeloFruta;
    [SerializeField] private GameObject _modeloEnergia;

    private TipoMunicao _tipoMunicao;
    private bool _jaAcertou;
    private int _alvosRestantes = 1;
    private readonly HashSet<IDanificavel> _alvosAtingidos = new HashSet<IDanificavel>();

    // perfuracao = quantos alvos extras ele atravessa (melhoria Fibra Perfurante).
    public void Configurar(TipoMunicao tipo, int perfuracao = 0)
    {
        _tipoMunicao = tipo;
        _alvosRestantes = 1 + Mathf.Max(0, perfuracao);
        AtualizarVisual();
        Destroy(gameObject, _tempoDeVida);
    }

    private void AtualizarVisual()
    {
        _modeloAgua.SetActive(true);
        _modeloFruta.SetActive(true);
        _modeloEnergia.SetActive(true);

        if (_tipoMunicao != TipoMunicao.Agua) _modeloAgua.SetActive(false);
        if (_tipoMunicao != TipoMunicao.Fruta) _modeloFruta.SetActive(false);
        if (_tipoMunicao != TipoMunicao.EnergiaPositiva) _modeloEnergia.SetActive(false);

    }

    private void Update()
    {
        if (_jaAcertou) return;

        float passo = _velocidade * Time.deltaTime;
        bool encontrouAlgo = Physics.SphereCast(transform.position, _raioDeAcerto, transform.forward, out RaycastHit impacto,
            passo, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        if (encontrouAlgo && TentarAcertar(impacto.collider)) return;

        transform.position += transform.forward * passo;
    }

    private void OnTriggerEnter(Collider outro)
    {
        TentarAcertar(outro);
    }

    // Retorna true se o colisor era um alvo válido (o projétil é consumido).
    private bool TentarAcertar(Collider colisor)
    {
        if (_jaAcertou) return false;

        IDanificavel alvo = colisor.GetComponentInParent<IDanificavel>();
        if (alvo == null || !alvo.AceitaTiro || _alvosAtingidos.Contains(alvo)) return false;

        _alvosAtingidos.Add(alvo);
        alvo.ReceberDano(_tipoMunicao);
        _alvosRestantes--;
        if (_alvosRestantes > 0) return false;

        _jaAcertou = true;
        Destroy(gameObject);
        return true;
    }
}