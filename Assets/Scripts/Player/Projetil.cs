using UnityEngine;

// Projétil saudável disparado pela luva tecnológica (água, fruta ou
// energia positiva). Viaja em linha reta e aplica dano ao colidir
// com qualquer IDanificavel.
public class Projetil : MonoBehaviour
{
    [SerializeField] private float _velocidade = 30f;
    [SerializeField] private float _tempoDeVida = 3f;
    private TipoMunicao _tipoMunicao;

    public void Configurar(TipoMunicao tipo)
    {
        _tipoMunicao = tipo;
        Destroy(gameObject, _tempoDeVida);
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