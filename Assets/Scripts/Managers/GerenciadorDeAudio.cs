using UnityEngine;

// Centraliza a reprodução de todos os efeitos sonoros do jogo:
// tiro, acerto correto, erro de munição e alarme de dano à cidade.
// Singleton acessado por outros scripts via GerenciadorDeAudio.Instancia.
public class GerenciadorDeAudio : MonoBehaviour
{
    public static GerenciadorDeAudio Instancia { get; private set; }

    [SerializeField] private AudioClip _somTiro;
    [SerializeField] private AudioClip _somAcerto;
    [SerializeField] private AudioClip _somErro;
    [SerializeField] private AudioClip _somAlarme;

    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        _audioSource = GetComponent<AudioSource>();
    }

    public void TocarSomTiro()
    {
        _audioSource.PlayOneShot(_somTiro);
    }

    public void TocarSomAcerto()
    {
        _audioSource.PlayOneShot(_somAcerto);
    }

    public void TocarSomErro()
    {
        _audioSource.PlayOneShot(_somErro);
    }

    public void TocarSomAlarme()
    {
        _audioSource.PlayOneShot(_somAlarme);
    }

    public void PararTodosOsSons()
    {
        _audioSource.Stop();
    }
}