// Contrato para qualquer objeto que pode receber dano de um projétil,
// mesmo que não seja um Inimigo (ex: um obstáculo destrutível do cenário).
public interface IDanificavel
{
    void ReceberDano(TipoMunicao municaoUsada);
}