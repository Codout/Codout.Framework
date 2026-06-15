using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Codout.Framework.Domain.Base;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Domain.Tests;

/// <summary>
/// ValidatableObject: validação via DataAnnotations (Validator.TryValidateObject com
/// validateAllProperties habilitado).
/// </summary>
public class ValidatableObjectTests
{
    private class RegistrationForm : ValidatableObject
    {
        [Required]
        public string? Name { get; set; }

        [Range(1, 10)]
        public int Score { get; set; } = 5;

        [StringLength(5)]
        public string? Nickname { get; set; }

        protected override IEnumerable<PropertyInfo> GetTypeSpecificSignatureProperties() => [];
    }

    [Fact]
    public void Object_with_all_annotations_satisfied_is_valid()
    {
        var form = new RegistrationForm { Name = "Ana", Score = 10, Nickname = "an" };

        form.IsValid().Should().BeTrue();
        form.ValidationResults().Should().BeEmpty();
    }

    [Fact]
    public void Missing_required_property_makes_object_invalid()
    {
        var form = new RegistrationForm { Name = null };

        form.IsValid().Should().BeFalse();
        form.ValidationResults().Should().ContainSingle()
            .Which.MemberNames.Should().Contain("Name");
    }

    [Fact]
    public void Out_of_range_property_is_reported()
    {
        var form = new RegistrationForm { Name = "Ana", Score = 11 };

        form.IsValid().Should().BeFalse("validateAllProperties=true valida além de [Required]");
        form.ValidationResults().Should().ContainSingle()
            .Which.MemberNames.Should().Contain("Score");
    }

    [Fact]
    public void Multiple_violations_are_all_reported()
    {
        var form = new RegistrationForm { Name = null, Score = 0, Nickname = "muito-longo" };

        var results = form.ValidationResults();

        results.Should().HaveCount(3);
        results.SelectMany(r => r.MemberNames)
            .Should().BeEquivalentTo(["Name", "Score", "Nickname"]);
    }
}
