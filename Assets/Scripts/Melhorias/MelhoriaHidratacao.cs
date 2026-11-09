using UnityEngine;

// Hidratação Constante: os cidadãos recuperam saúde aos poucos, sozinhos.
public class MelhoriaHidratacao : Melhoria
{
    private const int CuraPorNivel = 2;

    public override string Id => "hidratacao";
    public override string Nome => "HIDRATAÇÃO CONSTANTE";
    public override Color Cor => new Color(0.35f, 0.85f, 0.95f);

    public override string Descricao(int proximoNivel) =>
        $"Cidadãos recuperam <b>{CuraPorNivel * proximoNivel} de saúde por segundo</b>.";

    public override void Aplicar(AtributosDoJogador atributos, int nivel)
    {
        atributos.RegeneracaoPorSegundo += CuraPorNivel * nivel;
    }
}
