using UnityEngine;

// Projétil saudável disparado pela luva tecnológica (água, fruta ou
// energia positiva). Viaja em linha reta, mostra o modelo 3D certo
// conforme a munição, e aplica dano ao colidir com qualquer IDanificavel.
public class Projetil : MonoBehaviour
{
    [SerializeField] private float _velocidade = 30f;
    [SerializeField] private float _tempoDeVida = 3f;
    [SerializeField] private GameObject _modeloAgua;
    [SerializeField] private GameObject _modeloFruta;
    [SerializeField] private GameObject _modeloEnergia;

    private TipoMunicao _tipoMunicao;

    public void Configurar(TipoMunicao tipo)
    {
        _tipoMunicao = tipo;
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
        transform.position += transform.forward * _velocidade * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider outro)
    {
        IDanificavel alvo = outro.GetComponent<IDanificavel>();
        if (alvo != null)
        {
            alvo.ReceberDano(_tipoMunicao);
            Destroy(gameObject);
        }
    }
}