using System.Collections;
using UnityEngine;

// Centraliza todos os efeitos sonoros e a música de fundo (em loop).
// Cada ação importante tem seu próprio som (tiro por munição, abate,
// cura, chefão, especial, ondas, melhorias, vitória/derrota); sons sem
// clipe configurado reaproveitam os básicos com outro tom. Sons muito
// repetidos (tiro, acerto, abate) variam um pouco o tom a cada vez para
// não cansar. Cada fase tem sua música e o chefão tem uma própria; a
// troca é feita com transição suave (crossfade). Os volumes vêm das
// ConfiguracoesDoJogo. Singleton.
public class GerenciadorDeAudio : MonoBehaviour
{
    public static GerenciadorDeAudio Instancia { get; private set; }

    [SerializeField] private AudioClip _somTiro;
    [SerializeField] private AudioClip _somAcerto;
    [SerializeField] private AudioClip _somErro;
    [SerializeField] private AudioClip _somAlarme;
    [SerializeField] private AudioClip _musicaDeFundo;
    [SerializeField] private AudioClip _musicaDoChefao;
    [SerializeField] private float _duracaoDaTransicaoDeMusica = 1.5f;

    [Header("Opcionais (vazios = variação dos sons acima)")]
    [SerializeField] private AudioClip _somPowerUp;
    [SerializeField] private AudioClip _somComboSubiu;
    [SerializeField] private AudioClip _somComboQuebrou;
    [SerializeField] private AudioClip _somEvento;
    [SerializeField] private AudioClip _somChefao;
    [SerializeField] private AudioClip _somClique;

    [Header("Jogador")]
    [SerializeField] private AudioClip _somTiroAgua;
    [SerializeField] private AudioClip _somTiroFruta;
    [SerializeField] private AudioClip _somTiroEnergia;
    [SerializeField] private AudioClip _somTrocaMunicao;
    [SerializeField] private AudioClip _somEspecialPronto;
    [SerializeField] private AudioClip _somEspecialSoltar;
    [SerializeField] private AudioClip _somAgenteAtingido;

    [Header("Inimigos e cidadãos")]
    [SerializeField] private AudioClip _somInimigoDerrotado;
    [SerializeField] private AudioClip _somCura;
    [SerializeField] private AudioClip _somDesmaio;

    [Header("Chefão")]
    [SerializeField] private AudioClip _somChefaoFuria;
    [SerializeField] private AudioClip _somChefaoInvestida;
    [SerializeField] private AudioClip _somChefaoEscudo;
    [SerializeField] private AudioClip _somLacaio;
    [SerializeField] private AudioClip _somChefaoDerrotado;
    [SerializeField] private AudioClip _somChefaoArremesso;

    [Header("Progresso")]
    [SerializeField] private AudioClip _somOndaInicio;
    [SerializeField] private AudioClip _somOndaCompleta;
    [SerializeField] private AudioClip _somMelhoriaAbrir;
    [SerializeField] private AudioClip _somMelhoriaEscolher;
    [SerializeField] private AudioClip _somVitoria;
    [SerializeField] private AudioClip _somDerrota;

    [Header("Mixagem")]
    [Tooltip("Variação aleatória de tom (±) nos sons repetitivos.")]
    [SerializeField, Range(0f, 0.3f)] private float _variacaoDeTom = 0.08f;
    [SerializeField] private int _vozesSimultaneas = 8;
    [Tooltip("Volume da música enquanto toca o jingle de vitória/derrota.")]
    [SerializeField, Range(0f, 1f)] private float _musicaAbafada = 0.25f;

    private AudioSource _fonteEfeitos;
    private AudioSource _fonteMusica;
    private AudioSource _fonteMusicaReserva;
    private AudioSource[] _vozes;
    private float _fatorDaMusica = 1f;
    private Coroutine _transicao;
    private int _proximaVoz;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;

        _fonteEfeitos = GetComponent<AudioSource>();
        if (_fonteEfeitos == null) _fonteEfeitos = gameObject.AddComponent<AudioSource>();
        _fonteEfeitos.playOnAwake = false;

        // Várias fontes para sons com tom diferente tocarem ao mesmo tempo
        // (o pitch é da fonte, não do clipe).
        _vozes = new AudioSource[Mathf.Max(1, _vozesSimultaneas)];
        for (int i = 0; i < _vozes.Length; i++)
        {
            _vozes[i] = gameObject.AddComponent<AudioSource>();
            _vozes[i].playOnAwake = false;
        }

