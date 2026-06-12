using System.Data;
using Codout.Framework.Data;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Data.Tests;

/// <summary>
/// Contrato do IUnitOfWork e do (obsoleto) IUnitOfWorkProvider.
/// </summary>
public class UnitOfWorkContractTests
{
    [Fact]
    public void IUnitOfWork_is_disposable_and_async_disposable()
    {
        typeof(IUnitOfWork).Should().Implement<IDisposable>();
        typeof(IUnitOfWork).Should().Implement<IAsyncDisposable>();
    }

    [Fact]
    public void Commit_and_BeginTransaction_have_IsolationLevel_overloads()
    {
        typeof(IUnitOfWork).GetMethod("Commit", [typeof(IsolationLevel)]).Should().NotBeNull();
        typeof(IUnitOfWork).GetMethod("BeginTransaction", [typeof(IsolationLevel)]).Should().NotBeNull();
        typeof(IUnitOfWork).GetMethod("BeginTransactionAsync",
            [typeof(IsolationLevel), typeof(CancellationToken)]).Should().NotBeNull();
    }

    [Theory]
    [InlineData("CommitAsync")]
    [InlineData("RollbackAsync")]
    public void Async_transaction_methods_take_an_optional_CancellationToken(string methodName)
    {
        var method = typeof(IUnitOfWork).GetMethod(methodName, [typeof(CancellationToken)])!;

        method.GetParameters().Single().IsOptional.Should().BeTrue();
    }

    [Fact]
    public void InTransaction_helpers_are_part_of_the_contract()
    {
        typeof(IUnitOfWork).GetMethods().Select(m => m.Name)
            .Should().Contain(["InTransaction", "InTransactionAsync"]);
    }

    [Fact]
    public void IUnitOfWorkProvider_is_marked_obsolete_but_still_compiles()
    {
        var attribute = typeof(IUnitOfWorkProvider<>)
            .GetCustomAttributes(typeof(ObsoleteAttribute), false)
            .Cast<ObsoleteAttribute>()
            .Single();

        attribute.IsError.Should().BeFalse("ainda é warning, não erro — remoção prevista para versão futura");
        attribute.Message.Should().Contain("dependency injection");
    }

    [Fact]
    public void IUnitOfWorkProvider_type_parameter_is_covariant()
    {
        var t = typeof(IUnitOfWorkProvider<>).GetGenericArguments()[0];

        t.GenericParameterAttributes
            .HasFlag(System.Reflection.GenericParameterAttributes.Covariant)
            .Should().BeTrue("declarado como <out T>");
    }
}
