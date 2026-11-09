// Contrato para qualquer objeto que pode ser atingido por um projétil,
// mesmo que não seja um Inimigo (ex: power-up, cidadão contaminado).
// AceitaTiro = false faz o projétil passar direto (ex: cidadão saudável).
public interface IDanificavel
{
    bool AceitaTiro { get; }
    void ReceberDano(TipoMunicao municaoUsada);
}
