using Codout.Framework.Domain.Entities;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;
using Xunit;

namespace Codout.Framework.NH.Tests;

// ---------------------------------------------------------------------------
// Infraestrutura compartilhada pelos testes de NHRepository / NHUnitOfWork:
// entidade de teste, mapeamento FluentNHibernate e fixture com SessionFactory
// sobre SQLite (arquivo temporário, via MicrosoftDataSqliteDriver).
// ---------------------------------------------------------------------------

/// <summary>Entidade simples com PK int identity (transient quando Id == 0).</summary>
public class Widget : Entity<int>
{
    public virtual string Name { get; set; } = "";
    public virtual int Stock { get; set; }
}

/// <summary>
/// Driver NHibernate para Microsoft.Data.Sqlite, usado somente nos testes.
/// O NHibernate 5.6.0 NÃO traz um MicrosoftDataSqliteDriver embutido (apenas
/// SQLite20Driver, que reflete sobre System.Data.SQLite) — ver FINDINGS-D.md.
/// </summary>
public class MicrosoftDataSqliteTestDriver : ReflectionBasedDriver
{
    public MicrosoftDataSqliteTestDriver()
        : base(
            "Microsoft.Data.Sqlite",
            "Microsoft.Data.Sqlite",
            "Microsoft.Data.Sqlite.SqliteConnection",
            "Microsoft.Data.Sqlite.SqliteCommand")
    {
    }

    public override bool UseNamedPrefixInSql => true;
    public override bool UseNamedPrefixInParameter => true;
    public override string NamedPrefix => "@";
    public override bool SupportsMultipleOpenReaders => false;
}

/// <summary>Mapeamento FluentNHibernate básico da entidade de teste.</summary>
public class WidgetMap : ClassMap<Widget>
{
    public WidgetMap()
    {
        Table("widgets");
        Id(x => x.Id).GeneratedBy.Identity();
        Map(x => x.Name).Not.Nullable();
        Map(x => x.Stock);
    }
}

/// <summary>
/// Fixture de coleção: constrói um único ISessionFactory sobre um banco SQLite
/// em arquivo temporário (NHibernate abre/fecha conexões por sessão, então
/// :memory: não serve — o banco sumiria entre sessões). Cada classe de teste
/// limpa a tabela no construtor para garantir isolamento.
/// </summary>
public sealed class NHSqliteFixture : IDisposable
{
    private readonly string _dbFile;

    public ISessionFactory Factory { get; }

    public NHSqliteFixture()
    {
        _dbFile = Path.Combine(Path.GetTempPath(), $"codout-nh-tests-{Guid.NewGuid():N}.db");

        Factory = Fluently.Configure()
            .Database(SQLiteConfiguration.Standard
                .Driver<MicrosoftDataSqliteTestDriver>()
                .ConnectionString($"Data Source={_dbFile}"))
            .Mappings(m => m.FluentMappings.Add<WidgetMap>())
            .ExposeConfiguration(cfg =>
            {
                // Microsoft.Data.Sqlite não implementa GetSchema("DataTypes"),
                // usado pelo auto-import de keywords no build da factory.
                cfg.SetProperty(NHibernate.Cfg.Environment.Hbm2ddlKeyWords, "none");
                new SchemaExport(cfg).Create(useStdOut: false, execute: true);
            })
            .BuildSessionFactory();
    }

    public ISession OpenSession() => Factory.OpenSession();

    /// <summary>Remove todas as linhas da tabela de teste.</summary>
    public void ResetDatabase()
    {
        using var session = Factory.OpenSession();
        session.CreateSQLQuery("DELETE FROM widgets").ExecuteUpdate();
    }

    /// <summary>Insere um Widget já comitado, fora da sessão sob teste.</summary>
    public Widget Seed(string name, int stock = 0)
    {
        using var session = Factory.OpenSession();
        using var tx = session.BeginTransaction();
        var widget = new Widget { Name = name, Stock = stock };
        session.Save(widget);
        tx.Commit();
        return widget;
    }

    public void Dispose()
    {
        Factory.Dispose();
        if (File.Exists(_dbFile))
            File.Delete(_dbFile);
    }
}

[CollectionDefinition("NH")]
public class NHCollection : ICollectionFixture<NHSqliteFixture>;
