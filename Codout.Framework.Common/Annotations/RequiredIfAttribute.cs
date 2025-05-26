using System;
using System.ComponentModel.DataAnnotations;

namespace Codout.Framework.Common.Annotations;

/// <summary>
/// Verifica se uma propriedade possui um valor específico.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class RequiredIfAttribute : ConditionalAttributeBase
{
    #region Variáveis
    private readonly RequiredAttribute _innerAttribute = new RequiredAttribute();
    #endregion

    #region Propriedades
    /// <summary>
    /// Propriedade dependente.
    /// </summary>
    public string DependentProperty { get; set; }

    /// <summary>
    /// Alvo.
    /// </summary>
    public object TargetValue { get; set; }
    #endregion

    #region Construtores
    /// <summary>
    /// Construtor.
    /// </summary>
    /// <param name="dependentProperty"></param>
    /// <param name="targetValue"></param>
    public RequiredIfAttribute(string dependentProperty, object targetValue)
        : this(dependentProperty, targetValue, null)
    {
    }

    /// <summary>
    /// Construtor.
    /// </summary>
    /// <param name="dependentProperty"></param>
    /// <param name="targetValue"></param>
    /// <param name="errorMessage"></param>
    public RequiredIfAttribute(string dependentProperty, object targetValue, string errorMessage)
        : base(errorMessage)
    {
        DependentProperty = dependentProperty;
        TargetValue = targetValue;
    }
    #endregion

    #region IsValid
    /// <summary>
    /// Validates the specified value with respect to the current validation attribute.
    /// </summary>
    /// <returns>
    /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> class. 
    /// </returns>
    /// <param name="value">The value to validate.</param><param name="validationContext">The context information about the validation operation.</param>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // check if the current value matches the target value
        if (ShouldRunValidation(value, DependentProperty, TargetValue, validationContext))
        {
            // match => means we should try validating this field
            if (!_innerAttribute.IsValid(value))
                // validation failed - return an error
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new[] { validationContext.MemberName });
        }

        return ValidationResult.Success;
    }
    #endregion
}
