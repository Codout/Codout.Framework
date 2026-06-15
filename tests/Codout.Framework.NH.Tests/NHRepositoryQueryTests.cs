using FluentAssertions;
using NHibernate;
using Xunit;

namespace Codout.Framework.NH.Tests;

[Collection("NH")]
public class NHRepositoryQueryTests : IDisposable
{
    private readonly NHSqliteFixture _fixture;
    private readonly ISession _session;
    private readonly NHRepository<Widget> _repository;

    public NHRepositoryQueryTests(NHSqliteFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _session = _fixture.OpenSession();
        _repository = new NHRepository<Widget>(_session);
    }

    public void Dispose() => _session.Dispose();

    private void SeedMany(int count, string prefix = "Item")
    {
        for (var i = 1; i <= count; i++)
            _fixture.Seed($"{prefix}-{i:D2}", i);
    }

    [Fact]
    public void All_DeveRetornarTodasAsEntidades()
    {
        SeedMany(3);

        _repository.All().ToList().Should().HaveCount(3);
    }

    [Fact]
    public void All_SemDados_DeveRetornarVazio()
    {
        _repository.All().ToList().Should().BeEmpty();
    }

    [Fact]
    public void Where_DeveFiltrarPeloPredicado()
    {
        SeedMany(5);

        var result = _repository.Where(w => w.Stock > 3).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(w => w.Stock > 3);
    }

    [Fact]
    public void Where_DeveComporComOperadoresLinq()
    {
        SeedMany(5);

        var result = _repository.Where(w => w.Stock >= 2)
            .OrderByDescending(w => w.Stock)
            .Select(w => w.Name)
            .First();

        result.Should().Be("Item-05");
    }

    [Fact]
    public void WherePaged_DeveRetornarTotalDoFiltroEPaginar()
    {
        SeedMany(7);

        var page = _repository.WherePaged(w => w.Stock >= 3, out var total, index: 0, size: 2)
            .OrderBy(w => w.Stock)
            .ToList();

        total.Should().Be(5, "o total reflete todos os registros do filtro, não só a página");
        page.Should().HaveCount(2);
        page.Select(w => w.Stock).Should().ContainInOrder(3, 4);
    }

    [Fact]
    public void WherePaged_SegundaPagina_DevePularItensAnteriores()
    {
        SeedMany(7);

        var page = _repository.WherePaged(w => w.Stock >= 1, out var total, index: 2, size: 3)
            .OrderBy(w => w.Stock)
            .ToList();

        total.Should().Be(7);
        page.Should().HaveCount(1, "a última página contém só o resto");
        page[0].Stock.Should().Be(7);
    }

    // BUG?: WherePaged aplica Skip/Take na ordem natural da query sem exigir
    // OrderBy — em SQL o resultado de paginação sem ORDER BY não é determinístico.
    // Aqui apenas caracterizamos que a chamada funciona sem ordenação explícita.
    [Fact]
    public void WherePaged_SemOrdenacaoExplicita_NaoFalha()
    {
        SeedMany(4);

        var page = _repository.WherePaged(w => w.Stock > 0, out var total, index: 0, size: 10).ToList();

        total.Should().Be(4);
        page.Should().HaveCount(4);
    }

    [Fact]
    public void AllReadOnly_DeveRetornarTodasAsEntidades()
    {
        SeedMany(3);

        _repository.AllReadOnly().ToList().Should().HaveCount(3);
    }

    [Fact]
    public void AllReadOnly_DeveMarcarASessaoComoReadOnly()
    {
        _session.DefaultReadOnly.Should().BeFalse();

        _repository.AllReadOnly();

        _session.DefaultReadOnly.Should().BeTrue();
    }

    // BUG?: AllReadOnly/WhereReadOnly setam Session.DefaultReadOnly = true e NUNCA
    // restauram o valor — a sessão INTEIRA vira read-only dali em diante. Qualquer
    // entidade carregada depois (mesmo via All()/Get()) deixa de ser rastreada
    // pelo dirty-check e alterações silenciosamente não são persistidas no Flush.
    // Registrado em tests/FINDINGS-D.md.
    [Fact]
    public void AllReadOnly_EfeitoColateral_EntidadesCarregadasDepoisNaoSaoPersistidas()
    {
        var seeded = _fixture.Seed("Colateral", 1);

        _repository.AllReadOnly(); // liga o modo read-only da sessão

        var entity = _repository.Get(seeded.Id); // carregada APÓS o AllReadOnly
        entity.Stock = 999;
        _session.Flush();

        using var otherSession = _fixture.OpenSession();
        otherSession.Get<Widget>(seeded.Id).Stock
            .Should().Be(1, "a sessão ficou read-only e o dirty-check foi desligado");
    }

    [Fact]
    public void WhereReadOnly_DeveFiltrarEMarcarASessaoComoReadOnly()
    {
        SeedMany(4);

        var result = _repository.WhereReadOnly(w => w.Stock <= 2).ToList();

        result.Should().HaveCount(2);
        _session.DefaultReadOnly.Should().BeTrue();
    }

    [Fact]
    public void IncludeMany_DeveRetornarQueryableSemFalhar()
    {
        SeedMany(2);

        // NHibernate não suporta Include — o método devolve All().
        _repository.IncludeMany(w => w.Name).ToList().Should().HaveCount(2);
    }
}
