using UnityEngine;

// Cor e nome de exibição de cada tipo de munição. Usado pela UI (HUD,
// painel do chefão, legendas) para manter a mesma identidade visual.
public static class CoresDeMunicao
{
    public static Color Obter(TipoMunicao municao)
    {
        switch (municao)
        {
            case TipoMunicao.Agua: return new Color(0.3f, 0.68f, 1f);
            case TipoMunicao.Fruta: return new Color(1f, 0.45f, 0.38f);
            case TipoMunicao.EnergiaPositiva: return new Color(1f, 0.84f, 0.25f);
            default: return Color.white;
        }
    }

    public static string Nome(TipoMunicao municao)
    {
        switch (municao)
        {
            case TipoMunicao.Agua: return "Água";
            case TipoMunicao.Fruta: return "Fruta";
            case TipoMunicao.EnergiaPositiva: return "Energia Positiva";
            default: return municao.ToString();
        }
    }
}
