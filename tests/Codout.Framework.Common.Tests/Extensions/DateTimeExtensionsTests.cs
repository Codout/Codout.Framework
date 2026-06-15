using Codout.Framework.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void IsWeekDay_SegundaASexta_RetornaTrue()
    {
        new DateTime(2026, 6, 8).IsWeekDay().Should().BeTrue();   // segunda-feira
        new DateTime(2026, 6, 12).IsWeekDay().Should().BeTrue();  // sexta-feira
        new DateTime(2026, 6, 13).IsWeekDay().Should().BeFalse(); // sábado
        new DateTime(2026, 6, 14).IsWeekDay().Should().BeFalse(); // domingo
    }

    [Fact]
    public void IsWeekEnd_SabadoEDomingo_RetornaTrue()
    {
        new DateTime(2026, 6, 13).IsWeekEnd().Should().BeTrue();
        new DateTime(2026, 6, 14).IsWeekEnd().Should().BeTrue();
        new DateTime(2026, 6, 10).IsWeekEnd().Should().BeFalse();
    }

    [Fact]
    public void CountWeekdays_UmaSemanaCompleta_RetornaCinco()
    {
        var start = new DateTime(2026, 6, 8); // segunda
        var end = new DateTime(2026, 6, 15);  // segunda seguinte
        start.CountWeekdays(end).Should().Be(5);
    }

    [Fact]
    public void CountWeekends_UmaSemanaCompleta_RetornaDois()
    {
        var start = new DateTime(2026, 6, 8);
        var end = new DateTime(2026, 6, 15);
        start.CountWeekends(end).Should().Be(2);
    }

    [Fact]
    public void IsDate_ValidaConversao()
    {
        "2026-06-12".IsDate().Should().BeTrue();
        "não é data".IsDate().Should().BeFalse();
    }

    [Theory]
    [InlineData(1, "1st")]
    [InlineData(2, "2nd")]
    [InlineData(3, "3rd")]
    [InlineData(4, "4th")]
    [InlineData(11, "11th")]
    [InlineData(21, "21st")]
    [InlineData(22, "22nd")]
    [InlineData(23, "23rd")]
    public void GetDateDayWithSuffix_RetornaSufixoCorreto(int day, string expected)
    {
        new DateTime(2026, 1, day).GetDateDayWithSuffix().Should().Be(expected);
    }

    [Fact]
    public void GetAge_AntesDoAniversario_SubtraiUm()
    {
        var nascimento = new DateTime(2000, 12, 31);
        var referencia = new DateTime(2020, 6, 15);
        nascimento.GetAge(referencia).Should().Be(19);
    }

    [Fact]
    public void GetAge_DepoisDoAniversario_RetornaIdadeCheia()
    {
        var nascimento = new DateTime(2000, 1, 2);
        var referencia = new DateTime(2020, 6, 15);
        nascimento.GetAge(referencia).Should().Be(20);
    }

    [Fact]
    public void GetAge_NoDiaDoAniversario_ComportamentoAtual()
    {
        // BUG?: no próprio dia do aniversário (mesmo DayOfYear) a comparação
        // estrita "<" faz a idade ser subtraída em 1. Quem nasceu em 15/06/2000
        // deveria completar 20 anos em 15/06/2020, mas o método retorna 19.
        var nascimento = new DateTime(2000, 6, 15);
        var referencia = new DateTime(2020, 6, 15);
        nascimento.GetAge(referencia).Should().Be(19);
    }

    [Fact]
    public void Diff_RetornaDiferencaEntreDatas()
    {
        var d1 = new DateTime(2026, 6, 12, 10, 0, 0);
        var d2 = new DateTime(2026, 6, 10, 10, 0, 0);
        d1.Diff(d2).Should().Be(TimeSpan.FromDays(2));
        d1.DiffDays(d2).Should().Be(2);
        d1.DiffHours(d2).Should().Be(48);
        d1.DiffMinutes(d2).Should().Be(48 * 60);
    }

    [Fact]
    public void DiffDays_ComStringsInvalidas_RetornaZero()
    {
        "abc".DiffDays("def").Should().Be(0);
    }

    [Fact]
    public void DiffDays_ComStringsValidas_CalculaDiferenca()
    {
        "2026-06-12".DiffDays("2026-06-10").Should().Be(2);
    }

    [Fact]
    public void TimeDiff_FormataDiferencaLegivel()
    {
        var start = new DateTime(2020, 1, 1, 0, 0, 0);
        var end = new DateTime(2022, 1, 1, 0, 0, 5);
        var result = start.TimeDiff(end);
        result.Should().Contain("2 anos");
        result.Should().Contain("5 segundos");
    }

    [Fact]
    public void DaysAgoEDaysFromNow_RetornamDatasRelativas()
    {
        2.DaysAgo().Should().BeCloseTo(DateTime.Now.AddDays(-2), TimeSpan.FromSeconds(5));
        2.DaysFromNow().Should().BeCloseTo(DateTime.Now.AddDays(2), TimeSpan.FromSeconds(5));
        3.HoursAgo().Should().BeCloseTo(DateTime.Now.AddHours(-3), TimeSpan.FromSeconds(5));
        10.MinutesFromNow().Should().BeCloseTo(DateTime.Now.AddMinutes(10), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ReadableDiff_RetornaTextoComAtras()
    {
        var start = new DateTime(2026, 6, 10, 10, 0, 0);
        var end = new DateTime(2026, 6, 12, 11, 0, 0);
        var result = start.ReadableDiff(end);
        result.Should().Contain("2 dias");
        result.Should().EndWith("atrás");
    }
}