        _fonteMusica = CriarFonteDeMusica();
        _fonteMusica.clip = _musicaDeFundo;
        _fonteMusicaReserva = CriarFonteDeMusica();

        AplicarVolumes();
    }

    private void OnEnable()
    {
        ConfiguracoesDoJogo.AoMudar += AplicarVolumes;
    }

    private void OnDisable()
    {
        ConfiguracoesDoJogo.AoMudar -= AplicarVolumes;
    }

    private void Start()
    {
        if (_musicaDeFundo != null) _fonteMusica.Play();
    }

    private AudioSource CriarFonteDeMusica()
    {
        AudioSource fonte = gameObject.AddComponent<AudioSource>();
        fonte.loop = true;
        fonte.playOnAwake = false;
        // Música continua tocando mesmo com o jogo pausado (timeScale 0).
        fonte.ignoreListenerPause = true;
        return fonte;
    }

    private void AplicarVolumes()
    {
        if (_transicao == null) _fonteMusica.volume = ConfiguracoesDoJogo.VolumeMusica * _fatorDaMusica;
        _fonteEfeitos.volume = ConfiguracoesDoJogo.VolumeEfeitos;
        foreach (AudioSource voz in _vozes) voz.volume = ConfiguracoesDoJogo.VolumeEfeitos;
    }

    // ----- Básicos -----
    public void TocarSomTiro() => TocarVariado(_somTiro, 1f);
    public void TocarSomAcerto() => TocarVariado(_somAcerto, 1f);
    public void TocarSomErro() => Tocar(_somErro);
    public void TocarSomAlarme() => Tocar(_somAlarme);

    public void TocarSomPowerUp() => TocarOuVariar(_somPowerUp, _somAcerto, 1.5f);
    public void TocarSomComboQuebrou() => TocarOuVariar(_somComboQuebrou, _somErro, 0.7f);
    public void TocarSomEvento() => TocarOuVariar(_somEvento, _somAlarme, 1.3f);
    public void TocarSomChefao() => TocarOuVariar(_somChefao, _somAlarme, 0.6f);
    public void TocarSomClique() => TocarOuVariar(_somClique, _somAcerto, 2f);

    // O tom sobe a cada nível de combo (x2, x3...), dando sensação de progresso.
    public void TocarSomComboSubiu(int multiplicador)
    {
        AudioClip clipe = _somComboSubiu != null ? _somComboSubiu : _somAcerto;
        TocarComTom(clipe, 1f + 0.12f * (multiplicador - 1));
    }

    // ----- Jogador -----
    public void TocarSomTiro(TipoMunicao municao)
    {
        AudioClip proprio = municao == TipoMunicao.Agua ? _somTiroAgua
            : municao == TipoMunicao.Fruta ? _somTiroFruta
            : _somTiroEnergia;
        TocarVariado(proprio != null ? proprio : _somTiro, 1f);
    }

    public void TocarSomTrocaMunicao(TipoMunicao municao) => TocarOuVariar(_somTrocaMunicao, _somClique, 1.6f, 1f + 0.1f * (int)municao);
    public void TocarSomEspecialPronto() => TocarOuVariar(_somEspecialPronto, _somPowerUp, 1.8f);
    public void TocarSomAgenteAtingido() => TocarOuVariar(_somAgenteAtingido, _somErro, 0.7f);
    public void TocarSomEspecialSoltar() => TocarOuVariar(_somEspecialSoltar, _somAlarme, 0.5f);

    // ----- Inimigos e cidadãos -----
    public void TocarSomInimigoDerrotado() => TocarVariado(_somInimigoDerrotado != null ? _somInimigoDerrotado : _somAcerto, 1f);
    public void TocarSomCura() => TocarOuVariar(_somCura, _somPowerUp, 1.2f);
    public void TocarSomDesmaio() => TocarOuVariar(_somDesmaio, _somErro, 0.6f);

    // ----- Chefão -----
    public void TocarSomChefaoFuria() => TocarOuVariar(_somChefaoFuria, _somAlarme, 0.5f);
    public void TocarSomChefaoInvestida() => TocarOuVariar(_somChefaoInvestida, _somTiro, 0.5f);
    public void TocarSomChefaoEscudo() => TocarOuVariar(_somChefaoEscudo, _somPowerUp, 0.7f);
    public void TocarSomLacaio() => TocarVariado(_somLacaio != null ? _somLacaio : _somTiro, _somLacaio != null ? 1f : 0.6f);
    public void TocarSomChefaoArremesso() => TocarOuVariar(_somChefaoArremesso, _somTiro, 0.6f);
    public void TocarSomChefaoDerrotado() => TocarOuVariar(_somChefaoDerrotado, _somAlarme, 0.8f);

    // ----- Progresso -----
    public void TocarSomOndaInicio() => TocarOuVariar(_somOndaInicio, _somAlarme, 1.1f);
    public void TocarSomOndaCompleta() => TocarOuVariar(_somOndaCompleta, _somPowerUp, 1.3f);
    public void TocarSomMelhoriaAbrir() => TocarOuVariar(_somMelhoriaAbrir, _somPowerUp, 1f);
    public void TocarSomMelhoriaEscolher() => TocarOuVariar(_somMelhoriaEscolher, _somPowerUp, 1.4f);

    // Vitória/derrota: abafa a música para o jingle aparecer.
    public void TocarSomVitoria()
    {
        AbafarMusica();
        TocarOuVariar(_somVitoria, _somPowerUp, 1f);
    }

    public void TocarSomDerrota()
    {
        AbafarMusica();
        TocarOuVariar(_somDerrota, _somErro, 0.5f);
    }

    // ----- Música -----

    // Troca a música com crossfade. Mesma música já tocando = não faz nada.
    public void TrocarMusica(AudioClip nova)
    {
        if (nova == null || nova == _fonteMusica.clip) return;

        if (_transicao != null) StopCoroutine(_transicao);
        _transicao = StartCoroutine(FazerTransicao(nova));
    }

    public void TocarMusicaDoChefao() => TrocarMusica(_musicaDoChefao);

    private void AbafarMusica()
    {
        _fatorDaMusica = _musicaAbafada;
        if (_transicao == null) _fonteMusica.volume = ConfiguracoesDoJogo.VolumeMusica * _fatorDaMusica;
    }

    private IEnumerator FazerTransicao(AudioClip nova)
    {
        AudioSource saindo = _fonteMusica;
        AudioSource entrando = _fonteMusicaReserva;
        entrando.clip = nova;
        entrando.volume = 0f;
        entrando.Play();

        float duracao = Mathf.Max(0.01f, _duracaoDaTransicaoDeMusica);
        float volumeInicial = saindo.volume;
        for (float t = 0f; t < duracao; t += Time.unscaledDeltaTime)
        {
            float alvo = ConfiguracoesDoJogo.VolumeMusica * _fatorDaMusica;
            float progresso = t / duracao;
            entrando.volume = alvo * progresso;
            saindo.volume = volumeInicial * (1f - progresso);
            yield return null;
        }

        saindo.Stop();
        entrando.volume = ConfiguracoesDoJogo.VolumeMusica * _fatorDaMusica;
        _fonteMusica = entrando;
        _fonteMusicaReserva = saindo;
        _transicao = null;
    }

    public void PararTodosOsSons()
    {
        _fonteEfeitos.Stop();
        foreach (AudioSource voz in _vozes) voz.Stop();
    }

    // ----- Internos -----
    private void Tocar(AudioClip clipe)
    {
        if (clipe != null) _fonteEfeitos.PlayOneShot(clipe);
    }

    private void TocarOuVariar(AudioClip proprio, AudioClip alternativo, float tomDoAlternativo, float tomDoProprio = 1f)
    {
        if (proprio != null)
        {
            TocarComTom(proprio, tomDoProprio);
            return;
        }
        TocarComTom(alternativo, tomDoAlternativo);
    }

    private void TocarVariado(AudioClip clipe, float tomBase)
    {
        TocarComTom(clipe, tomBase * Random.Range(1f - _variacaoDeTom, 1f + _variacaoDeTom));
    }

    private void TocarComTom(AudioClip clipe, float tom)
    {
        if (clipe == null) return;

        if (Mathf.Approximately(tom, 1f))
        {
            _fonteEfeitos.PlayOneShot(clipe);
            return;
        }

        AudioSource voz = _vozes[_proximaVoz];
        _proximaVoz = (_proximaVoz + 1) % _vozes.Length;
        voz.pitch = tom;
        voz.PlayOneShot(clipe);
    }
}
