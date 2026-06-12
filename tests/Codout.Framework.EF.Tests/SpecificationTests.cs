using Codout.Framework.EF.Specifications;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.EF.Tests;

/// <summary>
/// Specification Pattern: Specification&lt;T&gt;, SpecificationEvaluator e as extensões
/// GetBySpecification / ListAsync / FirstOrDefaultAsync / CountAsync / AnyAsync.
/// </summary>
public class SpecificationTests : SqliteTestBase
{
    private sealed class AdultsByNameSpec : Specification<Customer>
    {
        public AdultsByNameSpec(bool noTracking = false, int? skip = null, int? take = null)
        {
            AddCriteria(c => c.Age >= 18);
            ApplyOrderBy(q => q.OrderBy(c => c.Name));

            if (noTracking)
                ApplyNoTracking();

            if (skip.HasValue && take.HasValue)
                ApplyPaging(skip.Value, take.Value);
        }
    }

    private sealed class BlogWithPostsSpec : Specification<Blog>
    {
        public BlogWithPostsSpec(string title, bool useStringInclude = false)
        {
            AddCriteria(b => b.Title == title);

            if (useStringInclude)
                AddInclude("Posts");
            else
                AddInclude(b => b.Posts);
        }
    }

    private void SeedCustomers()
    {
        using var context = CreateContext();
        context.Customers.AddRange(
            new Customer { Name = "Caio", Age = 65 },
            new Customer { Name = "Ana", Age = 30 },
            new Customer { Name = "Lia", Age = 10 },
            new Customer { Name = "Bia", Age = 40 });
        context.SaveChanges();
    }

    [Fact]
    public void Criteria_and_ordering_are_applied()
    {
        SeedCustomers();

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        var result = repository.GetBySpecification(new AdultsByNameSpec()).ToList();

        result.Select(c => c.Name).Should().Equal("Ana", "Bia", "Caio");
    }

    [Fact]
    public void Paging_applies_after_ordering()
    {
        SeedCustomers();

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        var page = repository.GetBySpecification(new AdultsByNameSpec(skip: 1, take: 1)).ToList();

        page.Should().ContainSingle().Which.Name.Should().Be("Bia");
    }

    [Fact]
    public void AsNoTracking_specification_does_not_track()
    {
        SeedCustomers();

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        _ = repository.GetBySpecification(new AdultsByNameSpec(noTracking: true)).ToList();

        context.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public void Includes_load_related_entities_by_expression_and_by_string()
    {
        using (var seed = CreateContext())
        {
            var blog = new Blog { Title = "b1" };
            blog.Posts.Add(new Post { BlogId = blog.Id!.Value, Content = "p1" });
            blog.Posts.Add(new Post { BlogId = blog.Id!.Value, Content = "p2" });
            seed.Blogs.Add(blog);
            seed.SaveChanges();
        }

        using (var context = CreateContext())
        {
            var repository = new EFRepository<Blog>(context);
            var loaded = repository.GetBySpecification(new BlogWithPostsSpec("b1")).Single();
            loaded.Posts.Should().HaveCount(2, "include por expressão");
        }

        using (var context = CreateContext())
        {
            var repository = new EFRepository<Blog>(context);
            var loaded = repository.GetBySpecification(new BlogWithPostsSpec("b1", useStringInclude: true)).Single();
            loaded.Posts.Should().HaveCount(2, "include por string");
        }
    }

    [Fact]
    public async Task Async_extension_helpers_evaluate_the_specification()
    {
        SeedCustomers();

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);
        var spec = new AdultsByNameSpec();

        (await repository.ListAsync(spec)).Should().HaveCount(3);
        (await repository.FirstOrDefaultAsync(spec))!.Name.Should().Be("Ana");
        (await repository.CountAsync(spec)).Should().Be(3);
        (await repository.AnyAsync(spec)).Should().BeTrue();
    }

    [Fact]
    public async Task Async_extension_helpers_honor_a_cancelled_token()
    {
        SeedCustomers();

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);
        var cancelled = new CancellationToken(canceled: true);

        await FluentActions.Awaiting(() => repository.ListAsync(new AdultsByNameSpec(), cancelled))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Specification_defaults_are_off()
    {
        var spec = new AdultsByNameSpec();

        spec.IsPagingEnabled.Should().BeFalse();
        spec.AsNoTracking.Should().BeFalse();
        spec.Skip.Should().Be(0);
        spec.Take.Should().Be(0);
        spec.Includes.Should().BeEmpty();
        spec.IncludeStrings.Should().BeEmpty();
    }
}
