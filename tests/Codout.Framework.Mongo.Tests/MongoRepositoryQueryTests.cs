using FluentAssertions;
using Xunit;

namespace Codout.Framework.Mongo.Tests;

[Collection("Mongo")]
public class MongoRepositoryQueryTests
{
    private readonly MongoFixture _fixture;
    private readonly MongoRepository<Gadget> _repository;

    public MongoRepositoryQueryTests(MongoFixture fixture)
    {
        _fixture = fixture;
        if (_fixture.IsAvailable)
            _fixture.ResetCollection();
        _repository = _fixture.IsAvailable ? _fixture.CreateRepository() : null!;
    }

    private void SeedMany(int count, string prefix = "Item")
    {
        for (var i = 1; i <= count; i++)
            _fixture.Seed($"{prefix}-{i:D2}", i);
    }

    [SkippableFact]
    public void All_DeveRetornarTodosOsDocumentos()
    {
        _fixture.EnsureAvailable();
        SeedMany(3);

        _repository.All().ToList().Should().HaveCount(3);
    }

    [SkippableFact]
    public void All_SemDados_DeveRetornarVazio()
    {
        _fixture.EnsureAvailable();

        _repository.All().ToList().Should().BeEmpty();
    }

    [SkippableFact]
    public void AllReadOnly_DeveRetornarTodosOsDocumentos()
    {
        _fixture.EnsureAvailable();
        SeedMany(3);

        _repository.AllReadOnly().ToList().Should().HaveCount(3);
    }

    [SkippableFact]
    public void Where_DeveFiltrarPeloPredicado()
    {
        _fixture.EnsureAvailable();
        SeedMany(5);

        var result = _repository.Where(g => g.Price > 3).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(g => g.Price > 3);
    }

    [SkippableFact]
    public void Where_DeveComporComOperadoresLinq()
    {
        _fixture.EnsureAvailable();
        SeedMany(5);

        var name = _repository.Where(g => g.Price >= 2)
            .OrderByDescending(g => g.Price)
            .Select(g => g.Name)
            .First();

        name.Should().Be("Item-05");
    }

    [SkippableFact]
    public void WhereReadOnly_DeveFiltrarPeloPredicado()
    {
        _fixture.EnsureAvailable();
        SeedMany(4);

        _repository.WhereReadOnly(g => g.Price <= 2).ToList().Should().HaveCount(2);
    }

    // BUG?: MongoRepository.WherePaged calcula `total` DEPOIS de aplicar
    // Skip/Take — ou seja, `total` devolve o tamanho da página (<= size), e não
    // o total de registros que satisfazem o filtro (como faz o NHRepository,
    // que conta antes de paginar). Um chamador que use `total` para montar o
    // paginador verá sempre "1 página". Registrado em tests/FINDINGS-D.md.
    [SkippableFact]
    public void WherePaged_Caracterizacao_TotalRetornaTamanhoDaPaginaENaoOTotalDoFiltro()
    {
        _fixture.EnsureAvailable();
        SeedMany(7);

        var page = _repository.WherePaged(g => g.Price >= 3, out var total, index: 0, size: 2).ToList();

        page.Should().HaveCount(2);
        // Comportamento atual (incorreto): 5 documentos satisfazem o filtro,
        // mas total reporta apenas o tamanho da página.
        total.Should().Be(2, "comportamento atual: Count() é aplicado após Skip/Take");
    }

    [SkippableFact]
    public void WherePaged_SegundaPagina_DevePularItensAnteriores()
    {
        _fixture.EnsureAvailable();
        SeedMany(7);

        var page = _repository.WherePaged(g => g.Price >= 1, out _, index: 2, size: 3)
            .OrderBy(g => g.Price)
            .ToList();

        page.Should().HaveCount(1, "a última página contém só o resto");
        page[0].Price.Should().Be(7);
    }

    [SkippableFact]
    public void WherePaged_ComPaginaMaiorQueOFiltro_TotalCoincideComOFiltro()
    {
        _fixture.EnsureAvailable();
        SeedMany(4);

        var page = _repository.WherePaged(g => g.Price > 1, out var total, index: 0, size: 50).ToList();

        page.Should().HaveCount(3);
        total.Should().Be(3, "quando a página cobre todos os resultados o total 'parece' certo");
    }

    [SkippableFact]
    public void IncludeMany_DeveRetornarQueryableSemFalhar()
    {
        _fixture.EnsureAvailable();
        SeedMany(2);

        // MongoDB não suporta Include — o método devolve o queryable da coleção.
        _repository.IncludeMany(g => g.Name).ToList().Should().HaveCount(2);
    }
}
