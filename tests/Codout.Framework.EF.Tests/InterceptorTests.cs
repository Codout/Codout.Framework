using Codout.Framework.EF.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Codout.Framework.EF.Tests;

/// <summary>
/// AuditableInterceptor e SoftDeleteInterceptor sobre SaveChanges/SaveChangesAsync.
/// </summary>
public class InterceptorTests : SqliteTestBase
{
    private sealed class FixedUserProvider(string? userId) : ICurrentUserProvider
    {
        public string? GetCurrentUserId() => userId;
    }

    // ---- AuditableInterceptor ----

    [Fact]
    public void Insert_stamps_CreatedAt_and_CreatedBy()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        using var context = CreateContext(new AuditableInterceptor(new FixedUserProvider("ana")));
        var document = new AuditedDocument { Title = "doc" };
        context.Add(document);
        context.SaveChanges();

        document.CreatedAt.Should().BeOnOrAfter(before);
        document.CreatedBy.Should().Be("ana");
        document.UpdatedAt.Should().BeNull("insert não marca update");
        document.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public async Task Update_stamps_UpdatedAt_and_UpdatedBy_async()
    {
        Guid id;
        using (var seed = CreateContext(new AuditableInterceptor(new FixedUserProvider("ana"))))
        {
            var document = new AuditedDocument { Title = "doc" };
            seed.Add(document);
            await seed.SaveChangesAsync();
            id = document.Id!.Value;
        }

        using var context = CreateContext(new AuditableInterceptor(new FixedUserProvider("bia")));
        var loaded = await context.AuditedDocuments.SingleAsync(d => d.Id == id);
        loaded.Title = "doc v2";
        await context.SaveChangesAsync();

        loaded.UpdatedAt.Should().NotBeNull();
        loaded.UpdatedBy.Should().Be("bia");
        loaded.CreatedBy.Should().Be("ana", "o CreatedBy original é preservado");
    }

    [Fact]
    public void Interceptor_works_without_a_current_user_provider()
    {
        using var context = CreateContext(new AuditableInterceptor());
        var document = new AuditedDocument { Title = "doc" };
        context.Add(document);
        context.SaveChanges();

        document.CreatedAt.Should().NotBe(default);
        document.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void Entity_implementing_only_the_Data_Auditing_IAuditable_is_not_audited()
    {
        // BUG?: existem DUAS interfaces IAuditable no ecossistema — a abstração pública
        // Codout.Framework.Data.Auditing.IAuditable e a duplicata local
        // Codout.Framework.EF.Interceptors.IAuditable. Por resolução de nomes, o
        // AuditableInterceptor enxerga apenas a duplicata LOCAL; entidades que
        // implementam somente a abstração pública são silenciosamente ignoradas.
        // Characterization test do comportamento atual — ver tests/FINDINGS-B.md.
        typeof(Data.Auditing.IAuditable).Should().NotBe(typeof(Interceptors.IAuditable));

        using var context = CreateContext(new AuditableInterceptor(new FixedUserProvider("ana")));
        var document = new DataAuditedDocument { Title = "doc" };
        context.Add(document);
        context.SaveChanges();

        document.CreatedAt.Should().Be(default, "a entidade não implementa a IAuditable local do interceptor");
        document.CreatedBy.Should().BeNull();
    }

    // ---- SoftDeleteInterceptor ----

    [Fact]
    public void Delete_becomes_soft_delete_keeping_the_row()
    {
        Guid id;
        using (var seed = CreateContext())
        {
            var item = new SoftItem { Name = "item" };
            seed.Add(item);
            seed.SaveChanges();
            id = item.Id!.Value;
        }

        using (var context = CreateContext(new SoftDeleteInterceptor(new FixedUserProvider("ana"))))
        {
            var item = context.SoftItems.Single(i => i.Id == id);
            context.Remove(item);
            context.SaveChanges();

            item.IsDeleted.Should().BeTrue();
            item.DeletedAt.Should().NotBeNull();
            item.DeletedBy.Should().Be("ana");
        }

        using var verify = CreateContext();
        var row = verify.SoftItems.Single(i => i.Id == id);
        row.IsDeleted.Should().BeTrue("a linha permanece no banco, marcada como deletada");
    }

    [Fact]
    public async Task Async_delete_is_also_soft()
    {
        Guid id;
        using (var seed = CreateContext())
        {
            var item = new SoftItem { Name = "item" };
            seed.Add(item);
            await seed.SaveChangesAsync();
            id = item.Id!.Value;
        }

        using (var context = CreateContext(new SoftDeleteInterceptor()))
        {
            var item = await context.SoftItems.SingleAsync(i => i.Id == id);
            context.Remove(item);
            await context.SaveChangesAsync();
        }

        using var verify = CreateContext();
        (await verify.SoftItems.CountAsync()).Should().Be(1);
        (await verify.SoftItems.SingleAsync(i => i.Id == id)).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Without_the_interceptor_delete_is_physical()
    {
        Guid id;
        using (var seed = CreateContext())
        {
            var item = new SoftItem { Name = "item" };
            seed.Add(item);
            seed.SaveChanges();
            id = item.Id!.Value;
        }

        using (var context = CreateContext())
        {
            context.Remove(context.SoftItems.Single(i => i.Id == id));
            context.SaveChanges();
        }

        using var verify = CreateContext();
        verify.SoftItems.Should().BeEmpty("baseline: sem o interceptor o DELETE é físico");
    }
}
