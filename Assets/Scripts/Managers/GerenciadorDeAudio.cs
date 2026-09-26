using UnityEngine;

// Centraliza a reprodução de todos os efeitos sonoros do jogo:
// tiro, acerto correto, erro de munição e alarme de dano à cidade.
// Também controla a música de fundo, em loop contínuo desde o início
// da partida. Singleton acessado por outros scripts via
// GerenciadorDeAudio.Instancia.
public class GerenciadorDeAudio : MonoBehaviour
{
    public static GerenciadorDeAudio Instancia { get; private set; }

    [SerializeField] private AudioClip _somTiro;
    [SerializeField] private AudioClip _somAcerto;
    [SerializeField] private AudioClip _somErro;
    [SerializeField] private AudioClip _somAlarme;
    [SerializeField] private AudioClip _musicaDeFundo;
    [SerializeField] private float _volumeMusica = 0.5f;

    private AudioSource _audioSourceEfeitos;
    private AudioSource _audioSourceMusica;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;

        _audioSourceEfeitos = GetComponent<AudioSource>();
        _audioSourceMusica = gameObject.AddComponent<AudioSource>();
        _audioSourceMusica.clip = _musicaDeFundo;
        _audioSourceMusica.loop = true;
        _audioSourceMusica.volume = _volumeMusica;
    }

    private void Start()
    {
        _audioSourceMusica.Play();
    }

    public void TocarSomTiro()
    {
        _audioSourceEfeitos.PlayOneShot(_somTiro);
    }

    public void TocarSomAcerto()
    {
        _audioSourceEfeitos.PlayOneShot(_somAcerto);
    }

    public void TocarSomErro()
    {
        _audioSourceEfeitos.PlayOneShot(_somErro);
    }

    public void TocarSomAlarme()
    {
        _audioSourceEfeitos.PlayOneShot(_somAlarme);
    }

    public void PararTodosOsSons()
    {
        _audioSourceEfeitos.Stop();
    }
}