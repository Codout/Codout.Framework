using FluentAssertions;
using NHibernate;
using Xunit;

namespace Codout.Framework.NH.Tests;

[Collection("NH")]
public class NHRepositoryAsyncTests : IDisposable
{
    private readonly NHSqliteFixture _fixture;
    private readonly ISession _session;
    private readonly NHRepository<Widget> _repository;

    public NHRepositoryAsyncTests(NHSqliteFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _session = _fixture.OpenSession();
        _repository = new NHRepository<Widget>(_session);
    }

    public void Dispose() => _session.Dispose();

    [Fact]
    public async Task SaveAsync_DevePersistirEntidade()
    {
        var widget = await _repository.SaveAsync(new Widget { Name = "Async", Stock = 1 });

        widget.Id.Should().BePositive();

        using var otherSession = _fixture.OpenSession();
        (await otherSession.GetAsync<Widget>(widget.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task SaveAsync_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var act = () => _repository.SaveAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAsync_PorChave_DeveRetornarEntidade()
    {
        var seeded = _fixture.Seed("BuscaAsync", 9);

        var found = await _repository.GetAsync(seeded.Id);

        found.Should().NotBeNull();
        found!.Stock.Should().Be(9);
    }

    [Fact]
    public async Task GetAsync_PorChaveInexistente_DeveRetornarNull()
    {
        (await _repository.GetAsync(99999)).Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_PorPredicado_DeveRetornarEntidadeUnica()
    {
        _fixture.Seed("Unico", 3);

        var found = await _repository.GetAsync(w => w.Name == "Unico");

        found.Should().NotBeNull();
        found.Stock.Should().Be(3);
    }

    [Fact]
    public async Task LoadAsync_DeveRetornarEntidadeExistente()
    {
        var seeded = _fixture.Seed("LoadAsync", 2);

        var loaded = await _repository.LoadAsync(seeded.Id);

        loaded!.Name.Should().Be("LoadAsync");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_DeveRetornarPrimeiroOuNull()
    {
        _fixture.Seed("Primeiro", 1);
        _fixture.Seed("Primeiro", 2);

        var found = await _repository.FirstOrDefaultAsync(w => w.Name == "Primeiro");
        var missing = await _repository.FirstOrDefaultAsync(w => w.Name == "Inexistente");

        found.Should().NotBeNull();
        missing.Should().BeNull();
    }

    [Fact]
    public async Task AnyAsync_DeveIndicarExistencia()
    {
        _fixture.Seed("Existe", 1);

        (await _repository.AnyAsync(w => w.Name == "Existe")).Should().BeTrue();
        (await _repository.AnyAsync(w => w.Name == "NaoExiste")).Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_DeveContarPeloPredicado()
    {
        _fixture.Seed("A", 1);
        _fixture.Seed("B", 5);
        _fixture.Seed("C", 8);

        (await _repository.CountAsync(w => w.Stock >= 5)).Should().Be(2);
    }

    [Fact]
    public async Task ToListAsync_DeveMaterializarPeloPredicado()
    {
        _fixture.Seed("L1", 1);
        _fixture.Seed("L2", 2);
        _fixture.Seed("L3", 3);

        var list = await _repository.ToListAsync(w => w.Stock > 1);

        list.Should().HaveCount(2);
        list.Should().OnlyContain(w => w.Stock > 1);
    }

    [Fact]
    public async Task UpdateAsync_DevePersistirAlteracoes()
    {
        var seeded = _fixture.Seed("UpAsync", 1);

        var entity = (await _repository.GetAsync(seeded.Id))!;
        entity.Stock = 55;
        await _repository.UpdateAsync(entity);
        await _session.FlushAsync();

        using var otherSession = _fixture.OpenSession();
        (await otherSession.GetAsync<Widget>(seeded.Id)).Stock.Should().Be(55);
    }

    [Fact]
    public async Task SaveOrUpdateAsync_TransienteInsere_DestacadaAtualiza()
    {
        var inserted = await _repository.SaveOrUpdateAsync(new Widget { Name = "SoU", Stock = 1 });
        await _session.FlushAsync();
        inserted.Id.Should().BePositive();

        var seeded = _fixture.Seed("SoU2", 1);
        seeded.Stock = 22;
        await _repository.SaveOrUpdateAsync(seeded);
        await _session.FlushAsync();

        using var otherSession = _fixture.OpenSession();
        (await otherSession.GetAsync<Widget>(seeded.Id)).Stock.Should().Be(22);
    }

    [Fact]
    public async Task DeleteAsync_DeveRemoverEntidade()
    {
        var seeded = _fixture.Seed("DelAsync", 1);

        var entity = (await _repository.GetAsync(seeded.Id))!;
        await _repository.DeleteAsync(entity);
        await _session.FlushAsync();

        using var otherSession = _fixture.OpenSession();
        (await otherSession.GetAsync<Widget>(seeded.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_PorPredicado_DeveRemoverApenasCorrespondentes()
    {
        _fixture.Seed("DelPred", 1);
        _fixture.Seed("DelPred", 2);
        _fixture.Seed("Fica", 3);

        await _repository.DeleteAsync(w => w.Name == "DelPred");
        await _session.FlushAsync();

        using var otherSession = _fixture.OpenSession();
        var remaining = otherSession.Query<Widget>().ToList();
        remaining.Should().ContainSingle(w => w.Name == "Fica");
    }

    [Fact]
    public async Task MergeAsync_DeveAtualizarAPartirDeInstanciaDestacada()
    {
        var seeded = _fixture.Seed("MergeAsync", 1);
        seeded.Stock = 33;

        var merged = await _repository.MergeAsync(seeded);
        await _session.FlushAsync();

        merged.Stock.Should().Be(33);

        using var otherSession = _fixture.OpenSession();
        (await otherSession.GetAsync<Widget>(seeded.Id)).Stock.Should().Be(33);
    }

    [Fact]
    public async Task RefreshAsync_DeveRecarregarEstadoDoBanco()
    {
        var seeded = _fixture.Seed("RefAsync", 10);

        var entity = (await _repository.GetAsync(seeded.Id))!;

        using (var otherSession = _fixture.OpenSession())
        using (var tx = otherSession.BeginTransaction())
        {
            var other = await otherSession.GetAsync<Widget>(seeded.Id);
            other.Stock = 321;
            await tx.CommitAsync();
        }

        var refreshed = await _repository.RefreshAsync(entity);

        refreshed.Should().BeSameAs(entity);
        refreshed.Stock.Should().Be(321);
    }

    [Fact]
    public async Task DeleteAsync_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var act = () => _repository.DeleteAsync((Widget)null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
