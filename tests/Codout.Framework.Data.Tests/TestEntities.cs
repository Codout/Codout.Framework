using System.Reflection;
using Codout.Framework.Data.Entity;

namespace Codout.Framework.Data.Tests;

/// <summary>
/// Implementações mínimas das abstrações de entidade, usadas pelos testes de contrato.
/// </summary>
internal class FakeEntity : IEntity
{
    public IEnumerable<PropertyInfo> GetSignatureProperties() => [];

    public bool IsTransient() => true;
}

internal class StringIdEntity : IEntity<string>
{
    public StringIdEntity(string id) => Id = id;

    public string Id { get; }

    public IEnumerable<PropertyInfo> GetSignatureProperties() => [];

    public bool IsTransient() => false;
}
