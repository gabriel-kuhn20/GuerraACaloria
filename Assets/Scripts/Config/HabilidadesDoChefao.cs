using System;

// Habilidades que o chefão de uma fase pode usar (combináveis).
// Definidas por fase em ConfiguracaoDeFase, para cada chefão ter um estilo.
[Flags]
public enum HabilidadesDoChefao
{
    Nenhuma = 0,
    Investida = 1 << 0,
    Lacaios = 1 << 1,
    Escudo = 1 << 2,
}
