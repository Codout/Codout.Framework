using System.Reflection;
using Codout.Framework.Data.Repository;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Data.Tests;

/// <summary>
/// Codout.Framework.Data é composto por abstrações puras (sem lógica concreta).
/// Estes testes travam o CONTRATO compilado no assembly — defaults de parâmetros,
/// overloads de CancellationToken e disposability — para que mudanças quebradoras
/// na interface sejam detectadas antes de quebrar as implementações (EF, NH, Mongo).
/// </summary>
public class RepositoryContractTests
{
    private static readonly Type Contract = typeof(IRepository<FakeEntity>);

    [Fact]
    public void IRepository_is_disposable()
    {
        Contract.Should().Implement<IDisposable>();
    }

    [Fact]
    public void WherePaged_has_default_index_0_and_size_50()
    {
        var method = Contract.GetMethod("WherePaged")!;
        var parameters = method.GetParameters();

        parameters.Should().HaveCount(4);
        parameters[1].IsOut.Should().BeTrue("total é retornado por out");

        parameters[2].Name.Should().Be("index");
        parameters[2].HasDefaultValue.Should().BeTrue();
        parameters[2].DefaultValue.Should().Be(0);

        parameters[3].Name.Should().Be("size");
        parameters[3].HasDefaultValue.Should().BeTrue();
        parameters[3].DefaultValue.Should().Be(50, "tamanho de página default do contrato");
    }

    [Theory]
    [InlineData("GetAsync")]
    [InlineData("LoadAsync")]
    [InlineData("DeleteAsync")]
    [InlineData("SaveAsync")]
    [InlineData("SaveOrUpdateAsync")]
    [InlineData("UpdateAsync")]
    [InlineData("MergeAsync")]
    [InlineData("RefreshAsync")]
    public void Every_async_command_has_a_CancellationToken_overload(string methodName)
    {
        var overloads = Contract.GetMethods().Where(m => m.Name == methodName).ToList();

        overloads.Should().NotBeEmpty();
        overloads.Should().Contain(
            m => m.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)),
            $"{methodName} deve ter overload com CancellationToken");
    }

    [Theory]
    [InlineData("FirstOrDefaultAsync")]
    [InlineData("AnyAsync")]
    [InlineData("CountAsync")]
    [InlineData("ToListAsync")]
    public void Async_query_helpers_take_an_optional_CancellationToken(string methodName)
    {
        var method = Contract.GetMethod(methodName)!;
        var last = method.GetParameters().Last();

        last.ParameterType.Should().Be(typeof(CancellationToken));
        last.IsOptional.Should().BeTrue("o token é opcional (default) nesses helpers");
    }

    [Fact]
    public void Synchronous_and_asynchronous_query_surface_is_complete()
    {
        var methodNames = Contract.GetMethods().Select(m => m.Name).Distinct();

        methodNames.Should().Contain(
        [
            "All", "AllReadOnly", "Where", "WhereReadOnly", "WherePaged",
            "Get", "Load", "Delete", "Save", "SaveOrUpdate", "Update",
            "Merge", "Refresh", "IncludeMany"
        ]);
    }

    [Fact]
    public void Type_constraint_requires_class_implementing_IEntity()
    {
        var t = typeof(IRepository<>).GetGenericArguments()[0];

        t.GenericParameterAttributes
            .HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)
            .Should().BeTrue();
        t.GetGenericParameterConstraints()
            .Should().Contain(typeof(Codout.Framework.Data.Entity.IEntity));
    }
}
