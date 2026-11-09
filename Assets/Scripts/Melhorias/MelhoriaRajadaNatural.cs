using UnityEngine;

// Rajada Natural: segurar o botão atira sozinho, cada nível mais rápido.
public class MelhoriaRajadaNatural : Melhoria
{
    private const float IntervaloInicial = 0.4f;
    private const float ReducaoPorNivel = 0.75f;

    public override string Id => "rajada_natural";
    public override string Nome => "RAJADA NATURAL";
    public override Color Cor => new Color(1f, 0.62f, 0.2f);

    public override string Descricao(int proximoNivel) => proximoNivel == 1
        ? "Segure o botão para <b>atirar sem parar</b>."
        : $"Rajada <b>{Mathf.RoundToInt((1f / ReducaoPorNivel - 1f) * 100f)}% mais rápida</b>.";

    public override void Aplicar(AtributosDoJogador atributos, int nivel)
    {
        atributos.IntervaloDeTiroAutomatico = IntervaloInicial * Mathf.Pow(ReducaoPorNivel, nivel - 1);
    }
}
