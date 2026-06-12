using FluentAssertions;
using NHibernate;
using Xunit;

namespace Codout.Framework.NH.Tests;

[Collection("NH")]
public class NHRepositoryCrudTests : IDisposable
{
    private readonly NHSqliteFixture _fixture;
    private readonly ISession _session;
    private readonly NHRepository<Widget> _repository;

    public NHRepositoryCrudTests(NHSqliteFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _session = _fixture.OpenSession();
        _repository = new NHRepository<Widget>(_session);
    }

    public void Dispose() => _session.Dispose();

    [Fact]
    public void Construtor_ComSessionNula_DeveLancarArgumentNullException()
    {
        var act = () => new NHRepository<Widget>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Save_DevePersistirEntidadeEGerarId()
    {
        var widget = _repository.Save(new Widget { Name = "Parafuso", Stock = 10 });

        widget.Id.Should().BePositive("o generator identity atribui o Id no Save");
        widget.IsTransient().Should().BeFalse();

        using var otherSession = _fixture.OpenSession();
        var persisted = otherSession.Get<Widget>(widget.Id);
        persisted.Should().NotBeNull();
        persisted.Name.Should().Be("Parafuso");
        persisted.Stock.Should().Be(10);
    }

    [Fact]
    public void Save_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var act = () => _repository.Save(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Get_PorChave_DeveRetornarEntidade()
    {
        var seeded = _fixture.Seed("Porca", 5);

        var found = _repository.Get(seeded.Id);

        found.Should().NotBeNull();
        found.Name.Should().Be("Porca");
    }

    [Fact]
    public void Get_PorChaveInexistente_DeveRetornarNull()
    {
        _repository.Get(99999).Should().BeNull();
    }

    [Fact]
    public void Get_PorPredicado_DeveRetornarEntidadeUnica()
    {
        _fixture.Seed("Arruela", 1);
        _fixture.Seed("Prego", 2);

        var found = _repository.Get(w => w.Name == "Prego");

        found.Should().NotBeNull();
        found.Stock.Should().Be(2);
    }

    [Fact]
    public void Get_PorPredicadoComMultiplosResultados_DeveLancarExcecao()
    {
        _fixture.Seed("Duplicado", 1);
        _fixture.Seed("Duplicado", 2);

        var act = () => _repository.Get(w => w.Name == "Duplicado");

        // Usa SingleOrDefault — mais de um resultado é erro.
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Load_ChaveExistente_DeveRetornarEntidade()
    {
        var seeded = _fixture.Seed("Martelo", 3);

        var loaded = _repository.Load(seeded.Id);

        loaded.Name.Should().Be("Martelo");
    }

    [Fact]
    public void Load_ChaveInexistente_DeveLancarAoAcessarProxy()
    {
        var proxy = _repository.Load(99999);

        // Load retorna proxy sem ir ao banco; o acesso a um membro dispara a carga.
        var act = () => _ = proxy.Name;
        act.Should().Throw<ObjectNotFoundException>();
    }

    [Fact]
    public void Update_DevePersistirAlteracoes()
    {
        var seeded = _fixture.Seed("Chave", 1);

        var entity = _repository.Get(seeded.Id);
        entity.Stock = 42;
        _repository.Update(entity);
        _session.Flush();

        using var otherSession = _fixture.OpenSession();
        otherSession.Get<Widget>(seeded.Id).Stock.Should().Be(42);
    }

    [Fact]
    public void Update_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var act = () => _repository.Update(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Delete_DeveRemoverEntidade()
    {
        var seeded = _fixture.Seed("Lixa", 1);

        var entity = _repository.Get(seeded.Id);
        _repository.Delete(entity);
        _session.Flush();

        using var otherSession = _fixture.OpenSession();
        otherSession.Get<Widget>(seeded.Id).Should().BeNull();
    }

    [Fact]
    public void Delete_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var act = () => _repository.Delete((Widget)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Delete_PorPredicado_DeveRemoverApenasCorrespondentes()
    {
        _fixture.Seed("Remover", 1);
        _fixture.Seed("Remover", 2);
        _fixture.Seed("Manter", 3);

        _repository.Delete(w => w.Name == "Remover");
        _session.Flush();

        using var otherSession = _fixture.OpenSession();
        var remaining = otherSession.Query<Widget>().ToList();
        remaining.Should().ContainSingle(w => w.Name == "Manter");
    }

    [Fact]
    public void SaveOrUpdate_ComEntidadeTransiente_DeveInserir()
    {
        var widget = _repository.SaveOrUpdate(new Widget { Name = "Novo", Stock = 1 });
        _session.Flush();

        widget.Id.Should().BePositive();

        using var otherSession = _fixture.OpenSession();
        otherSession.Get<Widget>(widget.Id).Should().NotBeNull();
    }

    [Fact]
    public void SaveOrUpdate_ComEntidadeDestacada_DeveAtualizar()
    {
        var seeded = _fixture.Seed("Velho", 1);
        seeded.Stock = 99; // instância destacada (sessão da Seed já foi fechada)

        _repository.SaveOrUpdate(seeded);
        _session.Flush();

        using var otherSession = _fixture.OpenSession();
        otherSession.Get<Widget>(seeded.Id).Stock.Should().Be(99);
    }

    [Fact]
    public void SaveOrUpdate_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var act = () => _repository.SaveOrUpdate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Merge_ComEntidadeDestacada_DeveRetornarInstanciaPersistenteAtualizada()
    {
        var seeded = _fixture.Seed("Mesclar", 1);

        var attached = _repository.Get(seeded.Id);
        seeded.Stock = 77;

        var merged = _repository.Merge(seeded);
        _session.Flush();

        merged.Should().BeSameAs(attached, "Merge devolve a instância já associada à sessão");
        merged.Stock.Should().Be(77);

        using var otherSession = _fixture.OpenSession();
        otherSession.Get<Widget>(seeded.Id).Stock.Should().Be(77);
    }

    [Fact]
    public void Merge_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var act = () => _repository.Merge(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Refresh_DeveRecarregarEstadoDoBanco()
    {
        var seeded = _fixture.Seed("Atualizar", 10);

        var entity = _repository.Get(seeded.Id);

        // Altera o banco por fora da sessão corrente.
        using (var otherSession = _fixture.OpenSession())
        using (var tx = otherSession.BeginTransaction())
        {
            var other = otherSession.Get<Widget>(seeded.Id);
            other.Stock = 123;
            tx.Commit();
        }

        var refreshed = _repository.Refresh(entity);

        refreshed.Should().BeSameAs(entity);
        refreshed.Stock.Should().Be(123);
    }

    [Fact]
    public void Refresh_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var act = () => _repository.Refresh(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Dispose_DeveSerIdempotenteENaoFecharASessao()
    {
        var repository = new NHRepository<Widget>(_session);

        repository.Dispose();
        repository.Dispose();

        // A sessão pertence ao UnitOfWork — o repositório não a fecha.
        _session.IsOpen.Should().BeTrue();
    }
}
