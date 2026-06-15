namespace Codout.DynamicLinq.Tests;

public class Address
{
    public string City { get; set; } = string.Empty;
}

public class Person
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Nickname { get; set; }

    public int Age { get; set; }

    public int? Score { get; set; }

    public decimal Salary { get; set; }

    public DateTime BirthDate { get; set; }

    public string Category { get; set; } = string.Empty;

    public Address Address { get; set; } = new();
}

public static class TestData
{
    /// <summary>
    ///     Conjunto fixo e determinístico usado pela maioria dos testes.
    /// </summary>
    public static IQueryable<Person> People()
    {
        return new List<Person>
        {
            new()
            {
                Id = 1, Name = "Ana", Nickname = "Aninha", Age = 30, Score = 10, Salary = 1000.50m,
                BirthDate = new DateTime(1994, 5, 10, 12, 0, 0), Category = "A",
                Address = new Address { City = "Vitoria" }
            },
            new()
            {
                Id = 2, Name = "Bruno", Nickname = null, Age = 25, Score = null, Salary = 2000.00m,
                BirthDate = new DateTime(1999, 1, 20, 12, 0, 0), Category = "B",
                Address = new Address { City = "Belo Horizonte" }
            },
            new()
            {
                Id = 3, Name = "Carla", Nickname = "", Age = 40, Score = 30, Salary = 3000.75m,
                BirthDate = new DateTime(1984, 8, 1, 12, 0, 0), Category = "A",
                Address = new Address { City = "Curitiba" }
            },
            new()
            {
                Id = 4, Name = "Daniel", Nickname = "Dani", Age = 35, Score = 20, Salary = 1500.25m,
                BirthDate = new DateTime(1989, 12, 31, 12, 0, 0), Category = "B",
                Address = new Address { City = "Vitoria" }
            },
            new()
            {
                Id = 5, Name = "Eduarda", Nickname = "Duda", Age = 28, Score = 50, Salary = 2500.00m,
                BirthDate = new DateTime(1996, 3, 15, 12, 0, 0), Category = "C",
                Address = new Address { City = "Salvador" }
            }
        }.AsQueryable();
    }

    public static Filter Single(string field, string @operator, object? value)
    {
        // O método Filters() de QueryableExtensions só aplica o filtro quando
        // Logic != null, então todo filtro simples precisa ser embrulhado.
        return new Filter
        {
            Logic = "and",
            Filters = new List<Filter>
            {
                new() { Field = field, Operator = @operator, Value = value }
            }
        };
    }

    public static List<Person> DataOf(DataSourceResult result)
    {
        return ((IEnumerable<Person>)result.Data!).ToList();
    }
}
