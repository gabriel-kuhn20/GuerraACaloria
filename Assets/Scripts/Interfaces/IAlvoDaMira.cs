// Contrato para o que a mira reconhece: se o alvo indica uma munição
// (fraqueza do inimigo ou o que cura o cidadão contaminado), a mira toma
// a cor dessa munição.
public interface IAlvoDaMira
{
    bool IndicaMunicao { get; }
    TipoMunicao MunicaoIndicada { get; }
}
