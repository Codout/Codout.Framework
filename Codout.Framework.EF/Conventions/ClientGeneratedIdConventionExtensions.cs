using Microsoft.EntityFrameworkCore;

namespace Codout.Framework.EF.Conventions;

/// <summary>
/// Registro idiomático da <see cref="ClientGeneratedIdConvention"/>. Chamar de
/// <c>ConfigureConventions(ModelConfigurationBuilder)</c> em qualquer DbContext
/// (base ou específico). O EF deduplica convenções por tipo, então é seguro chamar
/// em uma base e numa subclasse.
/// </summary>
public static class ClientGeneratedIdConventionExtensions
{
    public static ModelConfigurationBuilder AddCodoutClientGeneratedIdConvention(
        this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new ClientGeneratedIdConvention());
        return configurationBuilder;
    }
}
