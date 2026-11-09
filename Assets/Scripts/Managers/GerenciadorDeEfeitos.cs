using UnityEngine;

// Efeitos de "game feel": explosão de partículas quando um inimigo é
// derrotado, pontos flutuando no ar e tremor de câmera quando a cidade
// leva dano. O sistema de partículas é montado por código uma vez e
// copiado a cada explosão. Singleton.
public class GerenciadorDeEfeitos : MonoBehaviour
{
    public static GerenciadorDeEfeitos Instancia { get; private set; }

    [SerializeField] private Material _materialDasParticulas;
    [SerializeField] private float _intensidadeDoTremor = 0.2f;
    [SerializeField] private float _duracaoDoTremor = 0.3f;

    private ParticleSystem _modeloDeExplosao;
    private TremorDeCamera _tremor;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        _modeloDeExplosao = CriarModeloDeExplosao();
    }

    private void Start()
    {
        Camera camera = Camera.main;
        if (camera != null && !camera.TryGetComponent(out _tremor))
        {
            _tremor = camera.gameObject.AddComponent<TremorDeCamera>();
        }

        GerenciadorDeJogo.Instancia.AoReceberDano += TremerCamera;
    }

    private void OnDestroy()
    {
        if (GerenciadorDeJogo.Instancia != null) GerenciadorDeJogo.Instancia.AoReceberDano -= TremerCamera;
    }

    public void CriarExplosao(Vector3 posicao, Color cor, float escala = 1f)
    {
        ParticleSystem explosao = Instantiate(_modeloDeExplosao, posicao, Quaternion.identity);
        explosao.transform.localScale = Vector3.one * escala;
        explosao.gameObject.SetActive(true);

        ParticleSystem.MainModule principal = explosao.main;
        principal.startColor = new ParticleSystem.MinMaxGradient(cor, Color.white);
        explosao.Play();
        Destroy(explosao.gameObject, 2f);
    }

    public void MostrarPontos(Vector3 posicao, int pontos, int multiplicador)
    {
        string texto = multiplicador > 1 ? $"+{pontos}  x{multiplicador}" : $"+{pontos}";
        Color cor = multiplicador > 1 ? new Color(1f, 0.82f, 0.25f) : Color.white;
        TextoFlutuante.Criar(posicao + Vector3.up * 1.5f, texto, cor);
    }

    private void TremerCamera()
    {
        if (_tremor != null) _tremor.Tremer(_intensidadeDoTremor, _duracaoDoTremor);
    }

    private ParticleSystem CriarModeloDeExplosao()
    {
        GameObject objeto = new GameObject("ModeloDeExplosao");
        objeto.transform.SetParent(transform);
        objeto.SetActive(false);

        ParticleSystem particulas = objeto.AddComponent<ParticleSystem>();
        particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule principal = particulas.main;
        principal.duration = 0.5f;
        principal.loop = false;
        principal.playOnAwake = false;
        principal.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        principal.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        principal.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        principal.gravityModifier = 0.8f;
        principal.simulationSpace = ParticleSystemSimulationSpace.World;
        principal.scalingMode = ParticleSystemScalingMode.Hierarchy;
        principal.maxParticles = 60;

        ParticleSystem.EmissionModule emissao = particulas.emission;
        emissao.rateOverTime = 0f;
        emissao.SetBursts(new[] { new ParticleSystem.Burst(0f, 35) });

        ParticleSystem.ShapeModule forma = particulas.shape;
        forma.shapeType = ParticleSystemShapeType.Sphere;
        forma.radius = 0.3f;

        ParticleSystem.SizeOverLifetimeModule tamanho = particulas.sizeOverLifetime;
        tamanho.enabled = true;
        tamanho.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ParticleSystemRenderer renderizador = objeto.GetComponent<ParticleSystemRenderer>();
        renderizador.material = _materialDasParticulas != null
            ? _materialDasParticulas
            : new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

        return particulas;
    }
}
