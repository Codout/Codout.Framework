using Codout.Framework.Common.Helpers;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Helpers;

public class LimitedListTests
{
    [Fact]
    public void Add_AbaixoDoLimite_IncrementaCount()
    {
        var list = new LimitedList<int>(3) { 1, 2 };
        list.Count.Should().Be(2);
        list[0].Should().Be(1);
        list[1].Should().Be(2);
    }

    [Fact]
    public void Add_AlemDoLimite_DescartaOMaisAntigo()
    {
        var list = new LimitedList<int>(3) { 1, 2, 3, 4 };

        list.Count.Should().Be(3);
        list[0].Should().Be(2);
        list[1].Should().Be(3);
        list[2].Should().Be(4);
    }

    [Fact]
    public void Contains_EncontraItens()
    {
        var list = new LimitedList<string>(2) { "a", "b" };
        list.Contains("a").Should().BeTrue();
        list.Contains("z").Should().BeFalse();
    }

    [Fact]
    public void Clear_ZeraContagem()
    {
        var list = new LimitedList<int>(2) { 1, 2 };
        list.Clear();
        list.Count.Should().Be(0);
        list.Contains(1).Should().BeFalse();
    }

    [Fact]
    public void Indexer_ForaDoIntervalo_LancaExcecao()
    {
        var list = new LimitedList<int>(2);
        var act = () => list[5];
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ToArray_RetornaTodosOsSlots()
    {
        var list = new LimitedList<int>(3) { 1, 2, 3 };
        list.ToArray().Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Enumeracao_PercorreOsSlots()
    {
        var list = new LimitedList<int>(3) { 7, 8, 9 };
        var items = new List<int>();
        foreach (int item in list)
            items.Add(item);
        items.Should().Equal(7, 8, 9);
    }
}
